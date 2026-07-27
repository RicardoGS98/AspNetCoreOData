//-----------------------------------------------------------------------------
// <copyright file="DurationFunctionsDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DurationFunctions;

public class DurationCustomer
{
    public int Id { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// Non-nullable Edm.Duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Nullable Edm.Duration. Exercises the null propagation branch of the 'totalseconds' binding.
    /// </summary>
    public TimeSpan? NullableDuration { get; set; }

    public DateTimeOffset Start { get; set; }

    /// <summary>
    /// 'End sub Start' is typed as Edm.Duration by the URI parser.
    /// </summary>
    public DateTimeOffset End { get; set; }

    /// <summary>
    /// 'NullableEnd sub Start' produces a lifted subtraction, so the null check happens
    /// inside the argument of 'totalseconds'.
    /// </summary>
    public DateTimeOffset? NullableEnd { get; set; }
}
