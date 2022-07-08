using BlogProject.Data;
using BlogProject.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DeveloperBlog.xUnitTests
{
    public class BasicSlugServiceTests
    {
        // Properties
        private readonly BasicSlugService _sut;

        // Constructor
        public BasicSlugServiceTests()
        {
            _sut = new BasicSlugService(null);
        }

        
        [Fact]
        public void TestUrlFriendly_ToLowerCase()
        {
            string testTitle = "TESTTOLOWERCASE";
            string expected = "testtolowercase";

            var actual = _sut.UrlFriendly(testTitle);

            Assert.Equal(expected, actual);
        }
    }
}