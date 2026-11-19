using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using palestra.Data;

namespace palestra.Pages;

public abstract class PalestraPageModel : PageModel
{
    /// <summary>
    /// Esegue l'azione e trasforma gli errori (regole applicative o del database)
    /// in un messaggio nel riepilogo errori del form. Restituisce true se è andata a buon fine.
    /// </summary>
    protected async Task<bool> TryAsync(Func<Task> azione, params (string Chiave, string Messaggio)[] duplicati)
    {
        try
        {
            await azione();
            return true;
        }
        catch (RegolaDiBusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (MySqlException ex)
        {
            var log = HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
            log.LogError(ex, "Errore database {Numero}", ex.Number);
            ModelState.AddModelError(string.Empty, DbErrors.Translate(ex, duplicati));
        }
        return false;
    }
}
