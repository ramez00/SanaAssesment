using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServiceAbstraction_Task1.Constants;
public static class ContsantsVariables
{
    public const string DataFilePath = @"D:\Sana Assesment\SanaAssesment\data.txt";  // it's not a good Way to add fixed Path

    public const string LogFilePath = @"D:\Sana Assesment\SanaAssesment\logs.txt";

    public const string DataCachedKey = "service_cached_key";

    public const int CahedDuration = 30;
}