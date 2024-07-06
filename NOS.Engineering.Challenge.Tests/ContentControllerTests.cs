using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NOS.Engineering.Challenge.API.Controllers;
using NOS.Engineering.Challenge.API.Models;
using NOS.Engineering.Challenge.Managers;

namespace NOS.Engineering.Challenge.Tests
{
    public class ContentControllerTests
    {
        private readonly Mock<IContentsManager> _mockManager;
        private readonly Mock<ILogger<ContentController>> _mockLogger;
        private readonly ContentController _controller;

        public ContentControllerTests()
        {
            _mockManager = new Mock<IContentsManager>();
            _mockLogger = new Mock<ILogger<ContentController>>();
            _controller = new ContentController(_mockManager.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateContent_Returns_OkResult()
        {
            // Arrange
            var contentInput = new ContentInput
            {
                Title = "New Content",
                SubTitle = "Subtitle",
                Description = "Description",
                ImageUrl = "https://example.com/image.jpg",
                Duration = 120,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2),
                GenreList = new List<string> { "Action", "Adventure" }
            };

            var expectedCreatedContent = new Content
            {
                Id = Guid.NewGuid(),
                Title = contentInput.Title,
                SubTitle = contentInput.SubTitle,
                Description = contentInput.Description,
                ImageUrl = contentInput.ImageUrl,
                Duration = contentInput.Duration,
                StartTime = contentInput.StartTime,
                EndTime = contentInput.EndTime,
                GenreList = contentInput.GenreList
            };

            _mockManager.Setup(m => m.CreateContent(It.IsAny<ContentDto>())).ReturnsAsync(expectedCreatedContent);

            // Act
            var result = await _controller.CreateContent(contentInput);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualCreatedContent = Assert.IsAssignableFrom<Content>(okResult.Value);
            Assert.Equal(expectedCreatedContent.Id, actualCreatedContent.Id);
            Assert.Equal(expectedCreatedContent.Title, actualCreatedContent.Title);
            Assert.Equal(expectedCreatedContent.SubTitle, actualCreatedContent.SubTitle);
            Assert.Equal(expectedCreatedContent.Description, actualCreatedContent.Description);
            Assert.Equal(expectedCreatedContent.ImageUrl, actualCreatedContent.ImageUrl);
            Assert.Equal(expectedCreatedContent.Duration, actualCreatedContent.Duration);
            Assert.Equal(expectedCreatedContent.StartTime, actualCreatedContent.StartTime);
            Assert.Equal(expectedCreatedContent.EndTime, actualCreatedContent.EndTime);
            Assert.Equal(expectedCreatedContent.GenreList, actualCreatedContent.GenreList);

            _mockManager.Verify(m => m.CreateContent(It.IsAny<ContentDto>()), Times.Once);
        }

        [Fact]
        public async Task AddGenres_Returns_OkResult()
        {
            // Arrange
            var contentId = Guid.NewGuid();
            var genresToAdd = new List<string> { "Comedy", "Drama" };

            var originalContent = new Content
            {
                Id = contentId,
                Title = "Original Content",
                GenreList = new List<string> { "Action", "Adventure" }
            };

            var updatedContent = new Content
            {
                Id = contentId,
                Title = "Original Content",
                GenreList = originalContent.GenreList.Concat(genresToAdd).ToList()
            };

            _mockManager.Setup(m => m.GetContent(contentId)).ReturnsAsync(originalContent);
            _mockManager.Setup(m => m.UpdateContent(contentId, It.IsAny<ContentDto>())).ReturnsAsync(updatedContent);

            // Act
            var result = await _controller.AddGenres(contentId, genresToAdd);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualUpdatedContent = Assert.IsAssignableFrom<Content>(okResult.Value);
            Assert.Equal(updatedContent.Id, actualUpdatedContent.Id);
            Assert.Equal(updatedContent.Title, actualUpdatedContent.Title);
            Assert.Equal(updatedContent.GenreList, actualUpdatedContent.GenreList);

            _mockManager.Verify(m => m.UpdateContent(contentId, It.IsAny<ContentDto>()), Times.Once);
        }

        [Fact]
        public async Task RemoveGenres_Returns_OkResult()
        {
            // Arrange
            var contentId = Guid.NewGuid();
            var genresToRemove = new List<string> { "Action" };

            var originalContent = new Content
            {
                Id = contentId,
                Title = "Original Content",
                GenreList = new List<string> { "Action", "Adventure", "Comedy" }
            };

            var updatedContent = new Content
            {
                Id = contentId,
                Title = "Original Content",
                GenreList = originalContent.GenreList.Except(genresToRemove).ToList()
            };

            _mockManager.Setup(m => m.GetContent(contentId)).ReturnsAsync(originalContent);
            _mockManager.Setup(m => m.UpdateContent(contentId, It.IsAny<ContentDto>())).ReturnsAsync(updatedContent);

            // Act
            var result = await _controller.RemoveGenres(contentId, genresToRemove);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualUpdatedContent = Assert.IsAssignableFrom<Content>(okResult.Value);
            Assert.Equal(updatedContent.Id, actualUpdatedContent.Id);
            Assert.Equal(updatedContent.Title, actualUpdatedContent.Title);
            Assert.Equal(updatedContent.GenreList, actualUpdatedContent.GenreList);

            _mockManager.Verify(m => m.UpdateContent(contentId, It.IsAny<ContentDto>()), Times.Once);
        }
    }
}
