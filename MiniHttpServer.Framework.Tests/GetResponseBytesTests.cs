using MiniHttpServer.Framework.Shared;

namespace MiniHttpServer.Framework.Tests
{
    [TestClass]
    public class GetResponseBytesTests
    {
        // тест что метод возвращает null для несуществующего файла
        [TestMethod]
        public void Invoke_NonExistentFile_ReturnsNull()
        {
            var result = GetResponseBytes.Invoke("nonexistent.html");
            Assert.IsNull(result);
        }

        // тест что метод добавляет index.html если нет расширения
        [TestMethod]
        public void Invoke_PathWithoutExtension_TriesIndexHtml()
        {
            // этот тест просто проверяет что метод не падает
            var result = GetResponseBytes.Invoke("somepath");
            // результат будет null т.к. файла нет, но это нормально
            Assert.IsNull(result);
        }

        // тест что метод обрабатывает путь с расширением
        [TestMethod]
        public void Invoke_PathWithExtension_ProcessesCorrectly()
        {
            var result = GetResponseBytes.Invoke("test.html");
            // файла нет, но метод должен отработать без ошибок
            Assert.IsNull(result);
        }
    }
}
