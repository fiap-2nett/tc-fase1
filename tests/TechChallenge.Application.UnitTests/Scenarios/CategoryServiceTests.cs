using Moq;
using Xunit;
using System;
using System.Linq;
using FluentAssertions;
using System.Threading.Tasks;
using Moq.EntityFrameworkCore;
using System.Collections.Generic;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Extensions;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Application.Services;
using TechChallenge.Application.Core.Abstractions.Data;

namespace TechChallenge.Application.UnitTests.Scenarios
{
    public sealed class CategoryServiceTests
    {
        #region Read-Only Fields

        private readonly Mock<IDbContext> _dbContextMock;

        #endregion

        #region Constructors

        public CategoryServiceTests()
        {
            _dbContextMock = new();
        }

        #endregion

        #region Unit Tests

        #region GetAsync

        [Fact]
        public async Task GetAsync_Should_ReturnCategoryResponseEnumerableAsync()
        {
            // Arrange
            _dbContextMock.Setup(x => x.Set<Category, int>()).ReturnsDbSet(CategoryList());
            _dbContextMock.Setup(x => x.Set<Priority, byte>()).ReturnsDbSet(PriorityList());

            var categoryService = new CategoryService(_dbContextMock.Object);

            // Act
            var testResult = await categoryService.GetAsync();

            // Assert
            testResult.Should().NotBeNull();
            testResult.Should().HaveCount(CategoryList().Count());
            testResult.All(c => c.IdCategory > 0).Should().BeTrue();
            testResult.All(c => !c.Name.IsNullOrWhiteSpace()).Should().BeTrue();
        }

        [Fact]
        public async Task GetByIdAsync_Should_ReturnDetailedCategoryResponseAsync()
        {
            // Arrange
            _dbContextMock.Setup(x => x.Set<Category, int>()).ReturnsDbSet(CategoryList());
            _dbContextMock.Setup(x => x.Set<Priority, byte>()).ReturnsDbSet(PriorityList());

            var expectedResult = CategoryList().FirstOrDefault();
            var categoryService = new CategoryService(_dbContextMock.Object);

            // Act
            var testResult = await categoryService.GetByIdAsync(expectedResult.Id);

            // Assert
            testResult.Should().NotBeNull();
            testResult.IdCategory.Should().Be(expectedResult.Id);
            testResult.Name.Should().Be(expectedResult.Name);            

            testResult.Priority.Should().NotBeNull();
            testResult.Priority.IdPriority.Should().Be(PriorityList().FirstOrDefault(p => p.Id == expectedResult.IdPriority).Id);
            testResult.Priority.Name.Should().Be(PriorityList().FirstOrDefault(p => p.Id == expectedResult.IdPriority).Name);
        }

        #endregion

        #endregion

        #region Private Methods

        private IEnumerable<Priority> PriorityList()
        {
            yield return new Priority(idPriority: (byte)Priorities.Low, name: "Baixa", sla: 48);
            yield return new Priority(idPriority: (byte)Priorities.Medium, name: "Média", sla: 24);
            yield return new Priority(idPriority: (byte)Priorities.High, name: "Alta", sla: 8);
            yield return new Priority(idPriority: (byte)Priorities.Criticial, name: "Crítico", sla: 4);
        }

        private IEnumerable<Category> CategoryList()
        {
            yield return new Category(idCategory: 1, idPriority: (byte)Priorities.Criticial, name: "Indisponibilidade", description: default);
            yield return new Category(idCategory: 2, idPriority: (byte)Priorities.High, name: "Lentidão", description: default);
            yield return new Category(idCategory: 3, idPriority: (byte)Priorities.Medium, name: "Requisição", description: default);
            yield return new Category(idCategory: 4, idPriority: (byte)Priorities.Low, name: "Dúvida", description: default);
        }

        #endregion
    }
}
