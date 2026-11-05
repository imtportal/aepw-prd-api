using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.Sharepoint
{
    public class SharePointSettings
    {
        public string TenantId { get; set; } = string.Empty;
        
        public string ClientId { get; set; } = string.Empty;

        public string ClientSecret { get; set; } = string.Empty;

        //public string SiteUrl { get; set; } = string.Empty;
        //public string SiteId { get; set; } = string.Empty;
        public string DocumentLibrary { get; set; } = string.Empty;

        public string FolderPath { get; set; } = string.Empty;
    }
}
