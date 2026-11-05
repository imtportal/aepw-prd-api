 
namespace AEPW.Application.DTOs.Review
{
    public class ReviewUploadExcelJSON
    { 
        public long ReviewId { get; set; }
        public string ViewUri { get; set; }
        public string DownloadUri { get; set; }
        public string FileName { get; set; }
        public string ParseStatus { get; set; }
        public string ParseLogJson { get; set; }
        public string StagedValuesJson { get; set; } 
    }
}