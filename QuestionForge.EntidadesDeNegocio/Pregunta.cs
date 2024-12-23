using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionForge.EntidadesDeNegocio
{
    public class Pregunta
    {
        [Key]
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        [Required]
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Cerrada { get; set; }
        public List<Respuesta> Respuestas { get; set; } = new List<Respuesta>();

    }
}
