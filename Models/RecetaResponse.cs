using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace apinet.Models
{
public class RecetaResponse
{
    public List<Receta> Recetas { get; set; }
}
}