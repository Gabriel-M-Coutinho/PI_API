using PI_API.models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace PI_API.services;

public class PayPalMethod
{
    HttpClient client = new HttpClient();

    public void CreateOrder(Order order)
    {
        client.BaseAddress = new Uri("https://api-m.sandbox.paypal.com");
        var purchase_units = new List<object>();

        foreach (var item in order.orderItems)
        {
            purchase_units.Add(new
            {
                amount = new
                {
                    currency_code = "BRL",
                    value = item.quantity * item.price
                },
                description = item.name
            });
        }

        var orderData = new
        {
            intent = "CAPTURE",
            payment_source = new
            {
                paypal = new
                {
                    experience_context = new
                    {
                        return_url = "https://developer.paypal.com",
                        cancel_url = "https://www.bing.com",
                        user_action = "PAY_NOW"
                    }
                }
            },
            purchase_units
        };

        string json = JsonSerializer.Serialize(orderData);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
        client.PostAsync("/orders", content);
    }
};