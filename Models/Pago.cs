using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppEnvios.Models
{
    public enum EstadoPago
    {
        Pendiente,
        Pagado,
        Anulado
    }

    public enum MetodoPago
    {
        Efectivo,
        Transferencia,
        TarjetaCredito,
        TarjetaDebito
    }

    public class Pago
    {
        [Key]
        public int PagoId { get; set; }

        [Required]
        [Display(Name = "Envío")]
        public int EnvioId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Monto ($)")]
        public decimal Monto { get; set; }

        [Display(Name = "Estado del Pago")]
        public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;

        [Display(Name = "Método de Pago")]
        public MetodoPago Metodo { get; set; } = MetodoPago.Efectivo;

        [Display(Name = "Fecha de Pago")]
        public DateTime? FechaPago { get; set; }

        [MaxLength(200)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Display(Name = "Comisión (%)")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Comision { get; set; } = 0;

        [NotMapped]
        [Display(Name = "Monto Comisión ($)")]
        public decimal MontoComision => Math.Round(Monto * Comision / 100, 2);

        // Navigation
        [ForeignKey("EnvioId")]
        public Envio? Envio { get; set; }
    }
}
