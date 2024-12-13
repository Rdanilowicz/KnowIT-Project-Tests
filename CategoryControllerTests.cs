using KnowIT.Controllers;
using KnowIT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Project_Tests
{
    public class CategoryControllerTests
    {
        private readonly DbContextOptions<KnowledgeDbContext> _options;
        private readonly Mock<ILogger<CategoryController>> _mockLogger;
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;

        public CategoryControllerTests()
        {
            // Create in-memory database options for testing
            _options = new DbContextOptionsBuilder<KnowledgeDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Mock the ILogger<CategoryController>
            _mockLogger = new Mock<ILogger<CategoryController>>();

            // Mock the UserManager<IdentityUser>
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        // Helper Method to Create a Controller with a Fresh Context
        private CategoryController GetController(KnowledgeDbContext context)
        {
            return new CategoryController(_mockLogger.Object, context, _mockUserManager.Object);
        }

        [Fact]
        public void TestCategoryCreation_Success()
        {
            // Arrange: Set up the in-memory database and controller
            using (var context = new KnowledgeDbContext(_options))
            {
                var controller = GetController(context);

                // Act: Create a new category
                var category = new Category { Name = "Test Category" };
                var result = controller.Create(category).Result;  // Using .Result to wait for the async result

                // Assert: Check if the category is added to the database
                var createdCategory = context.Categories.FirstOrDefault(c => c.Name == "Test Category");
                Assert.NotNull(createdCategory); // Ensure category is created
                Assert.Equal("Test Category", createdCategory.Name); // Check name

                // Assert that the result is a redirect (assuming redirect on success)
                var redirectResult = result as RedirectToActionResult;
                Assert.NotNull(redirectResult);
                Assert.Equal("Index", redirectResult.ActionName);  // Assuming it redirects to Index
            }
        }

        [Fact]
        public void TestCategoryCreation_Failure_InvalidModel()
        {
            // Arrange: Set up the in-memory database and controller
            using (var context = new KnowledgeDbContext(_options))
            {
                var controller = GetController(context);

                // Act: Try creating an invalid category (e.g., empty name)
                var category = new Category { Name = "" };  // Empty name
                controller.ModelState.AddModelError("Name", "Name is required.");

                var result = controller.Create(category).Result;  // Using .Result to wait for async result

                // Assert: Check if the result is not a success
                Assert.False(controller.ModelState.IsValid);  // ModelState should be invalid
                var viewResult = result as ViewResult;
                Assert.NotNull(viewResult);  // It should return to the view to show errors
            }
        }

        [Fact]
        public void TestCategoryCreation_Failure_NullName()
        {
            // Arrange: Set up the in-memory database and controller
            using (var context = new KnowledgeDbContext(_options))
            {
                var controller = GetController(context);

                // Act: Try creating a category with a missing name
                var category = new Category { Name = null };  // Null name
                controller.ModelState.AddModelError("Name", "Name is required.");

                var result = controller.Create(category).Result;  // Using .Result to wait for async result

                // Assert: Ensure model validation fails
                Assert.False(controller.ModelState.IsValid);
                var viewResult = result as ViewResult;
                Assert.NotNull(viewResult);  // It should return to the view to show errors
            }
        }
    }
}
