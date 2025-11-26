using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PI_API.Config;
using PI_API.dto;
using PI_API.models;
using PI_API.services;
using Stripe.Checkout;
using Stripe;

namespace PI_API.controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ContextMongodb _context;
    private readonly CreditService _creditService;

    public PaymentsController(IConfiguration configuration, ContextMongodb context,CreditService creditService)
    {
        _creditService = creditService;
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("create-checkout")]
    [Authorize]
    public async Task<ActionResult> CreateCheckout([FromBody] CheckoutDTO dto)
    {
        if (!CreditPlans.Plans.TryGetValue(dto.Plan, out var planData))
            return BadRequest("Plano inválido.");

        var (price, credits) = planData;

        var options = new SessionCreateOptions()
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "brl",
                        UnitAmount = (long)(price * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Plano {dto.Plan} - {credits} créditos"
                        }
                    }
                }
            },
            Mode = "payment",
            SuccessUrl = $"http://localhost:5173/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = "http://localhost:5173/cancel"
        };

        var service = new SessionService();
        Session session = service.Create(options);

        // ✅ CRIAR PEDIDO NO MONGODB
        var order = new Order
        {
            SessionId = session.Id,
            Plan = dto.Plan,
            PackageName = $"Plano {dto.Plan}",
            Credits = credits,
            Price = price,
            IsPaid = false,
            Status = "pending",
            CustomerId = User.FindFirst("userid")?.Value,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Order.InsertOneAsync(order);

        return Ok(new { 
            checkoutUrl = session.Url,
            orderId = order.Id
        });
    }

    [HttpPost("confirm-payment")]
    public async Task<ActionResult> ConfirmPayment([FromBody] ConfirmPaymentDTO dto)
    {
        try
        {
            // Buscar a sessão no Stripe para verificar se foi paga
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(dto.SessionId);

            if (session == null)
                return NotFound("Sessão não encontrada");

            // Buscar o pedido no MongoDB
            var order = await _context.Order
                .Find(o => o.SessionId == dto.SessionId)
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound("Pedido não encontrado");

            // Verificar se o pagamento foi realizado
            if (session.PaymentStatus == "paid")
            {
                // ✅ ATUALIZAR PEDIDO PARA PAGO
                var update = Builders<Order>.Update
                    .Set(o => o.IsPaid, true)
                    .Set(o => o.Status, "paid")
                    .Set(o => o.PaidAt, DateTime.UtcNow);

                await _context.Order.UpdateOneAsync(
                    o => o.SessionId == dto.SessionId, 
                    update
                );
                await _creditService.AddCredits(order.CustomerId,order.Credits);
                
                return Ok(new { 
                    success = true,
                    message = "Pagamento confirmado!",
                    credits = order.Credits,
                    orderId = order.Id
                });
            }
            else
            {
                return Ok(new { 
                    success = false,
                    message = "Pagamento pendente ou falhou",
                    status = session.PaymentStatus
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                success = false, 
                message = "Erro ao confirmar pagamento",
                error = ex.Message 
            });
        }
    }

    [HttpGet("order-status/{sessionId}")]
    public async Task<ActionResult> GetOrderStatus(string sessionId)
    {
        var order = await _context.Order
            .Find(o => o.SessionId == sessionId)
            .FirstOrDefaultAsync();

        if (order == null)
            return NotFound("Pedido não encontrado");

        return Ok(new {
            orderId = order.Id,
            status = order.Status,
            isPaid = order.IsPaid,
            plan = order.Plan,
            packageName = order.PackageName,
            credits = order.Credits,
            amount = order.Price,
            createdAt = order.CreatedAt
        });
    }

    [HttpGet("customer-orders")]
    [Authorize]
    public async Task<ActionResult> GetCustomerOrders()
    {
        var userId = User.FindFirst("userid")?.Value;
        var orders = await _context.Order
            .Find(o => o.CustomerId == userId)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders);
    }
}