using Cache_Implementation_Task4.DTOs;
using Cache_Implementation_Task4.Requests;
using Cache_Implementation_Task4.Services;
using Cache_Implementation_Task4.Helper;
using Microsoft.Extensions.DependencyInjection;

var service = new ServiceCollection();
service.AddHttpClient();
service.AddTransient<fakestoreClient>();

var serviceProvider = service.BuildServiceProvider();

var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();


var cacheCapacity = 5;
var client = new fakestoreClient(factory);
var cache = new InMemoryCache<int, Post>(cacheCapacity);
var postService = new PostRequest(client, cache);

var postIdsToFetch = new[] { 
    1, 2, 1, 3, 2, 4, 1, 6 , 9, 6 ,6, 4, 3, 2, 1,
    5, 20, 6 ,8 ,7, 6, 9, 6, 8, 7, 4, 3,2,1,5,20 ,6, 9, 6, 8, 7, 4, 3,2,1,5
};

Console.WriteLine($"Cache capacity: {cacheCapacity} (LRU eviction)");
Console.WriteLine("Sequence: " + string.Join(", ", postIdsToFetch));
Console.WriteLine();

foreach (var id in postIdsToFetch)
{
    var (post, fromCache) = await postService.GetPostAsync(id, CancellationToken.None);
    var source = fromCache ? "cache hit" : "fetched";
    Console.WriteLine($"[{source}] id={post.id}, title={Shorten.Value(post.title)}");
}

Console.WriteLine();
Console.WriteLine($"Cache usage after run: {cache.Count}/{cache.Capacity} entries.");