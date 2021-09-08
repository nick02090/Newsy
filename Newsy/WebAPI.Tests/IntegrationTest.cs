using Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
        #region Constants
        private const string _mediaType = "application/json";

        private const string _testUserEmail = "test@integration.com";
        private const string _testUserPassword = "TestyTest123!";
        #endregion

        public readonly HttpClient TestClient;
        public User TestUser { get; private set; }

        public IntegrationTest(string dbName)
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
                            options.UseInMemoryDatabase(dbName);
                        });
                    });
                });
            TestClient = appFactory.CreateClient();
            TestClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_mediaType));
        }

        public async Task AuthenticateAsync()
        {
            TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", await GetJwtAsync());
        }

        public void Authenticate(string token)
        {
            TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
        }

        private async Task<string> GetJwtAsync()
        {
            // Register a test user
            TestUser = await CreateUser(new User 
            {
                FirstName = "Testy",
                LastName = "Test",
                Email = _testUserEmail,
                Password = _testUserPassword
            });

            // Authenticate this user (to get the token)
            return await AuthenticateUser(new User
            {
                Email = _testUserEmail,
                Password = _testUserPassword
            });
        }

        #region User CRUD, services and other helpers
        public static StringContent SerializeUser(User user)
        {
            var userJson = JsonConvert.SerializeObject(user, Formatting.Indented);
            return new StringContent(userJson, Encoding.UTF8, _mediaType);
        }

        public async Task<User> CreateUser(User user)
        {
            var response = await TestClient.PostAsync("api/users", SerializeUser(user));
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<User>(content);
        }

        public async Task<string> AuthenticateUser(User user)
        {
            var response = await TestClient.PostAsync("api/users/authenticate", SerializeUser(user));
            var content = await response.Content.ReadAsStringAsync();
            dynamic contentDynamic = JsonConvert.DeserializeObject<dynamic>(content);
            return contentDynamic.token;
        }
        #endregion
    }
}
