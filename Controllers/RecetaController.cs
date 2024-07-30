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
public class RecetaController : ControllerBase
{
    private readonly DataContextB _context;

    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _environment;
    public RecetaController(DataContextB context, IConfiguration config, IWebHostEnvironment environment)
    {
        _context = context;
        _config = config;
        _environment = environment;
    }
  // GET: api/Receta/posts
    [HttpGet("posts")]
    public async Task<ActionResult<IEnumerable<Receta>>> GetRecetas()
    {
        var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuario);

        var recetas = await _context.Recetas
            .Include(r => r.Comentarios)
            .Include(r => r.Like)
            .ToListAsync();

        return Ok(recetas);
    }
  // GET: api/Receta/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Receta>> GetReceta(int id)
    {
        var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuario);

        var receta = await _context.Recetas
            .Include(r => r.Comentarios)
            .Include(r => r.Like)
            .FirstOrDefaultAsync(r => r.RecetaID == id);

        if (receta == null)
        {
            return NotFound();
        }

        return Ok(receta);
    }

    // GET: api/Receta/ObtenerPostReceta
    [HttpGet("ObtenerPostReceta")]
    public async Task<ActionResult<IEnumerable<RecetaDto>>> obtn()
    {
        var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuario);

        var recetas = await _context.Recetas
            .Include(r => r.Comentarios)
            .ThenInclude(c => c.Usuario)
            .Include(r => r.Like)
            .Include(r => r.Usuario)
            .ToListAsync();

        var recetaDtos = recetas.Select(r => new RecetaDto
        {
            Usuario = new UsuarioDto
            {
                usuarioID = r.Usuario.UsuarioID,
                Nombre = r.Usuario.Nombre,
                Apellido = r.Usuario.Apellido,
                Foto = r.Usuario.Foto
            },
            FotoPortada = r.FotoPortada,
            Descripcion = r.Descripcion,
            Comentarios = r.Comentarios.Select(c => new ComentarioDto
            {
                Comentario = c.Coment,
                Usuario = new UsuarioDto
                {
                    usuarioID = c.Usuario.UsuarioID,
                    Nombre = c.Usuario.Nombre,
                    Apellido = c.Usuario.Apellido,
                    Foto = c.Usuario.Foto
                },
                FechaHora = c.FechaHora
            }).ToList(),
            Likes = new LikeDto
            {
                cantidad = r.Like.Count()
            }
        });

        return Ok(new { recetas = recetaDtos });
    }

    // GET: api/Receta/nueva
[HttpGet("nueva")]
public async Task<ActionResult<IEnumerable<RecetaDto>>> GetRecetasnueva()
{
    var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
    var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuario);

    if (user == null)
    {
        return BadRequest("Usuario no encontrado.");
    }

    var recetasFavoritasIds = await _context.RecetasFavoritas
        .Where(f => f.UsuarioId == user.UsuarioID)
        .Select(f => f.RecetaId)
        .ToListAsync();

    var recetas = await _context.Recetas
        .Include(r => r.Usuario)
        .Include(r => r.Comentarios)
            .ThenInclude(c => c.Usuario)
        .Where(r => !r.IsDeleted) // Filtrar las recetas eliminadas
        .OrderByDescending(r => r.FechaPublicacion) // Ordenar por fecha de publicación más reciente
        .ToListAsync();

    var recetaDtos = recetas.Select(r => new RecetaDto
    {
        RecetaID = r.RecetaID,
        Usuario = new UsuarioDto
        {
            usuarioID = r.Usuario.UsuarioID,
            Nombre = r.Usuario.Nombre ?? string.Empty, // Maneja valores nulos
            Apellido = r.Usuario.Apellido ?? string.Empty, // Maneja valores nulos
            Foto = r.Usuario.Foto ?? string.Empty // Maneja valores nulos
        },
        FotoPortada = r.FotoPortada ?? string.Empty, // Maneja valores nulos
        Descripcion = r.Descripcion ?? string.Empty, // Maneja valores nulos
        Comentarios = r.Comentarios.Select(c => new ComentarioDto
        {
            recetaID = c.RecetaID,
            ComentarioID = c.ComentarioID,
            Comentario = c.Coment ?? string.Empty, // Maneja valores nulos
            Usuario = new UsuarioDto
            {
                usuarioID = c.Usuario.UsuarioID,
                Nombre = c.Usuario.Nombre ?? string.Empty, // Maneja valores nulos
                Apellido = c.Usuario.Apellido ?? string.Empty, // Maneja valores nulos
                Foto = c.Usuario.Foto ?? string.Empty // Maneja valores nulos
            },
            FechaHora = c.FechaHora
        }).ToList(),
        Likes = new LikeDto
        {
            cantidad = _context.Likes.Count(l => l.RecetaID == r.RecetaID)
        },
        CantidadComentarios = r.Comentarios.Count,
        EsFavorita = recetasFavoritasIds.Contains(r.RecetaID) // Verificar si es favorita
    }).ToList();

    return Ok(new { recetas = recetaDtos });
}


    // POST: api/Receta/{id}/like
  [HttpPost("{id}/like")]
