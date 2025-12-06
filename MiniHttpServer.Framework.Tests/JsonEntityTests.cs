using MiniHttpServer.Framework.Settings;

namespace MiniHttpServer.Framework.Tests
{
    [TestClass]
    public class JsonEntityTests
    {
        // тест конструктора с параметрами
        [TestMethod]
        public void JsonEntity_Constructor_SetsAllProperties()
        {
            var entity = new JsonEntity(
                "/login",
                "/olara",
                "/search",
                "/chat",
                "http://search",
                "http://chat",
                "localhost",
                "8080"
            );

            Assert.AreEqual("/login", entity.LoginUri);
            Assert.AreEqual("/olara", entity.OlaraUri);
            Assert.AreEqual("/search", entity.SearcherPath);
            Assert.AreEqual("/chat", entity.ChatGPTPath);
            Assert.AreEqual("http://search", entity.SearcherUri);
            Assert.AreEqual("http://chat", entity.ChatGPTUri);
            Assert.AreEqual("localhost", entity.Domain);
            Assert.AreEqual("8080", entity.Port);
        }

        // тест пустого конструктора
        [TestMethod]
        public void JsonEntity_EmptyConstructor_CreatesInstance()
        {
            var entity = new JsonEntity();
            Assert.IsNotNull(entity);
        }

        // тест что можно установить свойства
        [TestMethod]
        public void JsonEntity_Properties_CanBeSet()
        {
            var entity = new JsonEntity();
            entity.SmtpServer = "smtp.gmail.com";
            entity.SmtpPort = 587;
            entity.FromEmail = "test@test.com";
            entity.AppPassword = "password123";
            entity.ConnectionString = "Server=localhost";

            Assert.AreEqual("smtp.gmail.com", entity.SmtpServer);
            Assert.AreEqual(587, entity.SmtpPort);
            Assert.AreEqual("test@test.com", entity.FromEmail);
            Assert.AreEqual("password123", entity.AppPassword);
            Assert.AreEqual("Server=localhost", entity.ConnectionString);
        }
    }
}
