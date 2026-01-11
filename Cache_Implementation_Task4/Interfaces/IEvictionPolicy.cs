using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache_Implementation_Task4.Interfaces;
public interface IEvictionPolicy<TKey>
{
    void RecordAdd(TKey key);
    void RecordAccess(TKey key);
    void RecordRemoval(TKey key);
    bool TryEvict(out TKey key);
}
