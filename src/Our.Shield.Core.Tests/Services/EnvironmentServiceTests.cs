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
                Mock.Of<IAppService>(),
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
        public async Task Upsert_ShouldThrowException_WhenNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _environmentService.Upsert(null));
        }

        [Fact]
        public async Task Upsert_ShouldInsertEnvironment()
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
        public async Task Upsert_ShouldUpdateEnvironment()
        {
            var env = new Models.Requests.UpsertEnvironmentRequest
            {
                Key = Guid.NewGuid(),
                Name = "Test Environment",
                Enabled = true,
                Icon = "icon-settings"
            };

            var result = await _environmentService.Upsert(env);

            Assert.False(result.HasError());
        }

        [Fact]
        public async Task Delete_ShouldThrowException_WhenEmptyGuid()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _environmentService.Delete(Guid.Empty));
        }

        [Fact]
        public async Task Delete_ShouldRemoveEnvironment_WhenNotNull()
        {
            var env = new Models.Requests.UpsertEnvironmentRequest
            {
                Name = "Test Environment",
                Enabled = true,
                Icon = "icon-settings"
            };

            await _environmentService.Upsert(env);

            var response = await _environmentService.Delete(env.Key);

            Assert.True(response.Successful);
        }
    }
}
