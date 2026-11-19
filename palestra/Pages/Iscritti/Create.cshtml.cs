using Microsoft.AspNetCore.Mvc;
using palestra.Data;
using palestra.Models;

namespace palestra.Pages.Iscritti;

public class CreateModel : PalestraPageModel
{
    private readonly PalestraRepository _repo;

    public CreateModel(PalestraRepository repo) => _repo = repo;

    [BindProperty]
    public IscrittoInput Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        Input.Nome = Input.Nome.Trim();
        Input.Cognome = Input.Cognome.Trim();
        Input.Email = Input.Email.Trim().ToLowerInvariant();

        if (!await TryAsync(() => _repo.CreaIscrittoAsync(Input), ("email", "Esiste già un iscritto con questa email.")))
            return Page();

        TempData["Ok"] = $"Iscritto {Input.Nome} {Input.Cognome} creato.";
        return RedirectToPage();
    }
}
