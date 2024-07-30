
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace apinet.Models
{
    public class DataContextB : DbContext
    {
        public DataContextB(DbContextOptions<DataContextB> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Receta> Recetas { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<Respuesta> Respuestas { get; set; }
        public DbSet<Like> Likes { get; set; }
       
        public DbSet<RecetaFavorita> RecetasFavoritas { get; set; }
         protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RecetaFavorita>()
                 .HasKey(rf => new { rf.RecetaId, rf.UsuarioId });

            base.OnModelCreating(modelBuilder);
            
            
        }
    }

}
