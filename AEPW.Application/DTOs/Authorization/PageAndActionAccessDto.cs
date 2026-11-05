namespace AEPW.Application.DTOs.Authorization
{

    public class PageAccessDto
    {
        public string Key { get; set; } = string.Empty;
        public bool Value { get; set; }
    }

    public class ActionAccessDto
    {
        public string Key { get; set; } = string.Empty;
        public bool Value { get; set; }
    }
}
