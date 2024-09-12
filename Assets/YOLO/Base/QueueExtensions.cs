using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QueueExtensions
{
    public static Queue<T> ToQueue<T>(this IEnumerable<T> source)
    {
        return new Queue<T>(source);
    }
}
