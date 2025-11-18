using System.Net;
using System.Text;
using System.Text.Json;

namespace PI_API.services;

public class MercadoPagoPaymentMethod
{
    HttpClient client = new HttpClient();

    public void CreateOrder()
    {
        client.BaseAddress = new Uri("https://api.mercadopago.com");
        

        var orderData = new
        {
            title = "Pedido de teste",
            quantity = 1,
            unit_price = 10.00
        };
        
        string json = JsonSerializer.Serialize(orderData);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
        client.PostAsync("/orders", content);
        
    }
}