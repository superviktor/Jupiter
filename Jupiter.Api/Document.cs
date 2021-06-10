using Newtonsoft.Json;

namespace Jupiter.Api
{
    public class Document
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
    }
}
