using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using palestra.Data;
using palestra.Models;

namespace palestra.Pages.Abbonamenti;

public class CreateModel : PalestraPageModel
{
    private readonly PalestraRepository _repo;

    public CreateModel(PalestraRepository repo) => _repo = repo;

    [BindProperty]
    public AbbonamentoInput Input { get; set; } = new();

    public SelectList Iscritti { get; private set; } = default!;

    private async Task CaricaAsync() =>
        Iscritti = new SelectList(await _repo.GetIscrittiAsync(), nameof(Opzione.Id), nameof(Opzione.Testo));

    public async Task OnGetAsync()
    {
        Input.DataInizio = DateTime.Today;
        await CaricaAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CaricaAsync();
        if (!ModelState.IsValid) return Page();

        AbbonamentoCreato? creato = null;
        if (!await TryAsync(async () => creato = await _repo.CreaAbbonamentoAsync(Input)))
            return Page();

        TempData["Ok"] = $"Abbonamento {Input.Tipo} creato: valido dal {Input.DataInizio:dd/MM/yyyy} al {creato!.DataFine:dd/MM/yyyy}.";
        return RedirectToPage();
    }
}
