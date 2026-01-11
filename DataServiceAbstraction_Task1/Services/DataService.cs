using DataServiceAbstraction_Task1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DataServiceAbstraction_Task1.Services;
public class DataService : IDataService
{
    private readonly ICacheService _cacheService;
    private readonly string _filePath;
    private readonly ILogger _logger;
    private readonly string _dataCachedKey;
    private readonly TimeSpan _duration;

    public DataService(ICacheService cacheService,ILogger logger)
    {
        _filePath = Constants.ContsantsVariables.DataFilePath;
        _dataCachedKey = Constants.ContsantsVariables.DataCachedKey;
        _duration = TimeSpan.FromMinutes(Constants.ContsantsVariables.CahedDuration);
        _cacheService = cacheService;
        _logger = logger;
    }
    public IEnumerable<string> GetData()
    {
        try
        {
            _logger.LogInformation("Starting to Get Data.");

            if (_cacheService.Get(_dataCachedKey) is IEnumerable<string> cachedData)
            {
                _logger.LogInformation("Data retrieved from cache.");
                return cachedData;
            }

            if (!File.Exists(_filePath))
            {
                _logger.LogError($"File not found: {_filePath}");
                throw new FileNotFoundException($"File not found: {_filePath}");
            }

            _logger.LogInformation("Reading data from file.");

            using var reader = new StreamReader(_filePath);

            var data = File.ReadAllLines(_filePath);

            _logger.LogInformation($"Successfully retrieved => {data.Count()} lines.");

            _cacheService.Set(_dataCachedKey, data,_duration);
            _logger.LogInformation("Data cached successfully.");

            return data;
        }
        catch (Exception exp)
        {
            _logger.LogError("Error occurred while getting data: " , exp);
            throw;
        }
    }
}
