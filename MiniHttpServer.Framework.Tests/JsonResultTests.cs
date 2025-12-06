using MiniHttpServer.Framework.Core.HttpResponse;

namespace MiniHttpServer.Framework.Tests
{
    [TestClass]
    public class JsonResultTests
    {
        private class TestEndpoint : BaseEndpoint
        {
            public IResponseResult CreateJsonResult(object data)
            {
                return Json(data);
            }
        }

        [TestMethod]
        public void JsonResult_CreatesWithData()
        {
            var endpoint = new TestEndpoint();
            var data = new { Name = "Test", Value = 123 };
            
            var result = endpoint.CreateJsonResult(data);
            
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void JsonResult_ImplementsInterface()
        {
            var endpoint = new TestEndpoint();
            var result = endpoint.CreateJsonResult(new { });
            
            Assert.IsInstanceOfType(result, typeof(IResponseResult));
        }

        [TestMethod]
        public void JsonResult_AcceptsNullData()
        {
            var endpoint = new TestEndpoint();
            var result = endpoint.CreateJsonResult(null);
            
            Assert.IsNotNull(result);
        }
    }
}
