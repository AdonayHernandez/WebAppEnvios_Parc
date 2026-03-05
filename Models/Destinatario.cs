using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppEnvios.Models
{
    public class Destinatario
    {
            [Key]
            public int DestinatarioId { get; set; }

            [Required]
            public string Nombre { get; set; }

            public string Telefono { get; set; }
            public string Direccion { get; set; }
            public string Ciudad { get; set; }
            public string Pais { get; set; }

            // Dueño de la dirección
            public int? ClienteId { get; set; }
            [ForeignKey("ClienteId")]
            public Cliente? Cliente { get; set; }

            // Relación
            public ICollection<Envio>? Envios { get; set; }
    }
}
