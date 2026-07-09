using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Music.User.Business.Abstractions;
using Music.User.Shared.Exceptions;
using Music.Catalogue.Shared.Exceptions;

namespace MusicUser.Controllers;

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public class UserController(IBusiness business) : ControllerBase
{
    [HttpPost(Name = "RegistrazioneUser")]
    [AllowAnonymous]
    public async Task<ActionResult> RegisterAsync(string nome, string cognome, DateTime dataNascita, string username, string email, string password,
        CancellationToken cancellationToken)
    {
        try
        {
            await business.RegisterAsync(nome, cognome, dataNascita, username, email, password, cancellationToken);
        }
        catch (DoubleRegisterException ex)
        {
            return Conflict(ex.Message);
        }

        return Ok();
    }

    [HttpPost(Name = "Login")]
    [AllowAnonymous]
    public async Task<ActionResult> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        try
        {
            string? token = await business.LoginAsync(email, password, cancellationToken);
            return Ok(token);
        }
        catch (Music.User.Shared.Exceptions.ModelNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    [HttpGet(Name = "GetCanzoniUtente")]
    [Authorize]
    public async Task<ActionResult> GetCanzoniUtenteAsync(CancellationToken cancellationToken)
    {
        string token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        try
        {
            var canzoni = await business.GetCanzoniUtenteAsync(token, cancellationToken);
            return Ok(canzoni);
        }
        catch (Music.Catalogue.Shared.Exceptions.ModelNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet(Name = "GetCanzoniPopolari")]
    [Authorize]
    public async Task<ActionResult> GetCanzoniPopolariAsync(CancellationToken cancellationToken)
    {
        string token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        try
        {
            var canzoni = await business.GetCanzoniPopolariAsync(token, cancellationToken);
            return Ok(canzoni);
        }
        catch (Music.Catalogue.Shared.Exceptions.ModelNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
