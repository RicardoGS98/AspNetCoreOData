//-----------------------------------------------------------------------------
// <copyright file="DurationFunctionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Commons;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DurationFunctions;

/// <summary>
/// End-to-end tests for the OData V4 canonical function 'totalseconds(Edm.Duration)'.
/// The data source is in-memory, so the LINQ provider is LINQ to Objects and the default
/// HandleNullPropagationOption resolves to True, which exercises the null propagation
/// branch of the binding.
/// </summary>
public class DurationFunctionsTests : WebApiTestBase<DurationFunctionsTests>
{
    public DurationFunctionsTests(WebApiTestFixture<DurationFunctionsTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        IEdmModel edmModel = DurationFunctionsEdmModel.GetEdmModel();

        services.ConfigureControllers(
            typeof(DurationCustomersController),
            typeof(RestrictedCustomersController),
            typeof(AllowedCustomersController),
            typeof(MetadataController));

        services.AddControllers().AddOData(opt =>
            opt.Count().Filter().OrderBy().Expand().SetMaxTop(null).Select()
               .AddRouteComponents("odata", edmModel));
    }

    #region $filter

    [Theory]
    [InlineData("$filter=totalseconds(Duration) gt 60", new[] { 3, 4 })]
    [InlineData("$filter=totalseconds(Duration) lt 60", new[] { 1, 2, 5 })]
    [InlineData("$filter=totalseconds(Duration) eq 3600", new[] { 3 })]
    [InlineData("$filter=totalseconds(Duration) ge 30 and totalseconds(Duration) le 3600", new[] { 2, 3 })]
    [InlineData("$filter=totalseconds(Duration) eq 0", new[] { 5 })]
    [InlineData("$filter=totalseconds(Duration) ne 0", new[] { 1, 2, 3, 4 })]
    [InlineData("$filter=3600 eq totalseconds(Duration)", new[] { 3 })]
    // The function returns Edm.Decimal, so the fractional part must survive.
    [InlineData("$filter=totalseconds(Duration) eq 1.5", new[] { 1 })]
    public async Task CanFilterByTotalSecondsOfDuration(string filter, int[] expectedIds)
    {
        // Arrange & Act
        JObject payload = await GetPayloadAsync($"odata/DurationCustomers?{filter}");

        // Assert
        Assert.Equal(expectedIds, GetIds(payload));
    }

    [Theory]
    [InlineData("$filter=totalseconds(NullableDuration) eq null", new[] { 2, 4 })]
    [InlineData("$filter=totalseconds(NullableDuration) ne null", new[] { 1, 3, 5 })]
    [InlineData("$filter=totalseconds(NullableDuration) gt 60", new[] { 5 })]
    [InlineData("$filter=totalseconds(NullableDuration) lt 60", new[] { 1, 3 })]
    [InlineData("$filter=totalseconds(NullableDuration) eq 1.5", new[] { 3 })]
    [InlineData("$filter=totalseconds(NullableDuration) ge 30", new[] { 1, 5 })]
    public async Task CanFilterByTotalSecondsOfNullableDuration(string filter, int[] expectedIds)
    {
        // Arrange & Act
        JObject payload = await GetPayloadAsync($"odata/DurationCustomers?{filter}");

        // Assert
        Assert.Equal(expectedIds, GetIds(payload));
    }

    [Theory]
    [InlineData("$filter=totalseconds(End sub Start) gt 3600", new[] { 3, 4 })]
    [InlineData("$filter=totalseconds(End sub Start) eq 3600", new[] { 2 })]
    // 'End' is before 'Start' for customer 5, so the duration is negative.
    [InlineData("$filter=totalseconds(End sub Start) lt 0", new[] { 5 })]
    // Lifted subtraction: the null check happens inside the argument of the function.
    [InlineData("$filter=totalseconds(NullableEnd sub Start) gt 3600", new[] { 3 })]
    [InlineData("$filter=totalseconds(NullableEnd sub Start) eq null", new[] { 2, 4 })]
    public async Task CanFilterByTotalSecondsOfDateTimeOffsetSubtraction(string filter, int[] expectedIds)
    {
        // Arrange & Act
        JObject payload = await GetPayloadAsync($"odata/DurationCustomers?{filter}");

        // Assert
        Assert.Equal(expectedIds, GetIds(payload));
    }

