using apinet.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace apinet.Controllers
{
	[Route("api/[controller]")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[ApiController]
	public class PropietarioController : ControllerBase
	{
		private readonly DataContext contexto;
		private readonly IConfiguration config;
 private readonly IWebHostEnvironment environment;
		public PropietarioController(DataContext contexto, IConfiguration config,IWebHostEnvironment env)
		{
			this.contexto = contexto;
			this.config = config;
			  environment = env;
		}
		// GET: api/<ValuesController>
		[HttpGet]
		public async Task<ActionResult<Propietario>> Get()
		{
			try
			{
				/*contexto.Inmuebles
                    .Include(x => x.Duenio)
                    .Where(x => x.Duenio.Nombre == "")//.ToList() => lista de inmuebles
                    .Select(x => x.Duenio)
                    .ToList();//lista de propietarios*/
				var usuario = User.Identity.Name;
				/*contexto.Contratos.Include(x => x.Inquilino).Include(x => x.Inmueble).ThenInclude(x => x.Duenio)
                    .Where(c => c.Inmueble.Duenio.Email....);*/
				/*var res = contexto.Propietarios.Select(x => new { x.Nombre, x.Apellido, x.Email })
                    .SingleOrDefault(x => x.Email == usuario);*/
				return await contexto.Propietarios.SingleOrDefaultAsync(x => x.Email == usuario);
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		// GET api/<controller>/5
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			try
			{
				var entidad = await contexto.Propietarios.SingleOrDefaultAsync(x => x.id_Propietario == id);
				return entidad != null ? Ok(entidad) : NotFound();
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}
		[HttpGet("obtenerusuario")]
        [Authorize]
		public async Task<ActionResult<Propietario>> ObtenerUsuario()
		{
			try
			{

				var email = HttpContext.User.FindFirst(ClaimTypes.Name).Value;

				return await contexto.Propietarios.FirstOrDefaultAsync(x => x.Email == email);

			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
[HttpGet("obtenerusuarioB")]
[Authorize]
public async Task<ActionResult<object>> ObtenerUsuarioB()
{
    try
    {
        var email = HttpContext.User.FindFirst(ClaimTypes.Name).Value;

        // Selecciona los campos necesarios del propietario y devuelve ese objeto proyectado
         var propietario = await contexto.Propietarios
            .Where(x => x.Email == email)
            .Select(x => new
            {
                x.id_Propietario,
                x.Nombre,
                x.Apellido,
                x.Dni,
                x.Email,
              //  x.Clave,
                x.Telefono,
             //   x.EstadoP,
            //    x.Avatar
            })
            .FirstOrDefaultAsync();

        return propietario;
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
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
					salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
					prf: KeyDerivationPrf.HMACSHA1,
					iterationCount: 1000,
					numBytesRequested: 256 / 8));
				Console.WriteLine(hashed);
				var p = await contexto.Propietarios.FirstOrDefaultAsync(x => x.Email == loginView.Usuario);
				if (p == null || p.Clave != hashed)
				{
					return BadRequest("Nombre de usuario o clave incorrecta");
				}
				else
				{
					var key = new SymmetricSecurityKey(
						System.Text.Encoding.ASCII.GetBytes(config["TokenAuthentication:SecretKey"]));
					var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
					var claims = new List<Claim>
					{
						new Claim(ClaimTypes.Name, p.Email),
						new Claim("FullName", p.Nombre + " " + p.Apellido),
						new Claim(ClaimTypes.Role, "Propietario"),
					};

					var token = new JwtSecurityToken(
						issuer: config["TokenAuthentication:Issuer"],
						audience: config["TokenAuthentication:Audience"],
						claims: claims,
						expires: DateTime.Now.AddMinutes(60),
						signingCredentials: credenciales
					);
					return Ok(new JwtSecurityTokenHandler().WriteToken(token));
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
    
		// POST api/<controller>
		[HttpPost]
		public async Task<IActionResult> Post([FromForm] Propietario entidad)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await contexto.Propietarios.AddAsync(entidad);
					contexto.SaveChanges();
					return CreatedAtAction(nameof(Get), new { id = entidad.id_Propietario }, entidad);
				}
				return BadRequest();
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}


		// PUT api/<controller>/5
		[HttpPut("actualizar")]
         [Authorize]
		public async Task<IActionResult> Put([FromBody] Propietario prop)
		{
			try
			{
				var email = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
				Propietario original = await contexto.Propietarios.FirstOrDefaultAsync(x => x.Email == email);

				if (prop.id_Propietario != original.id_Propietario)
				{
					return Unauthorized();
				}


			
				contexto.Entry(original).CurrentValues.SetValues(prop);
				//contexto.Propietarios.Update(prop);
				await contexto.SaveChangesAsync();

				return (IActionResult)await contexto.Propietarios.FirstOrDefaultAsync(x => x.Email == email);
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}





		// DELETE api/<controller>/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var p = contexto.Propietarios.Find(id);
					if (p == null)
						return NotFound();
					contexto.Propietarios.Remove(p);
					contexto.SaveChanges();
					return Ok(p);
				}
				return BadRequest();
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

/*
        [HttpPut("editar")]
    [Authorize]
    public async Task<IActionResult> Editar([FromBody] Propietario propietario)
    {
        try
        {
            var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
           
     var user = await contexto.Propietarios.SingleOrDefaultAsync(x => x.Email == usuario);
            int cantidad = contexto.Propietarios.Count(u => u.Email == propietario.Email && u.Email != user.Email);
            Console.WriteLine("CORREO DESDE EDITAR: "+propietario.Email);

            if (cantidad > 0)
            {
                Propietario? prop = null;
                return Ok(prop);
            }

            if (usuario == null) return Unauthorized("Token incorrecto");
            var dbUser = await contexto.Propietarios.SingleOrDefaultAsync(x => x.Email == user.Email);

            if (dbUser == null) return BadRequest("No se encontro el usuario");

            

            dbUser.Nombre = !string.IsNullOrEmpty(propietario.Nombre) ? propietario.Nombre : dbUser.Nombre;
            dbUser.Apellido = !string.IsNullOrEmpty(propietario.Apellido) ? propietario.Apellido : dbUser.Apellido;
            dbUser.Dni = !string.IsNullOrEmpty(propietario.Dni) ? propietario.Dni : dbUser.Dni;
            dbUser.Telefono = !string.IsNullOrEmpty(propietario.Telefono) ? propietario.Telefono : dbUser.Telefono;
            dbUser.Email = !string.IsNullOrEmpty(propietario.Email) ? propietario.Email : dbUser.Email;
           // dbUser.Avatar = !string.IsNullOrEmpty(propietario.Avatar) ? propietario.Avatar : dbUser.Avatar;
            Console.WriteLine("correo: " + dbUser.Email);

            if (propietario.Clave != null && propietario.Clave != "" && propietario.Clave != dbUser.Clave)
            {
                dbUser.Clave = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: propietario.Clave,
                salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8));
            }
            contexto.Update(dbUser);
            contexto.SaveChanges();
       
            Console.WriteLine("claveOriginal: " + propietario.Clave);
            Console.WriteLine("claveNueva: " + dbUser.Clave);
            Console.WriteLine("correoOriginal: " + propietario.Email);
            Console.WriteLine("correoNuevo: " + dbUser.Email);

            if (propietario.Email!=user.Email
                || !string.IsNullOrWhiteSpace(propietario.Clave)
                ||!string.IsNullOrEmpty(propietario.Clave))
            {
                return Ok("Reloguear");
            }
            

            var key = new SymmetricSecurityKey(                                                                                                 
                    System.Text.Encoding.ASCII.GetBytes(
                        config["TokenAuthentication:SecretKey"]
                    )
                );

            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, dbUser.Email),
                    new Claim("FullName", dbUser.Nombre + " " + dbUser.Apellido),
                    // new Claim(ClaimTypes.Role, "Propietario")
                };

            var token = new JwtSecurityToken(
                issuer: config["TokenAuthentication:Issuer"],
                audience: config["TokenAuthentication:Audience"],
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
*/

	[HttpPut("editar")]
[Authorize]
public async Task<IActionResult> Editar([FromBody] Propietario propietario)
{
    try
    {
        var usuario = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        var user = await contexto.Propietarios.SingleOrDefaultAsync(x => x.Email == usuario);
        
        if (user == null) return Unauthorized("Token incorrecto");
        
        
        
        int cantidad = contexto.Propietarios.Count(u => u.Email == propietario.Email && u.Email != user.Email);
        if (cantidad > 0)
        {
            return BadRequest("El correo electrónico ya está en uso por otro usuario.");
        }

        user.Nombre = propietario.Nombre ?? user.Nombre;
        user.Apellido = propietario.Apellido ?? user.Apellido;
        user.Dni = propietario.Dni ?? user.Dni;
        user.Telefono = propietario.Telefono ?? user.Telefono;
        user.Email = propietario.Email ?? user.Email;

        contexto.Propietarios.Update(user);
        await contexto.SaveChangesAsync();

        return Ok(new 
        {
            user.id_Propietario,
            user.Nombre,
            user.Apellido,
            user.Dni,
            user.Email,
            user.Telefono
        });
    }
    catch (Exception e)
    {
        return BadRequest(e.Message);
    }
}
[HttpPut("editClave")]
[Authorize]
public async Task<IActionResult> clave([FromBody] CambiarClaveModel model)
{
      try
    {
        var usuario = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        if (usuario == null)
        {
            return Unauthorized("Token incorrecto");
        }

        var dbUser = await contexto.Propietarios.SingleOrDefaultAsync(x => x.Email == usuario);
        if (dbUser == null)
        {
            return BadRequest("No se encontró el usuario");
        }

        // Validar la clave antigua
        var claveAntiguaHashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: model.ClaveAntigua,
            salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 1000,
            numBytesRequested: 256 / 8));

        if (dbUser.Clave != claveAntiguaHashed)
        {
            return BadRequest("La contraseña antigua es incorrecta.");
        }

        // Validar y actualizar la nueva clave
        if (model.ClaveNueva != model.ConfirmarClaveNueva)
        {
            return BadRequest("La nueva contraseña y la confirmación no coinciden.");
        }

        var nuevaClaveHashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: model.ClaveNueva,
            salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 1000,
            numBytesRequested: 256 / 8));

        dbUser.Clave = nuevaClaveHashed;
        contexto.Propietarios.Update(dbUser);
        await contexto.SaveChangesAsync();

        // Generar nuevo token
        var key = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(config["TokenAuthentication:SecretKey"]));
        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, dbUser.Email),
            new Claim("FullName", dbUser.Nombre + " " + dbUser.Apellido),
        };

        var token = new JwtSecurityToken(
            issuer: config["TokenAuthentication:Issuer"],
            audience: config["TokenAuthentication:Audience"],
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
// GET api/<controller>/GetAll
		[HttpGet("GetAll")]
		public async Task<IActionResult> GetAll()
		{
			try
			{

				//	return Ok(Convert.ToString(await contexto.Propietarios.ToListAsync()));
				return Ok(await contexto.Propietarios.ToListAsync());

			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
		// GET: api/Propietario/test
		[HttpGet("test")]
		[AllowAnonymous]
		public IActionResult Test()
		{
			try
			{
				return Ok("anduvo");
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		// GET: api/Propietarios/test/5
		[HttpGet("test/{codigo}")]
		[AllowAnonymous]
		public IActionResult Code(int codigo)
		{
			try
			{
				//StatusCodes.Status418ImATeapot //constantes con códigos
				return StatusCode(codigo, new { Mensaje = "Anduvo", Error = false });
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

       [HttpGet("user")]
    [Authorize]

    public async Task<ActionResult<Propietario>> GetUser() // devuelve el propietario logueado
    {
        try
        {
            var usuario =  HttpContext.User.FindFirst(ClaimTypes.Name).Value;
            if (usuario == null) return Unauthorized("Token no válido");
            var dbUser = await contexto.Propietarios.SingleOrDefaultAsync(x => x.Email == usuario);
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
        { //método sin autenticar, busca el propietario x email
            Console.WriteLine($"Email: {correo}");
            var propietario = await contexto.Propietarios.FirstOrDefaultAsync(x => x.Email == correo);
            var link = "";
            string localIPv4 = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                 ?.ToString();
            var dominio = environment.IsDevelopment() ? localIPv4 : "www.misitio.com";

            if (propietario != null)
            {
                var key = new SymmetricSecurityKey(
                                   System.Text.Encoding.ASCII.GetBytes(
                                       config["TokenAuthentication:SecretKey"]
                                   )
                               );
                var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, propietario.Email),
                    new Claim("FullName", propietario.Nombre + " " + propietario.Apellido),
                };

                var token = new JwtSecurityToken(
                    issuer: config["TokenAuthentication:Issuer"],
                    audience: config["TokenAuthentication:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(24),
                    signingCredentials: credenciales
                );


                link = $"http://{dominio}:5000/api/Propietario/token?access_token={new JwtSecurityTokenHandler().WriteToken(token)}";


                Console.WriteLine(link);

                string subject = "Pedido de Recuperacion de Contraseña";
                string body = @$"<html>
                <body>
                    <h1>Recuperación de Contraseña</h1>
                    <p>Estimado {propietario.Nombre},</p>
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

                return Ok(propietario);
            }
            else
            {
                return BadRequest("Nombre de usuario o clave incorrectos");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("ERRRROR");
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
                Email = User.Identity?.Name,
                Nombre = User.Claims.First(x => x.Type == "FullName").Value,
            };
            Console.WriteLine("ASDASD0" + perfil.Nombre);
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
            await enviarMail(perfil.Email, subject, body);

            var propietario = await contexto.Propietarios.FirstOrDefaultAsync(x => x.Email == perfil.Email);

            if (propietario != null)
            {
                propietario.Clave = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: nuevaClave,
                salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8));
                contexto.Update(propietario);
                contexto.SaveChanges();
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

   
    [HttpGet("sendEmail")]
    private async Task<IActionResult> enviarMail(string email, string subject, string body)
    {
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress("Sistema", config["SMTPUser"]));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("html") { Text = body, };

        using (var client = new SmtpClient())
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.Auto);
            await client.AuthenticateAsync(config["SMTPUser"], config["SMTPPass"]);
            await client.SendAsync(emailMessage);

            await client.DisconnectAsync(true);
        }
        return Ok();
    }

    }
    }