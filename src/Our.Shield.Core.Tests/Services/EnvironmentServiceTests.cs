using AutoMapper;
using Moq;
using Our.Shield.Core.Data.Accessors;
using Our.Shield.Core.Services;
using System;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web;
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
                Mock.Of<IUmbracoContextAccessor>(),
                Mock.Of<ILocalizedTextService>(),
                Mock.Of<IMapper>(),
                Mock.Of<ILogger>(),
                Mock.Of<DistributedCache>());
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
    }
}
