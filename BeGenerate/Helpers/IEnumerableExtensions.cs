using System;
using System.Collections.Generic;

namespace BeGenerate.Helpers;

internal static class IEnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T>? enumerable, Action<T> action)
    {
        if (enumerable is null)
            return;

        foreach (var item in enumerable)
            action(item);
    }
}
