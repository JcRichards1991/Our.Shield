using Moq;
using Our.Shield.Core.Data.Accessors;
using Our.Shield.Core.Services;
using System;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Web.Cache;
using Xunit;
using AutoMapper;

namespace Our.Shield.Core.Tests.Services
{
    public class EnvironmentServiceTests
    {
        private readonly IEnvironmentService _environmentService;

        public EnvironmentServiceTests()
        {
            _environmentService = new EnvironmentService(
                Mock.Of<IJobService>(),
                Mock.Of<IEnvironmentAccessor>(),
                Mock.Of<DistributedCache>(),
                Mock.Of<IMapper>(),
                Mock.Of<ILogger>());
        }

        [Fact]
        public async Task WhenNull_Upsert_ShouldThrowException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _environmentService.Upsert(null));
        }

        [Fact]
        public async Task WhenNotNull_Upsert_ShouldInsertOrUpdateEnvironment()
        {
            var env = new Models.Requests.UpsertEnvironmentRequest
            {
                Name = "Test Environment",
                Enabled = true,
                Icon = "icon-settings"
            };

            var result = await _environmentService.Upsert(env);

            Assert.False(result.HasError());
        }

        [Fact]
        public async Task WhenNull_Delete_ShouldThrowException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _environmentService.Delete(null));
        }

        [Fact]
        public async Task WhenNotNull_Delete_ShouldRemoveEnvironment()
        {
            var env = new Models.Requests.UpsertEnvironmentRequest
            {
                Name = "Test Environment",
                Enabled = true,
                Icon = "icon-settings"
            };

            await _environmentService.Upsert(env);

            var result = await _environmentService.Delete(env.Key);

            Assert.True(result);
        }
    }
}
