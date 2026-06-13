using ERPWebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Controllers;

[AllowAnonymous]
[AutoValidateAntiforgeryToken]
public class FileController : Controller
{
    private readonly ApplicationDbContext _context;
    public FileController(ApplicationDbContext context)
    {
        _context = context;

    }

    public ActionResult Index(int id)
    {
        var fileToRetrieve = _context.Files.Find(id);
        return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
    }
}
