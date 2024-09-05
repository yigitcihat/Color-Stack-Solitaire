using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class IListExtensions
{
    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = Random.Range(i, count);
            (ts[i], ts[r]) = (ts[r], ts[i]);
        }
    }

    public static List<T> Shuffled<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = Random.Range(i, count);
            (ts[i], ts[r]) = (ts[r], ts[i]);
        }

        return ts.ToList();
    }

    public static T Choice<T>(this IList<T> ts) => ts.Shuffled().First();
    public static List<T> Choices<T>(this IList<T> ts, int count) => ts.Shuffled().GetRange(0, count);

    public static T ChoiceAndPop<T>(this IList<T> ts)
    {
        var temp = ts.Shuffled().First();
        ts.Remove(temp);
        return temp;
    }

    public static List<T> ChoicesAndPop<T>(this IList<T> ts, int count)
    {
        var temp = ts.Shuffled().GetRange(0, count);
        temp.ForEach(c => ts.Remove(c));
        return temp;
    }

    public static T FirstAndPop<T>(this IList<T> ts)
    {
        var temp = ts.First();
        ts.Remove(temp);
        return temp;
    }
}