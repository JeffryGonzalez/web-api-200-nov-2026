using Alba;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Software.Api.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Software.Tests.Status;

public class GettingTheStatus(StatusContext ctx) :IClassFixture<StatusContext>
{
    [Fact]
    public async Task CanGetTheStatus()
    {
        var response =await ctx.Host.Scenario(api =>
        {
            api.Get.Url("/status");
            api.StatusCodeShouldBeOk();
          
        });

        var content = await response.ReadAsJsonAsync<StatusResponseMessage>();
        Assert.NotNull(content);
        Assert.Equal(42, content.NumberOfVendors);
    }
}

public class GettingTheStatusError(StatusContextError ctx) : IClassFixture<StatusContextError>
{
    [Fact]
    public async Task CanGetTheStatus()
    {
        var response = await ctx.Host.Scenario(api =>
        {
            api.Get.Url("/status");
            api.StatusCodeShouldBeOk();
        });
        var content = await response.ReadAsJsonAsync<StatusResponseMessage>();
        Assert.NotNull(content);
        Assert.Equal(-1, content.NumberOfVendors);
    }
}

public class StatusContext : IAsyncLifetime
{
    public IAlbaHost Host { get; set; } = null!;
    private WireMockServer _server = WireMockServer.Start();

    private PostgreSqlContainer _pgContainer = null!;

    public async ValueTask InitializeAsync()
    {
        _pgContainer = new PostgreSqlBuilder("postgres:17").Build();
        await _pgContainer.StartAsync();
        _server.Given(Request.Create().WithPath("/clerk-records/purchasing/vendors").UsingGet().WithParam("status", "approved"))
            .RespondWith(
            Response.Create()
            .WithStatusCode(200)
            .WithBodyAsJson(new { total = 42 }));

        Host = await AlbaHost.For<Program>(h =>
        {
            h.UseSetting("services:purchasing-vendors:http:0", _server.Urls[0]);
            h.UseSetting("ConnectionStrings:software", _pgContainer.GetConnectionString());

        });
    }
    public ValueTask DisposeAsync()
    {
        return _pgContainer.DisposeAsync();
    }
}

public class StatusContextError : IAsyncLifetime
{
    public IAlbaHost Host { get; set; } = null!;
    private WireMockServer _server = WireMockServer.Start();

    private PostgreSqlContainer _pgContainer = null!;

    public async ValueTask InitializeAsync()
    {
        _pgContainer = new PostgreSqlBuilder("postgres:17").Build();
        await _pgContainer.StartAsync();
        _server.Given(Request.Create().WithPath("/clerk-records/purchasing/vendors").UsingGet().WithParam("status", "approved"))
            .RespondWith(
            Response.Create()
            .WithStatusCode(500)
            );

        Host = await AlbaHost.For<Program>(h =>
        {
            h.UseSetting("services:purchasing-vendors:http:0", _server.Urls[0]);
            h.UseSetting("ConnectionStrings:software", _pgContainer.GetConnectionString());

        });
    }
    public ValueTask DisposeAsync()
    {
        return _pgContainer.DisposeAsync();
    }
}