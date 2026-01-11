using Cache_Implementation_Task4.DTOs;
using Cache_Implementation_Task4.Interfaces;

namespace Cache_Implementation_Task4.Requests;

public class PostRequest
{
    private readonly JsonPlaceholderClient _client;
    private readonly ICache<int,Post> _cache;

    public PostRequest(JsonPlaceholderClient client, ICache<int, Post> cache)
    {
        _client = client;
        _cache = cache;
    }

    public async Task<(Post post ,bool fromCache)> GetPostAsync(int id,CancellationToken cancellationToken)
    {
        if (_cache.TryGet(id, out var cached))
            return (cached, true);

        var post = await _client.GetPostAsync(id, cancellationToken);
        _cache.Set(id,post);
        return (post,false);
    }
}
