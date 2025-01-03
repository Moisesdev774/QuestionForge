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

        // Este campo puede seguir existiendo para la base de datos, pero ya no será enviado desde el frontend.
        public int IdUsuario { get; set; }

        public string? NombreUsuario { get; set; }

        [Required(ErrorMessage = "El Titulo Es requerido")]
        public string Titulo { get; set; }
        [Required(ErrorMessage = "La Descripcion Es requerido")]


        public string Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; }

        public bool Cerrada { get; set; }

        public List<Respuesta> Respuestas { get; set; } = new List<Respuesta>();
    }

}
