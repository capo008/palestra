using System.Data;
using Dapper;
using palestra.Models;

namespace palestra.Data;

public class PalestraRepository
{
    private readonly IDbConnectionFactory _db;

    public PalestraRepository(IDbConnectionFactory db) => _db = db;

    private const string LastId = "SELECT LAST_INSERT_ID();";

    // ------------------------------------------------------------------
    // Liste per le tendine dei form
    // ------------------------------------------------------------------
    public Task<List<Opzione>> GetIscrittiAsync() => ListaAsync(
        "SELECT id_iscritto AS Id, CONCAT(cognome, ' ', nome, ' (', email, ')') AS Testo FROM iscritto ORDER BY cognome, nome");

    public Task<List<Opzione>> GetCorsiAsync() => ListaAsync(
        "SELECT id_corso AS Id, nome AS Testo FROM corso ORDER BY nome");

    public Task<List<Opzione>> GetSaleAsync() => ListaAsync(
        "SELECT id_sala AS Id, CONCAT(nome, ' (', capienza, ' posti)') AS Testo FROM sala ORDER BY nome");

    public Task<List<Opzione>> GetIstruttoriAsync() => ListaAsync(
        "SELECT id_istruttore AS Id, CONCAT(cognome, ' ', nome) AS Testo FROM istruttore ORDER BY cognome, nome");

