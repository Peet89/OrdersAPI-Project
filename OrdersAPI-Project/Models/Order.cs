using System.Collections.Generic;

public class Order
{
    public string CustomerEmail { get; set; }
    public List<OrderItem> Items { get; set; }
}

public class OrderItem
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}