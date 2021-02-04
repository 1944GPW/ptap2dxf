// Some basic Unit Tests for the Ptap2DXF program
// Steve Malikoff 2017

using System;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ptap2DXF;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTests
    {
        readonly PTAP2DXF fakeApp = new PTAP2DXF();

        /// <summary>
        /// TEST BannerText - that List<byte>() {0, 126, 9, 9, 9, 126, 0} == 'A'
        /// </summary>
        [TestMethod]
        public void TestMethod1()
        {
            byte[] result = fakeApp.BannerText("A");
            if (result[0] == 0 && result[1] == 126 && result[2] == 9 && result[3] == 9 && result[4] == 9 && result[5] == 126 && result[6] == 0)
                Console.WriteLine("ok");
            else
                throw new FormatException();
        }


        /// <summary>
        /// TEST ToBitString - that the bit array {true, false, false, false, false, false, true, false} converts to the string "010000001"
        /// NOTE: Least Significant Bit here represents the LOWEST index value in the bit array (reversed from the adopted 8x8 character set pattern)
        /// </summary>
        [TestMethod]
        public void TestMethod2()
        {
            BitArray letterAasBits = new BitArray(new bool[] {true, false, false, false, false, false, true, false});
            string   letterAasString = "01000001";
            string result = PaperTapeExtensions.ToBitString(letterAasBits);
            if (result != letterAasString)
                throw new FormatException();
        }

        /// <summary>
        /// TEST ToChar - that the bit array {true, false, false, false, false, false, true, false} converts to the character 'A'
        /// </summary>
        [TestMethod]
        public void TestMethod3()
        {
            BitArray letterAasBits = new BitArray(new bool[] { true, false, false, false, false, false, true, false });
            char letterAasChar = 'A';
            char result = PaperTapeExtensions.ToChar(letterAasBits);
            if (result != letterAasChar)
                throw new FormatException();
        }

        /// <summary>
        /// TEST Usage - that the console help prints out correctly
        /// </summary>
        [TestMethod]
        public void TestMethod4()
        {
            Program.Usage();
        }
    }
}
