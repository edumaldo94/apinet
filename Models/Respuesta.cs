
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace apinet.Models;


public class Respuesta
{
    public int RecetaID { get; set;}
    public int RespuestaID { get; set; }
    public int ComentarioID { get; set; }
    public int UsuarioID { get; set; }
    public string RespuestasAtr { get; set; }
    public DateTime FechaHora { get; set; }
    public bool Eliminado { get; set; }

    public Comentario Comentario { get; set; }
    public Usuario Usuario { get; set; }
}