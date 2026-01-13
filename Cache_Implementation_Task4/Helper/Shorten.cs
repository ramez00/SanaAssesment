using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache_Implementation_Task4.Helper;
public static class Shorten
{
    public static string Value(string value, int maxLength = 40)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
            return value;

        return value[..maxLength] + "...";
    }
}
