using Microsoft.EntityFrameworkCore;
using OrderProcessing.Modules.Orders.Outbox;

namespace OrderProcessing.Modules.Orders.Persistence;

internal sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("orders");

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("outbox_messages");
            builder.HasKey(message => message.Id);
            builder.Property(message => message.Type).HasMaxLength(500).IsRequired();
            builder.Property(message => message.Payload).IsRequired();
            builder.Property(message => message.OccurredAt).IsRequired();
            builder.Property(message => message.ProcessedAt);
            builder.Property(message => message.Error).HasMaxLength(4000);
            builder.HasIndex(message => message.ProcessedAt);
        });
    }
}
