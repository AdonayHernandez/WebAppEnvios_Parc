using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAppEnvios.Models
{
    public class EstadoEnvio
        {
            [Key]
            public int EstadoId { get; set; }

            [Required]
            public string NombreEstado { get; set; }

            public string Descripcion { get; set; }

            // Relación
            public ICollection<Envio>? Envios { get; set; }
        }
}
