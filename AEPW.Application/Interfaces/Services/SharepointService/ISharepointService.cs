using AEPW.Application.DTOs.Review;
using AEPW.Domain.Models.Sharepoint;
using Microsoft.AspNetCore.Http;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.Interfaces.Services.SharepointService
{
    public interface ISharepointService
    {
        //Task<string> TestConnectionAsync();
        Task<DriveItem?> UploadFileAsync(Stream stream, string fileName, string folderPath);
        Task<string> DownloadFileAsync(string fileName, string folderPath);
        Task<string> CreateLaunchFoldersInSharepoint(SharepointHierarchyFolderCreationDto topics, string recordType);
        Task<string> CreateDataCollectionFolderInSharepoint(SharepointHierarchyFolderCreationDto topics);
        Task<bool> DeleteFileAsync(string itemId);
        Task<bool> DeleteFoldersAsync(List<string> folderNames);
        Task<bool> UpdateTemplateExcelForReview(string folderPath, IFormFile file);
        //Task<List<SharePointFileModel>> GetFilesAsync();

        //Task<(Stream Stream, string ContentType)> DownloadFileAsync(string fileName);

        //Task DeleteFileAsync(string fileName);
    }
}
