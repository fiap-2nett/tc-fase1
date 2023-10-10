using Xunit;
using FluentAssertions;
using NetArchTest.Rules;

namespace TechChallenge.ArchitectureTests
{
    public class DependencyTests
    {
        #region Constants

        private const string DomainNamespace = nameof(Domain);
        private const string ApplicationNamespace = nameof(Application);
        private const string InfrastructureNamespace = nameof(Infrastructure);
        private const string PersistenteNamespace = nameof(Persistence);
        private const string ApiNamespace = nameof(Api);

        private const string DomainEntitiesNamespace = $"{nameof(Domain)}.Entities";
        private const string ApplicationServicesNamespace = $"{nameof(Application)}.Services";
        private const string PersistenceRepositoriesNamespace = $"{nameof(Persistence)}.Repositories";
        private const string ApiControllersNamespace = $"{nameof(Api)}.Controllers";

        #endregion

        #region Test Scenarios

        [Fact]
        public void Domain_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange
            var assembly = typeof(Domain.AssemblyReference).Assembly;

            var otherProjects = new[]
            {
                ApplicationNamespace,
                InfrastructureNamespace,
                PersistenteNamespace,
                ApiNamespace
            };

            // Atc
            var testResult = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAll(otherProjects)
                .GetResult();

            // Assert
            testResult.IsSuccessful.Should().BeTrue();            
        }

        [Fact]
        public void Application_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange
            var assembly = typeof(Application.AssemblyReference).Assembly;

            var otherProjects = new[]
            {
                InfrastructureNamespace,
                PersistenteNamespace,
                ApiNamespace
            };

            // Atc
            var testResult = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAll(otherProjects)
                .GetResult();

            // Assert
            testResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Infrastructure_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange
            var assembly = typeof(Infrastructure.AssemblyReference).Assembly;

            var otherProjects = new[]
            {
                PersistenteNamespace,
                ApiNamespace
            };

            // Atc
            var testResult = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAll(otherProjects)
                .GetResult();

            // Assert
            testResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Persistence_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange
            var assembly = typeof(Persistence.AssemblyReference).Assembly;

            var otherProjects = new[]
            {
                PersistenteNamespace,
                ApiNamespace
            };

            // Atc
            var testResult = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAll(otherProjects)
                .GetResult();

            // Assert
            testResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Entities_Must_DerivedFromEntity()
        {
            // Arrange
            var assembly = typeof(Domain.AssemblyReference).Assembly;

            // Act
            var testResult = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace(DomainEntitiesNamespace)
                .Should()
                .Inherit(typeof(Domain.Core.Primitives.Entity<>))
                .GetResult();

            // Assert
            testResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Services_Should_Have_DependencyOnDomain()
        {
            // Arrange
            var assembly = typeof(Application.AssemblyReference).Assembly;

            // Act
            var testResult = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace(ApplicationServicesNamespace)
                .Should()
                .HaveDependencyOn(DomainNamespace)
                .GetResult();

            // Assert
            testResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Services_Should_Be_SealedClasses()
        {
            // Arrange
            var assembly = typeof(Application.AssemblyReference).Assembly;

            // Act
            var testResult = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace(ApplicationServicesNamespace)
                .Should()
                .BeSealed()
                .GetResult();

            // Assert
            testResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Repositories_Should_Be_SealedClasses()
        {
            // Arrange
            var assembly = typeof(Persistence.AssemblyReference).Assembly;

            // Act
            var testResult = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace(PersistenceRepositoriesNamespace)
                .Should()
                .BeSealed()
                .GetResult();

            // Assert
            testResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Controllers_Should_Be_SealedClasses()
        {
            // Arrange
            var assembly = typeof(Persistence.AssemblyReference).Assembly;

            // Act
            var testResult = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace(ApiControllersNamespace)
                .Should()
                .BeSealed()
                .GetResult();

            // Assert
            testResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Controllers_Must_InheritFromApiController()
        {
            // Arrange
            var assembly = typeof(Persistence.AssemblyReference).Assembly;

            // Act
            var testResult = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace(ApiControllersNamespace)
                .Should()
                .Inherit(typeof(Api.Infrastructure.ApiController))
                .GetResult();

            // Assert
            testResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Controllers_Should_Have_DependencyOnApplication()
        {
            // Arrange
            var assembly = typeof(Api.AssemblyReference).Assembly;

            // Act
            var testResult = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace(ApiControllersNamespace)
                .Should()
                .HaveDependencyOn(ApplicationNamespace)
                .GetResult();

            // Assert
            testResult.IsSuccessful.Should().BeTrue();
        }

        #endregion
    }
}
