using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using palestra.Data;
using palestra.Models;

namespace palestra.Pages.Lezioni;

public class CreateModel : PalestraPageModel
{
    private readonly PalestraRepository _repo;

    public CreateModel(PalestraRepository repo) => _repo = repo;

    [BindProperty]
    public LezioneInput Input { get; set; } = new();

    public SelectList Corsi { get; private set; } = default!;
    public SelectList Sale { get; private set; } = default!;
    public SelectList Istruttori { get; private set; } = default!;

    // 1 = lunedì ... 7 = domenica (ISO), come da vincolo CHECK sulla colonna
    public SelectList Giorni { get; } = new(new[]
    {
        new { Id = 1, Testo = "Lunedì" }, new { Id = 2, Testo = "Martedì" }, new { Id = 3, Testo = "Mercoledì" },
        new { Id = 4, Testo = "Giovedì" }, new { Id = 5, Testo = "Venerdì" }, new { Id = 6, Testo = "Sabato" },
        new { Id = 7, Testo = "Domenica" }
    }, "Id", "Testo");

    private async Task CaricaAsync()
    {
        Corsi = new SelectList(await _repo.GetCorsiAsync(), nameof(Opzione.Id), nameof(Opzione.Testo));
        Sale = new SelectList(await _repo.GetSaleAsync(), nameof(Opzione.Id), nameof(Opzione.Testo));
        Istruttori = new SelectList(await _repo.GetIstruttoriAsync(), nameof(Opzione.Id), nameof(Opzione.Testo));
    }

    public async Task OnGetAsync() => await CaricaAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        await CaricaAsync();
        if (!ModelState.IsValid) return Page();

        if (!await TryAsync(() => _repo.CreaLezioneAsync(Input),
                ("id_sala", "In quella sala c'è già una lezione nello stesso giorno e alla stessa ora.")))
            return Page();

        TempData["Ok"] = "Lezione creata.";
        return RedirectToPage();
    }
}
