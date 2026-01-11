using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServiceAbstraction_Task1.Interfaces;
public interface ICacheService
{
    void Set(string key, object value,TimeSpan duration);
    object Get(string key);
    void Delete(string key);
}