public async Task<IActionResult> DarLike(int id, [FromBody] Like like)
{
    // Obtener el usuario actual desde el contexto HTTP
    var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
    var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuario);

    if (user == null)
    {
        return Unauthorized("Usuario no autorizado.");
    }

    // Establecer el UsuarioID en el like basado en el usuario actual
    like.UsuarioID = user.UsuarioID;

    if (like == null || like.RecetaID != id)
    {
        return BadRequest("Datos del like no válidos.");
    }

    var receta = await _context.Recetas.FindAsync(id);
    if (receta == null)
    {
        return NotFound("Receta no encontrada.");
    }

    try
    {
        _context.Likes.Add(like);
        await _context.SaveChangesAsync();
        return Ok();
    }
    catch (Exception ex)
    {
        // Log the exception message for debugging purposes
        Console.WriteLine("Error al guardar el like: " + ex.Message);
        return StatusCode(500, "Error interno del servidor.");
    }
}

    // DELETE: api/Receta/{id}/deletlike/{usuarioId}
    [HttpDelete("{id}/deletlike")]
    public async Task<IActionResult> QuitarLike(int id)
    {
        var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuario);
    
        // Verificar que el like existe y pertenece al usuario autenticado
        var existingLike = await _context.Likes.FirstOrDefaultAsync(l => l.RecetaID == id && l.UsuarioID == user.UsuarioID);

        if (existingLike == null)
        {
            return NotFound("Like no encontrado para eliminar o no tienes permiso para eliminar este like.");
        }
      existingLike.UsuarioID=user.UsuarioID;
      existingLike.RecetaID=id;
        try
        {
            _context.Likes.Remove(existingLike);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            // Log the exception message for debugging purposes
            Console.WriteLine("Error al eliminar el like: " + ex.Message);
            return StatusCode(500, "Error interno del servidor.");
        }
    }

    // GET: api/Receta/{usuarioId}/obtenerLikes
    [HttpGet("obtenerLikes")]
    [Authorize]
    public ActionResult<List<Like>> ObtenerLikesUsuario()
    {
        var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = _context.Usuarios.SingleOrDefault(x => x.Correo == usuario);

        try
        {
            var likesUsuario = _context.Likes.Where(l => l.UsuarioID == user.UsuarioID).ToList();
            return Ok(likesUsuario);
        }
        catch (Exception ex)
        {
            // Log the exception message for debugging purposes
            Console.WriteLine("Error al obtener los likes del usuario: " + ex.Message);
            return StatusCode(500, "Error interno del servidor.");
        }
    }
      [HttpGet("receta/{recetaId}")]
    public async Task<IActionResult> GetComentariosPorReceta(int recetaId)
    {
        var receta = await _context.Recetas.FindAsync(recetaId);
        if (receta == null)
        {
            return NotFound("Receta no encontrada.");
        }

        var comentarios = await _context.Comentarios
            .Where(c => c.RecetaID == recetaId)
            .Include(c => c.Usuario) // Incluye la información del usuario
            .ToListAsync();

        if (comentarios == null || comentarios.Count == 0)
        {
            return NotFound("No se encontraron comentarios para esta receta.");
        }

        return Ok(comentarios);
    }



