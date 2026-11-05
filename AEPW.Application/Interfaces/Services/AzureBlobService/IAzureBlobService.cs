using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.Interfaces.Services.AzureBlobService
{
    public interface IAzureBlobService
    {
        Task<Stream> GetFileAsync(string containerName, string fileName);

    }
}
