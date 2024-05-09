using System;
using System.IO;
using Google.Protobuf;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Ipfs
{
    [TestFixture]
    public class NetworkProtocolTest
    {
        [Test]
        public void Stringing()
        {
            Assert.AreEqual("/tcp/8080", new MultiAddress("/tcp/8080").Protocols[0].ToString());
        }

        [Test]
        public void Register_Name_Already_Exists()
        {
            ExceptionAssert.Throws<ArgumentException>(() => NetworkProtocol.Register<NameExists>());
        }

        [Test]
        public void Register_Code_Already_Exists()
        {
            ExceptionAssert.Throws<ArgumentException>(() => NetworkProtocol.Register<CodeExists>());
        }

        private class NameExists : NetworkProtocol
        {
            public override string Name => "tcp";
            public override uint Code => 0x7FFF;
            public override void ReadValue(CodedInputStream stream) { }
            public override void ReadValue(TextReader stream) { }
            public override void WriteValue(CodedOutputStream stream) { }
        }

        private class CodeExists : NetworkProtocol
        {
            public override string Name => "x-tcp";
            public override uint Code => 6;
            public override void ReadValue(CodedInputStream stream) { }
            public override void ReadValue(TextReader stream) { }
            public override void WriteValue(CodedOutputStream stream) { }
        }
    }
}
