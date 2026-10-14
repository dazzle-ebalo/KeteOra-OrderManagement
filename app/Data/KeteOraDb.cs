using Microsoft.EntityFrameworkCore;

namespace KeteOra.Orders.Data;

// KeteOra's order-management data: products, customers (with NZ addresses), orders and order lines
public class KeteOraDb(DbContextOptions<KeteOraDb> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
}

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Street { get; set; } = "";
    public string Suburb { get; set; } = "";
    public string City { get; set; } = "";
    public string Region { get; set; } = "";
    public string Postcode { get; set; } = "";
    public List<Order> Orders { get; set; } = [];
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime PlacedAt { get; set; }
    public string Payment { get; set; } = "";      // Paid, Pending, Refunded
    public string Fulfillment { get; set; } = "";  // Unfulfilled, In transit, Delivered
    public string Delivery { get; set; } = "";
    public List<OrderLine> Lines { get; set; } = [];
}

public class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
