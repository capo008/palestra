using MySqlConnector;

namespace palestra.Data;

/// <summary>Violazione di una regola applicativa (non del database): il messaggio è mostrabile all'utente.</summary>
public class RegolaDiBusinessException : Exception
{
    public RegolaDiBusinessException(string message) : base(message) { }
}

public static class DbErrors
{
    /// <summary>
    /// Traduce un errore MariaDB/MySQL in un messaggio comprensibile.
    /// <paramref name="duplicati"/>: coppie (frammento del nome dell'indice UNIQUE, messaggio).
    /// </summary>
    public static string Translate(MySqlException ex, params (string Chiave, string Messaggio)[] duplicati)
    {
        switch (ex.Number)
        {
            case 1062: // duplicate entry (UNIQUE / PK)
                foreach (var (chiave, messaggio) in duplicati)
                    if (ex.Message.Contains(chiave, StringComparison.OrdinalIgnoreCase))
                        return messaggio;
                return "Esiste già un record con gli stessi dati.";
            case 1452: // FK: riga padre inesistente
                return "Uno dei riferimenti selezionati non esiste più (iscritto, corso, sala, istruttore o lezione).";
            case 3819: // CHECK violato (MySQL)
            case 4025: // CHECK violato (MariaDB)
                return "I dati inseriti non rispettano un vincolo del database: controlla valori e date.";
            case 1048: // NOT NULL
                return "Manca un campo obbligatorio.";
            case 1264: // valore fuori range
            case 1406: // valore troppo lungo
                return "Uno dei valori inseriti è fuori dai limiti consentiti.";
            default:
                return "Errore del database: l'operazione non è stata eseguita. Riprova o contatta l'amministratore.";
        }
    }
}