[HttpGet("buscarPorTipoCocina/{tipoCocina}")]
public async Task<ActionResult<IEnumerable<RecetaDto>>> BuscarPorTipoCocina(string tipoCocina)
{

var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = _context.Usuarios.SingleOrDefault(x => x.Correo == usuario);

    var recetas = await _context.Recetas
        .Include(r => r.Usuario)
        .Include(r => r.Comentarios)
            .ThenInclude(c => c.Usuario)
          .Where(r => r.TipoCocina == tipoCocina && !r.IsDeleted) 
        .OrderByDescending(r => r.FechaPublicacion)
        .ToListAsync();

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
        Titulo=r.Titulo,
        Descripcion = r.Descripcion,
        Ingredientes=r.Ingredientes,
        Pasos=r.Pasos,
        Porciones=r.Porciones,
        TiempoPreparacion=r.TiempoPreparacion,
        Dificultad=r.Dificultad,
        TipoCocina=r.TipoCocina,
        FotoPortada = r.FotoPortada,

    }).ToList();

    return Ok(new { recetas = recetaDtos });
}

[HttpGet("Obtenerxid/{recetaID}")]
[Authorize]
public async Task<ActionResult<RecetaDto>> ObtenerRecetaPorID(int recetaID)
{
    try
    {
        var usuarioCorreo = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuarioCorreo);

        if (user == null)
        {
            return BadRequest("Usuario no encontrado.");
        }

        var receta = await _context.Recetas
            .Include(r => r.Usuario)
            .SingleOrDefaultAsync(r => r.RecetaID == recetaID);

        if (receta == null)
        {
            return NotFound("Receta no encontrada.");
        }

        var recetaDto = new Receta
        {
            RecetaID = receta.RecetaID,
            Usuario = new Usuario
            {
                UsuarioID = receta.Usuario.UsuarioID,
                Nombre = receta.Usuario.Nombre,
                Apellido = receta.Usuario.Apellido,
                Foto = receta.Usuario.Foto
            },
            Titulo= receta.Titulo,
            Descripcion = receta.Descripcion,
Ingredientes=receta.Ingredientes,
Pasos= receta.Pasos,
Porciones= receta.Porciones,
TiempoPreparacion= receta.TiempoPreparacion,
Dificultad= receta.Dificultad,
TipoCocina= receta.TipoCocina,
FotoPortada= receta.FotoPortada,
        };

        return Ok(recetaDto);
    }
    catch (Exception ex)
    {
        return BadRequest("Error al obtener la receta: " + ex.Message);
    }
}
[HttpPost("crear")]
[Authorize]
public async Task<IActionResult> CrearReceta([FromBody] RecetaSubirDto recetaDto)
{
    try
    {
        var usuarioCorreo = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuarioCorreo);

        if (user == null)
        {
            return BadRequest("Usuario no encontrado.");
        }

        // Mapear RecetaSubirDto a Receta
        var receta = new Receta
        {
            UsuarioID = user.UsuarioID,
            Usuario = user,
            Titulo = recetaDto.Titulo,
            Descripcion = recetaDto.Descripcion,
            Ingredientes = recetaDto.Ingredientes,
            Pasos = recetaDto.Pasos,
            Porciones = recetaDto.Porciones,
            TiempoPreparacion = recetaDto.TiempoPreparacion,
            Dificultad = recetaDto.Dificultad,
            TipoCocina = recetaDto.TipoCocina,
            FotoPortada = recetaDto.FotoPortada,
            FechaPublicacion = DateTime.Now
        };

        // Decodifica la foto Base64 si está presente
        if (!string.IsNullOrEmpty(recetaDto.FotoPortada) && recetaDto.FotoPortada.Contains(","))
        {
            recetaDto.FotoPortada = recetaDto.FotoPortada.Split(',')[1];
        }

        if (!string.IsNullOrEmpty(recetaDto.FotoPortada) && IsBase64String(recetaDto.FotoPortada))
        {
            byte[] imageBytes = Convert.FromBase64String(recetaDto.FotoPortada);

            using (MemoryStream stream = new MemoryStream(imageBytes))
            {
                using (Image image = Image.FromStream(stream))
                {
                    // Guardar la imagen en formato PNG
                    string wwwPath = _environment.WebRootPath;
                    string path = Path.Combine(wwwPath, "Uploads", "recetaimg");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    // Asegúrate de que el nombre de la imagen sea único y con la extensión .png
                    string nombreFoto = $"img_receta_{user.UsuarioID}_{Guid.NewGuid()}.png";
                    string pathCompleto = Path.Combine(path, nombreFoto);

                    image.Save(pathCompleto, ImageFormat.Png); // Guardar como .png

                    receta.FotoPortada = $"uploads/recetaimg/{nombreFoto}";
                }
            }
        }

        // Inicializar las colecciones si no están inicializadas
        receta.Like = new List<Like>();
        receta.Comentarios = new List<Comentario>();
        receta.RecetasFavoritas = new List<RecetaFavorita>();

        _context.Recetas.Add(receta);
        await _context.SaveChangesAsync();

        return Ok(receta);
    }
    catch (Exception ex)
    {
        return BadRequest("Error al crear la receta: " + ex.Message);
    }
}


