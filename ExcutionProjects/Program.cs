using DataServiceAbstraction_Task1.Services;
using DataServiceAbstraction_Task1.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Cache_Implementation_Task4.DTOs;
using Cache_Implementation_Task4.Requests;
using Cache_Implementation_Task4.Services;
using Cache_Implementation_Task4.Helper;
using DataServiceAbstraction_Task1.Constants;

Console.WriteLine("Starting Application...");

var serviceProvider = new ServiceCollection()
    .AddMemoryCache()
    .AddSingleton<ILogger, LogService>()
    .AddSingleton<ICacheService, CacheService>()
    .AddTransient<IDataService, DataService>()
    .BuildServiceProvider();

var dataService = serviceProvider.GetRequiredService<IDataService>();

try
{
    Console.WriteLine("First Call Fetching Data...");

    var myData = dataService.GetData();

    foreach (var line in myData)
        Console.WriteLine(line);


    Console.WriteLine("\n Second Call Fetching Data...");

    var cachedData = dataService.GetData();
    
    foreach (var line in cachedData)
        Console.WriteLine(line);


    Console.WriteLine("-----------------------------------------------------------------------");
    Console.WriteLine(" ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  First Task Finished Successfully.");


    var ICacheService = serviceProvider.GetRequiredService<ICacheService>();
    ICacheService.Delete(ContsantsVariables.DataCachedKey);



    Console.WriteLine("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Starting Second Task...");

    var serviceProvider2 = new DataService(
                    serviceProvider.GetRequiredService<ICacheService>(),
                    serviceProvider.GetRequiredService<ILogger>(),
                    ContsantsVariables.SQLScriptFilePath
        );

    var SqlScript = serviceProvider2.GetData();

    Console.WriteLine("Show SQL Script...");

    foreach (var line in SqlScript)
        Console.WriteLine(line);

    Console.WriteLine("-----------------------------------------------------------------------");
    Console.WriteLine(" ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  Second Task Finished Successfully.");



    Console.WriteLine("-----------------------------------------------------------------------");
    Console.WriteLine("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Starting Third Task...");

    var cacheCapacity = 7;
    var client = new fakestoreClient();
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

    Console.WriteLine("-----------------------------------------------------------------------");
    Console.WriteLine(" ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  Third Task Finished Successfully.");


}
catch (Exception ex)
{
    Console.WriteLine($"Main Program caught an error: {ex.Message}");
}