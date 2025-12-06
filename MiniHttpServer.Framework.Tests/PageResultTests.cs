using MiniHttpServer.Framework.Core.HttpResponse;

namespace MiniHttpServer.Framework.Tests
{
    [TestClass]
    public class PageResultTests
    {
        private class TestEndpoint : BaseEndpoint
        {
            public IResponseResult CreatePageResult(string path, object data)
            {
                return Page(path, data);
            }
        }

        [TestMethod]
        public void PageResult_CreatesWithPathAndData()
        {
            var endpoint = new TestEndpoint();
            var data = new { Title = "Test Page" };
            var path = "template.html";
            
            var result = endpoint.CreatePageResult(path, data);
            
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PageResult_ImplementsInterface()
        {
            var endpoint = new TestEndpoint();
            var result = endpoint.CreatePageResult("test.html", new { });
            
            Assert.IsInstanceOfType(result, typeof(IResponseResult));
        }

        [TestMethod]
        public void PageResult_AcceptsEmptyPath()
        {
            var endpoint = new TestEndpoint();
            var result = endpoint.CreatePageResult("", new { });
            
            Assert.IsNotNull(result);
        }
    }
}
