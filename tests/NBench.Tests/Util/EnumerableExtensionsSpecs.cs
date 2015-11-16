// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using NBench.Util;
using Xunit;

namespace NBench.Tests.Util
{
    public class EnumerableExtensionsSpecs
    {
        [Fact]
        public void ShouldZipEvenNumberedCollection()
        {
            var data = new[] {1L, 2L, 3L, 4L};
            var results = new[] {3L, 7L};
            Func<long,long,long> map = (a, b) => a + b;

            var zipped = data.Zip(map);
            Assert.True(results.SequenceEqual(zipped));
        }
    }
}

