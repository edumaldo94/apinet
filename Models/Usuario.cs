using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace apinet.Models
{
    public class Usuario
    {
        [Key]
        public int UsuarioID { get; set; }
        
        public string Nombre { get; set; }
        
        public string Apellido { get; set; }
        
        public string? Clave { get; set; }

        
        public string Correo { get; set; }
    
        public string Foto { get; set; }

        public ICollection<Receta> Recetas { get; set; }
        public ICollection<Comentario> Comentarios { get; set; }
        public ICollection<Like> Likes { get; set; }
        public ICollection<RecetaFavorita> RecetasFavoritas { get; set; }
    }
}

























/*using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace apinet.Models
{
    public class Usuario
    {
        public enum enRoles
        {
            Administrador = 1,
            Empleado = 2,
            Propietario=3,
        }

        public int? UsuarioId { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Password { get; set; }
        public string? Correo { get; set; }
        public int Rol { get; set; }
        public string? Avatar { get; set; }
        public string? PasswordAnterior { get; set; }
        [NotMapped]
        public IFormFile? ImgAvatar { get; set; }

        public string RolNombre => Rol > 0 ? ((enRoles)Rol).ToString() : "";

        public static IDictionary<int, string> ObtenerRoles()
        {
            SortedDictionary<int, string> roles = new SortedDictionary<int, string>();
            Type tipoEnumRol = typeof(enRoles);
            foreach (var valor in Enum.GetValues(tipoEnumRol))
            {
                roles.Add((int)valor, Enum.GetName(tipoEnumRol, valor));
            }
            return roles;
        }
    }
}

///////////////////////////////////////////////////////////////////////////
para crear usuario body-Form se pone los datos ahi

Form Fields
nombre
Da
Apellido
Ca
Password
123
Correo
da@gmail.com
Rol
1
Avatar
/Uploads\\avatar_d03450b2-13d1-4b2b-82c4-db2975774e8c.png

para login   http://localhost:5000/Usuario/Login

cambair pass http://localhost:5000/Usuario/1

delet avatar http://localhost:5000/Usuario/Avatar/2

Delet User   http://localhost:5000/Usuario/User/4

userid       http://localhost:5000/Usuario/UserId/2
*/