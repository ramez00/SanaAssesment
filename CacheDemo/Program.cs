using CacheDemo.Cache;
using CacheDemo.Models;
using CacheDemo.Services;

var cacheCapacity = 3;
var client = new JsonPlaceholderClient();
var cache = new InMemoryCache<int, Post>(cacheCapacity);
var postService = new PostService(client, cache);

var postIdsToFetch = new[] { 1, 2, 1, 3, 2, 4, 1 };

Console.WriteLine($"Cache capacity: {cacheCapacity} (LRU eviction)");
Console.WriteLine("Sequence: " + string.Join(", ", postIdsToFetch));
Console.WriteLine();

foreach (var id in postIdsToFetch)
{
    var (post, fromCache) = await postService.GetPostAsync(id);
    var source = fromCache ? "cache hit" : "fetched";
    Console.WriteLine($"[{source}] id={post.Id}, title={Shorten(post.Title)}");
}

Console.WriteLine();
Console.WriteLine($"Cache usage after run: {cache.Count}/{cache.Capacity} entries.");

string Shorten(string value, int maxLength = 40)
{
    if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength) return value;
    return value[..maxLength] + "...";
}
