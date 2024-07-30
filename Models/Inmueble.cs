using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace apinet.Models;

public class Inmueble
{
  [Key]
  public int id_Inmuebles { get; set; }
  [ForeignKey(nameof(PropietarioId))]
  public int PropietarioId { get; set; }
  public double? Latitud { get; set; }
  public double? Longitud { get; set; }
  public string? Ubicacion { get; set; }

  [DisplayName("Direccion")]
  public string? Direccion { get; set; }
  public int? Ambientes { get; set; }
  [DisplayName("Uso")]
  public string? Uso { get; set; }

  [DisplayName("Tipo")]
  public string? Tipo { get; set; }


  [DisplayName("Precio")]
  [Required]
  public double? Precio { get; set; }
  public bool Disponible { get; set; }
  public int? EstadoIn { get; set; }
      public string? Foto { get; set; }
  [DisplayName("Nombre Propietario")]
    [NotMapped] 
  public string? ProprietorName { get; set; }
  [DisplayName("Apellido Propietario")]
    [NotMapped] 

  public string? ProprietorLastName { get; set; }

  public Propietario? Propietario;
}
