using AEPW.Application.Interfaces.Services.AzureBlobService;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace AEPW.Application.Services.AzureBlobService
{
    public sealed class AzureBlobService : IAzureBlobService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public AzureBlobService(IConfiguration configuration)
        {
            _configuration = configuration;
         }

        public async Task<Stream> GetFileAsync(
            string containerName,
            string fileName)
        {
            //string blobConnectionString = _configuration["AzureBlobStorage:ConnectionString"];
            string connectionString = _configuration["DataCollectionTemplate:ConnectionString"];

            var blobClient = new BlobClient(
                connectionString,
                containerName,
                fileName);

            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException(
                    $"Blob '{fileName}' not found in container '{containerName}'.");
            }

            return await blobClient.OpenReadAsync();
        }
    }
}