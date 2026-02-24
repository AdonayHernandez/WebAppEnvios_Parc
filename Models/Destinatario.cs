namespace WebAppEnvios.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

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

            // Relación
            public ICollection<Envio> Envios { get; set; }
        }
    }
}
