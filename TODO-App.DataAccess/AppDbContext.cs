using Microsoft.EntityFrameworkCore;
using TODO_App.Domain.Entities;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<ToDoTask> Tasks { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Налаштування зв'язків (Fluent API)
        
        // Користувач -> Таски (One-to-Many)
        modelBuilder.Entity<ToDoTask>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Якщо видалити юзера - видаляються його таски

        // Користувач -> Категорії (One-to-Many)
        modelBuilder.Entity<Category>()
            .HasOne(c => c.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Категорія -> Таски (One-to-Many)
        modelBuilder.Entity<ToDoTask>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull); // Якщо видалити категорію - таски залишаються, просто без категорії
    }
}