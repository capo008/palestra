using Microsoft.AspNetCore.Mvc;
using palestra.Data;
using palestra.Models;

namespace palestra.Pages.Sale;

public class CreateModel : PalestraPageModel
{
    private readonly PalestraRepository _repo;

    public CreateModel(PalestraRepository repo) => _repo = repo;

    [BindProperty]
    public SalaInput Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        Input.Nome = Input.Nome.Trim();

        if (!await TryAsync(() => _repo.CreaSalaAsync(Input), ("nome", "Esiste già una sala con questo nome.")))
            return Page();

        TempData["Ok"] = $"Sala «{Input.Nome}» creata.";
        return RedirectToPage();
    }
}
