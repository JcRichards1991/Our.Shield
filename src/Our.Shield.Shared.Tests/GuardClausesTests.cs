using System;
using Xunit;

namespace Our.Shield.Shared.Tests
{
    public class GuardClausesTests
    {
        [Fact]
        public void NotNull_Fail()
        {
            object testObj = null;

            Assert.Throws<ArgumentNullException>(() => GuardClauses.NotNull(testObj, nameof(testObj)));
        }

        [Fact]
        public void NotNull_Success()
        {
            object testObject = new
            {
                Prop = "Value"
            };

            GuardClauses.NotNull(testObject, nameof(testObject));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void NotNullOrWhiteSpace_Fail(string testString)
        {
            Assert.Throws<ArgumentNullException>(() => GuardClauses.NotNullOrWhiteSpace(testString, nameof(testString)));
        }

        [Fact]
        public void NotNullOrWhiteSpace_Success()
        {
            var testString = "Test String";

            GuardClauses.NotNullOrWhiteSpace(testString, nameof(testString));
        }
    }
}
