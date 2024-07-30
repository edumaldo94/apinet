using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace apinet.Models;

public class Pago
{

  [Key]
  public int? PagoId { get; set; }
  [ForeignKey(nameof(ContratoId))]
  public int? ContratoId { get; set; }


  public Contrato Contrato { get; set; }
  [DisplayName("Numero de Pago")]
  public int? NumeroPago { get; set; }
  [DisplayName("Concepto")]
  public string? Concepto { get; set; }

  [DisplayName("Fecha de Pago")]
  public DateTime? FechaPago { get; set; }

  [DisplayName("Importe")]
  public double? Importe { get; set; }

  [DisplayName("Estado de Pago")]
  public string? EstadoPago { get; set; }
  [NotMapped]
  public string? InquilinoNombre { get; set; }
  [DisplayName("Apellido Inquilino")]
  [NotMapped]
  public string? InquilinoApellido { get; set; }
  [NotMapped]
  public Inmueble? inmueble { get; set; }



}