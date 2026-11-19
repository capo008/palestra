using System.ComponentModel.DataAnnotations;

namespace palestra.Models;

// Le proprietà "int?" / "DateTime?" servono perché le select/date vuote
// producano un messaggio [Required] chiaro invece di un errore di binding.

public class Opzione
{
    public int Id { get; set; }
    public string Testo { get; set; } = "";
}

public record EsitoPrenotazione(string Esito, string Messaggio, int? IdPrenotazione)
{
    public bool Ok => Esito == "OK";
}

public record AbbonamentoCreato(int Id, DateTime DataFine);

public class SalaInput
{
    [Display(Name = "Nome sala")]
    [Required(ErrorMessage = "Il nome è obbligatorio"), StringLength(50, ErrorMessage = "Massimo 50 caratteri")]
    public string Nome { get; set; } = "";

    [Display(Name = "Capienza (posti)")]
    [Required(ErrorMessage = "La capienza è obbligatoria"), Range(1, 1000, ErrorMessage = "La capienza deve essere tra 1 e 1000")]
    public int? Capienza { get; set; }
}

public class IstruttoreInput
{
    [Required(ErrorMessage = "Il nome è obbligatorio"), StringLength(50, ErrorMessage = "Massimo 50 caratteri")]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "Il cognome è obbligatorio"), StringLength(50, ErrorMessage = "Massimo 50 caratteri")]
    public string Cognome { get; set; } = "";
}

public class CorsoInput
{
    [Display(Name = "Nome corso")]
    [Required(ErrorMessage = "Il nome è obbligatorio"), StringLength(50, ErrorMessage = "Massimo 50 caratteri")]
    public string Nome { get; set; } = "";
}

public class IscrittoInput
{
    [Required(ErrorMessage = "Il nome è obbligatorio"), StringLength(50, ErrorMessage = "Massimo 50 caratteri")]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "Il cognome è obbligatorio"), StringLength(50, ErrorMessage = "Massimo 50 caratteri")]
    public string Cognome { get; set; } = "";

    [Required(ErrorMessage = "L'email è obbligatoria"),
     EmailAddress(ErrorMessage = "Email non valida"),
     StringLength(100, ErrorMessage = "Massimo 100 caratteri")]
    public string Email { get; set; } = "";
}

public class AbbonamentoInput
{
    [Display(Name = "Iscritto")]
    [Required(ErrorMessage = "Seleziona un iscritto")]
    public int? IdIscritto { get; set; }

    [Required(ErrorMessage = "Seleziona il tipo di abbonamento")]
    [RegularExpression("^(mensile|trimestrale|annuale)$", ErrorMessage = "Tipo non valido")]
    public string? Tipo { get; set; }

    [Display(Name = "Data di inizio")]
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "La data di inizio è obbligatoria")]
    public DateTime? DataInizio { get; set; }
}

public class LezioneInput
{
    [Display(Name = "Corso")]
    [Required(ErrorMessage = "Seleziona un corso")]
    public int? IdCorso { get; set; }

    [Display(Name = "Sala")]
    [Required(ErrorMessage = "Seleziona una sala")]
    public int? IdSala { get; set; }

    [Display(Name = "Istruttore")]
    [Required(ErrorMessage = "Seleziona un istruttore")]
    public int? IdIstruttore { get; set; }

    [Display(Name = "Giorno della settimana")]
    [Required(ErrorMessage = "Seleziona il giorno")]
    [Range(1, 7, ErrorMessage = "Giorno non valido")]
    public int? GiornoSettimana { get; set; }

    [Display(Name = "Ora di inizio")]
    [Required(ErrorMessage = "L'ora di inizio è obbligatoria")]
    public TimeSpan? OraInizio { get; set; }
}

public class PrenotazioneInput
{
    [Display(Name = "Iscritto")]
    [Required(ErrorMessage = "Seleziona un iscritto")]
    public int? IdIscritto { get; set; }

    [Display(Name = "Lezione")]
    [Required(ErrorMessage = "Seleziona una lezione")]
    public int? IdLezione { get; set; }

    [Display(Name = "Data della lezione")]
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "La data è obbligatoria")]
    public DateTime? DataLezione { get; set; }
}
