
using Cache_Implementation_Task4.DTOs;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Cache_Implementation_Task4.Requests;
public class fakestoreClient
{
    private readonly HttpClient _httpClient;
    //Why this is bad:
    //    HttpClient does not close the TCP connection immediately
    //    Disposed sockets go into TIME_WAIT
    //    Under load → port exhaustion
    //    Error you’ll see:
    // System.Net.Http.HttpRequestException:
    // Only one usage of each socket address(protocol/network address/port) is normally permitted


    public fakestoreClient(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient();
    }
    public fakestoreClient()
    {
        _httpClient = new HttpClient();
    }

    public async Task<Post> GetPostAsync(int postId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"https://fakestoreapi.com/products/{postId}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<Post>(content) ?? throw new InvalidOperationException("Failed to deserialize response to Post.");
    }
}
