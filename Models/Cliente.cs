using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WebAppEnvios.Models
{
    public class Cliente : AuditableEntity
    {
        [Key]
        public int ClienteId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(20)]
        public string Telefono { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Direccion { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Identity
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }

        // Relación
        public ICollection<Envio>? Envios { get; set; }
        public ICollection<Destinatario>? Destinatarios { get; set; }
    }
}
