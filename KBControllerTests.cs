using KnowIT.Controllers;
using KnowIT.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Project_Tests
{
    public class KBControllerTests
    {
        private readonly DbContextOptions<KnowledgeDbContext> _options;
        private readonly Mock<ILogger<KBController>> _mockLogger;

        public KBControllerTests()
        {
            // Create in-memory database options
            _options = new DbContextOptionsBuilder<KnowledgeDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Mock the ILogger<KBController>
            _mockLogger = new Mock<ILogger<KBController>>();
        }

        // Helper Method to Create a Controller with a Fresh Context
        private KBController GetController(KnowledgeDbContext context)
        {
            return new KBController(_mockLogger.Object, context);
        }

        [Fact]
        public void TestArticleCreation_Success()
        {
            // Arrange: Set up the in-memory database and controller
            using (var context = new KnowledgeDbContext(_options))
            {
                var controller = GetController(context);

                // Act: Create a new article
                var article = new Article { Title = "Test Article", Content = "Test content." };
                var result = controller.Create(article).Result;  // Using .Result to wait for the async result

                // Assert: Check if the article is added to the database
                var createdArticle = context.Articles.FirstOrDefault(a => a.Title == "Test Article");
                Assert.NotNull(createdArticle); // Ensure article is created
                Assert.Equal("Test Article", createdArticle.Title); // Check title
                Assert.Equal("Test content.", createdArticle.Content); // Check content

                // Assert that the result is a redirect (assuming redirect on success)
                var redirectResult = result as RedirectToActionResult;
                Assert.NotNull(redirectResult);
                Assert.Equal("Index", redirectResult.ActionName);  // Assuming it redirects to Index
            }
        }

        [Fact]
        public void TestArticleCreation_Failure_InvalidModel()
        {
            // Arrange: Set up the in-memory database and controller
            using (var context = new KnowledgeDbContext(_options))
            {
                var controller = GetController(context);

                // Act: Try creating an invalid article (e.g., missing content)
                var article = new Article { Title = "Invalid Article", Content = "" };  // Empty content
                controller.ModelState.AddModelError("Content", "Content is required.");

                var result = controller.Create(article).Result;  // Using .Result to wait for async result

                // Assert: Check if the result is not a success
                Assert.False(controller.ModelState.IsValid);  // ModelState should be invalid
                var viewResult = result as ViewResult;
                Assert.NotNull(viewResult);  // It should return to the view to show errors
            }
        }

        [Fact]
        public void TestArticleCreation_Failure_NullTitle()
        {
            // Arrange: Set up the in-memory database and controller
            using (var context = new KnowledgeDbContext(_options))
            {
                var controller = GetController(context);

                // Act: Try creating an article with a missing title
                var article = new Article { Title = null, Content = "Test content." };
                controller.ModelState.AddModelError("Title", "Title is required.");

                var result = controller.Create(article).Result;  // Using .Result to wait for async result

                // Assert: Ensure model validation fails
                Assert.False(controller.ModelState.IsValid);
                var viewResult = result as ViewResult;
                Assert.NotNull(viewResult);  // It should return to the view to show errors
            }
        }
    }
}
