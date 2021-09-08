using Domain;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace WebAPI.Tests
{
    public class UsersControllerTests
    {
        #region General
        private static int dbCounter = 0;

        private IntegrationTest CreateIntegrationTest()
        {
            dbCounter++;
            return new IntegrationTest($"TestDb{dbCounter}");
        }
        #endregion

        #region GetUsers
        [Fact]
        public async Task GetUsers_WithOneUser_ReturnsThisUserResponse()
        {
            // Arrange
            var it = CreateIntegrationTest();
            await it.AuthenticateAsync();

            // Act
            var response = await it.TestClient.GetAsync("api/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<User>>(content);
            users.Should().HaveCount(1);

            users.SingleOrDefault().ID.Should().Be(it.TestUser.ID);
        }

        [Fact]
        public async Task GetUsers_WithoutAuthentication_ReturnsUnauthorizedError()
        {
            // Arrange
            var it = CreateIntegrationTest();

            // Act
            var response = await it.TestClient.GetAsync("api/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUsers_ByNonExistentLastName_ReturnsEmptyResponse()
        {
            // Arrange
            var it = CreateIntegrationTest();
            await it.AuthenticateAsync();

            // Act
            var response = await it.TestClient.GetAsync("api/users?lastName=Smith");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<User>>(content);
            users.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUsers_ByLastName_ReturnsFilteredList()
        {
            // Arrange
            var it = CreateIntegrationTest();
            await it.AuthenticateAsync();
            await it.CreateUser(new User
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@mail.com",
                Password = "JohnSmith123!"
            });
            await it.CreateUser(new User
            {
                FirstName = "Adam",
                LastName = "Smith",
                Email = "adam.smith@mail.com",
                Password = "AdamSmith123!"
            });
            await it.CreateUser(new User
            {
                FirstName = "John",
                LastName = "Cage",
                Email = "john.cage@mail.com",
                Password = "JohnCage123!"
            });
            await it.CreateUser(new User
            {
                FirstName = "John",
                LastName = "Wick",
                Email = "john.wick@mail.com",
                Password = "JohnWick123!"
            });

            // Act
            var response = await it.TestClient.GetAsync("api/users?lastName=Smith");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<User>>(content);
            users.Should().HaveCount(2);
        }
        #endregion

        #region GetUser
        [Fact]
        public async Task GetUser_WithoutAuthentication_ReturnsUnauthorizedError()
        {
            // Arrange
            var it = CreateIntegrationTest();

            // Act
            var response = await it.TestClient.GetAsync($"api/users/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUser_InvalidId_ReturnsNotFoundError()
        {
            // Arrange
            var it = CreateIntegrationTest();
            await it.AuthenticateAsync();

            // Act
            var response = await it.TestClient.GetAsync($"api/users/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUser_CorrectId_ReturnsUserResponse()
        {
            // Arrange
            var it = CreateIntegrationTest();
            await it.AuthenticateAsync();
            var newlyCreatedUser = await it.CreateUser(new User
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@mail.com",
                Password = "JohnSmith123!"
            });

            // Act
            var response = await it.TestClient.GetAsync($"api/users/{newlyCreatedUser.ID}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        #endregion

        #region PutUser
        [Fact]
        public async Task PutUser_UpdateOtherUser_UnauthorizedError()
        {
            // Arrange
            var it = CreateIntegrationTest();
            await it.AuthenticateAsync();
            var otherUser = await it.CreateUser(new User
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@mail.com",
                Password = "JohnSmith123!"
            });

            // Act
            var updatedOtherUser = new User
            {
                FirstName = "John",
                LastName = "Smith",
                ID = otherUser.ID,
                Email = "john.smith@mail.com",
                Password = "HaHaIChangedYourPassword"
            };
            var response = await it.TestClient.PutAsync($"api/users/{otherUser.ID}", IntegrationTest.SerializeUser(updatedOtherUser));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task PutUser_UpdateMyPassword_NoContentResponse()
        {
            // Arrange
            var it = CreateIntegrationTest();
            var otherUser = await it.CreateUser(new User
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@mail.com",
                Password = "JohnSmith123!"
            });
            var otherUserToken = await it.AuthenticateUser(new User 
            { 
                ID = otherUser.ID,
                Email = "john.smith@mail.com",
                Password = "JohnSmith123!"
            });
            it.Authenticate(otherUserToken);

            // Act
            var updatedOtherUser = new User
            {
                FirstName = "John",
                LastName = "Smith",
                ID = otherUser.ID,
                Email = "john.smith@mail.com",
                Password = "HaHaIChangedYourPassword"
            };
            var response = await it.TestClient.PutAsync($"api/users/{otherUser.ID}", IntegrationTest.SerializeUser(updatedOtherUser));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
        #endregion

        #region DeleteUser
        [Fact]
        public async Task DeleteUser_DeleteAnotherUser_UnauthorizedError()
        {
            // Arrange
            var it = CreateIntegrationTest();
            await it.AuthenticateAsync();
            var otherUser = await it.CreateUser(new User
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@mail.com",
                Password = "JohnSmith123!"
            });

            // Act
            var response = await it.TestClient.DeleteAsync($"api/users/{otherUser.ID}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteUser_DeleteMyself_NoContent()
        {
            // Arrange
            var it = CreateIntegrationTest();
            var otherUser = await it.CreateUser(new User
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@mail.com",
                Password = "JohnSmith123!"
            });
            var otherUserToken = await it.AuthenticateUser(new User
            {
                ID = otherUser.ID,
                Email = "john.smith@mail.com",
                Password = "JohnSmith123!"
            });
            it.Authenticate(otherUserToken);

            // Act
            var response = await it.TestClient.DeleteAsync($"api/users/{otherUser.ID}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Extra check if the user is really deleted
            await it.AuthenticateAsync();
            var getOtherUserResponse = await it.TestClient.GetAsync($"api/users/{otherUser.ID}");
            getOtherUserResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        #endregion
    }
}
