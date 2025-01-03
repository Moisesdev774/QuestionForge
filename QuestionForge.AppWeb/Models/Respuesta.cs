using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionForge.AppWeb.Models
{
    public class Respuesta
    {
        [Key]
        public int Id { get; set; }
        public int IdPregunta { get; set; }
        public int IdUsuario { get; set; }
        public string? Nombre { get; set; }
        public string Contenido { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
