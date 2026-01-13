
using Cache_Implementation_Task4.DTOs;
using System.Text.Json;

namespace Cache_Implementation_Task4.Requests;
public class fakestoreClient
{
    private readonly HttpClient _httpClient;

    public fakestoreClient(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient
        {
            BaseAddress = new Uri("https://fakestoreapi.com/")
        };
    }

    public async Task<Post> GetPostAsync(int postId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"products/{postId}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<Post>(content) ?? throw new InvalidOperationException("Failed to deserialize response to Post.");
    }
}
