using Microsoft.EntityFrameworkCore;
using OrderAutomation.Models;

namespace OrderAutomation.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ProductReceipt> ProductReceipts { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ProductReceipt>()
                .Property(pr => pr.PurchasePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductReceipt>()
                .HasOne(pr => pr.Product)
                .WithMany(p => p.ProductReceipts)
                .HasForeignKey(pr => pr.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductReceipt>()
                .HasOne(pr => pr.Supplier)
                .WithMany(s => s.ProductReceipts)
                .HasForeignKey(pr => pr.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Молочные продукты", Description = "Молоко, сыр, творог, йогурты и другие молочные продукты" },
                new Category { Id = 2, Name = "Мясные продукты", Description = "Мясо, колбасы, сосиски и мясные полуфабрикаты" },
                new Category { Id = 3, Name = "Хлебобулочные изделия", Description = "Хлеб, булочки, батоны и другая выпечка" },
                new Category { Id = 4, Name = "Кондитерские изделия", Description = "Торты, пирожные, конфеты, шоколад" },
                new Category { Id = 5, Name = "Овощи и фрукты", Description = "Свежие овощи, фрукты и зелень" },
                new Category { Id = 6, Name = "Напитки", Description = "Соки, воды, газированные напитки" },
                new Category { Id = 7, Name = "Замороженные продукты", Description = "Замороженные полуфабрикаты, овощи, ягоды" },
                new Category { Id = 8, Name = "Бакалея", Description = "Крупы, макароны, сахар, соль, мука" }
            );

            modelBuilder.Entity<Supplier>().HasData(
                new Supplier { Id = 1, Name = "ООО Продуктовый союз", ContactPerson = "Иванов Иван", Phone = "+7(123)456-78-90", Email = "info@product-union.ru" },
                new Supplier { Id = 2, Name = "ИП Петров С.В.", ContactPerson = "Петров Сергей", Phone = "+7(987)654-32-10", Email = "petrov@example.com" },
                new Supplier { Id = 3, Name = "Фермерское хозяйство 'Заря'", ContactPerson = "Сидорова Мария", Phone = "+7(111)222-33-44", Email = "farm@zarya.ru" }
            );
        }
    }
} 