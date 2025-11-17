using MongoDB.Bson.Serialization.Attributes;

namespace PI_API.models;

public class OrderItem
{   

    public string order_id;
    public string name {get; set;}
    public double price {get; set;}
    public int quantity {get; set;}

    public OrderItem(string name, double price, int quantity)
    {
        this.name = name;
        this.price = price;
        this.quantity = quantity;
    }
}