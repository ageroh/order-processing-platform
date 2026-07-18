using Microsoft.EntityFrameworkCore;
using OrderProcessing.Modules.Orders.Domain;
using OrderProcessing.Modules.Orders.Outbox;

namespace OrderProcessing.Modules.Orders.Persistence;

internal sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("orders");

        modelBuilder.Entity<Order>(builder =>
        {
            builder.ToTable("orders");
            builder.HasKey(order => order.Id);

            builder.Property(order => order.CustomerId).IsRequired();
            builder.Property(order => order.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(order => order.CreatedAt).IsRequired();
            builder.Property(order => order.AcceptedAt);
            builder.Property(order => order.RejectedAt);
            builder.Property(order => order.CancelledAt);
            builder.Property(order => order.RejectionReason).HasMaxLength(500);
            builder.Property(order => order.CancellationReason).HasMaxLength(500);
            builder.Property(order => order.Version).IsConcurrencyToken();

            builder.Property<decimal?>("_pricingSubtotalAmount")
                .HasColumnName("pricing_subtotal_amount")
                .HasPrecision(18, 2);
            builder.Property<decimal?>("_pricingTaxAmount")
                .HasColumnName("pricing_tax_amount")
                .HasPrecision(18, 2);
            builder.Property<decimal?>("_pricingAdditionalChargesAmount")
                .HasColumnName("pricing_additional_charges_amount")
                .HasPrecision(18, 2);
            builder.Property<decimal?>("_pricingTotalAmount")
                .HasColumnName("pricing_total_amount")
                .HasPrecision(18, 2);
            builder.Property<string?>("_pricingCurrency")
                .HasColumnName("pricing_currency")
                .HasMaxLength(3);

            builder.Ignore(order => order.Pricing);
            builder.Ignore(order => order.DomainEvents);

            builder.OwnsMany(order => order.Lines, line =>
            {
                line.ToTable("order_lines");
                line.WithOwner().HasForeignKey("order_id");
                line.HasKey(orderLine => orderLine.Id);
                line.Property(orderLine => orderLine.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();
                line.Property(orderLine => orderLine.ProductId)
                    .HasColumnName("product_id")
                    .IsRequired();
                line.Property(orderLine => orderLine.Quantity)
                    .HasColumnName("quantity")
                    .IsRequired();
                line.HasIndex(orderLine => orderLine.ProductId);
            });

            builder.Navigation(order => order.Lines)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.OwnsMany(order => order.Lifecycle, lifecycle =>
            {
                lifecycle.ToTable("order_lifecycle_events");
                lifecycle.WithOwner().HasForeignKey("order_id");
                lifecycle.HasKey(entry => entry.Id);
                lifecycle.Property(entry => entry.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();
                lifecycle.Property(entry => entry.Status)
                    .HasColumnName("status")
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();
                lifecycle.Property(entry => entry.Description)
                    .HasColumnName("description")
                    .HasMaxLength(500)
                    .IsRequired();
                lifecycle.Property(entry => entry.OccurredAt)
                    .HasColumnName("occurred_at")
                    .IsRequired();
                lifecycle.HasIndex(entry => entry.OccurredAt);
            });

            builder.Navigation(order => order.Lifecycle)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(order => order.CustomerId);
            builder.HasIndex(order => order.Status);
            builder.HasIndex(order => order.CreatedAt);
        });

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
