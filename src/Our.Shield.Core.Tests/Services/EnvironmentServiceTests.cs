using AutoMapper;
using Moq;
using Our.Shield.Core.Data.Accessors;
using Our.Shield.Core.Services;
using System;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Web.Cache;
using Xunit;

namespace Our.Shield.Core.Tests.Services
{
    public class EnvironmentServiceTests
    {
        private readonly IEnvironmentService _environmentService;

        public EnvironmentServiceTests()
        {
            _environmentService = new EnvironmentService(
                Mock.Of<IJobService>(),
                MockEnvironmentAccess(),
                Mock.Of<DistributedCache>(),
                Mock.Of<IMapper>(),
                Mock.Of<ILogger>());
        }

        private IEnvironmentAccessor MockEnvironmentAccess()
        {
            var dataAccessor = new Mock<IEnvironmentAccessor>();

            //dataAccessor
            //    .Setup(x => x.Create(It.IsAny<Models.IEnvironment>()))
            //    .ReturnsAsync(new Guid(""));

            return dataAccessor.Object;
        }

        [Fact]
        public async Task WhenNull_Upsert_ShouldThrowException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _environmentService.UpsertAsync(null));
        }

        [Fact]
        public async Task WhenNotNullAndNoKey_Upsert_ShouldInsertEnvironment()
        {
            var env = new Models.Requests.UpsertEnvironmentRequest
            {
                Name = "Test Environment",
                Enabled = true,
                Icon = "icon-settings"
            };

            var result = await _environmentService.UpsertAsync(env);

            Assert.False(result.HasError());
        }

        [Fact]
        public async Task WhenNotNullAndWithKey_Upsert_ShouldUpdateEnvironment()
        {
            var env = new Models.Requests.UpsertEnvironmentRequest
            {
                Key = Guid.NewGuid(),
                Name = "Test Environment",
                Enabled = true,
                Icon = "icon-settings"
            };

            var result = await _environmentService.UpsertAsync(env);

            Assert.False(result.HasError());
        }

        [Fact]
        public async Task WhenEmptyGuid_Delete_ShouldThrowException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _environmentService.DeleteAsync(Guid.Empty));
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

            await _environmentService.UpsertAsync(env);

            var response = await _environmentService.DeleteAsync(env.Key);

            Assert.True(response.Successful);
        }
    }
}
