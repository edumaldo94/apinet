using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace apinet.Models
{
public class CambiarClaveModel
{
    public string ClaveAntigua { get; set; }
    public string ClaveNueva { get; set; }
    public string ConfirmarClaveNueva { get; set; }
}
}