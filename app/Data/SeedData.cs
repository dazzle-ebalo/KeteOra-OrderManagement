namespace KeteOra.Orders.Data;

// Demo data: the database is rebuilt from this every time the app starts
public static class SeedData
{
    public static void Load(KeteOraDb db)
    {
        db.Products.AddRange(
            new Product { Id = 1, Sku = "KO-KETE-01", Name = "Harakeke woven kete", Category = "Homeware", Price = 64.90m, Stock = 18 },
            new Product { Id = 2, Sku = "KO-THROW-02", Name = "Merino wool throw", Category = "Homeware", Price = 149.00m, Stock = 7 },
            new Product { Id = 3, Sku = "KO-HONEY-03", Name = "Mānuka honey 500 g", Category = "Pantry", Price = 42.50m, Stock = 35 },
            new Product { Id = 4, Sku = "KO-MUG-04", Name = "Stoneware mug", Category = "Kitchen", Price = 24.00m, Stock = 52 },
            new Product { Id = 5, Sku = "KO-BALM-05", Name = "Kawakawa balm 50 ml", Category = "Wellbeing", Price = 18.75m, Stock = 4 },
            new Product { Id = 6, Sku = "KO-CANDLE-06", Name = "Beeswax candle set", Category = "Homeware", Price = 32.00m, Stock = 0 });

        db.Customers.AddRange(
            new Customer { Id = 1, Name = "Aroha Tane", Email = "aroha.tane@example.com", Phone = "021 555 0101", Street = "48 Cuba Street", Suburb = "Te Aro", City = "Wellington", Region = "Wellington", Postcode = "6011" },
            new Customer { Id = 2, Name = "Liam Kerr", Email = "liam.kerr@example.com", Phone = "022 555 0102", Street = "15 Jackson Street", Suburb = "Petone", City = "Lower Hutt", Region = "Wellington", Postcode = "5012" },
            new Customer { Id = 3, Name = "Mere Parata", Email = "mere.parata@example.com", Phone = "027 555 0103", Street = "7 Kenepuru Drive", Suburb = "Kenepuru", City = "Porirua", Region = "Wellington", Postcode = "5022" },
            new Customer { Id = 4, Name = "Sione Fifita", Email = "sione.fifita@example.com", Phone = "021 555 0104", Street = "220 Great South Road", Suburb = "Ōtāhuhu", City = "Auckland", Region = "Auckland", Postcode = "1062" },
            new Customer { Id = 5, Name = "Emma Walsh", Email = "emma.walsh@example.com", Phone = "027 555 0105", Street = "31 Riccarton Road", Suburb = "Riccarton", City = "Christchurch", Region = "Canterbury", Postcode = "8011" });

        db.Orders.AddRange(
            new Order
            {
                Id = 1001, CustomerId = 5, PlacedAt = new DateTime(2026, 10, 11, 10, 2, 0), Payment = "Paid", Fulfillment = "Delivered", Delivery = "Courier Overnight",
                Lines = [Line(2, 1, 149.00m), Line(4, 2, 24.00m)]
            },
            new Order
            {
                Id = 1002, CustomerId = 3, PlacedAt = new DateTime(2026, 10, 12, 11, 18, 0), Payment = "Refunded", Fulfillment = "Delivered", Delivery = "NZ Post Standard",
                Lines = [Line(5, 1, 18.75m)]
            },
            new Order
            {
                Id = 1003, CustomerId = 4, PlacedAt = new DateTime(2026, 10, 13, 14, 7, 0), Payment = "Paid", Fulfillment = "In transit", Delivery = "Courier Overnight",
                Lines = [Line(1, 1, 64.90m), Line(3, 2, 42.50m)]
            },
            new Order
            {
                Id = 1004, CustomerId = 2, PlacedAt = new DateTime(2026, 10, 14, 8, 15, 0), Payment = "Pending", Fulfillment = "Unfulfilled", Delivery = "Click & Collect",
                Lines = [Line(3, 1, 42.50m)]
            },
            new Order
            {
                Id = 1005, CustomerId = 1, PlacedAt = new DateTime(2026, 10, 14, 9, 41, 0), Payment = "Paid", Fulfillment = "Unfulfilled", Delivery = "NZ Post Standard",
                Lines = [Line(1, 1, 64.90m), Line(4, 1, 24.00m)]
            });

        db.SaveChanges();
    }

    private static OrderLine Line(int productId, int quantity, decimal unitPrice) =>
        new() { ProductId = productId, Quantity = quantity, UnitPrice = unitPrice };
}
