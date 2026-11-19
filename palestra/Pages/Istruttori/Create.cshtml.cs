using Microsoft.AspNetCore.Mvc;
using palestra.Data;
using palestra.Models;

namespace palestra.Pages.Istruttori;

public class CreateModel : PalestraPageModel
{
    private readonly PalestraRepository _repo;

    public CreateModel(PalestraRepository repo) => _repo = repo;

    [BindProperty]
    public IstruttoreInput Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        Input.Nome = Input.Nome.Trim();
        Input.Cognome = Input.Cognome.Trim();

        if (!await TryAsync(() => _repo.CreaIstruttoreAsync(Input)))
            return Page();

        TempData["Ok"] = $"Istruttore {Input.Nome} {Input.Cognome} creato.";
        return RedirectToPage();
    }
}
