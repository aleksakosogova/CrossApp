using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Reports;

public static class QueryExtensions
{
    public static IEnumerable<T> TopBy<T, TKey>(
        this IEnumerable<T> source, 
        Func<T, TKey> keySelector, 
        int n) =>
        source.OrderByDescending(keySelector).Take(n);
}