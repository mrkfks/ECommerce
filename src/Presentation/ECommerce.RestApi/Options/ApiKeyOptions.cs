using System.Collections.Generic;

namespace ECommerce.RestApi.Options
{
    public class ApiKeyOptions
    {
        public Dictionary<string, int> Keys { get; set; } = new();
    }
}