    #endregion

    #region $orderby

    [Theory]
    [InlineData("$orderby=totalseconds(Duration)", "5 > 1 > 2 > 3 > 4")]
    [InlineData("$orderby=totalseconds(Duration) desc", "4 > 3 > 2 > 1 > 5")]
    [InlineData("$orderby=totalseconds(NullableDuration)", "2 > 4 > 3 > 1 > 5")]
    [InlineData("$orderby=totalseconds(NullableDuration) desc", "5 > 1 > 3 > 2 > 4")]
    [InlineData("$orderby=totalseconds(End sub Start)", "5 > 1 > 2 > 3 > 4")]
    public async Task CanOrderByTotalSeconds(string orderby, string expected)
    {
        // Arrange & Act
        JObject payload = await GetPayloadAsync($"odata/DurationCustomers?{orderby}");

        // Assert
        Assert.Equal(expected, GetIdOrder(payload));
    }

    #endregion

    #region $compute

    [Theory]
    [InlineData("$compute=totalseconds(Duration) as DurationSeconds&$filter=DurationSeconds gt 60", new[] { 3, 4 })]
    [InlineData("$compute=totalseconds(Duration) as DurationSeconds&$filter=DurationSeconds eq 1.5", new[] { 1 })]
    [InlineData("$compute=totalseconds(NullableDuration) as NullableSeconds&$filter=NullableSeconds eq null", new[] { 2, 4 })]
    public async Task CanUseTotalSecondsInDollarCompute(string query, int[] expectedIds)
    {
        // Arrange & Act
        JObject payload = await GetPayloadAsync($"odata/DurationCustomers?{query}");

        // Assert
        Assert.Equal(expectedIds, GetIds(payload));
    }

    [Fact]
    public async Task CanOrderByAndSelectADollarComputeOfTotalSeconds()
    {
        // Arrange & Act
        JObject payload = await GetPayloadAsync(
            "odata/DurationCustomers?$compute=totalseconds(Duration) as DurationSeconds&$orderby=DurationSeconds desc&$select=Id,DurationSeconds");

        // Assert
        Assert.Equal(new[] { 4, 3, 2, 1, 5 }, GetIds(payload));
        Assert.Equal(new decimal?[] { 9000m, 3600m, 30m, 1.5m, 0m }, GetDecimals(payload, "DurationSeconds"));
    }

    [Fact]
    public async Task CanFilterAndSelectADollarComputeOfTotalSecondsOfSubtraction()
    {
        // Arrange & Act
        JObject payload = await GetPayloadAsync(
            "odata/DurationCustomers?$compute=totalseconds(End sub Start) as ElapsedSeconds&$filter=ElapsedSeconds ge 7200&$select=Id,ElapsedSeconds");

        // Assert
        Assert.Equal(new[] { 3, 4 }, GetIds(payload));
        Assert.Equal(new decimal?[] { 7200m, 86400m }, GetDecimals(payload, "ElapsedSeconds"));
    }

