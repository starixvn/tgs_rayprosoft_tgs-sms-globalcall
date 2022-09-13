using System;
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

            var actual = Uri.EscapeDataString(xmlEncodeString);

            Assert.AreEqual("%26%2332178%3B%26%2338913%3B", actual);
        }

        [Test]
        public void Sms_Content_Encode_2()
        {
            var srcText = "网页";

            var xmlEncodeString = Microsoft.Security.Application.Encoder.XmlEncode(srcText);

            var actual = Uri.EscapeDataString(xmlEncodeString);

            Assert.AreEqual("%26%2332593%3B%26%2339029%3B", actual);
        }

        [Test]
        public void Sms_Content_Encode_Space()
        {
            var srcText = " ";

            var actual = Uri.EscapeUriString(srcText);

            Assert.AreEqual("%20", actual);
        }

        [Test]
        public void Sms_Content_Encode_3()
        {
            var srcText = "Your ID verification code is: 277860";

            var xmlEncodeString = Microsoft.Security.Application.Encoder.XmlEncode(srcText);

            var actual = Uri.EscapeDataString(xmlEncodeString);

            Assert.AreEqual("Your%20ID%20verification%20code%20is%3A%20277860", actual);
        }
    }
}
