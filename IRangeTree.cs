﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MB.Algodat
{
    /// <summary>
    /// Range tree interface.
    /// </summary>
    /// <typeparam name="TKey">The type of the range.</typeparam>
    /// <typeparam name="TValue">The type of the data items.</typeparam>
    public interface IRangeTree<TKey, TValue>
        : IEnumerable<RangeValuePair<TKey, TValue>>
        , ICollection<RangeValuePair<TKey, TValue>>
    {
        IEnumerable<TValue> Values { get; }

        IEnumerable<RangeValuePair<TKey, TValue>> this[TKey value] { get; }
        IEnumerable<RangeValuePair<TKey, TValue>> this[TKey from, TKey to] { get; }

        void Add(TKey from, TKey to, TValue value);
        void Rebuild();
    }
}
