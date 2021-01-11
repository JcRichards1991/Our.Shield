using Our.Shield.Shared.Extensions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Our.Shield.Shared.Tests.Extensions
{
    public class EnumerableExtensionTests
    {
        [Theory]
        [InlineData("item1,item2,item3,item4", "item1", 0)]
        [InlineData("item1,item2,item3,item4", "item2", 1)]
        [InlineData("item1,item2,item3,item4", "item3", 2)]
        [InlineData("item1,item2,item3,item4", "item4", 3)]
        public void WhenItemInList_ThenIndexShouldReturn_Success(string csv, string item, int expectedIndex)
        {
            var list = csv.Split(',').ToList();

            Assert.Equal(expectedIndex, list.IndexOf(x => x == item));
        }

        [Fact]
        public void WhenItemNotInList_ThenIndexShouldNotReturn_Success()
        {
            var list = new List<string>
            {
                "item1",
                "item2",
                "item3",
                "item4"
            };

            Assert.Equal(-1, list.IndexOf(x => x == "item5"));
        }
    }
}
