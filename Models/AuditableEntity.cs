using System;
using System.ComponentModel.DataAnnotations;

namespace WebAppEnvios.Models
{
    /// <summary>
    /// Clase base de auditoría. Todas las entidades principales la heredan
    /// para tener trazabilidad de creación y modificación.
    /// </summary>
    public abstract class AuditableEntity
    {
        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Fecha de Modificación")]
        public DateTime? FechaModificacion { get; set; }

        [MaxLength(256)]
        [Display(Name = "Creado Por")]
        public string? CreadoPor { get; set; }

        [MaxLength(256)]
        [Display(Name = "Modificado Por")]
        public string? ModificadoPor { get; set; }
    }
}
