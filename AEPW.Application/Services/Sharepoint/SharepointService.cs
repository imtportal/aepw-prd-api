using AEPW.Application.DTOs.Review;
using AEPW.Application.Interfaces.Services.SharepointService;
using AEPW.Domain.Models.Sharepoint;
using Azure.Core;
using Azure.Identity;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ExternalConnectors;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime;
using System.Text;

namespace AEPW.Application.Services.Sharepoint
{
    public class SharepointService: ISharepointService
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly SharePointSettings _settings;
        private readonly IConfiguration _configuration;

        public SharepointService(IOptions<SharePointSettings> options, IConfiguration configuration)
        {
            try
            {
                _settings = options.Value;

                var credential = new ClientSecretCredential(
                    _settings.TenantId,
                    _settings.ClientId,
                    _settings.ClientSecret
                );

                _configuration = configuration;

                //Console.WriteLine("_settings.TenantId = " + _settings.TenantId);
                //Console.WriteLine("_settings.ClientId = " + _settings.ClientId); 
                //Console.WriteLine("_settings.ClientSecret = " + _settings.ClientSecret); 

                _graphServiceClient = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });
  
            }

            catch(Exception ex)
            {

            }
        }

        private async Task<DriveItem> CreateFolderIfNotExistsAsync(
        string driveId,
        string parentPath,
        string folderName)
        {
            //string fullPath = string.IsNullOrEmpty(parentPath)
            //    ? $"{_configuration["SharepointReviewRootDirectoryUrl"] ?? "https://amicustechnologydev.sharepoint.com/sites/JayDevSite/aepwimtevidencelib"}/{folderName}"
            //    : $"{_configuration["SharepointReviewRootDirectoryUrl"] ?? "https://amicustechnologydev.sharepoint.com/sites/JayDevSite/aepwimtevidencelib"}/{parentPath}/{folderName}";
            
            string fullPath = string.IsNullOrEmpty(parentPath)
                ? $"{folderName}"
                //: $"{_configuration["SharepointReviewRootDirectoryUrl"] ?? "https://amicustechnologydev.sharepoint.com/sites/JayDevSite/aepwimtevidencelib"}/{parentPath}/{folderName}";
                : $"{parentPath}/{folderName}";

            try
            {
                return await _graphServiceClient
                    .Drives[driveId]
                    .Root
                    .ItemWithPath(fullPath)
                    .GetAsync();
            }
            catch (Exception e)
            {
                var newFolder = new DriveItem
                {
                    Name = folderName,
                    Folder = new Folder()
                };

                // Root folder creation
                if (string.IsNullOrEmpty(parentPath))
                {
                    var root = await _graphServiceClient
                        .Drives[driveId]
                        .Root
                        .GetAsync();

                    return await _graphServiceClient
                        .Drives[driveId]
                        .Items[root.Id]
                        .Children
                        .PostAsync(newFolder);
                }

                var parentFolder = await _graphServiceClient
                    .Drives[driveId]
                    .Root
                    .ItemWithPath(parentPath)
                    .GetAsync();

                return await _graphServiceClient
                    .Drives[driveId]
                    .Items[parentFolder.Id]
                    .Children
                    .PostAsync(newFolder);
            }
        }

        private async Task<DriveItem> CreateFolderAsync(
        string driveId,
        string parentPath,
        string folderName)
            {
            var newFolder = new DriveItem
            {
                Name = folderName,
                Folder = new Folder()
            };

            // Create under root
            if (string.IsNullOrEmpty(parentPath))
            {
                var root = await _graphServiceClient
                    .Drives[driveId]
                    .Root
                    .GetAsync();

                return await _graphServiceClient
                    .Drives[driveId]
                    .Items[root.Id]
                    .Children
                    .PostAsync(newFolder);
            }

            // Create under an existing parent folder
            var parentFolder = await _graphServiceClient
                .Drives[driveId]
                .Root
                .ItemWithPath(parentPath)
                .GetAsync();

            DriveItem? result = await _graphServiceClient
                .Drives[driveId]
                .Items[parentFolder.Id]
                .Children
                .PostAsync(newFolder);

            return result;
        }

        public async Task<string> CreateDataCollectionFolderInSharepoint(SharepointHierarchyFolderCreationDto project)
        {
            var documentLibrary = await GetDocumentLibraryAsync();
            string quarterFolderUrl;

            // 1. Ensure Project folder exists
            await CreateFolderIfNotExistsAsync(
                documentLibrary.Id,
                string.Empty,
                project.ProjectPin);

            // 2. Ensure Quarter folder exists
            string quarterPath = project.ProjectPin;

            DriveItem quarterFolder = await CreateFolderIfNotExistsAsync(
                documentLibrary.Id,
                quarterPath,
                project.Quarter);

            quarterFolderUrl = quarterFolder.WebUrl;

            return quarterFolderUrl;

        }
        public async Task<string> CreateLaunchFoldersInSharepoint(SharepointHierarchyFolderCreationDto project, string recordType)
        {
            var documentLibrary = await GetDocumentLibraryAsync();
            string quarterFolderUrl;

            // 1. Ensure Project folder exists
            await CreateFolderIfNotExistsAsync(
                documentLibrary.Id,
                string.Empty,
                project.ProjectPin);

            // 2. Ensure Quarter folder exists
            string quarterPath = project.ProjectPin;

            DriveItem quarterFolder = await CreateFolderIfNotExistsAsync(
                documentLibrary.Id,
                quarterPath,
                project.Quarter);

            quarterFolderUrl = quarterFolder.WebUrl;

            //string topicParentPath = $"{_configuration["SharepointReviewRootDirectoryUrl"] ?? "https://amicustechnologydev.sharepoint.com/sites/JayDevSite/aepwimtevidencelib"}/{project.ProjectPin}/{project.Quarter}";
            string topicParentPath = recordType.ToLower() == "corporate" ? $"{project.Title}/{project.Quarter}" : $"{project.ProjectPin}/{project.Quarter}";

            // 3. Process Topics
            foreach (var topic in project.Topics)
            {
                // Ensure Topic folder exists
                await CreateFolderIfNotExistsAsync(
                    documentLibrary.Id,
                    topicParentPath,
                    topic.TopicName);

                string metricParentPath =
                    $"{topicParentPath}/{topic.TopicName}";

                
                foreach (var k in project.L1MetricCodes)
                {
                    string parentMetricCode = k.Key;

                    SharepointHierarchyFolderCreationL1TopTopic parentMetric = k.Value;

                    if (parentMetric.CategoryCode != topic.TopicName)
                    {
                        continue;
                    }

                    await CreateFolderIfNotExistsAsync(
                                documentLibrary.Id,
                                metricParentPath,
                                (parentMetricCode)
                            );


                    // L1 folder creation
                    foreach (L1MetricCodeEvidenceTypeCodeDto l1MetricCodeObj in parentMetric.L1MetricCodes)
                    {
                        await CreateFolderIfNotExistsAsync(
                                    documentLibrary.Id,
                                    metricParentPath + "/" + parentMetricCode,
                                    (l1MetricCodeObj.MetricCode));
                       
                           //if (!String.IsNullOrEmpty(evCode))
                           //{
                           //     await CreateFolderIfNotExistsAsync(
                           //            documentLibrary.Id,
                           //            metricParentPath + "/" + parentMetricCode + "/" + l1MetricCodeObj.MetricCode,
                           //            (evCode));
                           // }
                           //else
                           // {
                            //}

                         

                    }
                }


            }

            // Evidence Type Folder creation
            foreach (var topic in project.Topics)
            {
                foreach (var k in project.L1MetricCodes)
                {
                    string parentMetricCode = k.Key;
                    SharepointHierarchyFolderCreationL1TopTopic parentMetric = k.Value;

                    if (parentMetric.CategoryCode != topic.TopicName)
                    {
                        continue;
                    }


                    foreach (L1MetricCodeEvidenceTypeCodeDto l1MetricCodeObj in parentMetric.L1MetricCodes)
                    {
                        string metricParentPath =
                         $"{topicParentPath}/{topic.TopicName}";

                        foreach (string evCode in l1MetricCodeObj.EvidenceTypeCodes)
                        {
                            if (!String.IsNullOrEmpty(evCode))
                            {
                                await CreateFolderIfNotExistsAsync(
                                       documentLibrary.Id,
                                       metricParentPath + "/" + parentMetricCode + "/" + l1MetricCodeObj.MetricCode,
                                       (evCode));
                            }
                        }
                    }

                }

            }

                // 4. Process Metric Code folders
                //foreach (string metricCode in topic.TopLevelMetricCodes)
                //{
                //    await CreateFolderIfNotExistsAsync(
                //        documentLibrary.Id,
                //        metricParentPath,
                //        metricCode);
                //}
            

            return quarterFolderUrl;
        }

        public async Task<DriveItem?> UploadFileAsync(Stream stream, string fileName, string folderPath)
        {

            var documentLibrary = await GetDocumentLibraryAsync();

            // //Open a readable stream 
            // using var stream = file.OpenReadStream();
            //It targets the specific document library(Drives[documentLibrary.Id]), navigates from its root folder(Root),
            //targets your specific target file path(ItemWithPath),
            //and uses an HTTP PUT request(.Content.PutAsync) to stream the file contents directly up to SharePoint.
            var uploadPath = $"{folderPath}/{fileName}";


            DriveItem? response = await _graphServiceClient
                .Drives[documentLibrary.Id]
                .Root
                .ItemWithPath(uploadPath)
                .Content
                .PutAsync(stream); 

            //delay here
            //await Task.Delay(5000);

            return response;
        }

        public async Task<bool> UpdateTemplateExcelForReview(string folderPath, IFormFile file)
        {
            var documentLibrary = await GetDocumentLibraryAsync();
            var site = await GetSiteAsync();

            string fileName = String.Empty;
            string quarterFolderUrl;
            string driveId = documentLibrary.Id; 

            DriveItemCollectionResponse? children = await _graphServiceClient
                .Drives[driveId]
                .Root
                .ItemWithPath(folderPath)
                .Children
                .GetAsync();
            
            try
            {
                if (children != null) {
                    foreach (var child in children.Value)
                    {
                        if(String.IsNullOrEmpty(fileName))
                        {
                            fileName = child.Name;
                            break;
                        }

                        // await DeleteFileAsync(child.Id);
                    }
                }

                Stream stream = file.OpenReadStream();
                await UploadFileAsync(stream, fileName, folderPath);

            }
            catch
            {
                return false;
            }

            return true;

        }

        public async Task<bool> DeleteFileAsync(string itemId)
        {

            var documentLibrary = await GetDocumentLibraryAsync();

            // //Open a readable stream 
            // using var stream = file.OpenReadStream();
            //It targets the specific document library(Drives[documentLibrary.Id]), navigates from its root folder(Root),
            //targets your specific target file path(ItemWithPath),
            //and uses an HTTP PUT request(.Content.PutAsync) to stream the file contents directly up to SharePoint.
            //var uploadPath = $"{folderPath}/{fileName}.xlsx";


            try
            {
                await _graphServiceClient
                .Drives[documentLibrary.Id]
                .Items[itemId]
                .DeleteAsync();
 
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
             
        }

        public async Task<bool> DeleteFoldersAsync (List<string> folderNames)
        {

            var documentLibrary = await GetDocumentLibraryAsync();

            // //Open a readable stream 
            // using var stream = file.OpenReadStream();
            //It targets the specific document library(Drives[documentLibrary.Id]), navigates from its root folder(Root),
            //targets your specific target file path(ItemWithPath),
            //and uses an HTTP PUT request(.Content.PutAsync) to stream the file contents directly up to SharePoint.
            //var uploadPath = $"{folderPath}/{fileName}.xlsx";

            try
            {
                foreach(string folderName in folderNames)
                {
                    await _graphServiceClient
                    .Drives[documentLibrary.Id]
                    .Root
                    .ItemWithPath(folderName)
                    .DeleteAsync()
                    ;

                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public async Task<string> DownloadFileAsync(string fileName, string accessToken)
        { 
            var documentLibrary = await GetDocumentLibraryAsync();
            //var downloadPath = $"{folderPath}/{fileName}";
            var httpClient = new HttpClient();

            var app = ConfidentialClientApplicationBuilder
                .Create("") // client id
                .WithClientSecret("") // client secret
                .WithAuthority($"") // tenantid
                .Build();


            var result = await app.AcquireTokenForClient(
            new[] { "https://amicustechnologydev.sharepoint.com/.default" })
            .ExecuteAsync();

            accessToken = result.AccessToken;

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
      
            string sharePointUrl = fileName;

            var encodedUrl = Convert.ToBase64String(

            System.Text.Encoding.UTF8.GetBytes(sharePointUrl))

            .TrimEnd('=')

            .Replace('/', '_')

            .Replace('+', '-');

            // Get the file stream from Graph
            Stream stream = await _graphServiceClient
            .Shares[sharePointUrl]
            .DriveItem
            //.Root
            //.WithUrl(fileName)
            .Content
            //.Request()
            .GetAsync();

            // Convert to Base64 string (easy to send back to UI)
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                byte[] fileBytes
                    = ms.ToArray();
                    //= await httpClient.GetByteArrayAsync(sharePointUrl);
                return Convert.ToBase64String(fileBytes);
            }
        }

        private async Task<Site?> GetSiteAsync()
        {
            //var hostname = "amicustechnologydev.sharepoint.com";
            var hostname = _configuration["SharePointSettings:HostName"];

            var relativePath = "/sites/" + _configuration["SharePointSettings:RelativePath"];
            //var relativePath = "/sites" + / JayDevSite";
            //var relativePath = "/sites/fcd5145-8e02-4979-aeef-796304a431f0";
            string finalSitePath = $"{hostname}:{relativePath}";
            //string finalSitePath = $"{hostname}{relativePath}";


            var site = await _graphServiceClient
                .Sites[$"{finalSitePath}"]
                .GetAsync();
            
            if (site == null)
            {
                throw new Exception("SharePoint site not found.");
            }

            return site;
        }

        private async Task<Drive> GetDocumentLibraryAsync()
        {
            var site = await GetSiteAsync();

            //var site = await _graphServiceClient
            //    .Sites["fcd5145-8e02-4979-aeef-796304a431f0"]
            //    .GetAsync();

            var drives = await _graphServiceClient
                .Sites[site.Id]
                .Drives
                .GetAsync();

            var documentLibrary = drives?.Value?
                .FirstOrDefault(d =>
                    d.Name != null &&
                    d.Name.Equals(
                        _settings.DocumentLibrary,
                        StringComparison.OrdinalIgnoreCase));

            if (documentLibrary == null)
            {
                throw new Exception("Document library not found.");
            }

            return documentLibrary;
        }
    }
}
