using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace WpfApp1.Models
{
    public class Клиент
    {
        public int ID_Клиента { get; set; }
        public string ФИО { get; set; } = null!;
        public string Паспортные_Данные { get; set; } = null!;
        public string? Телефон { get; set; }
        public DateTime Дата_Рождения { get; set; }

        public virtual ICollection<Займ> Займы { get; set; } = new List<Займ>();
    }

    public class Кредитный_Продукт
    {
        public int ID_Продукта { get; set; }
        public string Название { get; set; } = null!;
        public decimal Процентная_Ставка { get; set; }
        public decimal Макс_Сумма { get; set; }
        public int Макс_Срок_Месяцев { get; set; }

        public virtual ICollection<Займ> Займы { get; set; } = new List<Займ>();
    }

    public class Займ
    {
        public int ID_Займа { get; set; }
        public int ID_Клиента { get; set; }
        public int ID_Продукта { get; set; }
        public decimal Сумма_Займа { get; set; }
        public DateTime Дата_Выдачи { get; set; }
        public string Статус { get; set; } = "Активен";

        public virtual Клиент Клиент { get; set; } = null!;
        public virtual Кредитный_Продукт Продукт { get; set; } = null!;
        public virtual ICollection<Платеж> Платежи { get; set; } = new List<Платеж>();
    }

    public class Платеж
    {
        public int ID_Платежа { get; set; }
        public int ID_Займа { get; set; }
        public DateTime Дата_Платежа { get; set; }
        public decimal Сумма_Платежа { get; set; }
        public string Тип_Платежа { get; set; } = "Основной долг";

        public virtual Займ Займ { get; set; } = null!;
    }

    public class BankDbContext : DbContext
    {
        public DbSet<Клиент> Клиенты { get; set; }
        public DbSet<Кредитный_Продукт> Кредитные_Продукты { get; set; }
        public DbSet<Займ> Займы { get; set; }
        public DbSet<Платеж> Платежи { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=PC;Database=УчетЗаймов;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Клиент>(entity => {
                entity.HasKey(e => e.ID_Клиента);
                entity.HasIndex(e => e.Паспортные_Данные).IsUnique();
            });

            modelBuilder.Entity<Кредитный_Продукт>().HasKey(e => e.ID_Продукта);

            modelBuilder.Entity<Займ>(entity => {
                entity.HasKey(e => e.ID_Займа);
                entity.HasOne(d => d.Клиент).WithMany(p => p.Займы).HasForeignKey(d => d.ID_Клиента);
                entity.HasOne(d => d.Продукт).WithMany(p => p.Займы).HasForeignKey(d => d.ID_Продукта);
            });

            modelBuilder.Entity<Платеж>(entity => {
                entity.HasKey(e => e.ID_Платежа);
                entity.HasOne(d => d.Займ).WithMany(p => p.Платежи).HasForeignKey(d => d.ID_Займа);
            });
        }
    }
}