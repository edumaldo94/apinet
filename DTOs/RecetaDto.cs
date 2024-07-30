
public class RecetaDto
{
    public UsuarioDto Usuario { get; set; }
     public int RecetaID { get; set; }
 public int? CantidadComentarios  { get; set; }
    public string? FotoPortada { get; set; }
    public string? Descripcion { get; set; }
    public List<ComentarioDto>? Comentarios { get; set; }
    public LikeDto? Likes { get; set; }
     public bool EsFavorita { get; set; }
}