
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace SecureNotes.API.Tests.Security
{
    public class HstsTests
    {
        private static TestServer CreateServer(string environment)
        {
            var builder = new WebHostBuilder()
                .UseEnvironment(environment)
                .ConfigureServices(services =>
                {
                    services.AddHsts(options =>
                    {
                        options.MaxAge = TimeSpan.FromDays(30);
                        options.IncludeSubDomains = false;
                        options.Preload = false;
                    });
                })
                .Configure(app =>
                {
                    if (!string.Equals(
                        environment,
                        "Development",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        app.UseHsts();
                    }

                    app.Run(async context =>
                    {
                        context.Response.StatusCode = 200;
                        await context.Response.WriteAsync("HSTS test");
                    });
                });

            return new TestServer(builder);
        }

        [Fact]
        public async Task Production_Https_Should_Return_Hsts_Header()
        {
            using var server = CreateServer("Production");

            using var client = server.CreateClient();
            client.BaseAddress = new Uri("https://securenotes.example/");

            using var response = await client.GetAsync("/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.True(
                response.Headers.TryGetValues(
                    "Strict-Transport-Security",
                    out var values));

            Assert.Equal("max-age=2592000", Assert.Single(values));
        }

        [Fact]
        public async Task Development_Https_Should_Not_Return_Hsts_Header()
        {
            using var server = CreateServer("Development");

            using var client = server.CreateClient();
            client.BaseAddress = new Uri("https://securenotes.example/");

            using var response = await client.GetAsync("/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.False(
                response.Headers.Contains("Strict-Transport-Security"));
        }

        [Fact]
        public async Task Production_Hsts_Should_Not_Enable_Subdomains_Or_Preload()
        {
            using var server = CreateServer("Production");

            using var client = server.CreateClient();
            client.BaseAddress = new Uri("https://securenotes.example/");

            using var response = await client.GetAsync("/");

            Assert.True(
                response.Headers.TryGetValues(
                    "Strict-Transport-Security",
                    out var values));

            var hsts = Assert.Single(values);

            Assert.DoesNotContain(
                "includeSubDomains",
                hsts,
                StringComparison.OrdinalIgnoreCase);

            Assert.DoesNotContain(
                "preload",
                hsts,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
