using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using apinet.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
namespace apinet.Controllers

{
    [Route("api/[controller]")]
   [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class ComentarioController : ControllerBase
    {
        private readonly DataContextB _context;

        public ComentarioController(DataContextB context)
        {
            _context = context;
        }


// POST: api/Receta/{id}/comentar
[HttpPost("{id}/comentar")]
public async Task<IActionResult> ComentarPosteo(int id, [FromBody] CrearComentarioDto comentarioDto)
{
    var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
    var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuario);

    if (user == null)
    {
        return Unauthorized("Usuario no autorizado.");
    }

    var receta = await _context.Recetas.FindAsync(id);
    if (receta == null)
    {
        return NotFound("Receta no encontrada.");
    }

    var comentario = new Comentario
    {
        UsuarioID = user.UsuarioID,
        RecetaID = id,
        Coment = comentarioDto.Coment,
        FechaHora = DateTime.Now
    };

    try
    {
        _context.Comentarios.Add(comentario);
        await _context.SaveChangesAsync();

        var comentarioRespuestaDto = new ComentarioDto
        {
            ComentarioID = comentario.ComentarioID,
            Comentario = comentario.Coment,
            FechaHora = comentario.FechaHora,
            Usuario = new UsuarioDto
            {
                usuarioID = user.UsuarioID,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Foto = user.Foto
            }
        };

        return Ok(comentarioRespuestaDto);
    }
    catch (Exception ex)
    {
        // Log the exception message for debugging purposes
        Console.WriteLine("Error al guardar el comentario: " + ex.Message);
        return StatusCode(500, "Error interno del servidor.");
    }
}
 [HttpDelete("{comentarioId}")]
    public async Task<IActionResult> DeleteComentario(int comentarioId)
    {
         var usuarioCorreo = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var usuario = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuarioCorreo);

        if (usuario == null)
        {
            return Unauthorized("Usuario no autorizado.");
        }

        var comentario = await _context.Comentarios.FindAsync(comentarioId);
        if (comentario == null)
        {
            return NotFound("Comentario no encontrado.");
        }

        var receta = await _context.Recetas.FindAsync(comentario.RecetaID);
        if (receta == null)
        {
            return NotFound("Receta no encontrada.");
        }

        // Verificar si el usuario es el autor del comentario o el autor de la publicación
    if (comentario.UsuarioID != usuario.UsuarioID && receta.UsuarioID != usuario.UsuarioID)
    {
        return Forbid("No tienes permiso para eliminar este comentario.");
    }


        try
        {
            _context.Comentarios.Remove(comentario);
            await _context.SaveChangesAsync();
            return Ok("Comentario eliminado exitosamente.");
        }
        catch (Exception ex)
        {
            // Log the exception message for debugging purposes
            Console.WriteLine("Error al eliminar el comentario: " + ex.Message);
            return StatusCode(500, "Error interno del servidor.");
        }
    

    }

        // Agregar comentario
        [HttpPost]
        public async Task<ActionResult<Comentario>> AgregarComentario([FromBody] Comentario comentario)
        {
            try
            {
                _context.Comentarios.Add(comentario);
                await _context.SaveChangesAsync();
                return Ok(comentario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Agregar respuesta
        [HttpPost("respuesta")]
        public async Task<ActionResult<Respuesta>> AgregarRespuesta([FromBody] Respuesta respuesta)
        {
            try
            {
                _context.Respuestas.Add(respuesta);
                await _context.SaveChangesAsync();
                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Obtener respuestas por comentario
        [HttpGet("comentario/{comentarioId}/respuestas")]
        public async Task<ActionResult<IEnumerable<Respuesta>>> GetRespuestasPorComentario(int comentarioId)
        {
            try
            {
                var respuestas = await _context.Respuestas
                    .Where(r => r.ComentarioID == comentarioId && !r.Eliminado)
                    .Include(r => r.Usuario)
                    .ToListAsync();

                return Ok(respuestas);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

           [HttpPost("{id}/comentario")]
    public async Task<IActionResult> PostComentario(int id, [FromBody] Comentario comentario)
    {
        var receta = await _context.Recetas.FindAsync(id);
        if (receta == null)
        {
            return NotFound();
        }

        _context.Comentarios.Add(comentario);
        await _context.SaveChangesAsync();

        return Ok();
    }
    }
    
}
