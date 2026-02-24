namespace WebAppEnvios.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace WebAppEnvios.Models
    {
        public class Envio
        {
            [Key]
            public int EnvioId { get; set; }

            // Foreign Keys
            public int ClienteId { get; set; }
            public int DestinatarioId { get; set; }
            public int EstadoId { get; set; }

            public DateTime FechaEnvio { get; set; }
            public DateTime? FechaEntrega { get; set; }

            public decimal Costo { get; set; }

            // Navigation Properties
            [ForeignKey("ClienteId")]
            public Cliente Cliente { get; set; }

            [ForeignKey("DestinatarioId")]
            public Destinatario Destinatario { get; set; }

            [ForeignKey("EstadoId")]
            public EstadoEnvio EstadoEnvio { get; set; }

            public ICollection<Paquete> Paquetes { get; set; }
        }
    }
}