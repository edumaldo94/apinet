using apinet.Models;
 using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using MailKit.Net.Smtp;
using MimeKit;

namespace apinet.Controllers //<-- No hay punto y coma aquí
{
   
    [Route("api/[controller]")]
     [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class InmuebleController : ControllerBase
    {
        private readonly DataContext applicationDbContext;
        private readonly IConfiguration config;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IWebHostEnvironment environment;

        public InmuebleController(DataContext applicationDbContext, IConfiguration config, IHttpContextAccessor contextAccessor, IWebHostEnvironment environment)
        {
            this.applicationDbContext = applicationDbContext;
            this.config= config;
            this.contextAccessor = contextAccessor;
            this.environment = environment;
        }

        [HttpGet("obtener")]
        public async Task<ActionResult<List<Inmueble>>> Get()
        {

            try
            {

                var email = HttpContext.User.FindFirst(ClaimTypes.Name).Value;

                var propietario = await applicationDbContext.Propietarios.FirstOrDefaultAsync(x => x.Email == email);

                var inmuebles = await applicationDbContext.Inmuebles.Where(x => x.PropietarioId == propietario.id_Propietario).ToListAsync();


                return inmuebles;


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());

            }




        }

        [HttpGet("contrato")]
        public async Task<ActionResult<List<Inmueble>>> GetAlquilados()
        {

            try
            {

                var email = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
                var propietario = await applicationDbContext.Propietarios.FirstOrDefaultAsync(x => x.Email == email);
                var inmuebles = await applicationDbContext.Inmuebles.Join(
                    applicationDbContext.Contratos.Where(x => x.Fecha_Fin > DateTime.Now && x.Fecha_Inicio < DateTime.Now),
                    inm => inm.id_Inmuebles,
                    com => com.InmuebleId,
                    (inm, com) => inm)
                    .Where(x => x.PropietarioId == propietario.id_Propietario).ToListAsync();

                return inmuebles;


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());

            }
        }

