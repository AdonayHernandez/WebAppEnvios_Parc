using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppEnvios.Models
{
    public class HistorialEstado
    {
        [Key]
        public int HistorialId { get; set; }

        [Required]
        [Display(Name = "Envío")]
        public int EnvioId { get; set; }

        [MaxLength(100)]
        [Display(Name = "Estado Anterior")]
        public string? EstadoAnterior { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Estado Nuevo")]
        public string EstadoNuevo { get; set; }

        [Required]
        [Display(Name = "Fecha de Cambio")]
        public DateTime FechaCambio { get; set; } = DateTime.Now;

        [MaxLength(200)]
        [Display(Name = "Usuario")]
        public string? CambiadoPor { get; set; }

        [MaxLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        // Navigation
        [ForeignKey("EnvioId")]
        public Envio? Envio { get; set; }
    }
}
