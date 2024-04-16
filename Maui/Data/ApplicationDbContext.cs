﻿using Maui.Models;
using Microsoft.EntityFrameworkCore;

namespace Maui.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public virtual DbSet<Utente> Utente { get; set; }
        public virtual DbSet<Prodotto> Prodotto { get; set; }
        public virtual DbSet<ProdottoAcquistato> ProdottiAcquistati { get; set; }
        public virtual DbSet<Ordine> Ordini { get; set; }


    }

}
