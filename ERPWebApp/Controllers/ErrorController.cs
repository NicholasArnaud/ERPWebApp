using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ERPWebApp.Controllers;

[AutoValidateAntiforgeryToken]
public class ErrorController : Controller
{
    public ErrorController()
    {

    }
    public IActionResult Index(int statusCode, string supportMessage)
    {
        return View(new ErrorResult { StatusCode = statusCode, SupportMessage = supportMessage });
    }

    public IActionResult Error()
    {
        var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionDetails != null)
        {
            ViewBag.ErrorPath = exceptionDetails.Path;
            ViewBag.ErrorMessage = exceptionDetails.Error.Message;
            ViewBag.ErrorSource = exceptionDetails.Error.Source;
            ViewBag.ErrorStackTrace = exceptionDetails.Error.StackTrace;
            //ViewBag.requestID = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View(new ErrorViewModel { RequestId = requestId });
        //return View("Error");
    }
}
