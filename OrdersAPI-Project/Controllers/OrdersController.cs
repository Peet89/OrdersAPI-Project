using Microsoft.AspNetCore.Mvc;
using System;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public IActionResult ProcessOrder([FromBody] Order order)
    {
        try
        {
            var processor = new OrderProcessor();
            processor.ProcessOrder(order);
            return Ok(new { Message = "Orden procesada con éxito." });
        }
        catch (ArgumentException ex)
        {
            return StatusCode(500, $"Error de validación: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ocurrió un error inesperado: {ex.Message}");
        }
    }
}