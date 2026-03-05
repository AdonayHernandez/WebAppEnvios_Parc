using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppEnvios.Models
{
    public enum TipoPaquete
    {
        Normal,
        Fragil,
        Documentos,
        Electronico,
        Alimentacion
    }

    public class Paquete
    {
        [Key]
        public int PaqueteId { get; set; }

        [Required]
        [Display(Name = "Envío")]
        public int EnvioId { get; set; }

        [Required]
        [Column(TypeName = "decimal(8,2)")]
        [Display(Name = "Peso (kg)")]
        public decimal Peso { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        [Display(Name = "Largo (cm)")]
        public decimal? Largo { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        [Display(Name = "Ancho (cm)")]
        public decimal? Ancho { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        [Display(Name = "Alto (cm)")]
        public decimal? Alto { get; set; }

        [MaxLength(300)]
        [Display(Name = "Descripción del contenido")]
        public string? Descripcion { get; set; }

        [Display(Name = "Tipo de paquete")]
        public TipoPaquete Tipo { get; set; } = TipoPaquete.Normal;

        [ForeignKey("EnvioId")]
        public Envio? Envio { get; set; }
    }
}