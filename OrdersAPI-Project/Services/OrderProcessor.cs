using System;
using System.Text;
using System.Linq;
using System.Net.Mail;
using Microsoft.Data.SqlClient; 
using Azure.Storage.Blobs;

public class OrderProcessor
{
    public void ProcessOrder(Order order)
    {
        Console.WriteLine("Iniciando procesamiento de la orden...");

        if (string.IsNullOrEmpty(order.CustomerEmail) || !order.Items.Any())
        {
            throw new ArgumentException("Datos de la orden inválidos.");
        }

        decimal finalPrice = order.Items.Sum(item => item.Price) * 1.21m;
        Console.WriteLine($"Precio final calculado: {finalPrice:C}");

        var connectionString = "Server=localhost;Database=MyOrdersDB;User Id=admin;Password=secret;TrustServerCertificate=True;";
        string query = $"INSERT INTO Orders (CustomerEmail, Total, Status) VALUES ('{order.CustomerEmail}', {finalPrice}, 'processed')";
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var createTableCommand = new SqlCommand("IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Orders' and xtype='U') CREATE TABLE Orders (Id INT PRIMARY KEY IDENTITY, CustomerEmail NVARCHAR(100), Total DECIMAL(18, 2), Status NVARCHAR(50))", connection);
                connection.Open();
                createTableCommand.ExecuteNonQuery();
                var insertCommand = new SqlCommand(query, connection);
                insertCommand.ExecuteNonQuery();
                Console.WriteLine("Orden guardada en la base de datos.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al guardar en la base de datos: {ex.Message}");
            throw;
        }

        try
        {
            var storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=youraccount;AccountKey=SUPER_SECRET_KEY==;EndpointSuffix=core.windows.net";
            var blobServiceClient = new BlobServiceClient(storageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("receipts");
            containerClient.CreateIfNotExists();
            var receiptContent = $"Recibo para {order.CustomerEmail}.\nTotal: {finalPrice:C}\nGracias por su compra.";
            var blobName = $"receipt-{Guid.NewGuid()}.txt";
            using (var stream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(receiptContent)))
            {
                containerClient.UploadBlob(blobName, stream);
            }
            Console.WriteLine($"Recibo '{blobName}' subido a Azure Blob Storage.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al subir el recibo a Azure: {ex.Message}");
        }

        try
        {
            using (var smtpClient = new SmtpClient("smtp.example.com", 587))
            {
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new System.Net.NetworkCredential("user@example.com", "SuperSecretPassword123");
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("noreply@example.com"),
                    Subject = "Tu orden ha sido procesada",
                    Body = $"Gracias por tu compra. El total de tu orden es {finalPrice:C}.",
                };
                mailMessage.To.Add(order.CustomerEmail);
                smtpClient.Send(mailMessage);
                Console.WriteLine("Email de confirmación enviado.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar el email: {ex.Message}");
        }

        Console.WriteLine("Procesamiento de la orden completado con éxito.");
    }
}