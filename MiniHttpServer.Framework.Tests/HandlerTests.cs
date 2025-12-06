using MiniHttpServer.Framework.Core.Abstracts;
using System.Net;

namespace MiniHttpServer.Framework.Tests
{
    [TestClass]
    public class HandlerTests
    {
        private class TestHandler : Handler
        {
            public bool WasCalled { get; private set; }

            public override void HandleRequest(HttpListenerContext context)
            {
                WasCalled = true;
                if (Successor != null)
                {
                    Successor.HandleRequest(context);
                }
            }
        }

        [TestMethod]
        public void Handler_CanSetSuccessor()
        {
            var handler1 = new TestHandler();
            var handler2 = new TestHandler();
            
            handler1.Successor = handler2;
            
            Assert.AreEqual(handler2, handler1.Successor);
        }

        [TestMethod]
        public void Handler_CallsSuccessor()
        {
            var handler1 = new TestHandler();
            var handler2 = new TestHandler();
            handler1.Successor = handler2;
            
            handler1.HandleRequest(null);
            
            Assert.IsTrue(handler1.WasCalled);
            Assert.IsTrue(handler2.WasCalled);
        }

        [TestMethod]
        public void Handler_WorksWithoutSuccessor()
        {
            var handler = new TestHandler();
            
            handler.HandleRequest(null);
            
            Assert.IsTrue(handler.WasCalled);
        }
    }
}
