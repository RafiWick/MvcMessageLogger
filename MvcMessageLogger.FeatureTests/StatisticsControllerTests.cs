using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MvcMessageLogger.DataAccess;
using MvcMessageLogger.Models;

namespace MvcMessageLogger.FeatureTests
{
    [Collection("Controller Tests")]
    public class StatisticsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public StatisticsControllerTests(WebApplicationFactory<Program> factory)
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
        public async Task UsersByNumberOfMessages_ShowsCorrectOrderOfUsersByNumberOfMessages()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            var user3 = new User { Name = "Jim Jones", Username = "jj", Email = "jim@gmail.com", Password = "abcdefg" };
            var user4 = new User { Name = "Frank Kelly", Username = "kfrank", Email = "frank@gmail.com", Password = "abdefg" };

            var message0 = new Models.Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 24, 0).ToUniversalTime() };
            var message1 = new Models.Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 23, 0).ToUniversalTime() };
            var message2 = new Models.Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 22, 0).ToUniversalTime() };
            var message3 = new Models.Message { Content = "tst tst tst", CreatedAt = new DateTime(2023, 8, 7, 14, 54, 0).ToUniversalTime() };
            var message4 = new Models.Message { Content = "check check check", CreatedAt = new DateTime(2023, 8, 3, 4, 24, 0).ToUniversalTime() };
            var message5 = new Models.Message { Content = "check check check", CreatedAt = new DateTime(2023, 4, 3, 12, 24, 0).ToUniversalTime() };
            var message6 = new Models.Message { Content = "the the", CreatedAt = new DateTime(2023, 8, 7, 21, 2, 0).ToUniversalTime() };
            var message7 = new Models.Message { Content = "them them all all but is", CreatedAt = new DateTime(2023, 8, 7, 4, 24, 0).ToUniversalTime() };
            var message8 = new Models.Message { Content = "yes yes", CreatedAt = new DateTime(2023, 8, 7, 4, 24, 0).ToUniversalTime() };
            var message9 = new Models.Message { Content = "no no and and maybe maybe", CreatedAt = new DateTime(2023, 5, 7, 4, 54, 0).ToUniversalTime() };

            user1.Messages.Add(message9);
            user1.Messages.Add(message8);
            user1.Messages.Add(message7);
            user1.Messages.Add(message6);
            user2.Messages.Add(message5);
            user2.Messages.Add(message4);
            user2.Messages.Add(message3);
            user3.Messages.Add(message2);
            user3.Messages.Add(message1);
            user4.Messages.Add(message0);
            context.Users.Add(user1);
            context.Users.Add(user2);
            context.Users.Add(user3);
            context.Users.Add(user4);
            user1.LoggedIn = true;
            context.SaveChanges();

            var response = await client.GetAsync($"/statistics");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("<span id=\"1\">John Doe: 4 messages", html);
            Assert.Contains("<span id=\"2\">Jane Doe: 3 messages", html);
            Assert.Contains("<span id=\"3\">Jim Jones: 2 messages", html);
            Assert.Contains("<span id=\"4\">Frank Kelly: 1 messages", html);
        }

        [Fact]
        public async Task AllMostCommonWords_ShowsTop10WordsForAllAndEachUser()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            var user3 = new User { Name = "Jim Jones", Username = "jj", Email = "jim@gmail.com", Password = "abcdefg" };
            var user4 = new User { Name = "Frank Kelly", Username = "kfrank", Email = "frank@gmail.com", Password = "abdefg" };

            var message0 = new Models.Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 24, 0).ToUniversalTime() };
            var message1 = new Models.Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 23, 0).ToUniversalTime() };
            var message2 = new Models.Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 22, 0).ToUniversalTime() };
            var message3 = new Models.Message { Content = "tst tst tst", CreatedAt = new DateTime(2023, 8, 7, 14, 54, 0).ToUniversalTime() };
            var message4 = new Models.Message { Content = "check check check can can", CreatedAt = new DateTime(2023, 8, 3, 4, 24, 0).ToUniversalTime() };
            var message5 = new Models.Message { Content = "check check check", CreatedAt = new DateTime(2023, 4, 3, 12, 24, 0).ToUniversalTime() };
            var message6 = new Models.Message { Content = "the the", CreatedAt = new DateTime(2023, 8, 7, 21, 2, 0).ToUniversalTime() };
            var message7 = new Models.Message { Content = "them them all all but is", CreatedAt = new DateTime(2023, 8, 7, 4, 24, 0).ToUniversalTime() };
            var message8 = new Models.Message { Content = "yes yes", CreatedAt = new DateTime(2023, 8, 7, 4, 24, 0).ToUniversalTime() };
            var message9 = new Models.Message { Content = "no no and and maybe maybe", CreatedAt = new DateTime(2023, 5, 7, 4, 54, 0).ToUniversalTime() };

            user1.Messages.Add(message9);
            user1.Messages.Add(message8);
            user1.Messages.Add(message7);
            user1.Messages.Add(message6);
            user2.Messages.Add(message5);
            user2.Messages.Add(message4);
            user2.Messages.Add(message3);
            user3.Messages.Add(message2);
            user3.Messages.Add(message1);
            user4.Messages.Add(message0);
            context.Users.Add(user1);
            context.Users.Add(user2);
            context.Users.Add(user3);
            context.Users.Add(user4);
            user1.LoggedIn = true;
            context.SaveChanges();

            var response = await client.GetAsync($"/statistics");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("1:test  2:check  3:tst  4:no  5:and  6:maybe  7:yes  8:them  9:all  10:the", html);
            Assert.Contains("John Doe: (1): no  (2): and  (3): maybe  (4): yes  (5): them  (6): all  (7): the  (8): but  (9): is", html);
            Assert.Contains("Jane Doe: (1): check  (2): tst  (3): can", html);
            Assert.Contains("Jim Jones: (1): test", html);
            Assert.Contains("Frank Kelly: (1): test", html);
        }

        [Fact]
        public async Task BusiestHour_ShowsCorrectHourWithMostPosts()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            var user3 = new User { Name = "Jim Jones", Username = "jj", Email = "jim@gmail.com", Password = "abcdefg" };
            var user4 = new User { Name = "Frank Kelly", Username = "kfrank", Email = "frank@gmail.com", Password = "abdefg" };

            var message0 = new Models.Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 24, 0).ToUniversalTime() };
            var message1 = new Models.Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 23, 0).ToUniversalTime() };
            var message2 = new Models.Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 22, 0).ToUniversalTime() };
            var message3 = new Models.Message { Content = "tst tst tst", CreatedAt = new DateTime(2023, 8, 7, 14, 54, 0).ToUniversalTime() };
            var message4 = new Models.Message { Content = "check check check", CreatedAt = new DateTime(2023, 8, 3, 4, 24, 0).ToUniversalTime() };
            var message5 = new Models.Message { Content = "check check check", CreatedAt = new DateTime(2023, 4, 3, 12, 24, 0).ToUniversalTime() };
            var message6 = new Models.Message { Content = "the the", CreatedAt = new DateTime(2023, 8, 7, 21, 2, 0).ToUniversalTime() };
            var message7 = new Models.Message { Content = "them them all all but is", CreatedAt = new DateTime(2023, 8, 7, 4, 24, 0).ToUniversalTime() };
            var message8 = new Models.Message { Content = "yes yes", CreatedAt = new DateTime(2023, 8, 7, 4, 24, 0).ToUniversalTime() };
            var message9 = new Models.Message { Content = "no no and and maybe maybe", CreatedAt = new DateTime(2023, 5, 7, 4, 54, 0).ToUniversalTime() };

            user1.Messages.Add(message9);
            user1.Messages.Add(message8);
            user1.Messages.Add(message7);
            user1.Messages.Add(message6);
            user2.Messages.Add(message5);
            user2.Messages.Add(message4);
            user2.Messages.Add(message3);
            user3.Messages.Add(message2);
            user3.Messages.Add(message1);
            user4.Messages.Add(message0);
            context.Users.Add(user1);
            context.Users.Add(user2);
            context.Users.Add(user3);
            context.Users.Add(user4);
            user1.LoggedIn = true;
            context.SaveChanges();

            var response = await client.GetAsync($"/statistics");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("the hour with the most posts is 8 PM on 8/7/2023", html);
        }
    }
}