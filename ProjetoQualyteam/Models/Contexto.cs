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
            
        public DbSet<Arquivo> arquivos { get; set; }

    }
}
