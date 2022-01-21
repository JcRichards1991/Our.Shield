using System;
using System.Collections.Generic;
using Xunit;

namespace Our.Shield.Shared.Tests
{
    public class GuardClausesTests
    {
        [Fact]
        public void WhenNull_ThenShouldThrowException()
        {
            object testObj = null;

            Assert.Throws<ArgumentNullException>(() => GuardClauses.NotNull(testObj, nameof(testObj)));
        }

        [Fact]
        public void WhenNotNull_ThenShouldNotThrowException()
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
        public void WhenStringIsNullOrWhiteSpace_ThenShouldThrowException(string testString)
        {
            Assert.Throws<ArgumentNullException>(() => GuardClauses.NotNullOrWhiteSpace(testString, nameof(testString)));
        }

        [Fact]
        public void WhenStringIsNotNullOrWhiteSpace_ThenShouldNotThrowException()
        {
            var testString = "Test String";

            GuardClauses.NotNullOrWhiteSpace(testString, nameof(testString));
        }

        [Fact]
        public void WhenListIsNull_ThenShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => GuardClauses.NotEmpty<string>(null, nameof(List<string>)));
        }

        [Fact]
        public void WhenListIsEmpty_ThenShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => GuardClauses.NotEmpty(new List<string>(), nameof(List<string>)));
        }

        [Fact]
        public void WhenListIsNotNullOrEmpty_ThenShouldNotThrowException()
        {
            var list = new List<string>
            {
                "value1",
                "value2",
                "value3"
            };

            GuardClauses.NotEmpty(list, nameof(list));
        }
    }
}
