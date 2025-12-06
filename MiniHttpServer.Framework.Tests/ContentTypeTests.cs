using MiniHttpServer.Framework.Shared;

namespace MiniHttpServer.Framework.Tests
{
    [TestClass]
    public class ContentTypeTests
    {
        // тест для html файлов
        [TestMethod]
        public void GetContentType_HtmlFile_ReturnsTextHtml()
        {
            var result = ContentType.GetContentType("index.html");
            Assert.AreEqual("text/html; charset=UTF-8", result);
        }

        // тест для css
        [TestMethod]
        public void GetContentType_CssFile_ReturnsTextCss()
        {
            var result = ContentType.GetContentType("style.css");
            Assert.AreEqual("text/css; charset=UTF-8", result);
        }

        // тест для js
        [TestMethod]
        public void GetContentType_JsFile_ReturnsApplicationJs()
        {
            var result = ContentType.GetContentType("script.js");
            Assert.AreEqual("application/javascript; charset=UTF-8", result);
        }

        // тест для json
        [TestMethod]
        public void GetContentType_JsonFile_ReturnsApplicationJson()
        {
            var result = ContentType.GetContentType("data.json");
            Assert.AreEqual("application/json; charset=UTF-8", result);
        }

        // тест для картинок png
        [TestMethod]
        public void GetContentType_PngFile_ReturnsImagePng()
        {
            var result = ContentType.GetContentType("image.png");
            Assert.AreEqual("image/png", result);
        }

        // тест для jpg
        [TestMethod]
        public void GetContentType_JpgFile_ReturnsImageJpeg()
        {
            var result = ContentType.GetContentType("photo.jpg");
            Assert.AreEqual("image/jpeg", result);
        }

        // тест для неизвестного расширения
        [TestMethod]
        public void GetContentType_UnknownExtension_ReturnsDefaultHtml()
        {
            var result = ContentType.GetContentType("file.xyz");
            Assert.AreEqual("text/html; charset=UTF-8", result);
        }

        // тест что регистр не важен
        [TestMethod]
        public void GetContentType_UpperCaseExtension_ReturnsCorrectType()
        {
            var result = ContentType.GetContentType("FILE.CSS");
            Assert.AreEqual("text/css; charset=UTF-8", result);
        }
    }
}
