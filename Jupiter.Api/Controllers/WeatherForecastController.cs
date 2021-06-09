using System.IO;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
    }
}
