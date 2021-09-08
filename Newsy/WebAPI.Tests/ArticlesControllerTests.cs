using Domain;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace WebAPI.Tests
{
    public class ArticlesControllerTests
    {

        #region General
        private static int dbCounter = 0;

        private IntegrationTest CreateIntegrationTest()
        {
            dbCounter++;
            return new IntegrationTest($"TestArticlesDb{dbCounter}");
        }
        #endregion

        #region GetArticles
        [Fact]
        public async Task GetArticles_WithoutAnyInDb_ReturnsEmptyResponse()
        {
            // Arrange
            var it = CreateIntegrationTest();
            await it.AuthenticateAsync();

            // Act
            var response = await it.TestClient.GetAsync("api/articles");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var articles = JsonConvert.DeserializeObject<List<Article>>(content);
            articles.Should().BeEmpty();
        }

        [Fact]
        public async Task GetArticles_WithoutAuthentication_ReturnsUnauthorizedError()
        {
            // Arrange
            var it = CreateIntegrationTest();

            // Act
            var response = await it.TestClient.GetAsync("api/articles");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetArticles_ByNonExistentAuthorId_ReturnsEmptyResponse()
        {
            // Arrange
            var it = CreateIntegrationTest();
            await it.AuthenticateAsync();

            // Act
            var response = await it.TestClient.GetAsync($"api/articles?authorID={Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var articles = JsonConvert.DeserializeObject<List<Article>>(content);
            articles.Should().BeEmpty();
        }

        [Fact]
        public async Task GetArticles_ByPartOfTheTitle_ReturnsFilteredList()
        {
            // Arrange
            var it = CreateIntegrationTest();
            await it.AuthenticateAsync();
            await it.CreateArticle(new Article
            {
                Author = it.TestUser,
                Body = "Blah blah blah...",
                Description = "My best blah ever",
                Title = "King Blah"
            });
            await it.CreateArticle(new Article
            {
                Author = it.TestUser,
                Body = "Once upon a time there was a boy...",
                Description = "Old fairytale about a lost boy",
                Title = "Lost boy and the King Baltazar"
            });
            await it.CreateArticle(new Article
            {
                Author = it.TestUser,
                Body = "Test test test",
                Description = "This is for test purposes",
                Title = "Test"
            });

            // Act
            var response = await it.TestClient.GetAsync("api/articles?titlePart=King");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var articles = JsonConvert.DeserializeObject<List<Article>>(content);
            articles.Should().HaveCount(2);
        }
        #endregion

        #region GetArticle
        [Fact]
        public async Task GetArticle_WithoutAuthentication_ReturnsUnauthorizedError()
        {
            // Arrange
            var it = CreateIntegrationTest();

            // Act
            var response = await it.TestClient.GetAsync($"api/articles/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetArticle_InvalidId_ReturnsNotFoundError()
        {
            // Arrange
            var it = CreateIntegrationTest();
            await it.AuthenticateAsync();

            // Act
            var response = await it.TestClient.GetAsync($"api/articles/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetArticle_CorrectId_ReturnsArticleResponse()
        {
            // Arrange
            var it = CreateIntegrationTest();
            await it.AuthenticateAsync();
            var newlyCreatedArticle = await it.CreateArticle(new Article
            {
                Author = it.TestUser,
                Body = "Blah blah blah...",
                Description = "My best blah ever",
                Title = "King Blah"
            });

            // Act
            var response = await it.TestClient.GetAsync($"api/articles/{newlyCreatedArticle.ID}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        #endregion

        #region PutArticle
        [Fact]
        public async Task PutArticle_UpdateOthersUserArticle_UnauthorizedError()
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
            var othersUserArticle = await it.CreateArticle(new Article
            {
                Author = new User { 
                    ID = otherUser.ID,
                    Email = "john.smith@mail.com",
                    Password = "JohnSmith123!"
                },
                Body = "Blah blah blah...",
                Description = "My best blah ever",
                Title = "King Blah"
            });
            // set authentication back to test user
            await it.AuthenticateAsync();

            // Act
            var updatedOthersUserArticle = new Article
            {
                Author = new User
                {
                    ID = otherUser.ID,
                    Email = "john.smith@mail.com"
                },
                ID = othersUserArticle.ID,
                Body = "Hahah your article is gone!",
                Description = "Worst article ever",
                Title = "Who cares"
            };
            var response = await it.TestClient.PutAsync($"api/articles/{othersUserArticle.ID}", IntegrationTest.SerializeObject(updatedOthersUserArticle));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task PutArticle_UpdateMyArticle_NoContentResponse()
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
            var othersUserArticle = await it.CreateArticle(new Article
            {
                Author = new User
                {
                    ID = otherUser.ID,
                    Email = "john.smith@mail.com",
                    Password = "JohnSmith123!"
                },
                Body = "Blah blah blah...",
                Description = "My best blah ever",
                Title = "King Blah"
            });

            // Act
            var updatedOthersUserArticle = new Article
            {
                Author = new User
                {
                    ID = otherUser.ID,
                    Email = "john.smith@mail.com"
                },
                ID = othersUserArticle.ID,
                Body = "Hahah your article is gone!",
                Description = "Worst article ever",
                Title = "Who cares"
            };
            var response = await it.TestClient.PutAsync($"api/articles/{othersUserArticle.ID}", IntegrationTest.SerializeObject(updatedOthersUserArticle));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
        #endregion

        #region DeleteArticle
        [Fact]
        public async Task DeleteArticle_DeleteOthersUserArticle_UnauthorizedError()
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
            var othersUserArticle = await it.CreateArticle(new Article
            {
                Author = new User
                {
                    ID = otherUser.ID,
                    Email = "john.smith@mail.com",
                    Password = "JohnSmith123!"
                },
                Body = "Blah blah blah...",
                Description = "My best blah ever",
                Title = "King Blah"
            });
            // set authentication back to test user
            await it.AuthenticateAsync();

            // Act
            var response = await it.TestClient.DeleteAsync($"api/articles/{othersUserArticle.ID}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteArticle_DeleteByMyself_NoContent()
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
            var othersUserArticle = await it.CreateArticle(new Article
            {
                Author = new User
                {
                    ID = otherUser.ID,
                    Email = "john.smith@mail.com",
                    Password = "JohnSmith123!"
                },
                Body = "Blah blah blah...",
                Description = "My best blah ever",
                Title = "King Blah"
            });

            // Act
            var response = await it.TestClient.DeleteAsync($"api/articles/{othersUserArticle.ID}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Extra check if the article is really deleted
            await it.AuthenticateAsync();
            var getArticleResponse = await it.TestClient.GetAsync($"api/articles/{othersUserArticle.ID}");
            getArticleResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        #endregion
    }
}
