using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using CacheDemo.Models;

namespace CacheDemo.Services;

public class JsonPlaceholderClient
{
    private readonly HttpClient _httpClient;

    public JsonPlaceholderClient(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient
        {
            BaseAddress = new Uri("https://jsonplaceholder.typicode.com/")
        };
    }

    public async Task<Post> GetPostAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"posts/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var post = await response.Content.ReadFromJsonAsync<Post>(cancellationToken: cancellationToken);
        return post ?? throw new InvalidOperationException("No content returned from the service.");
    }
}

