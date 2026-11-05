using System.Collections.Generic;

namespace AEPW.Application.DTOs.User
{
    public sealed class BulkImportUserRequestDto
    {
        public List<BulkImportUserItemDto> Users { get; set; } = new();
        public string CreatedBy { get; set; } = string.Empty;
    }
}
