using Cache_Implementation_Task4.DTOs;
using Cache_Implementation_Task4.Requests;
using Cache_Implementation_Task4.Services;

var cacheCapacity = 3;
var client = new JsonPlaceholderClient();
var cache = new InMemoryCache<int, Post>(cacheCapacity);
var postService = new PostRequest(client, cache);

var postIdsToFetch = new[] { 1, 2, 1, 3, 2, 4, 1, 6 , 9, 6 ,6, 4, 3, 2, 1, 5, 20, 6 ,8 ,7, 6, 9, 6, 8, 7, 4, 3,2,1,5,20 };

Console.WriteLine($"Cache capacity: {cacheCapacity} (LRU eviction)");
Console.WriteLine("Sequence: " + string.Join(", ", postIdsToFetch));
Console.WriteLine();

foreach (var id in postIdsToFetch)
{
    var (post, fromCache) = await postService.GetPostAsync(id, CancellationToken.None);
    var source = fromCache ? "cache hit" : "fetched";
    Console.WriteLine($"[{source}] id={post.id}, title={Shorten(post.title)}");
}

Console.WriteLine();
Console.WriteLine($"Cache usage after run: {cache.Count}/{cache.Capacity} entries.");

string Shorten(string value, int maxLength = 40)
{
    if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength) return value;
    return value[..maxLength] + "...";
}
