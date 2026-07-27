//-----------------------------------------------------------------------------
// <copyright file="DurationFunctionsDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DurationFunctions;

public class DurationFunctionsDataSource
{
    private static IList<DurationCustomer> customers;

    // totalseconds(Duration)             => 1.5, 30, 3600, 9000, 0
    // totalseconds(NullableDuration)     => 30, null, 1.5, null, 9000
    // totalseconds(End sub Start)        => 30, 3600, 7200, 86400, -60
    // totalseconds(NullableEnd sub Start) => 30, null, 7200, null, -60
    static DurationFunctionsDataSource()
    {
        customers = new List<DurationCustomer>
        {
            new DurationCustomer
            {
                Id = 1,
                Name = "Alpha",
                Duration = TimeSpan.FromSeconds(1.5),
                NullableDuration = new TimeSpan(0, 0, 30),
                Start = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2023, 1, 1, 0, 0, 30, TimeSpan.Zero),
                NullableEnd = new DateTimeOffset(2023, 1, 1, 0, 0, 30, TimeSpan.Zero)
            },
            new DurationCustomer
            {
                Id = 2,
                Name = "Beta",
                Duration = new TimeSpan(0, 0, 30),
                NullableDuration = null,
                Start = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2023, 1, 1, 1, 0, 0, TimeSpan.Zero),
                NullableEnd = null
            },
            new DurationCustomer
            {
                Id = 3,
                Name = "Gamma",
                Duration = new TimeSpan(1, 0, 0),
                NullableDuration = TimeSpan.FromSeconds(1.5),
                Start = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2023, 1, 1, 2, 0, 0, TimeSpan.Zero),
                NullableEnd = new DateTimeOffset(2023, 1, 1, 2, 0, 0, TimeSpan.Zero)
            },
            new DurationCustomer
            {
                Id = 4,
                Name = "Delta",
                Duration = new TimeSpan(2, 30, 0),
                NullableDuration = null,
                Start = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2023, 1, 2, 0, 0, 0, TimeSpan.Zero),
                NullableEnd = null
            },
            new DurationCustomer
            {
                Id = 5,
                Name = "Epsilon",
                Duration = TimeSpan.Zero,
                NullableDuration = new TimeSpan(2, 30, 0),
                // End is before Start on purpose: totalseconds(End sub Start) is negative.
                Start = new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2023, 1, 1, 11, 59, 0, TimeSpan.Zero),
                NullableEnd = new DateTimeOffset(2023, 1, 1, 11, 59, 0, TimeSpan.Zero)
            }
        };
    }

    public static IList<DurationCustomer> Customers => customers;
}
