using System.Diagnostics;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using apinet.Models;

namespace apinet.Controllers;


    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class InquilinoController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;

        public InquilinoController(DataContext applicationDbContext, IConfiguration config)
        {
            this.contexto = applicationDbContext;
            this.config = config;
        }
    [HttpGet("{id}")]
public async Task<ActionResult<Inquilino>> Get(int id)
{
    try
    {
        var email = HttpContext.User.FindFirst(ClaimTypes.Name).Value;

        if (id == 0)
        {
            return BadRequest();
        }

        var contratoActivo = contexto.Inquilinos
            .Join(
                contexto.Contratos,
                inq => inq.id_Inquilino,
                com => com.InquilinoId,
                (inq, com) => new { Inquilino = inq, Contrato = com }
            )
            .Where(joinResult => joinResult.Contrato.InmuebleId == id && joinResult.Contrato.Estado == "Activo")
            .Select(joinResult => new {
                Inquilino = joinResult.Inquilino,
                Contrato = joinResult.Contrato
            })
            .FirstOrDefault();

        if (contratoActivo != null)
        {
            // Si se encontró un contrato activo, devolver el inquilino asociado
            return Ok(contratoActivo.Inquilino);
        }
        else
        {
            // Si no se encontró ningún contrato activo, devolver un resultado vacío
             return NotFound("No hay Contrato activo");
        }
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message.ToString());
    }
}

[HttpGet("inquiloActual/{id}")]
[Authorize]
public async Task<IActionResult> obtenerInquiloAct(int id)
{
    try
    {
        var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
       
        var user = await contexto.Propietarios.SingleOrDefaultAsync(x => x.Email == usuario);
        var fecha = DateTime.Today;

       var contratoActivo = await contexto.Contratos
            .Include(c => c.Inquilino)
            .Include(c => c.Inmueble)
            .FirstOrDefaultAsync(c =>
                c.InmuebleId == id &&
                c.Inmueble.PropietarioId == user.id_Propietario &&
                c.Estado == "Activo" &&
                c.Fecha_Fin >= fecha);


        if (contratoActivo != null)
        {
            var inquilinoActivo = contratoActivo.Inquilino;
            return Ok(inquilinoActivo);
        }
        else
        {
            return NotFound("No hay inquilino activo para este inmueble.");
        }
    }
    catch (Exception e)
    {
        return BadRequest(e.Message);
    }
}
    }
