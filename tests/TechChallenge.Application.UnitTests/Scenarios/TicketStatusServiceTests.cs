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
    public sealed class TicketStatusServiceTests
    {
        #region Read-Only Fields

        private readonly Mock<IDbContext> _dbContextMock;

        #endregion

        #region Constructors

        public TicketStatusServiceTests()
        {
            _dbContextMock = new();
        }

        #endregion

        #region Unit Tests

        #region GetAsync

        [Fact]
        public async Task GetAsync_Should_ReturnStatusResponseEnumerableAsync()
        {
            // Arrange            
            _dbContextMock.Setup(x => x.Set<TicketStatus, byte>()).ReturnsDbSet(TicketStatusList());

            var ticketStatusService = new TicketStatusService(_dbContextMock.Object);

            // Act
            var testResult = await ticketStatusService.GetAsync();

            // Assert
            testResult.Should().NotBeNull();
            testResult.Should().HaveCount(TicketStatusList().Count());
            testResult.All(c => c.IdStatus > 0).Should().BeTrue();
            testResult.All(c => !c.Name.IsNullOrWhiteSpace()).Should().BeTrue();
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_Should_ReturnStatusResponseAsync()
        {
            // Arrange
            _dbContextMock.Setup(x => x.Set<TicketStatus, byte>()).ReturnsDbSet(TicketStatusList());

            var expectedResult = TicketStatusList().FirstOrDefault();
            var ticketStatusService = new TicketStatusService(_dbContextMock.Object);

            // Act
            var testResult = await ticketStatusService.GetByIdAsync(expectedResult.Id);

            // Assert
            testResult.Should().NotBeNull();
            testResult.IdStatus.Should().Be(expectedResult.Id);
            testResult.Name.Should().Be(expectedResult.Name);
        }

        #endregion

        #endregion

        #region Private Methods

        private IEnumerable<TicketStatus> TicketStatusList()
        {
            yield return new TicketStatus(idTicketStatus: (byte)TicketStatuses.New, name: "Novo");
            yield return new TicketStatus(idTicketStatus: (byte)TicketStatuses.Assigned, name: "Atribuído");
            yield return new TicketStatus(idTicketStatus: (byte)TicketStatuses.InProgress, name: "Em andamento");
            yield return new TicketStatus(idTicketStatus: (byte)TicketStatuses.OnHold, name: "Em espera");
            yield return new TicketStatus(idTicketStatus: (byte)TicketStatuses.Completed, name: "Concluído");
            yield return new TicketStatus(idTicketStatus: (byte)TicketStatuses.Cancelled, name: "Cancelado");
        }

        #endregion
    }
}
