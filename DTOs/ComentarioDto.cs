public class ComentarioDto
{
     public int?  recetaID{ get; set; }
    public int ComentarioID { get; set; }
    public string Comentario { get; set; }
    
    public UsuarioDto Usuario { get; set; }
    public DateTime FechaHora { get; set; }
}
