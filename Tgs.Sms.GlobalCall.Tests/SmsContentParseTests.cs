using System.Net;
using NUnit.Framework;

namespace Tgs.Sms.GlobalCall.Tests
{
    public class SmsContentParseTests
    {
        [Test]
        public void Sms_Content_Encode_1()
        {
            var srcText = "網頁";

            var xmlEncodeString = Microsoft.Security.Application.Encoder.XmlEncode(srcText);

            var actual = WebUtility.UrlEncode(xmlEncodeString);

            Assert.AreEqual("%26%2332178%3B%26%2338913%3B", actual);
        }

        [Test]
        public void Sms_Content_Encode_2()
        {
            var srcText = "网页";

            var xmlEncodeString = Microsoft.Security.Application.Encoder.XmlEncode(srcText);

            var actual = WebUtility.UrlEncode(xmlEncodeString);

            Assert.AreEqual("%26%2332593%3B%26%2339029%3B", actual);
        }
    }
}
