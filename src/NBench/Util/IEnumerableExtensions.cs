// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
namespace NBench.Util
{
    /// <summary>
    /// Extension methods for <see cref="IEnumerable{T}"/> designed to make it easier to 
    /// perform math operations on .NET collections.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Zip consecutive pairs of elements in an even-numbered collection into a new map.
        /// </summary>
        /// <example>
        ///     var collection = new []{ 1,2,3,4};
        ///     var zipped = collection.Zip((a,b) => a+b).ToList();
        ///     Console.Writeline(zipped[0]); //prints 3
        ///     Console.Writeline(zipped[1]); //prints 7 
        /// </example>
        /// <typeparam name="T">The input datatype.</typeparam>
        /// <typeparam name="TResult">The output datatype.</typeparam>
        /// <param name="collection">The collection we're operating on. Must be even-numbered.</param>
        /// <param name="map">The mapping function for operating on pairs.</param>
        /// <returns>A new collection of items mapped list this.</returns>
        public static IEnumerable<TResult> Zip<T, TResult>(this IEnumerable<T> collection, Func<T, T, TResult> map)
        {
            Contract.Requires(collection != null);
            Contract.Requires(map != null);
            var enumerable = collection as T[] ?? collection.ToArray();
            var length = enumerable.Length;
            Contract.Assert(length % 2 == 0);

            var result = new List<TResult>(length / 2);

            for (var i = 0; i < length; i += 2)
            {
                result.Add(map(enumerable[i], enumerable[i+1]));
            }

            return result;
        }
    }
}

