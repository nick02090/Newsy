using Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Context;

namespace WebAPI.Tests
{
    public class IntegrationTest
    {
        protected readonly HttpClient TestClient;

        protected IntegrationTest()
        {
            var appFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder => 
                {
                    builder.ConfigureServices(services =>
                    {
                        var context = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(NewsyContext));
                        if (context != null)
                        {
                            services.Remove(context);
                            var options = services.Where(r => (r.ServiceType == typeof(DbContextOptions))
                              || (r.ServiceType.IsGenericType && r.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))).ToArray();
                            foreach (var option in options)
                            {
                                services.Remove(option);
                            }
                        }
                        services.AddDbContext<NewsyContext>(options => 
                        {
                            options.UseInMemoryDatabase("NewsyTest");
                        });
                    });
                });
            TestClient = appFactory.CreateClient();
            TestClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected async Task AuthenticateAsync()
        {
            TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", await GetJwtAsync());
        }

        protected User TestUser { get; private set; }

        private async Task<string> GetJwtAsync()
        {
            // Register a test user
            var registerRequest = new Dictionary<string, string>
            {
                { "firstName", "Testy" },
                { "lastName", "Test" },
                { "email", "test@integration.com" },
                { "password", "TestyTest123!" }
            };
            var registerRequestJson = JsonConvert.SerializeObject(registerRequest, Formatting.Indented);
            var registerRequestString = new StringContent(registerRequestJson, Encoding.UTF8, "application/json");
            var registerResponse = await TestClient.PostAsync("api/users", registerRequestString);
            var registerResponseString = await registerResponse.Content.ReadAsStringAsync();
            TestUser = JsonConvert.DeserializeObject<User>(registerResponseString);

            // Authenticate this user (to get the token)
            var authenticateRequest = new Dictionary<string, string>
            {
                { "email", "test@integration.com" },
                { "password", "TestyTest123!" }
            };
            var authenticateRequestJson = JsonConvert.SerializeObject(authenticateRequest, Formatting.Indented);
            var authenticateRequestString = new StringContent(authenticateRequestJson, Encoding.UTF8, "application/json");
            var authenticateResponse = await TestClient.PostAsync("api/users/authenticate", authenticateRequestString);
            var authenticateResponseString = await authenticateResponse.Content.ReadAsStringAsync();
            dynamic authenticateResponseDynamic = JsonConvert.DeserializeObject<dynamic>(authenticateResponseString);
            return authenticateResponseDynamic.token;
        }
    }
}
