using System.Threading;
using System.Threading.Tasks;
using CacheDemo.Cache;
using CacheDemo.Models;

namespace CacheDemo.Services;

public class PostService
{
    private readonly JsonPlaceholderClient _client;
    private readonly ICache<int, Post> _cache;

    public PostService(JsonPlaceholderClient client, ICache<int, Post> cache)
    {
        _client = client;
        _cache = cache;
    }

    public async Task<(Post post, bool fromCache)> GetPostAsync(int id, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGet(id, out var cached))
        {
            return (cached, true);
        }

        var post = await _client.GetPostAsync(id, cancellationToken);
        _cache.Set(id, post);
        return (post, false);
    }
}

