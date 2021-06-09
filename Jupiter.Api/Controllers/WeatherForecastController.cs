using System.IO;
using System.Text;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Jupiter.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public WeatherForecastController(ILogger<WeatherForecastController> logger, ApplicationDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            this._configuration = configuration;
        }

        [HttpGet("/database")]
        public IActionResult GetFromDb()
        {
            var entities = _context.Entities;

            return Ok(entities);
        }

        [HttpGet("/azure-storage")]
        public IActionResult GetFromAzureStorage()
        {
            var connectionString = _configuration["Dependencies:Azure:Blob"];
            var container = "container";
            var blob = "blob";

            var blobServiceClient = new BlobServiceClient(connectionString);
            blobServiceClient.DeleteBlobContainer(container);
            blobServiceClient.CreateBlobContainer(container);
            var containerClient = blobServiceClient.GetBlobContainerClient(container);
            var blobClient = containerClient.GetBlobClient(blob);
            using var stream = new MemoryStream();
            blobClient.Upload(stream, true);
            var blobs = containerClient.GetBlobsAsync();
            return Ok(blobs);
        }

        [HttpPost("/message-broker")]
        public IActionResult PostToMessageBroker()
        {
            var message = "message";
            var factory = new ConnectionFactory { UserName = "jupiter", Password = "jupiter-pwd", HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "", routingKey: "queue", basicProperties: null, body: body);
            return Ok(message);
        }
    }
}
