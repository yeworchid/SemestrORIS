using MiniHttpServer.Framework.Shared;

namespace MiniHttpServer.Framework.Tests
{
    [TestClass]
    public class ContentTypeExtendedTests
    {
        // тесты для шрифтов
        [TestMethod]
        public void GetContentType_WoffFile_ReturnsFontWoff()
        {
            var result = ContentType.GetContentType("font.woff");
            Assert.AreEqual("font/woff", result);
        }

        [TestMethod]
        public void GetContentType_Woff2File_ReturnsFontWoff2()
        {
            var result = ContentType.GetContentType("font.woff2");
            Assert.AreEqual("font/woff2", result);
        }

        [TestMethod]
        public void GetContentType_TtfFile_ReturnsFontTtf()
        {
            var result = ContentType.GetContentType("font.ttf");
            Assert.AreEqual("font/ttf", result);
        }

        // тесты для других картинок
        [TestMethod]
        public void GetContentType_GifFile_ReturnsImageGif()
        {
            var result = ContentType.GetContentType("animation.gif");
            Assert.AreEqual("image/gif", result);
        }

        [TestMethod]
        public void GetContentType_SvgFile_ReturnsImageSvg()
        {
            var result = ContentType.GetContentType("icon.svg");
            Assert.AreEqual("image/svg+xml", result);
        }

        [TestMethod]
        public void GetContentType_WebpFile_ReturnsImageWebp()
        {
            var result = ContentType.GetContentType("photo.webp");
            Assert.AreEqual("image/webp", result);
        }

        [TestMethod]
        public void GetContentType_IcoFile_ReturnsImageIcon()
        {
            var result = ContentType.GetContentType("favicon.ico");
            Assert.AreEqual("image/x-icon", result);
        }

        // тест для текстовых файлов
        [TestMethod]
        public void GetContentType_TxtFile_ReturnsTextPlain()
        {
            var result = ContentType.GetContentType("readme.txt");
            Assert.AreEqual("text/plain; charset=UTF-8", result);
        }

        // тест для php (возвращает html)
        [TestMethod]
        public void GetContentType_PhpFile_ReturnsTextHtml()
        {
            var result = ContentType.GetContentType("index.php");
            Assert.AreEqual("text/html; charset=UTF-8", result);
        }

        // тест для htm
        [TestMethod]
        public void GetContentType_HtmFile_ReturnsTextHtml()
        {
            var result = ContentType.GetContentType("page.htm");
            Assert.AreEqual("text/html; charset=UTF-8", result);
        }

        // тест для jpeg
        [TestMethod]
        public void GetContentType_JpegFile_ReturnsImageJpeg()
        {
            var result = ContentType.GetContentType("image.jpeg");
            Assert.AreEqual("image/jpeg", result);
        }
    }
}
