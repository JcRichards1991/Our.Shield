using Moq;
using Our.Shield.Core.Data.Accessors;
using Our.Shield.Core.Services;
using Our.Shield.Core.Tests.Implementers.Data.Accessors;
using System;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Xunit;

namespace Our.Shield.Core.Tests.Services
{
    public class EnvironmentServiceTests
    {
        protected readonly IEnvironmentAccessor _dataAccessor;
        protected readonly IEnvironmentService _environmentService;

        public EnvironmentServiceTests()
        {
            _dataAccessor = new TestEnvironmentAccessor();
            _environmentService = new EnvironmentService(_dataAccessor, Mock.Of<ILogger>());
        }

        [Fact]
        public async Task WhenNull_Upsert_ShouldThrowException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _environmentService.Upsert(null));
        }

        [Fact]
        public async Task WhenNotNull_Upsert_ShouldInsertOrUpdateEnvironment()
        {
            var env = new Models.Environment
            {
                Name = "Test Env",
                Enabled = true,
                Icon = "icon-cog",
                SortOrder = 0
            };

            Assert.True(await _environmentService.Upsert(env));
        }

        [Fact]
        public async Task WhenNull_Delete_ShouldThrowException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _environmentService.Delete(null));
        }

        [Fact]
        public async Task WhenNotNull_Delete_ShouldRemoveEnvironment()
        {
            var env = new Models.Environment
            {
                Name = "Test Env",
                Enabled = true,
                Icon = "icon-cog",
                SortOrder = 0
            };

            await _environmentService.Upsert(env);

            var result = await _environmentService.Delete(env);

            Assert.True(result);
        }
    }
}
