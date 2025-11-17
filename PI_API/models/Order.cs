namespace PI_API.models;

public class Order
{
    string id { get; set; }
    List<OrderItem> orderItems = new List<OrderItem>();

    public double GetTotal()
    {
        double total = 0;
        foreach (var orderItem in orderItems)
        {
             total += orderItem.price * orderItem.quantity;
        }
        return total;
    }
    
}