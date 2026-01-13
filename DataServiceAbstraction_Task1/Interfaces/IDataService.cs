using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServiceAbstraction_Task1.Interfaces;
public interface IDataService
{
    IEnumerable<string> GetDataAsync();
}