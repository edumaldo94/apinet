using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using apinet.Models;

var builder = WebApplication.CreateBuilder(args);


// Configurar la URL
builder.WebHost.UseUrls("http://localhost:5000","http://localhost:5001", "http://*:5000","http://*:5001");
var configuration = builder.Configuration;
// Agregar servicios al contenedor
 builder.Services.AddControllersWithViews();
 builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

 builder.Services.AddRazorPages();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddCookie(options =>
               {
                   options.LoginPath = "/Usuario/Login";
                   options.LogoutPath = "/Usuario/Logout";
                   options.AccessDeniedPath = "/Home/Restringido";
               })
               .AddJwtBearer(options =>
               {
                
                   options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,
                       ValidIssuer = configuration["TokenAuthentication:Issuer"],
                       ValidAudience = configuration["TokenAuthentication:Audience"],
                       IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(
                           configuration["TokenAuthentication:SecretKey"])),
                   };
                   options.Events = new JwtBearerEvents
                   {
                   /*  OnMessageReceived = context =>
                       {
                           var accessToken = context.Request.Query["access_token"];
                           var path = context.HttpContext.Request.Path;
                           if (!string.IsNullOrEmpty(accessToken) &&
                           path.StartsWithSegments("/chatsegurohub"))
                           {
                               context.Token = accessToken;
                           }*/
                            OnMessageReceived = context =>
             {
                // Leer el token desde el query string
                var accessToken = context.Request.Query["access_token"];
                // Si el request es para el Hub u otra ruta seleccionada...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/chatsegurohub") ||
                    path.StartsWithSegments("/api/Usuario/reset") ||
                    path.StartsWithSegments("/api/Usuario/token")))
                {//reemplazar las urls por las necesarias ruta ⬆
                    context.Token = accessToken;
                }
                           return Task.CompletedTask;
                       }
                   };
               });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
            builder.Services.AddAuthorization(options =>
            {
               options.AddPolicy("Empleado", policy => policy.RequireClaim(ClaimTypes.Role, "Administrador", "Empleado"));
                options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
            });
      builder.Services.AddMvc();
            builder.Services.AddSignalR();
            //builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
    
     
           builder.Services.AddDbContext<DataContextB>(options =>
                options.UseMySql(configuration.GetConnectionString("DatabaseConnectionString"),
                new MySqlServerVersion(new Version(8, 0, 2))));


            builder.Services.AddHttpContextAccessor();
           
       


// Configurar la autenticación


// Configurar el inicio de la aplicación
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(x => x.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
app.UseStaticFiles();
app.UseRouting();
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
