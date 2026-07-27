//-----------------------------------------------------------------------------
// <copyright file="DurationFunctionsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DurationFunctions;

public class DurationCustomersController : ODataController
{
    [EnableQuery]
    public ActionResult<IEnumerable<DurationCustomer>> Get()
    {
        return Ok(DurationFunctionsDataSource.Customers);
    }
}

/// <summary>
/// Every function is allowed but 'totalseconds'.
/// </summary>
public class RestrictedCustomersController : ODataController
{
    [EnableQuery(AllowedFunctions = AllowedFunctions.AllFunctions & ~AllowedFunctions.TotalSeconds)]
    public ActionResult<IEnumerable<DurationCustomer>> Get()
    {
        return Ok(DurationFunctionsDataSource.Customers);
    }
}

/// <summary>
/// Only the date/time functions are allowed, which is where 'totalseconds' belongs.
/// </summary>
public class AllowedCustomersController : ODataController
{
    [EnableQuery(AllowedFunctions = AllowedFunctions.AllDateTimeFunctions)]
    public ActionResult<IEnumerable<DurationCustomer>> Get()
    {
        return Ok(DurationFunctionsDataSource.Customers);
    }
}
