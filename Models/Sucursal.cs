using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAppEnvios.Models
{
    public class Sucursal
    {
        [Key]
        public int SucursalId { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Nombre de Sucursal")]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Departamento")]
        public string Departamento { get; set; }

        [MaxLength(200)]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        [MaxLength(20)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Display(Name = "Activa")]
        public bool Activa { get; set; } = true;

        // Relación
        public ICollection<Envio>? Envios { get; set; }
    }
}
