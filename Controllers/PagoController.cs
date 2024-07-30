using System.Diagnostics;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;



using apinet.Models;

namespace apinet.Controllers;
 [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class PagoController : ControllerBase
    { 
    private readonly DataContext applicationDbContext;
    private readonly IConfiguration config;

    public PagoController(DataContext applicationDbContext, IConfiguration config)
    {
        this.applicationDbContext = applicationDbContext;
        this.config = config;
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<List<Pago>>> GetPagoXContrato(int id)
    {

        try
        {
            var pagos = await applicationDbContext.Pagos.Include(x => x.Contrato).Where(x =>
                 x.ContratoId == id
                ).ToListAsync();

            return Ok(pagos);

            }
            catch (Exception ex)
            {
            return BadRequest(ex.Message.ToString());

            }

        }
  [HttpGet("cadapago/{contratoId}")]
[Authorize]
public async Task<IActionResult> GetPagos(int contratoId)
{
    try
    {
        var usuario = User.Identity.Name;
        if (usuario == null) 
            return Unauthorized("Token no válido");

        // Consulta para obtener solo los campos necesarios de Pago
        var pagos = applicationDbContext.Pagos
            .Where(p => p.ContratoId == contratoId)
            .Select(p => new
            {
                p.PagoId,
                p.NumeroPago,
                p.ContratoId,
               // p.Concepto,
                p.FechaPago,
                p.Importe,
             //   p.EstadoPago,
               // p.InquilinoNombre,
              //  p.InquilinoApellido,
               // p.inmueble
                // No incluir p.Contrato u otras propiedades de navegación
            })
            .ToArray();

        return Ok(pagos);
    }
    catch (Exception e)
    {
        return BadRequest(e.Message);
    }
}


    }