    [Fact]
    public async Task ADollarComputeOfTotalSecondsOfANullableDurationIsNull()
    {
        // Arrange & Act
        JObject payload = await GetPayloadAsync(
            "odata/DurationCustomers?$compute=totalseconds(NullableDuration) as NullableSeconds&$select=Id,NullableSeconds");

        // Assert
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, GetIds(payload));
        Assert.Equal(new decimal?[] { 30m, null, 1.5m, null, 9000m }, GetDecimals(payload, "NullableSeconds"));
    }

    [Fact]
    public async Task TotalSecondsIsSerializedAsADecimalAndNotTruncated()
    {
        // Arrange
        string queryUrl = "odata/DurationCustomers?$filter=Id eq 1&$select=DurationSeconds&$compute=totalseconds(Duration) as DurationSeconds";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"value\":[{\"DurationSeconds\":1.5}]}", await response.Content.ReadAsStringAsync());
    }

    #endregion

    #region AllowedFunctions validation

    [Theory]
    [InlineData("$filter=totalseconds(Duration) gt 60")]
    // $orderby is the only path that goes through QueryValidatorHelpers.
    [InlineData("$orderby=totalseconds(Duration)")]
    public async Task TotalSecondsIsRejectedWhenNotAllowed(string query)
    {
        // Arrange
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync($"odata/RestrictedCustomers?{query}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            "Function 'totalseconds' is not allowed. To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.",
            await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task OnlyTotalSecondsIsRejectedOnTheRestrictedEndpoint()
    {
        // Arrange & Act
        JObject payload = await GetPayloadAsync("odata/RestrictedCustomers?$filter=contains(Name,'lph')");

        // Assert
        Assert.Equal(new[] { 1 }, GetIds(payload));
    }

    [Fact]
    public async Task TotalSecondsIsAllowedByAllDateTimeFunctions()
    {
        // Arrange & Act
        JObject filtered = await GetPayloadAsync("odata/AllowedCustomers?$filter=totalseconds(Duration) gt 60");
        JObject ordered = await GetPayloadAsync("odata/AllowedCustomers?$orderby=totalseconds(Duration) desc");

        // Assert
        Assert.Equal(new[] { 3, 4 }, GetIds(filtered));
        Assert.Equal("4 > 3 > 2 > 1 > 5", GetIdOrder(ordered));
    }

    [Fact]
    public async Task StringFunctionsAreRejectedByAllDateTimeFunctions()
    {
        // Arrange
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("odata/AllowedCustomers?$filter=contains(Name,'lph')");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Function 'contains' is not allowed.", await response.Content.ReadAsStringAsync());
    }

    #endregion

    #region $metadata

    [Fact]
    public async Task DurationPropertiesAreMappedToEdmDuration()
    {
        // Arrange
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("odata/$metadata");
        var stream = await response.Content.ReadAsStreamAsync();
        IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
        var reader = new ODataMessageReader(message);
        IEdmModel edmModel = reader.ReadMetadataDocument();

        // Assert
        var customerType = edmModel.SchemaElements.OfType<IEdmEntityType>().Single(et => et.Name == "DurationCustomer");
        AssertHasProperty(customerType, "Duration", "Edm.Duration", isNullable: false);
        AssertHasProperty(customerType, "NullableDuration", "Edm.Duration", isNullable: true);
    }

    #endregion

    private async Task<JObject> GetPayloadAsync(string queryUrl)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
        HttpClient client = CreateClient();

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadAsObject<JObject>();
    }

    private static int[] GetIds(JObject payload)
    {
        return payload["value"].Select(item => (int)item["Id"]).ToArray();
    }

    private static decimal?[] GetDecimals(JObject payload, string propertyName)
    {
        return payload["value"].Select(item => item.Value<decimal?>(propertyName)).ToArray();
    }

    private static string GetIdOrder(JObject payload)
    {
        StringBuilder sb = new StringBuilder();
        foreach (int id in GetIds(payload))
        {
            sb.Append(id).Append(" > ");
        }

        sb.Remove(sb.Length - 3, 3); // remove the last " > "
        return sb.ToString();
    }

    private static void AssertHasProperty(IEdmEntityType entityType, string propertyName, string expectTypeName, bool isNullable)
    {
        Assert.NotNull(entityType);
        var property = entityType.DeclaredProperties.Single(p => p.Name == propertyName);
        Assert.Equal(expectTypeName, property.Type.Definition.FullTypeName());
        Assert.Equal(isNullable, property.Type.IsNullable);
    }
}
