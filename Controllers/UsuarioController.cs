
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using apinet.Models;
 using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using MailKit.Net.Smtp;
using MimeKit;
using System.Drawing;
using System.Drawing.Imaging;
namespace apinet.Controllers
{

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
public class UsuarioController : ControllerBase
{
    private readonly DataContextB _context;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _environment;

    public UsuarioController(DataContextB context, IConfiguration config, IWebHostEnvironment environment)
    {
        _context = context;
        _config = config;
        _environment = environment;
    }

    // POST api/<controller>/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromForm] LoginView loginView)
    {
        try
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: loginView.Clave,
                salt: System.Text.Encoding.ASCII.GetBytes(_config["Salt"]),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8));

            var user = await _context.Usuarios.FirstOrDefaultAsync(x => x.Correo == loginView.Usuario);
            if (user == null || user.Clave != hashed)
            {
                return BadRequest("Nombre de usuario o clave incorrecta");
            }
            else
            {
                var key = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(_config["TokenAuthentication:SecretKey"]));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Correo),
                    new Claim("FullName", user.Nombre + " " + user.Apellido),
                    new Claim(ClaimTypes.Role, "Usuario")
                };

                var token = new JwtSecurityToken(
                    issuer: _config["TokenAuthentication:Issuer"],
                    audience: _config["TokenAuthentication:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(60),
                    signingCredentials: credentials);

                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("obtenerusuario")]
    [Authorize]
    public async Task<ActionResult<object>> ObtenerUsuario()
    {
        try
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.Name).Value;

            var usuario = await _context.Usuarios
                .Where(x => x.Correo == email)
                .Select(x => new
                {
                    x.UsuarioID,
                    x.Nombre,
                    x.Apellido,
                    x.Correo,
                    x.Foto
                })
                .FirstOrDefaultAsync();

            return usuario;
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
   [HttpPost("crear")]
        [AllowAnonymous]
        public async Task<IActionResult> CrearUsuario([FromBody] UsuarioDto usuarioDto)
        {
            try
            {
                // Verifica si el usuario ya existe
                var existingUser = await _context.Usuarios.FirstOrDefaultAsync(x => x.Correo == usuarioDto.Correo);
                if (existingUser != null)
                {
                    return BadRequest("El correo ya está en uso.");
                }

                // Hashea la contraseña
                string hashedClave = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: usuarioDto.Clave,
                    salt: System.Text.Encoding.ASCII.GetBytes(_config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8));

                // Crea la entidad Usuario
                var usuario = new Usuario
                {
                    Nombre = usuarioDto.Nombre,
                    Apellido = usuarioDto.Apellido,
                    Correo = usuarioDto.Correo,
                    Clave = hashedClave,
                    Likes = new List<Like>(),
                    Recetas = new List<Receta>(),
                    Comentarios = new List<Comentario>(),
                    RecetasFavoritas = new List<RecetaFavorita>()
                };

                // Asignar foto por defecto si no se proporciona una foto
                if (string.IsNullOrEmpty(usuarioDto.Foto))
                {
                    usuario.Foto = "uploads/usuarios/avatar_55.jpg"; // Ruta relativa a la foto por defecto
                }
                else
                {
                    // Decodifica la foto Base64 si está presente
                    if (usuarioDto.Foto.Contains(","))
                    {
                        usuarioDto.Foto = usuarioDto.Foto.Split(',')[1];
                    }

                    if ((usuarioDto.Foto.Length % 4 == 0) && Regex.IsMatch(usuarioDto.Foto, @"^[a-zA-Z0-9\+/]*={0,3}$"))
                    {
                        byte[] imageBytes = Convert.FromBase64String(usuarioDto.Foto);

                        string wwwPath = _environment.WebRootPath;
                        string path = Path.Combine(wwwPath, "Uploads", "usuarios");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        // Ensure the image name is unique
                        string nombreFoto = $"img_usuario_{Guid.NewGuid()}.jpg";
                        
                        string pathCompleto = Path.Combine(path, nombreFoto);

                        using (MemoryStream stream = new MemoryStream(imageBytes))
                        {
                            System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                            image.Save(pathCompleto, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }

                        usuario.Foto = $"uploads/usuarios/{nombreFoto}";
                    }
                }

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest("Error al crear el usuario: " + ex.Message);
            }
        }
    
    

 [HttpPut("editar")]
    [Authorize]
    public async Task<IActionResult> Editar([FromBody] UsuarioPerfilDto usuarioDto)
    {
        try
        {
            var userEmail = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
            var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == userEmail);

            if (user == null) return Unauthorized("Token incorrecto");

            int cantidad = _context.Usuarios.Count(u => u.Correo == usuarioDto.Correo && u.Correo != user.Correo);
            if (cantidad > 0)
            {
                return BadRequest("El correo electrónico ya está en uso por otro usuario.");
            }

            user.Nombre = usuarioDto.Nombre ?? user.Nombre;
            user.Apellido = usuarioDto.Apellido ?? user.Apellido;
            user.Correo = usuarioDto.Correo ?? user.Correo;

            _context.Usuarios.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new 
            {
                user.UsuarioID,
                user.Nombre,
                user.Apellido,
                user.Correo,
           
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
[HttpPut("actualizarFoto")]
[Authorize]
public async Task<IActionResult> ActualizarFoto([FromBody] ActualizarFotoDto actualizarFotoDto)
{
    try
    {
        var userEmail = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == userEmail);

        if (user == null) return Unauthorized("Token incorrecto");

        if (!string.IsNullOrEmpty(actualizarFotoDto.Foto) && actualizarFotoDto.Foto.Contains(","))
        {
            actualizarFotoDto.Foto = actualizarFotoDto.Foto.Split(',')[1];
        }

        if (!string.IsNullOrEmpty(actualizarFotoDto.Foto) && IsBase64String(actualizarFotoDto.Foto))
        {
            byte[] imageBytes = Convert.FromBase64String(actualizarFotoDto.Foto);

            using (MemoryStream stream = new MemoryStream(imageBytes))
            {
                using (Image image = Image.FromStream(stream))
                {
                    string wwwPath = _environment.WebRootPath;
                    string path = Path.Combine(wwwPath, "Uploads", "usuarioFotos");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    string nombreFoto = $"img_usuario_{user.UsuarioID}_{Guid.NewGuid()}.png";
                    string pathCompleto = Path.Combine(path, nombreFoto);

                    image.Save(pathCompleto, ImageFormat.Png); // Guardar como .png

                    user.Foto = $"uploads/usuarioFotos/{nombreFoto}";
                }
            }
        }
        else
        {
            return BadRequest("La foto proporcionada no es válida.");
        }

        _context.Usuarios.Update(user);
        await _context.SaveChangesAsync();

        return Ok(new { user.UsuarioID, user.Foto });
    }
    catch (Exception e)
    {
        return BadRequest(e.Message);
    }
}

private bool IsBase64String(string base64)
{
    Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
    return Convert.TryFromBase64String(base64, buffer, out _);
}

    [HttpPut("editClave")]
    [Authorize]
    public async Task<IActionResult> clave([FromBody] CambiarClaveModel model)
    {
        try
        {
            var userEmail = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if (userEmail == null)
            {
                return Unauthorized("Token incorrecto");
            }

            var dbUser = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == userEmail);
            if (dbUser == null)
            {
                return BadRequest("No se encontró el usuario");
            }

            var claveAntiguaHashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: model.ClaveAntigua,
                salt: System.Text.Encoding.ASCII.GetBytes(_config["Salt"]),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8));

            if (dbUser.Clave != claveAntiguaHashed)
            {
                return BadRequest("La contraseña antigua es incorrecta.");
            }

            if (model.ClaveNueva != model.ConfirmarClaveNueva)
            {
                return BadRequest("La nueva contraseña y la confirmación no coinciden.");
            }

            var nuevaClaveHashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: model.ClaveNueva,
                salt: System.Text.Encoding.ASCII.GetBytes(_config["Salt"]),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8));

            dbUser.Clave = nuevaClaveHashed;
            _context.Usuarios.Update(dbUser);
            await _context.SaveChangesAsync();

            var key = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(_config["TokenAuthentication:SecretKey"]));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, dbUser.Correo),
                new Claim("FullName", dbUser.Nombre + " " + dbUser.Apellido),
            };

            var token = new JwtSecurityToken(
                issuer: _config["TokenAuthentication:Issuer"],
                audience: _config["TokenAuthentication:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credenciales
            );

            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<ActionResult<Usuario>> GetUser() 
    {
        try
        {
            var userEmail = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
            if (userEmail == null) return Unauthorized("Token no válido");
            var dbUser = await _context.Usuarios.SingleOrDefaultAsync(x => x.Correo == userEmail);
            if (dbUser == null) return BadRequest("El usuario no existe");
            return dbUser;
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("email")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByEmail([FromForm] string correo)
    {
        try
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.Correo == correo);
            var link = "";
            string localIPv4 = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?.ToString();
            var dominio = _environment.IsDevelopment() ? localIPv4 : "www.misitio.com";

            if (usuario != null)
            {
                var key = new SymmetricSecurityKey(
                    System.Text.Encoding.ASCII.GetBytes(
                        _config["TokenAuthentication:SecretKey"]
                    )
                );
                var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Correo),
                    new Claim("FullName", usuario.Nombre + " " + usuario.Apellido),
                };

                var token = new JwtSecurityToken(
                    issuer: _config["TokenAuthentication:Issuer"],
                    audience: _config["TokenAuthentication:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(24),
                    signingCredentials: credenciales
                );

                link = $"http://{dominio}:5000/api/Usuario/token?access_token={new JwtSecurityTokenHandler().WriteToken(token)}";

                string subject = "Pedido de Recuperacion de Contraseña";
                string body = @$"<html>
                    <body>
                        <h1>Recuperación de Contraseña</h1>
                        <p>Estimado {usuario.Nombre},</p>
                        <p>Hemos recibido una solicitud para restablecer tu contraseña.</p>
                        <p>Por favor, haz clic en el siguiente enlace para crear una nueva contraseña:</p>
                        <p><a href='{link}'>Restablecer Contraseña</a></p>
                        <p>Si no solicitaste el restablecimiento de contraseña, puedes ignorar este correo electrónico.</p>
                        <p>Este enlace expirará en 24 horas por motivos de seguridad.</p>
                        <p>Atentamente,</p>
                        <p>Tu equipo de soporte</p>
                    </body>
                </html>";

                await enviarMail(correo, subject, body);

                return Ok(usuario);
            }
            else
            {
                return BadRequest("Nombre de usuario o clave incorrectos");
            }

        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("token")]
    [Authorize]
    public async Task<IActionResult> Token()
    {
        try
        {
            var perfil = new
            {
                Correo = User.Identity?.Name,
                Nombre = User.Claims.First(x => x.Type == "FullName").Value,
            };
            Random rand = new Random(Environment.TickCount);
            string randomChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
            string nuevaClave = "";
            for (int i = 0; i < 8; i++)
            {
                nuevaClave += randomChars[rand.Next(0, randomChars.Length)];
            }

            string subject = "Nueva Clave de Ingreso";
            string body = @$"<html>
                <body>
                    <h1>Recuperación de Contraseña</h1>
                    <p>Estimado {perfil.Nombre},</p>
                    <p>Hemos generado una nueva contraseña para tu cuenta.</p>
                    <p>Tu nueva contraseña es: <strong>{nuevaClave}</strong></p>
                    <p>Por favor, inicia sesión con esta nueva contraseña y cámbiala lo antes posible.</p>
                    <p>Si no solicitaste un cambio de contraseña, por favor contáctanos de inmediato.</p>
                    <p>Atentamente,</p>
                    <p>Tu equipo de soporte</p>
                </body>
            </html>";
            await enviarMail(perfil.Correo, subject, body);

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.Correo == perfil.Correo);

            if (usuario != null)
            {
                usuario.Clave = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: nuevaClave,
                    salt: System.Text.Encoding.ASCII.GetBytes(_config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8));
                _context.Update(usuario);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("sendEmail")]
    private async Task<IActionResult> enviarMail(string email, string subject, string body)
    {
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress("Sistema", _config["SMTPUser"]));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("html") { Text = body, };

        using (var client = new SmtpClient())
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.Auto);
            await client.AuthenticateAsync(_config["SMTPUser"], _config["SMTPPass"]);
            await client.SendAsync(emailMessage);

            await client.DisconnectAsync(true);
        }
        return Ok();
    }
}
}