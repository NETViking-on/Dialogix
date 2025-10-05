using Microsoft.EntityFrameworkCore;
using Dialogix.Models;

namespace Dialogix.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options)
            : base(options)
        {
        }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id); 
                entity.Property(e => e.User)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(e => e.Text)
                      .IsRequired()
                      .HasMaxLength(2000);
                entity.Property(e => e.CreatedAt)
                      .IsRequired();
            });
        }
    }
}
