using Microsoft.AspNetCore.Mvc;
using palestra.Data;
using palestra.Models;

namespace palestra.Pages.Corsi;

public class CreateModel : PalestraPageModel
{
    private readonly PalestraRepository _repo;

    public CreateModel(PalestraRepository repo) => _repo = repo;

    [BindProperty]
    public CorsoInput Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        Input.Nome = Input.Nome.Trim();

        if (!await TryAsync(() => _repo.CreaCorsoAsync(Input), ("nome", "Esiste già un corso con questo nome.")))
            return Page();

        TempData["Ok"] = $"Corso «{Input.Nome}» creato.";
        return RedirectToPage();
    }
}