// Función para verificar si una cadena es Base64 válida
private bool IsBase64String(string base64)
{
    Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
    return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
}


[HttpPut("Actualizar/{recetaID}")]
[Authorize]
public async Task<IActionResult> ActualizarReceta(int recetaID, [FromBody] RecetaSubirDto recetaDto)
{
    try
    {
        var usuarioCorreo = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuarioCorreo);

        if (user == null)
        {
            return BadRequest("Usuario no encontrado.");
        }

        var receta = await _context.Recetas.SingleOrDefaultAsync(r => r.RecetaID == recetaID);

        if (receta == null)
        {
            return NotFound("Receta no encontrada.");
        }

        if (receta.UsuarioID != user.UsuarioID)
        {
            return Unauthorized("No tienes permiso para actualizar esta receta.");
        }

        // Actualizar los campos de la receta
        receta.Titulo = recetaDto.Titulo;
        receta.Descripcion = recetaDto.Descripcion;
        receta.Ingredientes = recetaDto.Ingredientes;
        receta.Pasos = recetaDto.Pasos;
        receta.Porciones = recetaDto.Porciones.ToString();  // Si Porciones es un entero, conviértelo a cadena
        receta.TiempoPreparacion = recetaDto.TiempoPreparacion;
        receta.Dificultad = recetaDto.Dificultad;
        receta.TipoCocina = recetaDto.TipoCocina;
      //    receta.FotoPortada = recetaDto.FotoPortada;
        // Manejar la actualización de la imagen de portada si es necesario
     if (!string.IsNullOrEmpty(recetaDto.FotoPortada) && recetaDto.FotoPortada.Contains(","))
        {
            recetaDto.FotoPortada = recetaDto.FotoPortada.Split(',')[1];
        }

        if (!string.IsNullOrEmpty(recetaDto.FotoPortada) && IsBase64String(recetaDto.FotoPortada))
        {
            byte[] imageBytes = Convert.FromBase64String(recetaDto.FotoPortada);

            using (MemoryStream stream = new MemoryStream(imageBytes))
            {
                using (Image image = Image.FromStream(stream))
                {
                    // Guardar la imagen en formato PNG
                    string wwwPath = _environment.WebRootPath;
                    string path = Path.Combine(wwwPath, "Uploads", "recetaimg");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    // Asegúrate de que el nombre de la imagen sea único y con la extensión .png
                    string nombreFoto = $"img_receta_{user.UsuarioID}_{Guid.NewGuid()}.png";
                    string pathCompleto = Path.Combine(path, nombreFoto);

                    image.Save(pathCompleto, ImageFormat.Png); // Guardar como .png

                    receta.FotoPortada = $"uploads/recetaimg/{nombreFoto}";
                }
            }
        }
        else
        {
            // Mantener la foto actual si no se ha cambiado
            receta.FotoPortada = receta.FotoPortada;
        }


        _context.Recetas.Update(receta);
        await _context.SaveChangesAsync();

        return Ok(receta);
    }
    catch (Exception ex)
    {
        return BadRequest("Error al actualizar la receta: " + ex.Message);
    }
}



