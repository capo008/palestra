using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using palestra.Data;
using palestra.Models;

namespace palestra.Pages.Prenotazioni;

public class CreateModel : PalestraPageModel
{
    private readonly PalestraRepository _repo;

    public CreateModel(PalestraRepository repo) => _repo = repo;

    [BindProperty]
    public PrenotazioneInput Input { get; set; } = new();

    public SelectList Iscritti { get; private set; } = default!;
    public SelectList Lezioni { get; private set; } = default!;

    private async Task CaricaAsync()
    {
        Iscritti = new SelectList(await _repo.GetIscrittiAsync(), nameof(Opzione.Id), nameof(Opzione.Testo));
        Lezioni = new SelectList(await _repo.GetLezioniAsync(), nameof(Opzione.Id), nameof(Opzione.Testo));
    }

    public async Task OnGetAsync()
    {
        Input.DataLezione = DateTime.Today;
        await CaricaAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CaricaAsync();
        if (!ModelState.IsValid) return Page();

        EsitoPrenotazione? esito = null;
        if (!await TryAsync(async () =>
                esito = await _repo.PrenotaAsync(Input.IdIscritto!.Value, Input.IdLezione!.Value, Input.DataLezione!.Value)))
            return Page();

        // La stored procedure non solleva errore per i casi "normali" (abbonamento scaduto,
        // lezione piena...): li comunica con un esito, che mostriamo all'utente.
        if (!esito!.Ok)
        {
            ModelState.AddModelError(string.Empty, esito.Messaggio);
            return Page();
        }

        TempData["Ok"] = esito.Messaggio;
        return RedirectToPage();
    }
}
