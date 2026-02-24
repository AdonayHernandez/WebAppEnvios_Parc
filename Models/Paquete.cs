namespace WebAppEnvios.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace WebAppEnvios.Models
    {
        public class Paquete
        {
            [Key]
            public int PaqueteId { get; set; }

            public int EnvioId { get; set; }

            public decimal Peso { get; set; }

            [ForeignKey("EnvioId")]
            public Envio Envio { get; set; }
        }
    }
}