//-----------------------------------------------------------------------------
// <copyright file="DurationFunctionsEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DurationFunctions;

public class DurationFunctionsEdmModel
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<DurationCustomer>("DurationCustomers");

        // Same CLR type served by controllers with different AllowedFunctions settings.
        builder.EntitySet<DurationCustomer>("RestrictedCustomers");
        builder.EntitySet<DurationCustomer>("AllowedCustomers");

        return builder.GetEdmModel();
    }
}
