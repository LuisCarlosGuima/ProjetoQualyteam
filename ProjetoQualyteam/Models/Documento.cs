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
    public class Documento
    {
        [Required]

        [Display(Name = "Código")]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Título")]
        [Column("Titulo")]
        public string Titulo { get; set; }

        [Required]
        [Display(Name = "Processo")]
        [Column("Processo")]
        public string Processo { get; set; }

        [Required]
        [Display(Name = "Categoria")]
        [Column("Categoria")]
        public string Categoria { get; set; }

        [Column("extensaoarquivo")]
        public string ExtensaoDoArquivo { get; set; }

        [Column("Arquivofile")]
        public byte[] Arquivo { get; set; }
    }

    [Table("Processo")]
    public class Processo
    {
        [Required]
        [Column("Processo")]
        [Display(Name = "Nome do Processo")]
        public string NovoProcesso { get; set; }

        [Key]
        public int Id { get; set; }
    }

    [Table("Categoria")]
    public class Categoria
    {
        [Required]
        [Column("categoria")]
        [Display(Name ="Nome categoria")]
        public string NomeCategoria { get; set; }

        [Key]
        public int Id { get; set; }

        [Column("Idprocesso")]
        public int Idprocesso { get; set; }

    }


}
