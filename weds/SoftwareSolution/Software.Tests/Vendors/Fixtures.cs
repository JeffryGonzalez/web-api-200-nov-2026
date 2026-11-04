

using Alba;
using Alba.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Software.Api.Vendors;
using Testcontainers.PostgreSql;

namespace Software.Tests.Vendors;

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

}
