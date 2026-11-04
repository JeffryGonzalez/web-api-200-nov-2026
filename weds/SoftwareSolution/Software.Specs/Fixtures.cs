using Alba;
using Alba.Security;
using Bobcat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Software.Api.Vendors;
using System.Security.Claims;
using Testcontainers.PostgreSql;
namespace Software.Specs;

[CollectionDefinition("VendorsSystemTests")]
public class VendorsSystemTestCollection : ICollectionFixture<VendorsFixture>;
public class VendorsFixture : IAsyncLifetime
{
    public IAlbaHost Host { get; set; } = null!;

    private PostgreSqlContainer _pgContainer = null!;

    public async ValueTask InitializeAsync()
    {
        _pgContainer = new PostgreSqlBuilder("postgres:17").Build();

        await _pgContainer.StartAsync();
        Host = await AlbaHost.For<Program>(config =>
        {
            var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(1969, 4, 20, 23, 59, 59, TimeSpan.FromHours(04)));


            var fakeUserService = Substitute.For<ILookupRequestingUsers>();
            fakeUserService.GetRequestingUserId().Returns("carl@netscape.com");
            config.UseSetting("ConnectionStrings:software", _pgContainer.GetConnectionString());
            config.ConfigureServices(sp =>
            {
                sp.AddScoped(s => fakeUserService);
                sp.AddSingleton<TimeProvider>(s => fakeTimeProvider);
            });
        }, new AuthenticationStub());
    }
    public async ValueTask DisposeAsync()
    {
        await Host.DisposeAsync();
        await _pgContainer.DisposeAsync();
    }
    private readonly IList<Claim> _claims = new List<Claim>();

    [Given("The user {userId} with roles {roles}")]
    internal Task GivenUser(string userId, params string[] roles)
    {
       _claims.Add(new Claim("sub", userId));
        foreach (var role in roles)
        {
            _claims.Add(new Claim(ClaimTypes.Role, role));
        }
        return Task.CompletedTask;
    }

    private  VendorCreateModel? _request;
    private VendorDetailsModel? _response;  
    [When("The {request} is posted")]
    public async  Task WhenVendorIsPosted(VendorCreateModel request)
    {
       var response = await Host.Scenario(api =>
       {
           foreach (var claim in _claims)
           {
               api.WithClaim(claim);
           }
           api.Post.Json(request).ToUrl("/vendors");
           api.StatusCodeShouldBe(201); // Created

       });

        var body = response.ReadAsJson<VendorDetailsModel>();
        _request = request;
        _response = body;
    }

    [Then("The response maps to the request")]
    public async Task ThenTheResponseMaps()
    {
        Assert.NotNull(_request);
        Assert.NotNull(_response);
        Assert.Equal(_request.Name  , _response.Name);
        Assert.Equal(_request.Url, _response.Url);
        Assert.Equal(_request.PointOfContact, _response!.PointOfContact);

    
    }
}