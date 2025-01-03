using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionForge.AppWeb.Models
{
    public class Pregunta
    {
        public int Id { get; set; }
        public string? NombreUsuario { get; set; }

        [Required(ErrorMessage = "El Titulo es requerido")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La Descripcion es requerida")]
        public string Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }

        public bool Cerrada { get; set; }

        public List<Respuesta> Respuestas { get; set; } = new List<Respuesta>();
       
    }

}