    public Task<List<Opzione>> GetLezioniAsync() => ListaAsync(@"
        SELECT l.id_lezione AS Id,
               CONCAT(c.nome, ' - ',
                      ELT(l.giorno_settimana, 'Lun', 'Mar', 'Mer', 'Gio', 'Ven', 'Sab', 'Dom'), ' ',
                      TIME_FORMAT(l.ora_inizio, '%H:%i'), ' - ',
                      s.nome, ' (', i.cognome, ')') AS Testo
        FROM lezione l
        JOIN corso c       ON c.id_corso = l.id_corso
        JOIN sala s        ON s.id_sala = l.id_sala
        JOIN istruttore i  ON i.id_istruttore = l.id_istruttore
        ORDER BY l.giorno_settimana, l.ora_inizio, c.nome");

    private async Task<List<Opzione>> ListaAsync(string sql)
    {
        await using var c = await _db.OpenAsync();
        return (await c.QueryAsync<Opzione>(sql)).ToList();
    }

    // ------------------------------------------------------------------
    // Inserimenti semplici (i vincoli UNIQUE/CHECK/FK sono nel database)
    // ------------------------------------------------------------------
    public async Task<int> CreaSalaAsync(SalaInput m)
    {
        await using var c = await _db.OpenAsync();
        return await c.ExecuteScalarAsync<int>(
            "INSERT INTO sala (nome, capienza) VALUES (@Nome, @Capienza); " + LastId, m);
    }

    public async Task<int> CreaIstruttoreAsync(IstruttoreInput m)
    {
        await using var c = await _db.OpenAsync();
        return await c.ExecuteScalarAsync<int>(
            "INSERT INTO istruttore (nome, cognome) VALUES (@Nome, @Cognome); " + LastId, m);
    }

    public async Task<int> CreaCorsoAsync(CorsoInput m)
    {
        await using var c = await _db.OpenAsync();
        return await c.ExecuteScalarAsync<int>(
            "INSERT INTO corso (nome) VALUES (@Nome); " + LastId, m);
    }

    public async Task<int> CreaIscrittoAsync(IscrittoInput m)
    {
        await using var c = await _db.OpenAsync();
        return await c.ExecuteScalarAsync<int>(
            "INSERT INTO iscritto (nome, cognome, email) VALUES (@Nome, @Cognome, @Email); " + LastId, m);
    }

    // ------------------------------------------------------------------
    // Lezione: oltre a UNIQUE(sala, giorno, ora) impediamo che lo stesso
    // istruttore tenga due lezioni alla stessa ora (ipotesi aggiuntiva).
    // ------------------------------------------------------------------
    public async Task<int> CreaLezioneAsync(LezioneInput m)
    {
        await using var c = await _db.OpenAsync();

        var istruttoreOccupato = await c.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM lezione
            WHERE id_istruttore = @IdIstruttore
              AND giorno_settimana = @GiornoSettimana
              AND ora_inizio = @OraInizio", m);
        if (istruttoreOccupato > 0)
            throw new RegolaDiBusinessException("L'istruttore ha già una lezione in quel giorno e a quell'ora.");

        return await c.ExecuteScalarAsync<int>(@"
            INSERT INTO lezione (id_corso, id_sala, id_istruttore, giorno_settimana, ora_inizio)
            VALUES (@IdCorso, @IdSala, @IdIstruttore, @GiornoSettimana, @OraInizio); " + LastId, m);
    }

    // ------------------------------------------------------------------
    // Abbonamento: data_fine calcolata dal tipo (ultimo giorno incluso) e
    // niente periodi sovrapposti per lo stesso iscritto. Tutto in transazione,
    // con la riga dell'iscritto bloccata per evitare inserimenti concorrenti.
    // ------------------------------------------------------------------
    public static DateTime CalcolaDataFine(string tipo, DateTime inizio) => tipo switch
    {
        "mensile"     => inizio.Date.AddMonths(1).AddDays(-1),
        "trimestrale" => inizio.Date.AddMonths(3).AddDays(-1),
        "annuale"     => inizio.Date.AddYears(1).AddDays(-1),
        _ => throw new RegolaDiBusinessException("Tipo di abbonamento non valido.")
    };

    public async Task<AbbonamentoCreato> CreaAbbonamentoAsync(AbbonamentoInput m)
    {
        var inizio = m.DataInizio!.Value.Date;
        var fine = CalcolaDataFine(m.Tipo!, inizio);

        await using var c = await _db.OpenAsync();
        await using var tx = await c.BeginTransactionAsync();

        var iscritto = await c.ExecuteScalarAsync<int?>(
            "SELECT id_iscritto FROM iscritto WHERE id_iscritto = @IdIscritto FOR UPDATE",
            new { m.IdIscritto }, tx);
        if (iscritto is null)
            throw new RegolaDiBusinessException("L'iscritto selezionato non esiste.");

        var sovrapposti = await c.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM abbonamento
            WHERE id_iscritto = @IdIscritto
              AND data_inizio <= @Fine AND data_fine >= @Inizio",
            new { m.IdIscritto, Inizio = inizio, Fine = fine }, tx);
        if (sovrapposti > 0)
            throw new RegolaDiBusinessException("L'iscritto ha già un abbonamento che si sovrappone a questo periodo.");

        var id = await c.ExecuteScalarAsync<int>(@"
            INSERT INTO abbonamento (id_iscritto, tipo, data_inizio, data_fine)
            VALUES (@IdIscritto, @Tipo, @Inizio, @Fine); " + LastId,
            new { m.IdIscritto, m.Tipo, Inizio = inizio, Fine = fine }, tx);

        await tx.CommitAsync();
        return new AbbonamentoCreato(id, fine);
    }

    // ------------------------------------------------------------------
    // Prenotazione: tutta la logica (abbonamento valido, posti, duplicati,
    // data/giorno coerenti) è nella stored procedure sp_prenota.
    // ------------------------------------------------------------------
    public async Task<EsitoPrenotazione> PrenotaAsync(int idIscritto, int idLezione, DateTime dataLezione)
    {
        var p = new DynamicParameters();
        p.Add("p_id_iscritto", idIscritto, DbType.Int32);
        p.Add("p_id_lezione", idLezione, DbType.Int32);
        p.Add("p_data_lezione", dataLezione.Date, DbType.Date);
        p.Add("p_esito", dbType: DbType.String, direction: ParameterDirection.Output, size: 30);
        p.Add("p_messaggio", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
        p.Add("p_id_prenotazione", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await using var c = await _db.OpenAsync();
        await c.ExecuteAsync("sp_prenota", p, commandType: CommandType.StoredProcedure);

        return new EsitoPrenotazione(
            p.Get<string>("p_esito"),
            p.Get<string>("p_messaggio"),
            p.Get<int?>("p_id_prenotazione"));
    }
}
