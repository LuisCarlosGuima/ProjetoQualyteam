using Microsoft.EntityFrameworkCore;
using ProjetoQualyteam.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoQualyteam.Models
{
    public class Contexto : DbContext
    {
        public Contexto(DbContextOptions<Contexto> options) : base(options)
        {
            Database.EnsureCreated();
        }
            
        public DbSet<Documento> Documentos { get; set; }
        public DbSet<Processo> Processos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }

    }
}
