using MiniHttpServer.Framework.Core.Attributes;

namespace MiniHttpServer.Framework.Tests
{
    [TestClass]
    public class AttributesTests
    {
        // проверяем что атрибут Endpoint можно применить к классу
        [TestMethod]
        public void EndpointAttribute_CanBeAppliedToClass()
        {
            var attr = new EndpointAttribute();
            Assert.IsNotNull(attr);
        }

        // проверяем HttpGet без параметров
        [TestMethod]
        public void HttpGet_WithoutRoute_RouteIsNull()
        {
            var attr = new HttpGet();
            Assert.IsNull(attr.Route);
        }

        // проверяем HttpGet с роутом
        [TestMethod]
        public void HttpGet_WithRoute_RouteIsSet()
        {
            var attr = new HttpGet("/api/test");
            Assert.AreEqual("/api/test", attr.Route);
        }

        // проверяем HttpPost без параметров
        [TestMethod]
        public void HttpPost_WithoutRoute_RouteIsNull()
        {
            var attr = new HttpPost();
            Assert.IsNull(attr.Route);
        }

        // проверяем HttpPost с роутом
        [TestMethod]
        public void HttpPost_WithRoute_RouteIsSet()
        {
            var attr = new HttpPost("/api/create");
            Assert.AreEqual("/api/create", attr.Route);
        }
    }
}
