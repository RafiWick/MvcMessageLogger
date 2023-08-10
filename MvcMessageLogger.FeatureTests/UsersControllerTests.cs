using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MvcMessageLogger.DataAccess;
using MvcMessageLogger.Models;

namespace MvcMessageLogger.FeatureTests
{
    [Collection("Controller Tests")]
    public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public UsersControllerTests(WebApplicationFactory<Program> factory)
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
        public async Task Index_ReturnsViewWithAllUsers()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            context.Users.Add(user1);
            context.Users.Add(user2);
            context.SaveChanges();

            var response = await client.GetAsync("/users");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains(user1.Name, html);
            Assert.Contains(user2.Name, html);
            Assert.Contains(user1.Username, html);
            Assert.Contains(user2.Username, html);
            Assert.DoesNotContain(user1.Email, html);
            Assert.DoesNotContain(user2.Email, html);
        }

        [Fact]
        public async Task New_ReturnsViewWithNewUserForm()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/users/new");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("<form method=\"post\" action=\"/users\">", html);
            Assert.Contains("input", html);
            Assert.Contains("label", html);
            Assert.Contains("Username", html);
            Assert.Contains("Email", html);
            Assert.Contains("Name", html);
            Assert.Contains("Password", html);
        }

        [Fact]
        public async Task Create_AddsNewUserToDbAndRedirectsToIndexViewWithNewUser()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            context.Users.Add(user1);
            context.Users.Add(user2);
            context.SaveChanges();

            var formData = new Dictionary<string, string>
            {
                {"Name", "Jane Doe"},
                {"Username", "j_doe" },
                {"Email", "jane@gmail.com" },
                {"Password", "abdefg" }
            };

            var response = await client.PostAsync("/users", new FormUrlEncodedContent(formData));
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            Assert.Contains("Jane Doe", html);
            Assert.Contains("j_doe", html);
        }

        [Fact]
        public async Task Show_ReturnsViewWithOneUserAndItsMessages()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            var message1 = new Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 24, 0).ToUniversalTime() };
            user1.Messages.Add(message1);
            context.Users.Add(user1);
            context.Users.Add(user2);
            user1.LoggedIn = true;
            context.SaveChanges();

            var response = await client.GetAsync($"/users/{user1.Id}");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            Assert.Contains(user1.Name, html);
            Assert.Contains(user1.Username, html);
            Assert.Contains(message1.Content, html);
            Assert.Contains(message1.CreatedAt.ToShortDateString(), html);
            Assert.Contains(message1.CreatedAt.ToShortTimeString(), html);
            Assert.DoesNotContain(user2.Name, html);
            Assert.DoesNotContain(user2.Username, html);
        }

        [Fact]
        public async Task LogIn_GET_ReturnsLogInForm()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/users/login");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("<form method=\"post\" action=\"/users/login\">", html);
            Assert.Contains("input", html);
            Assert.Contains("label", html);
            Assert.Contains("Password", html);
            Assert.Contains("Email", html);
        }

        [Fact]
        public async Task LogIn_POST_RedirectsToCorrectUserShowPage()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "123" };
            user1.Password = user1.Encrypt("123");
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            user2.LoggedIn = false;
            var message1 = new Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 24, 0).ToUniversalTime() };
            user1.Messages.Add(message1);
            context.Users.Add(user1);
            context.Users.Add(user2);
            user1.LoggedIn = true;
            context.SaveChanges();

            var formData = new Dictionary<string, string>
            {
                {"Email", "john@gmail.com" },
                {"Password", "123" }
            };

            var response = await client.PostAsync($"/users/login", new FormUrlEncodedContent(formData));
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            Assert.Contains(user1.Name, html);
            Assert.Contains(user1.Username, html);
            Assert.Contains(message1.Content, html);
            Assert.Contains(message1.CreatedAt.ToShortDateString(), html);
            Assert.Contains(message1.CreatedAt.ToShortTimeString(), html);
            Assert.DoesNotContain(user2.Name, html);
            Assert.DoesNotContain(user2.Username, html);
        }

        [Fact]
        public async Task LogOut_ReturnsIndexWithLogInLink()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            user1.Password = user1.Encrypt("abcdefg");
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            user2.Password = user2.Encrypt("abdefg");
            user2.LoggedIn = false;
            var message1 = new Models.Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 24, 0).ToUniversalTime() };
            user1.Messages.Add(message1);
            context.Users.Add(user1);
            context.Users.Add(user2);
            user1.LoggedIn = true;
            context.SaveChanges();

            var formData = new Dictionary<string, string>
            {
            };

            var response = await client.PostAsync($"/users/logout", new FormUrlEncodedContent(formData));
            ;
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("<a href=\"/users/login\">Log In</a>", html);
        }

        [Fact]
        public async Task EditPasswodCheck_ReturnsPasswordField()
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

            var response = await client.GetAsync($"/users/{user1.Id}/editcheck");

            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains($"<form method=\"post\" action=\"/users/{user1.Id}/editcheck\">", html);
            Assert.Contains("Password:", html);
        }

        [Fact]
        public async Task EditForPasswodCheck_AddsErrorMessageWithIncorrectPassword()
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

            var formData = new Dictionary<string, string>
            {
                {"Password", "abefg" }
            };

            var response = await client.PostAsync($"/users/{user1.Id}/editcheck", new FormUrlEncodedContent(formData));


            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("Password Incorect", html);
            Assert.Contains("Please try agian", html);
        }

        [Fact]
        public async Task Edit_ReturnsViewWithPrePopulatedUserForm()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            context.Users.Add(user1);
            context.Users.Add(user2);
            context.SaveChanges();

            var response = await client.GetAsync($"/users/{user1.Id}/edit?pcode={user1.Password}");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains($"<form method=\"post\" action=\"/users/{user1.Id}\">", html);
            Assert.Contains("input", html);
            Assert.Contains("label", html);
            Assert.Contains("Username", html);
            Assert.Contains("Email", html);
            Assert.Contains("Name", html);
            Assert.Contains("Password", html);
            Assert.Contains(user1.Name, html);
            Assert.Contains(user1.Username, html);
            Assert.Contains(user1.Email, html);
        }

        [Fact]
        public async Task Update_ReturnsShowWithNewUserData()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            context.Users.Add(user1);
            context.Users.Add(user2);
            context.SaveChanges();

            var formData = new Dictionary<string, string>
            {
                {"Name", "Jim Jones"},
                {"Username", "jj" },
                {"Email", "jjones@gmail.com" },
                {"Password", "hijklmnop" }
            };

            var response = await client.PostAsync($"/users/{user1.Id}", new FormUrlEncodedContent(formData));
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            Assert.Contains("Jim Jones", html);
            Assert.Contains("jj", html);
        }

        [Fact]
        public async Task Delete_ReturnsIndexWithoutDeletedUser()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            context.Users.Add(user1);
            context.Users.Add(user2);
            context.SaveChanges();

            var formData = new Dictionary<string, string>
            {
            };

            var response = await client.PostAsync($"/users/{user1.Id}/delete", new FormUrlEncodedContent(formData));
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("Jim Jomes", html);
            Assert.DoesNotContain("jj", html);
        }

        [Fact]
        public async Task Follow_ReturnsShowPageWithoutFollowButtom()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            var message1 = new Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 24, 0).ToUniversalTime() };
            user1.Messages.Add(message1);
            context.Users.Add(user1);
            context.Users.Add(user2);
            user1.LoggedIn = true;
            user2.LoggedIn = false;
            context.SaveChanges();

            var formData = new Dictionary<string, string>
            {
            };

            var response = await client.PostAsync($"/users/{user2.Id}/follow", new FormUrlEncodedContent(formData));
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("<h5>Following", html);
            Assert.DoesNotContain($"<form method=\"post\" action=\"/users/{user2.Id}/follow\">", html);
        }

        [Fact]
        public async Task UnFollow_ReturnsShowPageWithoutUnFollowButtom()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            var message1 = new Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 24, 0).ToUniversalTime() };
            user1.Messages.Add(message1);
            context.Users.Add(user1);
            context.Users.Add(user2);
            user1.Following.Add(user2);
            user2.Followers.Add(user1);
            user1.LoggedIn = true;
            user2.LoggedIn = false;
            context.SaveChanges();

            var formData = new Dictionary<string, string>
            {
            };

            var response = await client.PostAsync($"/users/{user2.Id}/unfollow", new FormUrlEncodedContent(formData));
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.DoesNotContain("<h5>Following", html);
            Assert.Contains($"<form method=\"post\" action=\"/users/{user2.Id}/follow\">", html);
        }

        [Fact]
        public async Task Following_ReturnsPageListOfUsersFollowedByUser()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            var user3 = new User { Name = "Jim Jones", Username = "jj", Email = "jim@gmail.com", Password = "abcdefg" };
            var user4 = new User { Name = "Frank Kelly", Username = "kfrank", Email = "frank@gmail.com", Password = "abdefg" };
            var message1 = new Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 24, 0).ToUniversalTime() };
            user1.Messages.Add(message1);
            context.Users.Add(user1);
            context.Users.Add(user2);
            context.Users.Add(user3);
            context.Users.Add(user4);

            user1.LoggedIn = true;
            user1.Following.Add(user2);
            user1.Following.Add(user3);
            user1.Following.Add(user4);
            context.SaveChanges();



            var response = await client.GetAsync($"/users/{user1.Id}/following");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("Jane Doe", html);
            Assert.Contains("Jim Jones", html);
            Assert.Contains("Frank Kelly", html);
        }

        [Fact]
        public async Task Followers_ReturnsPageListOfUsersFollowingUser()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John Doe", Username = "jdoe", Email = "john@gmail.com", Password = "abcdefg" };
            var user2 = new User { Name = "Jane Doe", Username = "j_doe", Email = "jane@gmail.com", Password = "abdefg" };
            var user3 = new User { Name = "Jim Jones", Username = "jj", Email = "jim@gmail.com", Password = "abcdefg" };
            var user4 = new User { Name = "Frank Kelly", Username = "kfrank", Email = "frank@gmail.com", Password = "abdefg" };
            var message1 = new Message { Content = "test test test", CreatedAt = new DateTime(2023, 8, 7, 14, 24, 0).ToUniversalTime() };
            user1.Messages.Add(message1);
            context.Users.Add(user1);
            context.Users.Add(user2);
            context.Users.Add(user3);
            context.Users.Add(user4);

            user1.LoggedIn = true;
            user1.Followers.Add(user2);
            user1.Followers.Add(user3);
            user1.Followers.Add(user4);
            context.SaveChanges();



            var response = await client.GetAsync($"/users/{user1.Id}/followers");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("Jane Doe", html);
            Assert.Contains("Jim Jones", html);
            Assert.Contains("Frank Kelly", html);
        }

        [Fact]
        public async Task Feed_ShowsAllMessagesByActiveUsersFollowers()
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
            user2.LoggedIn = false;
            user3.LoggedIn = false;
            user1.LoggedIn = false;
            user4.Following.Add(user3);
            user4.Following.Add(user1);
            context.SaveChanges();

            var response = await client.GetAsync($"/users/feed");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("test test test", html);
            Assert.Contains("no no and and maybe maybe", html);
            Assert.DoesNotContain("check check check", html);
        }
    }
}