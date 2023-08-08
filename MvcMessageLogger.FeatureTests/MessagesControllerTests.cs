using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MvcMessageLogger.DataAccess;
using MvcMessageLogger.Models;

namespace MvcMessageLogger.FeatureTests
{
    [Collection("Controller Tests")]
    public class MessagesControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public MessagesControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private MvcMessageLoggerContext GetDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<MvcMessageLoggerContext>();
            optionsBuilder.UseInMemoryDatabase("TestDatabase");

            var context = new MvcMessageLoggerContext(optionsBuilder.Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return context;
        }

        [Fact]
        public async Task New_ReturnsViewOfNewMessageForm()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            var message1 = new Models.Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 24, 0).ToUniversalTime() };
            user1.Messages.Add(message1);
            context.Users.Add(user1);
            context.Users.Add(user2);
            user1.LoggedIn = true;
            context.SaveChanges();

            var response = await client.GetAsync($"/users/{user1.Id}/messages/new");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains(user1.Name, html);
            Assert.Contains($"<form method=\"post\" action=\"/users/{user1.Id}/messages\">", html);
            Assert.Contains("textarea", html);
            Assert.Contains("Create Message", html);
        }
        [Fact]
        public async Task Create_RedirectsToUserShowPageShowingNewMessage()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            context.Users.Add(user1);
            context.Users.Add(user2);
            user1.LoggedIn = true;
            context.SaveChanges();

            var formData = new Dictionary<string, string>
            {
                {"Content", "test test test" }
            };

            var response = await client.PostAsync($"/users/{user1.Id}/messages", new FormUrlEncodedContent(formData));
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains(user1.Name, html);
            Assert.Contains(user1.Username, html);
            Assert.Contains("test test test", html);
            Assert.DoesNotContain(user2.Name, html);
            Assert.DoesNotContain(user2.Username, html);
        }
    }
}