using DataServiceAbstraction_Task1.Services;
using DataServiceAbstraction_Task1.Interfaces;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Starting Application...");

var serviceProvider = new ServiceCollection()
    .AddMemoryCache()
    .AddSingleton<ILogger, LogService>()
    .AddSingleton<ICacheService, CacheService>()
    .AddSingleton<IDataService, DataService>()
    .BuildServiceProvider();

var dataService = serviceProvider.GetRequiredService<IDataService>();

try
{
    var myData = dataService.GetData();

    foreach (var line in myData)
        Console.WriteLine(line);
}
catch (Exception ex)
{
    Console.WriteLine($"Main Program caught an error: {ex.Message}");
}