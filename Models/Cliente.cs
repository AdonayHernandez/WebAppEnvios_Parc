using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebAppEnvios.Models;
using WebAppEnvios.Models.WebAppEnvios.Models;

namespace WebAppEnvios.Models
{
    public class Cliente
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

        // Relación
        public ICollection<Envio> Envios { get; set; }
    }
}
