using System.Text.Json.Serialization;

namespace CacheDemo.Models;

public record Post(
    [property: JsonPropertyName("userId")] int UserId,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string Body);

