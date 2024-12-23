using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionForge.EntidadesDeNegocio
{
    public class Respuesta
    {
        [Key]
        public int Id { get; set; }
        public int IdPregunta { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Contenido { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
