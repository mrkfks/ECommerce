namespace Dashboard.Web.Models
{
    public class GlobalAttributeCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "text";
        public bool IsRequired { get; set; }
        public bool IsFilterable { get; set; }
        public List<GlobalAttributeValueDto> Values { get; set; } = new();
    }
}
