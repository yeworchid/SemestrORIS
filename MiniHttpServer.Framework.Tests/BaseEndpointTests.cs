using MiniHttpServer.Framework.Core.HttpResponse;

namespace MiniHttpServer.Framework.Tests
{
    [TestClass]
    public class BaseEndpointTests
    {
        private class TestEndpoint : BaseEndpoint
        {
            public IResponseResult TestJson(object data)
            {
                return Json(data);
            }

            public IResponseResult TestPage(string path, object data)
            {
                return Page(path, data);
            }
        }

        [TestMethod]
        public void BaseEndpoint_CanCreateInstance()
        {
            var endpoint = new TestEndpoint();
            Assert.IsNotNull(endpoint);
        }

        [TestMethod]
        public void BaseEndpoint_JsonMethod_ReturnsResult()
        {
            var endpoint = new TestEndpoint();
            var data = new { test = "value" };
            var result = endpoint.TestJson(data);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void BaseEndpoint_PageMethod_ReturnsResult()
        {
            var endpoint = new TestEndpoint();
            var result = endpoint.TestPage("test.html", new { });
            Assert.IsNotNull(result);
        }
    }
}
