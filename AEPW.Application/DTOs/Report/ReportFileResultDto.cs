namespace AEPW.Application.DTOs.Report
{
    public class ReportFileResultDto
    {
        public byte[] Excel { get; set; } = [];

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;
    }
}