        [HttpPut("disponible/{id}")]//Cambiar Estado
        public async Task<ActionResult<Inmueble>> Put([FromForm] byte estado, int id)
        {
            var e =false;
            if(estado == 1)
            {
                e = true;
            }
            else
            {
                e = false;
            }

            try
            {

                var email = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
                var propietario = await applicationDbContext.Propietarios.FirstOrDefaultAsync(x => x.Email == email);
                Inmueble inmuebleV = await applicationDbContext.Inmuebles.FirstOrDefaultAsync(x => x.id_Inmuebles == id && x.PropietarioId == propietario.id_Propietario);
                if (inmuebleV == null)
                {
                    return NotFound();
                }

                inmuebleV.Disponible = e;
                applicationDbContext.Update(inmuebleV);
                await applicationDbContext.SaveChangesAsync();
                return inmuebleV;

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    
   [HttpPut("inmuEstado/{id}")]
    [Authorize]
    public async Task<IActionResult> inmuEstado(int id)
    {
        try
        {
        var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
           
     var user = await applicationDbContext.Propietarios.SingleOrDefaultAsync(x => x.Email == usuario);

             var inmueble = await applicationDbContext.Inmuebles
            .SingleOrDefaultAsync(e => e.id_Inmuebles == id);
            if (inmueble == null) return NotFound();
            if (inmueble.PropietarioId != user.id_Propietario) return Unauthorized("Acceso denegado");
            inmueble.Disponible =  !inmueble.Disponible ;
     applicationDbContext.Update(inmueble);

// Guardar los cambios en la base de datos
await applicationDbContext.SaveChangesAsync();

            return Ok(inmueble);
        }
        catch (Exception e)
        {
            // Manejo de errores
            return BadRequest(e.Message);
        }
    }

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [HttpGet("{id}")]
   
    public async Task<IActionResult> GetInmueble(int id)
    {
        try
        {
           var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
                var user = await applicationDbContext.Propietarios.SingleOrDefaultAsync(x => x.Email == usuario);

  var inmueble = await applicationDbContext.Inmuebles
            .SingleOrDefaultAsync(e => e.id_Inmuebles == id);

          if (inmueble != null)
        {
            if (inmueble.PropietarioId != user.id_Propietario)
            {
                return Unauthorized("Acceso denegado");
            }
            else
            {
                // Si coincide, devuelve el inmueble
                return Ok(inmueble);
            }
        }
        else
        {
            return NotFound("Inmueble no encontrado");
        }
    }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("alquiladas")]
    [Authorize]
    public async Task<IActionResult> GetAlquiladas()
    {
        try
        {
         var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
            
            var user = await applicationDbContext.Propietarios.SingleOrDefaultAsync(x => x.Email == usuario);
            var fecha = DateTime.Today;

        
        var inmuebles = await applicationDbContext.Contratos
            .Where(e => e.Inmueble.PropietarioId == user.id_Propietario && 
                        e.Estado == "Activo" && 
                        e.Fecha_Fin >= fecha)
            .Select(e => e.Inmueble)
            .ToListAsync();

            Console.WriteLine("COUNT: " + inmuebles.Count);
            return Ok(inmuebles);

        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    //==========================================
 

[HttpPost("crear")]
[Authorize]
public async Task<IActionResult> crearInmueble([FromBody] Inmueble inmueble)
{
    try
    {
        var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await applicationDbContext.Propietarios.SingleOrDefaultAsync(x => x.Email == usuario);

        inmueble.PropietarioId = user.id_Propietario;

        // Verifica que estadoIn tenga un valor
        if (inmueble.EstadoIn == 0)
        {
            inmueble.EstadoIn = 1;
        }

        // Decodifica la foto Base64 si está presente
        if (!string.IsNullOrEmpty(inmueble.Foto) && inmueble.Foto.Contains(","))
        {
            inmueble.Foto = inmueble.Foto.Split(',')[1];
        }

        if (!string.IsNullOrEmpty(inmueble.Foto) && (inmueble.Foto.Length % 4 == 0) && Regex.IsMatch(inmueble.Foto, @"^[a-zA-Z0-9\+/]*={0,3}$"))
        {
            byte[] imageBytes = Convert.FromBase64String(inmueble.Foto);

            string wwwPath = environment.WebRootPath;
            string path = Path.Combine(wwwPath, "Uploads", "inmuebles");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Asegúrate de que el nombre de la imagen sea único
            string nombreFoto = $"img_inmueble_{user.id_Propietario}_{Guid.NewGuid()}.jpg";
            string pathCompleto = Path.Combine(path, nombreFoto);

            using (MemoryStream stream = new MemoryStream(imageBytes))
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                image.Save(pathCompleto, System.Drawing.Imaging.ImageFormat.Jpeg);
            }

            inmueble.Foto = $"uploads/inmuebles/{nombreFoto}";
        }

        applicationDbContext.Inmuebles.Add(inmueble);
        await applicationDbContext.SaveChangesAsync();

        return Ok(inmueble);
    }
    catch (Exception ex)
    {
        return BadRequest("Error al convertir la cadena base64 a imagen: " + ex.Message);
    }
}


 [HttpGet("propiedadesUsuario")]
[Authorize]
public async Task<IActionResult> GetInmu()
{
    try
    {
        var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await applicationDbContext.Propietarios.SingleOrDefaultAsync(x => x.Email == usuario);
        if (string.IsNullOrEmpty(usuario))
        {
            return Unauthorized("Token no válido o usuario no identificado");
        }

        var propiedades = from inmueble in applicationDbContext.Inmuebles
                          join propietario in applicationDbContext.Propietarios
                          on inmueble.PropietarioId equals propietario.id_Propietario
                          where propietario.Email == user.Email
                          select inmueble;

        var propiedadesList = await propiedades.ToListAsync();

        if (propiedadesList.Any())
        {
            return Ok(propiedadesList);
        }
        else
        {
            return NotFound("No se encontraron propiedades para el usuario.");
        }
    }
    catch (Exception e)
    {
        return BadRequest("Error en la solicitud: " + e.Message);
    }
}



    //==========================================

    
 [HttpGet("qweqwe")]
[Authorize]
 public async Task<ActionResult<object>> ObtenerUsuarioB()
{
    try
    {
     var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await applicationDbContext.Propietarios.SingleOrDefaultAsync(x => x.Email == usuario);

        // Selecciona los campos necesarios del propietario y devuelve ese objeto proyectado
         var inmueble = await applicationDbContext.Inmuebles
            .Where(x => x.PropietarioId == user.id_Propietario)
            .Select(x => new
            {
                x.id_Inmuebles,
                x.PropietarioId,
                x.Latitud,
                x.Longitud,
                x.Ubicacion,
                x.Direccion,
                x.Ambientes,
                x.Uso,
                x.Tipo,
                x.Precio,
x.Disponible,
x.EstadoIn,
x.Foto
            })
            .ToListAsync();

        
        return inmueble;
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}

    
    }
}
