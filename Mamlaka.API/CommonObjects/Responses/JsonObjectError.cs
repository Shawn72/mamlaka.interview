using Newtonsoft.Json;

namespace Mamlaka.API.CommonObjects.Responses;
public class JsonObjectError
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }
}
