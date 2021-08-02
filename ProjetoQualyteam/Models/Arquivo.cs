using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoQualyteam.Models
{
    [Table("arquivo")]
    public class Arquivo
    {
        [Required]
        
        [Display(Name = "Código")]
        [Column("id")]
        public int id { get; set; }

        [Required]
        [Display(Name = "Titulo")]
        [Column("titulo")]
        public string titulo { get; set; }

        [Required]
        [Display(Name = "Processo")]
        [Column("processo")]
        public string processo { get; set; }

        [Required]
        [Display(Name = "Categoria")]
        [Column("categoria")]
        public string categoria { get; set; }
        
        [Column("extensaoarquivo")]
        public string Extensaoarquivo { get; set; }

        [MaxLength]
        public byte[] Arquivofile { get; set; }

    }
}
