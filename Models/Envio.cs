using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppEnvios.Models
{
    public class Envio : AuditableEntity
    {
        [Key]
        public int EnvioId { get; set; }

        // Foreign Keys
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [Display(Name = "Destinatario")]
        public int DestinatarioId { get; set; }

        [Display(Name = "Estado")]
        public int EstadoId { get; set; }

        [Display(Name = "Sucursal de Origen")]
        public int? SucursalId { get; set; }

        [Required]
        [Display(Name = "Fecha de Envío")]
        public DateTime FechaEnvio { get; set; }

        [Display(Name = "Fecha Estimada de Entrega")]
        public DateTime? FechaEntrega { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Costo ($)")]
        public decimal Costo { get; set; }

        [MaxLength(100)]
        [Display(Name = "Número de Seguimiento")]
        public string? NumeroSeguimiento { get; set; }

        [MaxLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        // Navigation Properties
        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        [ForeignKey("DestinatarioId")]
        public Destinatario? Destinatario { get; set; }

        [ForeignKey("EstadoId")]
        public EstadoEnvio? EstadoEnvio { get; set; }

        [ForeignKey("SucursalId")]
        public Sucursal? Sucursal { get; set; }

        public ICollection<Paquete>? Paquetes { get; set; }
        public ICollection<Pago>? Pagos { get; set; }
        public ICollection<HistorialEstado>? HistorialEstados { get; set; }
    }
}