using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace apinet.Models;


public class Contrato
{
   [DisplayName("N°")]

   [Key]
   public int id_Contrato { get; set; }

   [ForeignKey(nameof(InmuebleId))]
   public int InmuebleId { get; set; }
   [ForeignKey(nameof(InquilinoId))]
   public int InquilinoId { get; set; }

   [DisplayName("Fecha de inicio")]
   public DateTime? Fecha_Inicio { get; set; }
   [DisplayName("Fecha de Finalizacion")]
   public DateTime? Fecha_Fin { get; set; }

   [DisplayName("Monto del Alquiler")]
   public double? Monto { get; set; }

   [DisplayName("Estado")]
   public string? Estado { get; set; }
   public int? EstadoC { get; set; }
   // [NotMapped]
   public Inmueble? Inmueble { get; set; }
   // [NotMapped]
   public Inquilino? Inquilino { get; set; }
   // [NotMapped]
   public List<Pago>? Pagos { get; set; }

}
/*
{

  
  ----------METODO POST-------------
  http://localhost:5000/Contrato

    "inmuebleId": 20,
    "inquilinoId": 6,
    "fecha_Inicio": "2025-01-02",
    "fecha_Fin": "2027-01-02",
    "monto": 102355,
    "estado": "Activo",
    "estadoC": null,
  "inmueble": null,
  "inquilino": null,
  "pagos":[
     null
    ]
  }

----------METODO PUT-------------
http://localhost:5000/Contrato/56
  {
  "id_Contrato":56,
  "inmuebleId": 19,
  "inquilinoId": 5,
  "fecha_Inicio": "2024-04-29T00:00:00",
  "fecha_Fin": "2024-08-25",
  "monto": 550555,
  "estado": "Activo",
 "pagos":[
     null
    ]
} 
*/