[HttpDelete("Eliminar/{recetaID}")]
[Authorize]
public async Task<IActionResult> EliminarReceta(int recetaID)
{
    try
    {
        var usuarioCorreo = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuarioCorreo);

        if (user == null)
        {
            return BadRequest("Usuario no encontrado.");
        }

        var receta = await _context.Recetas.SingleOrDefaultAsync(r => r.RecetaID == recetaID);

        if (receta == null)
        {
            return NotFound("Receta no encontrada.");
        }

        if (receta.UsuarioID != user.UsuarioID)
        {
            return Unauthorized("No tienes permiso para eliminar esta receta.");
        }

        // Marcar la receta como eliminada
        receta.IsDeleted = true;

        _context.Recetas.Update(receta);
        await _context.SaveChangesAsync();

        return Ok("Receta eliminada correctamente.");
    }
    catch (Exception ex)
    {
        return BadRequest("Error al eliminar la receta: " + ex.Message);
    }
}

[HttpGet("buscarRecetas/{query}")]
public async Task<ActionResult<IEnumerable<RecetaDto>>> BuscarRecetas(string query)
{
    var usuarioCorreo = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuarioCorreo);
       var recetas = await _context.Recetas
        .Include(r => r.Usuario)
        .Include(r => r.Comentarios)
            .ThenInclude(c => c.Usuario)
        .Where(r => !r.IsDeleted && 
                    (r.Titulo.Contains(query) || r.Descripcion.Contains(query) || 
                     r.Ingredientes.Contains(query) || r.Dificultad.Contains(query) || 
                     r.TipoCocina.Contains(query) || r.TiempoPreparacion.Contains(query)))
        .OrderByDescending(r => r.FechaPublicacion)
        .ToListAsync();

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
        FotoPortada = r.FotoPortada,
       
    }).ToList();

    return Ok(new { recetas = recetaDtos });
}
[HttpGet("ObtenerRecetasPorUsuario/{usuarioID}")]
[Authorize]
public async Task<ActionResult<IEnumerable<RecetaDto>>> ObtenerRecetasPorUsuario(int usuarioID)
{
    try
    {
        var usuarioCorreo = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == usuarioCorreo);

        if (user == null)
        {
            return BadRequest("Usuario no encontrado.");
        }

        var recetas = await _context.Recetas
            .Include(r => r.Usuario)
            .Where(r => r.Usuario.UsuarioID == usuarioID && !r.IsDeleted)
            .ToListAsync();

        if (recetas == null || recetas.Count == 0)
        {
            return NotFound("No se encontraron recetas para este usuario.");
        }

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

        return Ok(recetaDtos);
    }
    catch (Exception ex)
    {
        return BadRequest("Error al obtener las recetas: " + ex.Message);
    }
}

}