using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
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

        [HttpGet("/cosmosdb")]
        public async Task<IActionResult> GetFromCosmosDb()
        {
            //1 option: use windows cosmos db emulator
            //2 option: uncomment cosmos db section in docker compose to run linux emulator of cosmos db 
            //need additional setup
            //install wsl -> windows terminal -> ubuntu 
            //Retrieve the IP address of your local machine
            //IPADDR=$(ifconfig | grep "inet " | grep -Fv 127.0.0.1 | awk '{print $2}' | head -n 1)
            //to gerate certificate local-cosmos-certificate.pem
            //sudo apt-get install dos2unix # install dos2unix
            //sudo dos2unix install-cosmos-certificate.sh # convert script 
            //sudo bash install-cosmos-certificate.sh # get certificate
            var cosmosClient = new CosmosClient(_configuration["Dependencies:CosmosDb:Uri"],
                _configuration["Dependencies:CosmosDb:PrimaryKey"]);
            var database = await cosmosClient.CreateDatabaseIfNotExistsAsync("database");
            //Type is partition key
            var container = await database.Database.CreateContainerIfNotExistsAsync("container", "/Type");
            var doc = new Document
            {
                Id = Guid.NewGuid().ToString(),
                Type = "type",
                Text = "text"
            };
            await container.Container.CreateItemAsync(doc);

            var sqlQueryText = "SELECT * FROM c";
            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = container.Container.GetItemQueryIterator<Document>(queryDefinition);
            var docs = new List<Document>();
            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                docs.AddRange(currentResultSet);
            }

            await database.Database.DeleteAsync();

            return Ok(docs);
        }
    }
}
