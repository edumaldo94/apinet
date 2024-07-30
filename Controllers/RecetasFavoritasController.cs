using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using apinet.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
 using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
[Route("api/[controller]")]
 [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
public class RecetasFavoritasController : ControllerBase
{
    private readonly DataContextB _context;

    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _environment;
    public RecetasFavoritasController(DataContextB context, IConfiguration config, IWebHostEnvironment environment)
    {
        _context = context;
        _config = config;
        _environment = environment;
    }


   [HttpPost("AgregarAFavoritos/{recetaId}")]
    [Authorize]
    public async Task<IActionResult> AgregarAFavoritos(int recetaId)
    {
        try
        {
            // Obtener el usuario autenticado
            var usuarioCorreo = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
            var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuarioCorreo);

            if (user == null)
            {
                return BadRequest("Usuario no encontrado.");
            }

            // Verificar si la receta ya está en los favoritos del usuario
            var existeFavorito = await _context.RecetasFavoritas
                .AnyAsync(f => f.RecetaId == recetaId && f.UsuarioId == user.UsuarioID);

            if (existeFavorito)
            {
                return BadRequest("La receta ya está en los favoritos.");
            }

            // Crear una nueva entrada en RecetasFavoritas
            var nuevaFavorita = new RecetaFavorita
            {
                RecetaId = recetaId,
                UsuarioId = user.UsuarioID
            };

            _context.RecetasFavoritas.Add(nuevaFavorita);
            await _context.SaveChangesAsync();

            return Ok("Receta agregada a favoritos.");
        }
        catch (Exception ex)
        {
            return BadRequest("Error al agregar la receta a favoritos: " + ex.Message);
        }
    }
[HttpDelete("EliminarDeFavoritos/{recetaId}")]
[Authorize]
public async Task<IActionResult> EliminarDeFavoritos(int recetaId)
{
    try
    {
        // Obtener el usuario autenticado
        var usuarioCorreo = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuarioCorreo);

        if (user == null)
        {
            return BadRequest("Usuario no encontrado.");
        }

        // Buscar la receta en los favoritos del usuario
        var favorito = await _context.RecetasFavoritas
            .SingleOrDefaultAsync(f => f.RecetaId == recetaId && f.UsuarioId == user.UsuarioID);

        if (favorito == null)
        {
            return NotFound("La receta no está en los favoritos.");
        }

        // Eliminar la entrada de RecetasFavoritas
        _context.RecetasFavoritas.Remove(favorito);
        await _context.SaveChangesAsync();

        return Ok("Receta eliminada de favoritos.");
    }
    catch (Exception ex)
    {
        return BadRequest("Error al eliminar la receta de favoritos: " + ex.Message);
    }
}

 [HttpGet("ObtenerFavoritas")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<RecetaDto>>> ObtenerRecetasFavoritas()
    {
        try
        {
            // Obtener el correo del usuario autenticado
            var usuarioCorreo = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
            var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuarioCorreo);

            if (user == null)
            {
                return BadRequest("Usuario no encontrado.");
            }

            // Obtener la lista de recetas favoritas del usuario
            var recetasFavoritasIds = await _context.RecetasFavoritas
                .Where(f => f.UsuarioId == user.UsuarioID)
                .Select(f => f.RecetaId)
                .ToListAsync();

            // Obtener las recetas que están en la lista de favoritos
            var recetas = await _context.Recetas
                .Include(r => r.Usuario)
                .Include(r => r.Comentarios)
                    .ThenInclude(c => c.Usuario)
                .Where(r => recetasFavoritasIds.Contains(r.RecetaID) && !r.IsDeleted)
                .OrderByDescending(r => r.FechaPublicacion)
                .ToListAsync();

          /*  var recetaDtos = recetas.Select(r => new RecetaDto
            {
                RecetaID = r.RecetaID,
                Usuario = new UsuarioDto
                {
                    usuarioID = r.Usuario.UsuarioID,
                    Nombre = r.Usuario.Nombre ?? string.Empty,
                    Apellido = r.Usuario.Apellido ?? string.Empty,
                    Foto = r.Usuario.Foto ?? string.Empty
                },
                
                FotoPortada = r.FotoPortada ?? string.Empty,
                Descripcion = r.Descripcion ?? string.Empty,
                Comentarios = r.Comentarios.Select(c => new ComentarioDto
                {
                    recetaID = c.RecetaID,
                    ComentarioID = c.ComentarioID,
                    Comentario = c.Coment ?? string.Empty,
                    Usuario = new UsuarioDto
                    {
                        usuarioID = c.Usuario.UsuarioID,
                        Nombre = c.Usuario.Nombre ?? string.Empty,
                        Apellido = c.Usuario.Apellido ?? string.Empty,
                        Foto = c.Usuario.Foto ?? string.Empty
                    },
                    FechaHora = c.FechaHora
                }).ToList(),
                Likes = new LikeDto
                {
                    cantidad = _context.Likes.Count(l => l.RecetaID == r.RecetaID)
                },
                CantidadComentarios = r.Comentarios.Count
            }).ToList();*/
   var recetaDtos = recetas.Select(r => new Receta
        {
            RecetaID = r.RecetaID,
            Usuario = new Usuario
            {
               UsuarioID = r.Usuario.UsuarioID,
                Nombre = r.Usuario.Nombre,
                Apellido = r.Usuario.Apellido,
                Foto = r.Usuario.Foto
            },
            Titulo = r.Titulo,
            Descripcion = r.Descripcion,
            Ingredientes = r.Ingredientes,
            Pasos = r.Pasos,
            Porciones = r.Porciones,
            TiempoPreparacion = r.TiempoPreparacion,
            Dificultad = r.Dificultad,
            TipoCocina = r.TipoCocina,
            FotoPortada = r.FotoPortada
        }).ToList();
            return Ok(new { recetas = recetaDtos });
        }
        catch (Exception ex)
        {
            return BadRequest("Error al obtener las recetas favoritas: " + ex.Message);
        }
    }


}