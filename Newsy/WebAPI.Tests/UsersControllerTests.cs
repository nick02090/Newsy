using Domain;
using FluentAssertions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace WebAPI.Tests
{
    public class UsersControllerTests : IntegrationTest
    {
        [Fact]
        public async Task GetUsers_WithOneUser_ReturnsThisUserResponse()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await TestClient.GetAsync("api/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<User>>(content);
            users.Should().HaveCount(1);

            users.SingleOrDefault().Should().BeEquivalentTo(TestUser);
        }
    }
}
