// PTAP2DXF - Paper tape binary image to Drawing Interchange Format
//
// A command line tool for Windows, Linux and Mac that generates one of more DXFs (Drawing Interchange Files) of up to 8-level ASCII teletype paper tape from an input binary file.
// The output DXF in turn is then loaded onto a home CNC stencil cutting machine to produce sections of paper tape that can be stuck together with tape joiners 
// (generated with the /JOINER option) to produce a paper tape that should be readable by an ASR-33 Teletype or other vintage device.
// The program can also generate text banners in an 8x8 font on paper tape, or a banner can be prepended as a label to a data tape output eg. 'ABS LDR'
// Baudot 5-level tapes can be generated, or custom bit-width tapes.
// Run with no arguments to get command help, or use the /HELP or --help parameter.
// Builds under Visual Studio versions 2008, 2010, 2013, 2015 and 2017. Builds under Mono and VS2017 .NET Core. 
// 
// Written in C# by Steve Malikoff 2017 in Brisbane, Australia. Uses the DxfMaker library by David S Tufts.
// You can contact me at steven <at> malikoff <dot> com or see the project page at https://github.com/1944GPW for more details.
//
// No warranties or fit-for-purpose given or implied. Use at your own risk.
// Feel free to modify as you wish, but please leave this attribution and authors details intact, thank you.
//
// Building instructions
// The ptap2dxf solution should build in VS2008 or higher, as a standalone EXE.
// For .NET Core on Windows, run the exe as follows (git bash shown here):
//     Change to  $ cd ./ptap2dxf/ptap2dxf/bin/Debug/netcoreapp2.1
//     Run with   $ dotnet ptap2dxf.dll --help
// 
// For .NET Core for Linux or Mac use VS2017 or VS2019 (Community Edition works fine). Run 'Restore Nuget Packages' for the solution.
//
// For .NET Core leave the following defined. Actually you can leave DOTNETCORE defined for all builds on DNX, COREFX or Mono.
#define DOTNETCORE

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JA.Planar;    // DxfMaker DXF-generation class by David S. Tufts  https://twitter.com/davidscotttufts

namespace Ptap2DXF
{
    /// <summary>
    /// For console output, the user can have the line numbering apply to a particular section if desired.
    /// The default is to only number the actual data bytes (CODE) not the leader, trailer or banner so that those 
    /// values can be re-input to make the adhesive tape joiners. All is useful when leader and trailer tape is required.
    /// </summary>
    [Flags]
    public enum Numbering { NONE = 0, BANNER = 1, LEADER = 2, CODE = 4, TRAILER = 8, ALL = 15 }
    public enum Parity { NONE = 0, EVEN = 1, ODD = 2 }

    /// <summary>
    /// Main class to deal with processing the input, generation of hole patterns, output.
    /// </summary>
    public class PTAP2DXF
    {
        public DxfMaker dxf = new DxfMaker();   // Instantiate the class

        /// <summary>
        /// Default constructor
        /// </summary>
        public PTAP2DXF()
        {
        }

        /// <summary>
        /// Paper tape generation function.
        /// Takes a lot of parameters to control the input, output, sectioning, positioning and extra output.
        /// </summary>
        /// <param name="someInputFileName">Path to intput file eg. 'DEC-11-L2PC-PO.ptap'</param>
        /// <param name="someOutputFileName">Path to output file, written with a .DXF filetype eg. DEC-11-L2PC-PO.DXF</param>
        /// <param name="startOfData">Arbitrary start byte in input file</param>
        /// <param name="lengthOfData">Arbitrary length in bytes of generated sequence</param>
        /// <param name="rowsPerSegment">Hole rows per single piece of paper tape</param>
        /// <param name="interSegmentGap">Normally zero, so that twp adjacent segments share a common edge for CNC cutting. If non-zero then that gives a small gap and seperate edge to each segment </param>
        /// <param name="segmentsPerDXF">The desired number of equal-length one-inch-wide segments adjacent to eachother required to fit left to right across a CNC cutting mat</param>
        /// <param name="leader">Length of blank leader tape, specified as # of rows of holes</param>
        /// <param name="trailer">Length of blank trailer tape, specified as # of rows of holes</param>
        /// <param name="mark">Console output character to represent a mark (data bit = 1)</param>
        /// <param name="space">Console output character to represent a space (data bit = 0)</param>
        /// <param name="drawvee">Draw start '/\' and end '/\' ends on first and last segments respectively</param>
        /// <param name="joiningTape">Normally false for regular paper tape lengths. If true, produces a punched tape piece that has tabs each side. Used for cutting joiners from adhesive material</param>
        /// <param name="level">Number of data bits to be punched eg. 8 for 8-level (byte-width ASR33, PC05 etc.) or 5 for 5-level (Baudot). Default is 8</param>
        /// <param name="sprocketPos">Position of sprocket feed hole. Zero puts it at the right edge, 7 at left edge. For the default 8-level tape it is between the 3rd and 4th data holes from the right, ie. 3</param>
        /// <param name="mirror">Produce a horizontal mirror of the tape. Mostly used in conjuncton with the joiner, so that adhesive tape sections can be stuck to the underside of the paper tape</param>
        /// <param name="messageText">Text message string to be punched directly, abrogating an input file or banner text</param>
        /// <param name="quiet">Supress console output</param>
        /// <param name="showLineNumbers">Show line number for each row at left of console output</param>
        /// <param name="showASCIIchars">Show ASCII character of each row. Control characters are represented in angled brackets</param>
        /// <param name="showControlChars">Show control characters in output</param>
        /// <param name="dryRun">Run generation of paper tape and DXF but do not produce the output file</param>
        /// <param name="invertPattern">Invert the Mark/Space bit pattern (ie. logical NOT). If true, a zero bit then generates a punched hole and a one bit becomes a blank</param>
        /// <param name="banner">Add punched letters in 8x8 font. Upper case, digits and basic punctuation are supported only</param>
        /// <param name="numberedSection">Add line numbers to all or a particular desired section of the output</param>
        /// <param name="parity">Add parity {NONE, EVEN, ODD} using the most significant bit (leftmost hole)</param>
        /// <param name="chadless">Teletype Corporation Chadless holes</param>
        /// <param name="wheatstone">Generate 2-level Morse tape in USN Wheatstone format</param>
        /// <param name="cablecode">Generate 2-level Morse tape in Cable Code format</param>
        /// <returns>Return value. Zero for success, 1 for error. Useable by DOS ERRORLEVEL checking</returns>
        public int Generate(string someInputFileName, string someOutputFileName, int startOfData, int lengthOfData, int rowsPerSegment, int interSegmentGap, int segmentsPerDXF, 
                            int leader, int trailer, char mark, char space, bool drawVee, bool joiningTape, int level, int sprocketPos, bool mirror, bool quiet, bool showLineNumbers, bool showASCIIchars, bool showControlChars, 
                            bool dryRun, bool invertPattern, bool baudot, string banner, string messageText, Numbering numberedSection, Parity parity, bool chadless, bool wheatstone, bool cablecode)
        {
            #region Constants that determine one-inch-wide 0.1 inch-spaced 8-level paper tape
            // Constants that determine one-inch-wide 0.1 inch-spaced 8-level ASCII teletype paper tape. The base units are metric and we then derive imperial from them.
            float dataHoleRadius = (float)(1.83 / 2);
            float feedHoleRadius = (float)(1.17 / 2);
            float inch = 25.4f;
            float holeSpacing = inch / 10;
            float tapeWidth = joiningTape ? (2 * inch) : inch;
            #endregion //Constants that determine one-inch-wide 0.1 inch-spaced 8-level ASCII teletype paper tape

            // Items pertaining to other-level widths eg. Baudot 11/16 inch (17.46 mm) for five bit codes, and 1 inch (25.4 mm) for tapes with six or more bits
            // Things pertaining to Morse (USN Wheatstone and cable code) tape
            float fifteenThirtyseconds = (15 / 32) * inch;  // ref http://navy-radio.com/morse/mx491u-spec-01.jpg
            if (wheatstone || cablecode)
            {
                level = 2;
                baudot = false;
                banner = "";
                sprocketPos = 1;
            }

            float elevenSixteenths = 17.46f;
            if (level == 5)
                tapeWidth = elevenSixteenths;
            if (level < 5)
                tapeWidth = level * holeSpacing + holeSpacing + holeSpacing;

            #region Variables
            List<BitArray> msgbits = new List<BitArray>();  // The binary representation of the input
            List<Tuple<int,int>> joinerParams2 = new List<Tuple<int,int>>();
            int byteCount = 0;
            byte[] bannerBytes = new byte[] { };
            byte[] fileBytes = new byte[] { };
            float segmentSideBySideSpacing = (float)interSegmentGap;
            int bannerCount = 0;
            int leaderCount = 0;
            int codeOffset = 0;
            int codeCount = 0;
            int trailerCount = 0;
            int chadlessStartAngle = 130;
            int chadlessEndAngle = 50;
            #endregion //Variables

            #region Input filename check
            // Input filename checking
            if (string.IsNullOrEmpty(someInputFileName) && string.IsNullOrEmpty(messageText) && string.IsNullOrEmpty(banner) && leader == 0 && trailer == 0)
            {
                Console.WriteLine("No input filename, message text or banner specified");
                return 1;
            }
            if (!string.IsNullOrEmpty(someInputFileName))
            {
                if (File.Exists(someInputFileName))
                    fileBytes = File.ReadAllBytes(someInputFileName);
                else
                {
                    Console.WriteLine("Cannot open " + someInputFileName);
                    return 1;
                }
            }
            #endregion //Input filename check

            #region Message text string
            // Punch the text string given directly on the command line, if specified
            if (!string.IsNullOrEmpty(messageText))
                fileBytes = Encoding.ASCII.GetBytes(messageText);
            #endregion //Message text string

            #region Baudot
            // If Baudot requested then recode the ASCII BitArray list, including insertion of change to Letters or Figures character sets
            // We won't know the final length until it has been completely parsed.
            // For more information see https://en.wikipedia.org/wiki/Baudot_code and http://www.dcode.fr/baudot-code
            if (baudot)
            {
                List<int?> baudotMsg = new List<int?>();
                //Default to Letters character set (ITA2 LETRS)
                bool figuresSet = false;
                foreach (byte z in fileBytes)
                {
                    char c = Convert.ToChar(z);
                    if (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || c == ' ')
                    {
                        if (c == ' ')
                            baudotMsg.Add(Baudot.GetLetter(c));
                        else
                        {
                            if (char.IsDigit(c))
                            {
                                if (!figuresSet)
                                {
                                    baudotMsg.Add(Baudot.GetLetter(Baudot.ShiftToFigures)); // ITA2 FIGS
                                    figuresSet = true;
                                }
                                baudotMsg.Add(Baudot.GetFigure(c));
                            }
                            else
                            {
                                if (figuresSet)
                                {
                                    baudotMsg.Add(Baudot.GetFigure(Baudot.ShiftToLetters)); // ITA2 LETRS
                                    figuresSet = false;
                                }
                                baudotMsg.Add(Baudot.GetLetter(c));
                            }
                        }
                    }
                }
                // We now know the final Baudot message size, so copy the conversion back to the main file byte buffer
                if (baudotMsg.Any())
                {
                    fileBytes = new byte[baudotMsg.Count];
                    for (int i = 0; i < baudotMsg.Count; i++)
                    {
                        int? b = baudotMsg[i];
                        fileBytes[i] = Convert.ToByte(b);
                    }
                }
            }
            #endregion //Baudot

            #region Morse
            #region Wheatstone
            if (wheatstone)
            {
                Wheatstone wheat = new Wheatstone();
                List<int> wheatstoneMsg = new List<int>();
                foreach (byte z in fileBytes)
                {
                    char c = Convert.ToChar(z);
                    if (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c))
                    {
                        wheatstoneMsg.AddRange(wheat.GetLetter(c));
                    }
                }
                // We now know the final Wheatstone Morse message size, so copy the conversion back to the main file byte buffer
                if (wheatstoneMsg.Any())
                {
                    fileBytes = new byte[wheatstoneMsg.Count];
                    for (int i = 0; i < wheatstoneMsg.Count; i++)
                    {
                        int? b = wheatstoneMsg[i];
                        fileBytes[i] = Convert.ToByte(b);
                    }
                }
            }
            #endregion //Wheatstone
            #region Cable Code
            if (cablecode)
            {
                CableCode cable = new CableCode();
                List<int> cablecodeMsg = new List<int>();
                foreach (byte z in fileBytes)
                {
                    char c = Convert.ToChar(z);
                    if (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c))
                    {
                        cablecodeMsg.AddRange(cable.GetLetter(c));
                    }
                }
                // We now know the final Cable Code Morse message size, so copy the conversion back to the main file byte buffer
                if (cablecodeMsg.Any())
                {
                    fileBytes = new byte[cablecodeMsg.Count];
                    for (int i = 0; i < cablecodeMsg.Count; i++)
                    {
                        int? b = cablecodeMsg[i];
                        fileBytes[i] = Convert.ToByte(b);
                    }
                }
            }
            #endregion //Cable Code
            #endregion //Morse

            #region Leader
            // Add leader, if specified
            for (int m = 0; m < leader; m++)
                msgbits.Add(new BitArray(new byte[] { 0 }));
            leaderCount = leader;
            #endregion //Leader

            #region Banner
            // Add text banner, if specified
            // Is a 8x8-font text banner required? If so, generate it up front so it can be added in due course
            if (!string.IsNullOrEmpty(banner))
            {
                bannerBytes = BannerText(banner);
                if (string.IsNullOrEmpty(someInputFileName))
                {
                    string trimmedBanner = banner.Trim();
                    someOutputFileName = "BANNER_" + (mirror ? "MIRROR_" : "") + trimmedBanner.Substring(0, Math.Min(8, trimmedBanner.Length)) + ".DXF";
                }
            }
            for (int i = 0; i < bannerBytes.Length - 1; i++)
                msgbits.Add(new BitArray(new byte[] { bannerBytes[i] }));
            bannerCount = bannerBytes.Count();
            #endregion //Banner

            #region Code/Data
            // Check if range has pushed it to before beginning of data, pre-pad with leader if necessary
            if (startOfData < 0 && lengthOfData >= 0)
            {
                int prePad = Math.Abs(startOfData);
                for (int i = 0; i < prePad; i++)
                    msgbits.Add(new BitArray(new byte[] { 0 }));    // Specified range has exceeded beginning of code, pad with leader
                startOfData = 0;
                lengthOfData = Math.Abs(lengthOfData - prePad);
            }
            // Add paper tape binary, either whole or subrange, if specified
            if (startOfData >= 0 && lengthOfData >= 0)
            {
                for (int i = startOfData; i < startOfData + lengthOfData; i++)
                    if (i < fileBytes.Length)
                        msgbits.Add(new BitArray(new byte[] {fileBytes[i]}));
                    else
                        msgbits.Add(new BitArray(new byte[] { 0 }));    // Specified range has exceeded end of code, pad with trailer
                byteCount = startOfData;
                codeCount = lengthOfData;
            }
            else
            {
                // Both sentinel -1 markers not changed, therefore add the whole data file
                msgbits.AddRange(fileBytes.Select(b => new BitArray(new byte[] {b})));
                codeCount = fileBytes.Count();
            }
            #endregion //Code/Data

            #region Trailer
            // Add trailer, if specified
            for (int m = 0; m < trailer; m++)
                msgbits.Add(new BitArray(new byte[] { 0 }));
            #endregion //Trailer


            #region Precalculate offsets
            // Precalculate the line numbers
            int[] numbers;
            numbers = Enumerable.Repeat(-1, bannerCount + leaderCount + codeCount + trailerCount).ToArray();    // Generate the initial set of monotonically increasing integers into an array
            // Determine which of the desired output sections are to be numbered
            for (int ban = 0; ban < bannerCount; ban++)
                if ((numberedSection & Numbering.BANNER) > 0)
                    numbers[ban] = ban + 1;
            for (int lead = 0; lead < leaderCount; lead++)
                if ((numberedSection & Numbering.LEADER) > 0)
                    numbers[lead + bannerCount] = lead + 1 + (numberedSection == Numbering.BANNER ? bannerCount : 0);
            for (int code = 0; code < codeCount; code++)
                if ((numberedSection & Numbering.CODE) > 0)
                    numbers[code + leaderCount + bannerCount] = code + codeOffset +  (numberedSection == Numbering.LEADER ? leaderCount : 0) + (numberedSection == Numbering.BANNER ? bannerCount : 0);
            for (int trail = 0; trail < trailerCount; trail++)
                if ((numberedSection & Numbering.TRAILER) > 0)
                    numbers[trail + codeCount + leaderCount + bannerCount] = trail + 1 + (numberedSection == Numbering.CODE ? codeCount : 0) + (numberedSection == Numbering.LEADER ? leaderCount : 0) + (numberedSection == Numbering.BANNER ? bannerCount : 0);
            #endregion //Precalculate offsets

            int overallCount = 0;                           // Combined banner, leader, code and trailer counter
            int totalRows = msgbits.Count;                  // number of rows of actual output. Equivalent to the number of bytes in the input file
            int segments = totalRows / rowsPerSegment + 1;  // A segment is one strip of one inch wide (for 8-level) paper tape of desired length, usually to fit the cutting mat length ('height') of the CNC stencil cutter
            int currentRow = 0;                             // The code byte offset from the start of the input file
            int currentDXF = 1;                             // There may be multiple DXFs emitted if the input file's length in bytes exceeds what can be cut on a single CNC cutting mat (eg. 12" x 12") at one time
            int currentSegment = segmentsPerDXF;            // The page of DXFs for a multipage set of output files
            int absoluteRow = 0;                            // Includes banner, leader, data, trailer. Used for additional joiner info

            // If segment switch used, show a '+---------+' at the start in the console output where a joiner would go
            if (currentRow % rowsPerSegment == 0 && !quiet)
            {
                if (showLineNumbers)
                    Console.Write("           ");
                Console.WriteLine("+-" + new string('-', level) + "+");
            }

            // If chadless then the console mark character will be an uppercase U
            if (chadless)
                mark = 'U';

            // Start the big loop!
            for (int j = 0; j < segments; j++)
            {
                int rowsThisSegment = Math.Min(rowsPerSegment, totalRows - currentRow); // Number of bytes (one byte's worth of mark/space holes) to fit top to bottom on CNC stencil cutting mat
                float segmentXorigin = j * (tapeWidth + segmentSideBySideSpacing);

                // Determine segment edge length as #bytes * 2.54mm  (one-tenth inch)
                float tapeLengthOfHoles = rowsThisSegment * holeSpacing;
                float tapeLengthTotal = tapeLengthOfHoles;
                #region Draw long edges
                // Draw paper tape long edges, ie. '||'. Note that by default, adjacent segments share a cutting edge but may be spaced apart into two separate edges with a +ve value for /INTER-SEGMENT-GAP
                dxf.DXF_Line(segmentXorigin, 0, 0, segmentXorigin, tapeLengthTotal, 0);
                dxf.DXF_Line(tapeWidth + segmentXorigin, 0, 0, tapeWidth + segmentXorigin, tapeLengthTotal, 0);
                #endregion //Draw long edges
                #region Draw first segment
                // If very first segment, draw Start Vee and line
                if (j == 0)
                {
                    if (joiningTape)
                    {
                        // We are producing adhesive tape joiner pieces with tabs to fasten the paper tape segments together. Therefore draw alignment marks instead of normal flat ends or vee.
                        // Small indents on the joiner leading and trailing tabs edges indicate where to position along paper tape long edges.
                        float outer = tapeWidth / 2;
                        float outerThreeQuarter = outer * 0.75f;
                        dxf.DXF_Line(segmentXorigin, tapeLengthTotal, 0, outerThreeQuarter + segmentXorigin, tapeLengthTotal, 0);
                        dxf.DXF_Line(outer + segmentXorigin, tapeLengthTotal, 0, outer + tapeWidth + segmentXorigin, tapeLengthTotal, 0);
                        dxf.DXF_Line(outerThreeQuarter + segmentXorigin, tapeLengthTotal, 0, outer + segmentXorigin, tapeLengthTotal - holeSpacing, 0);
                        dxf.DXF_Line(outer + segmentXorigin, tapeLengthTotal - holeSpacing, 0, outer + segmentXorigin, tapeLengthTotal, 0);
                        dxf.DXF_Line(outer + tapeWidth + segmentXorigin, tapeLengthTotal, 0, outer + tapeWidth + segmentXorigin, tapeLengthTotal - holeSpacing, 0);
                        dxf.DXF_Line(outer + tapeWidth + segmentXorigin, tapeLengthTotal - holeSpacing, 0, tapeWidth - outerThreeQuarter + segmentXorigin, tapeLengthTotal, 0);
                        dxf.DXF_Line(tapeWidth - outerThreeQuarter + segmentXorigin, tapeLengthTotal, 0, tapeWidth + segmentXorigin, tapeLengthTotal, 0);
                    }
                    else
                    {
                        if (drawVee)
                        {
                            // Start Vee /\
                            dxf.DXF_Line(segmentXorigin, tapeLengthTotal, 0, tapeWidth / 2 + segmentXorigin, tapeLengthTotal + holeSpacing * 2, 0);   // '/' LEFT EDGE PART
                            dxf.DXF_Line(tapeWidth / 2 + segmentXorigin, tapeLengthTotal + holeSpacing * 2, 0, tapeWidth + segmentXorigin, tapeLengthTotal, 0); // '\' RIGHT EDGE PART
                        }
                        else
                        {
                            dxf.DXF_Line(segmentXorigin, tapeLengthTotal, 0, tapeWidth + segmentXorigin, tapeLengthTotal, 0);   // '-' TOP EDGE PART
                        }
                    }
                }
                #endregion //Draw first segment
                #region Draw last segment
                // If very last segment, draw line and End Vee
                if (j == (segments - 1))
                {
                    if (joiningTape)
                    {
                        // Draw alignment marks instead of vee
                        float outer = tapeWidth / 2;
                        float outerThreeQuarter = outer * 0.75f;
                        dxf.DXF_Line(segmentXorigin, 0, 0, outerThreeQuarter + segmentXorigin, 0, 0);
                        dxf.DXF_Line(outer + segmentXorigin, 0, 0, outer + tapeWidth + segmentXorigin, 0, 0);
                        dxf.DXF_Line(outerThreeQuarter + segmentXorigin, 0, 0, outer + segmentXorigin, holeSpacing, 0);
                        dxf.DXF_Line(outer + segmentXorigin, holeSpacing, 0, outer + segmentXorigin, 0, 0);
                        dxf.DXF_Line(outer + tapeWidth + segmentXorigin, 0, 0, outer + tapeWidth + segmentXorigin, holeSpacing, 0);
                        dxf.DXF_Line(outer + tapeWidth + segmentXorigin, holeSpacing, 0, tapeWidth - outerThreeQuarter + segmentXorigin, 0, 0);
                        dxf.DXF_Line(tapeWidth - outerThreeQuarter + segmentXorigin, 0, 0, tapeWidth + segmentXorigin, 0, 0);
                    }
                    else
                    {
                        if (drawVee)
                        {
                            // End Vee /\
                            dxf.DXF_Line(segmentXorigin, tapeLengthTotal, 0, tapeWidth + segmentXorigin, tapeLengthTotal, 0);
                            dxf.DXF_Line(tapeWidth / 2 + segmentXorigin, holeSpacing * 2, 0, tapeWidth + segmentXorigin, 0, 0); // FLAT END
                            dxf.DXF_Line(segmentXorigin, 0, 0, tapeWidth / 2 + segmentXorigin, holeSpacing * 2, 0);
                        }
                        else
                        {
                            dxf.DXF_Line(segmentXorigin, 0, 0, tapeWidth + segmentXorigin, 0, 0);
                            dxf.DXF_Line(segmentXorigin, tapeLengthTotal, 7, tapeWidth + segmentXorigin, tapeLengthTotal, 0);
                        }
                    }
                }
                #endregion //Draw last segment
                #region Draw top and bottom edges
                else
                {
                    if (!drawVee && (j >= 0 && j < segments - 1))
                    {
                        // Draw segment flat ends at top and bottom for all but beginning and end segments
                        dxf.DXF_Line(segmentXorigin, tapeLengthTotal, 6, tapeWidth + segmentXorigin, tapeLengthTotal, 0);
                        dxf.DXF_Line(tapeWidth + segmentXorigin, 0, 0, segmentXorigin, 0, 0);
                    }
                }
                #endregion //Draw top and bottom edges
                // Set origin
                float Xpos = holeSpacing + segmentXorigin + (tapeWidth - tapeWidth) / 2;
                float Ypos = tapeLengthOfHoles - (holeSpacing / 2);

                // Generate data and sprocket holes
                for (int rowPos = currentRow; rowPos < currentRow + rowsThisSegment; rowPos++)
                {
                    BitArray ba = msgbits[rowPos];

                    // Apply parity, if requested. Uses the MSB ie. high bit, or leftmost hole position. Default is NONE (ie. use the MSB for data)
                    if (parity == Parity.EVEN)
                    {
                        int count = 0;
                        for (int i = 0; i < level - 1; i++)
                            if (ba.Get(i))
                                count++;
                        if (count % 2 == 1)
                            ba.Set(level - 1, true);
                    }
                    if (parity == Parity.ODD)
                    {
                        int count = 0;
                        for (int i = 0; i < level - 1; i++)
                            if (ba.Get(i))
                                count++;
                        if (count % 2 == 0)
                            ba.Set(level - 1, true);
                    }

                    if (invertPattern)
                        ba = ba.Not();  // Invert bit pattern, if desired
                    if (!dryRun)
                    {
                        Xpos = holeSpacing + segmentXorigin + (tapeWidth - tapeWidth) / 2;
                        if (mirror)
                        {
                            // Produce a mirror image of the holes to make paper or sticky vinyl joiners that go on the reverse side (underside) of the paper tape, if desired.
                            // Position sprocket feed hole accordingly.
                            for (int i = 0; i <= level - 1; i++)
                            {
                                if (ba.Get(i))
                                    if (chadless)
                                        dxf.DXF_Arc(Xpos, Ypos, 0, dataHoleRadius, chadlessStartAngle, chadlessEndAngle);
                                    else
                                        dxf.DXF_Circle(Xpos, Ypos, 0, dataHoleRadius);
                                Xpos += holeSpacing;
                                if (i == sprocketPos)
                                {
                                    if (chadless)
                                        dxf.DXF_Arc(Xpos, Ypos, 0, feedHoleRadius, chadlessStartAngle, chadlessEndAngle);
                                    else
                                        dxf.DXF_Circle(Xpos, Ypos, 0, feedHoleRadius);
                                    Xpos += holeSpacing;
                                }
                            }
                            // If the sprocket hole is desired to be at the extreme right, draw it last
                            if (sprocketPos > level -1)
                            {
                                if (chadless)
                                    dxf.DXF_Arc(Xpos, Ypos, 0, feedHoleRadius, chadlessStartAngle, chadlessEndAngle);
                                else
                                    dxf.DXF_Circle(Xpos, Ypos, 0, feedHoleRadius);
                                Xpos += holeSpacing;
                            }
                        }
                        else
                        {
                            // Normal punching: MSB on LHS, LSB on RHS. Position sprocket feed hole accordingly.
                            // If the sprocket hole is desired to be at the extreme left, draw it first
                            if (sprocketPos > level - 1)
                            {
                                if (chadless)
                                    dxf.DXF_Arc(Xpos, Ypos, 0, feedHoleRadius, chadlessStartAngle, chadlessEndAngle);
                                else
                                    dxf.DXF_Circle(Xpos, Ypos, 0, feedHoleRadius);
                                Xpos += holeSpacing;
                            }
                            for (int i = level - 1; i >= 0; i--)
                            {
                                if (ba.Get(i))
                                    if (chadless)
                                        dxf.DXF_Arc(Xpos, Ypos, 0, dataHoleRadius, chadlessStartAngle, chadlessEndAngle);
                                    else
                                        dxf.DXF_Circle(Xpos, Ypos, 0, dataHoleRadius);
                                Xpos += holeSpacing;
                                if (i == sprocketPos)
                                {
                                    if (chadless)
                                        dxf.DXF_Arc(Xpos, Ypos, 0, feedHoleRadius, chadlessStartAngle, chadlessEndAngle);
                                    else
                                        dxf.DXF_Circle(Xpos, Ypos, 0, feedHoleRadius);
                                    Xpos += holeSpacing;
                                }
                            }
                        }
                        Ypos -= holeSpacing;
                    }
                    if (!quiet)
                    {
                        if (showLineNumbers)
                        {
                            if (overallCount < numbers.Count() && numbers[overallCount] >= 0)
                                Console.Write("#" + numbers[overallCount].ToString("D8") + "  ");
                            else
                                Console.Write("           ");
                            overallCount++;
                        }
                        // Emit the data line on the console
                        string displayNLevelRow = PaperTapeExtensions.FormatNLevelRow(ba, level, sprocketPos, mark, space);
                        if (mirror)
                            displayNLevelRow = displayNLevelRow.Reverse();
                        Console.Write(displayNLevelRow);
                        if (invertPattern)
                            Console.Write(" INVERTED ");
                        if (mirror)
                            Console.Write(" MIRROR ");
                        if (joiningTape)
                            Console.Write(" JOINER ");
                        if (chadless)
                            Console.Write(" CHADLESS ");
                        if (wheatstone)
                            Console.Write(" MORSE (WHEATSTONE) ");
                        if (cablecode)
                            Console.Write(" MORSE (CABLE CODE) ");
                        if (baudot)
                        {
                            Console.Write(" BAUDOT ");
                            //TODO FIX  Console.Write(ba.ToBaudotName());
                        }
                        char c = ba.ToChar();
                        if (showASCIIchars && !baudot)
                        {
                            if ((char.IsLetterOrDigit(c) || char.IsPunctuation(c)) && c < 128)
                                Console.Write("    " + c);
                        }
                        if (showControlChars && !baudot)
                            Console.Write("    " + c.ToASCIIControlCodeName());
                        Console.WriteLine();
                    }
                    absoluteRow++;
                }
                joinerParams2.Add(new Tuple<int,int>(currentRow, (absoluteRow - bannerBytes.Count() - leader)));    // Keep joiner code and absolute positions

                // If segment switch used, show a '+---------+' at this point in the console output where a joiner would go
                // Show a correct width line for the relevant level of tape. eg. +---------+ for 8-level, or +------+ for 5-level
                if (currentRow % rowsPerSegment == 0 && !quiet)
                {
                    if (showLineNumbers)
                        Console.Write("           ");   // pad out the width of the line number
                    Console.WriteLine("+-" + new string('-', level) + "+");  // see https://stackoverflow.com/questions/411752/best-way-to-repeat-a-character-in-c-sharp#411762
                }
                currentRow = currentRow + rowsThisSegment;
                // Save output if segmentsPerDXF used
                if (!dryRun)
                {
                    if (segmentsPerDXF > 0)
                    {
                        if (--currentSegment <= 0)
                        {
                            // Write the output DXF file
                            string outputName = someOutputFileName.CleanFilename();
                            if (string.IsNullOrEmpty(outputName))
                                outputName = someInputFileName;
                            string[] tokens = outputName.Split('.');
                            if (tokens.Count() > 1)
                                outputName = tokens[0] + "_" + currentDXF.ToString("D4") + ".dxf";
                            dxf.DXF_Save(outputName);
                            dxf = new DxfMaker(); // Start a new DXF drawing
                            currentDXF++;
                            currentSegment = segmentsPerDXF;
                            currentSegment = Math.Min(segmentsPerDXF, segments - currentSegment);
                        }
                    }
                }
            }

            int segmentNumber = 0;
            // Write paper tape joiner ranges and absolute overall positions, so they may be created as a second step to the primary paper tape CNC cutting
            if (!joiningTape && !quiet)
                joinerParams2.ForEach(joinPos => Console.WriteLine("Joiner {0:D4}: data byte {1:D8}  absolute position {2:D8}", segmentNumber++, joinPos.Item1, joinPos.Item2));

            // Save DXF output at end if no segmentsPerDXF specified
            if (!dryRun)
            {
                if (segmentsPerDXF == 0)
                {
                    string outputName = someOutputFileName.CleanFilename();
                    if (string.IsNullOrEmpty(someOutputFileName))
                    {
                        string[] tokens = someInputFileName.Split('.');
                        if (tokens.Count() > 1)
                            outputName = tokens[0] + ".dxf";
                    }
                    return dxf.DXF_Save(outputName) ? 0 : 1;
                }
            }
            return 0;   // Return an ERRORLEVEL for use in MSDOS batchfile scripting if desired
        }

        /// <summary>
        /// Characters for an 8x8 character set font (upper case and punctuation only)
        /// Arranged as vertical slices through the letter with Least Significant Bit at the top of the letter
        /// An encoding of http://overcode.yak.net/12.sizes@8x8font.png?size1=O 
        /// </summary>
        /// <param name="someMessage">The message string to be encoded</param>
        /// <returns>An array of bytes containg the 8x8 font representation of the string. Lower case is not supported</returns>
        public byte[] BannerText(string someMessage)
        {
            List<List<byte>> letters = new List<List<byte>>()
            {
                    new List<byte>() {0, 0, 0, 0, 0, 0},                    // space
                    new List<byte>() {0, 0, 0, 95, 0, 0, 0},                // !
                    new List<byte>() {0, 0, 0, 3, 0, 3, 0},                 // "
                    new List<byte>() {0, 20, 20, 127, 20, 127, 20, 20, 0},  // #
                    new List<byte>() {0, 0, 36, 42, 127, 42, 18, 0},        // $
                    new List<byte>() {0, 0, 38, 22, 8, 52, 50, 0},          // %
                    new List<byte>() {0, 48, 74, 69, 75, 48, 80, 0},        // &
                    new List<byte>() {0, 0, 0, 0, 3, 0, 0, 0},              // '
                    new List<byte>() {0, 0, 0, 28, 34, 65, 0, 0},           // (
                    new List<byte>() {0, 0, 0, 65, 34, 28, 0, 0},           // )
                    new List<byte>() {0, 34, 20, 8, 127, 8, 20, 34, 0},     // *
                    new List<byte>() {0, 8, 8, 8, 127, 8, 8, 8, 0},         // +
                    new List<byte>() {0, 0, 0, 0, 176, 112, 0, 0},          // ,
                    new List<byte>() {0, 8, 8, 8, 8, 8, 8, 8, 0},           // -
                    new List<byte>() {0, 0, 0, 0, 96, 96, 0, 0, 0},         // .
                    new List<byte>() {64, 32, 16, 8, 4, 2, 1, 0},           // /
                    new List<byte>() {0, 0, 62, 65, 73, 65, 62, 0},         // 0
                    new List<byte>() {0, 0, 66, 127, 64, 0, 0},             // 1
                    new List<byte>() {0, 0, 66, 97, 81, 73, 70, 0},         // 2
                    new List<byte>() {0, 0, 34, 65, 73, 73, 54, 0},         // 3
                    new List<byte>() {0, 0, 12, 10, 73, 127, 72, 0},        // 4
                    new List<byte>() {0, 0, 47, 73, 73, 73, 49, 0},         // 5
                    new List<byte>() {0, 0, 62, 73, 73, 73, 50, 0},         // 6
                    new List<byte>() {0, 0, 1, 113, 9, 5, 3, 0},            // 7
                    new List<byte>() {0, 0, 54, 73, 73, 73, 54, 0},         // 8
                    new List<byte>() {0, 0, 38, 73, 73, 73, 62, 0},         // 9
                    new List<byte>() {0, 0, 0, 0, 54, 54, 0, 0},            // :
                    new List<byte>() {0, 0, 0, 0, 182, 118, 0},             // ;
                    new List<byte>() {0, 0, 8, 20, 34, 65, 0},              // <
                    new List<byte>() {0, 20, 20, 20, 20, 20, 20},           // =
                    new List<byte>() {0, 0, 65, 34, 20, 8, 0},              // >
                    new List<byte>() {0, 2, 1, 81, 9, 6, 0},                // ?
                    new List<byte>() {0, 62, 65, 93, 85, 14, 0},            // @
                    new List<byte>() {0, 126, 9, 9, 9, 126, 0},             // A
                    new List<byte>() {0, 127, 73, 73, 73, 54, 0},           // B
                    new List<byte>() {0, 62, 65, 65, 65, 34, 0},            // C
                    new List<byte>() {0, 127, 65, 65, 65, 62, 0},           // D
                    new List<byte>() {0, 127, 73, 73, 73, 73, 0},           // E
                    new List<byte>() {0, 127, 9, 9, 9, 9, 0},               // F
                    new List<byte>() {0, 62, 65, 73, 73, 26, 0},            // G
                    new List<byte>() {0, 127, 8, 8, 8, 127, 0},             // H
                    new List<byte>() {0, 0, 65, 127, 65, 0, 0},             // I
                    new List<byte>() {0, 48, 64, 65, 63, 1, 0},             // J
                    new List<byte>() {0, 63, 8, 8, 20, 99, 0},              // K
                    new List<byte>() {0, 0, 0, 127, 64, 64, 0},             // L
                    new List<byte>() {0, 127, 2, 4, 8, 4, 2, 127},          // M
                    new List<byte>() {0, 127, 2, 12, 16, 127, 0},           // N
                    new List<byte>() {0, 62, 65,65, 65, 62, 0},             // O
                    new List<byte>() {0, 0, 127, 9, 9, 6, 0},               // P
                    new List<byte>() {0, 62, 65, 65, 193, 62, 0},           // Q
                    new List<byte>() {0, 127, 9, 25, 41, 70, 0},            // R
                    new List<byte>() {0, 38, 73, 73, 73, 50, 0},            // S
                    new List<byte>() {0, 1, 1, 127, 1, 1, 0},               // T
                    new List<byte>() {0, 63, 64, 64, 64, 63, 0},            // U
                    new List<byte>() {0, 7, 24, 96, 24, 7, 0},              // V
                    new List<byte>() {7, 24, 96, 24, 96, 24, 7},            // W
                    new List<byte>() {0, 99, 20, 8, 20, 99, 0},             // X
                    new List<byte>() {0, 3, 4, 120, 4, 3, 0},               // Y
                    new List<byte>() {0, 97, 81, 73, 69, 67, 0},            // Z
                    new List<byte>() {0, 0, 127, 65, 65, 0, 0},             // [
                    new List<byte>() {1, 2, 4, 8, 16, 32, 64},              // \
                    new List<byte>() {0, 0, 65, 65, 127, 0, 0},             // ]
                    new List<byte>() {0, 4, 2, 1, 2, 4, 0},                 // ^
                    new List<byte>() {0, 128, 128, 128, 128, 128, 128, 128},// _
                    new List<byte>() {0, 0, 1, 2, 0, 0, 0},                 // `
                };
            List<byte> bannerChars = new List<byte>();
            foreach (char c in someMessage.ToUpper().Where( c => c < 128))
                bannerChars.AddRange(letters[c - ' ']);
            return bannerChars.ToArray();
        }
    }

    /// <summary>
    /// PTAP2DXF main class for Paper Tape to DXF
    /// Full .NET framework or Mono:    run the generated PTAP2DXF.EXE [arguments] in a DOS shell, PowerShell or Git Bash shell
    /// Linux/Mac .NET Core execution:  $ dotnet ptap2dxf.dll [arguments] (Please visit https://www.microsoft.com/net/core for .NET Core on those platforms)
    /// </summary>
    public class Program
    {
        const string VERSION = "1.2";   // Nothing complicated, here
#if DOTNETCORE
        const string sep = "--";  // Unix-style fullword separator
#else
        cost string sep = "/";   // Windows DOS shell traditional argument separator
#endif
        /// <summary>
        /// Program entry point.
        /// Prints details about the program and help list of switches
        /// </summary>
        /// <param name="args">See Usage for argument descriptions</param>
        /// <returns></returns>
        static int Main(string[] args)
        {
            string inputFileName = string.Empty;    // Input file name eg. DEC-11-L2PC-PO.ptap
            string outputFileName = string.Empty;   // Output DXF file eg. DEC-11-L2PC-PO.dxf
            string messageText = string.Empty;      // Quoted string containing the message to be punched directly
            string banner = string.Empty;           // Banner text eg. 'ABS LDR'
            int segment = int.MaxValue;             // Length of one piece of tape of a length able cut on the CNC stencil cutter  eg. 80
            int interSegmentGap = 0;                // Spacing, if desired, in mm between side-by-side cut segments. Normally 0, meaning cut edges are shared
            int segmentsPerDXF = 0;                 // Number of segments in a single DXF file. Generally (CNC cutter width / 1") - 1. eg. 8
            bool quiet = false;                     // If true, supress showing tape being produced in console shell window
            bool lineNumbers = false;               // Show line numbers prefixing output on console window eg.     '#00000118  |OO  O.O O|'
            bool showASCIIChars = false;            // Print the ASCII character to the right of each row on the console output eg. 'a'
            bool showControlChars = false;          // Print the ASCII string representing the ASCII control characters eg. '<BEL>'
            bool dryRun = false;                    // Run through complete operation with any switches but do not write a DXF file
            int leader = 0;                         // Number of blank rows (with sprocket holes) to be issued before starting processing input file
            int trailer = 0;                        // Number of blank rows (with sprocket holes) to be issued after finishing processing input file
            char mark = 'O';                        // The console character output to represent a mark (data bit = 1)
            char space = ' ';                       // The console character output to represent a space (data bit = 0)
            bool invertPattern = false;             // Flip (logical NOT) the Mark/Space pattern ie. hole --> no hole and no hole --> hole
            bool drawVee = false;                   // If true, draw a '/\' at beginning of first segment and a '/\' and end of last segment TODO FIX
            bool joiningTape = false;               // Default to 2 * tape width for making joining adhesive tape
            int level = 8;                          // Number of data bits to be punched eg. 8 for 8-level (byte-width ASR33, PC05 etc.) or 5 for 5-level (Baudot). Default is 8
            int sprocketPos = 3;                    // Position of sprocket feed hole. Zero puts it at the right edge, 7 at left edge. For the default 8-level tape it is between bits 2 and 3, ie. 3
            bool mirror = false;                    // Mirror image of tape so that underside joiners can be made (from paper, vinyl or contact plastic)
            bool baudot = false;                    // Convert ASCII alphanumeric and basic punctuation characters to Baudot code (forces level=5). Default is 8-level ASCII
            Parity parity = Parity.NONE;            // Use high bit (leftmost hole) as a parity bit, can be EVEN or ODD. Default is no parity
            bool chadless = false;                  // Punch Teletype Corp chadless holes. The chad becomes an arc rather than a cut out circle
            bool wheatstone = false;                // Punch Morse (Wheatstone coding)
            bool cablecode = false;                 // Punch Morse (cable coding)

            bool waitAtEnd = false;                 // Issue a 'Press a key to End' before returning to command prompt
            int start = -1;
            int length = -1;
            Numbering requestedNumbering = Numbering.CODE;  // Default is to number the console output lines for the code part only
            int errorLevel = 0;                     // Argument processing result and completion status. If > 0 something went horribly wrong

            // If no input or args then show Help and quit
            if (!args.Any())
            {
                Usage();
                return 0;
            }
            // Parse command line input arguments and switches. See Usage function for switch formats and explanations
            string[] tokens;
            foreach (var parameter in args)
            {
                tokens = parameter.ToUpper().Trim().Split('=');
                switch (tokens[0])
                {
                    case "-A":  case "/A":  case "--A":  case "-ASC":    case "--ASC":    case "-ASCII":  case "/ASCII":  case "--ASCII":
                        showASCIIChars = true;
                        break;

                    case "-BF": case "/BF": case "--BF":    case "--BANFILE":   case "/BANFILE":    case "--BANNERFILE":    case "/BANNERFILE":
                        try
                        {
                            // Banner filename checking
                            string bannerFilename = string.Empty;
                            if (tokens.Count() > 1)
                            {
                                bannerFilename = parameter.Split('=').Last();
                                if (string.IsNullOrEmpty(bannerFilename))
                                {
                                    Console.WriteLine("No banner filename specified");
                                    errorLevel = 1;
                                }
                                if (!string.IsNullOrEmpty(bannerFilename))
                                {
                                    if (File.Exists(bannerFilename))
                                        banner = File.ReadAllText(bannerFilename);
                                    else
                                    {
                                        Console.WriteLine("Cannot open banner file " + bannerFilename);
                                        errorLevel = 1;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad banner finename specified. Use " + sep + "bannerfile=\"/path/to/bannerfile\"");
                            errorLevel = 1;
                        }
                        break;

                    case "-BT":  case "/BT":  case "--BT":  case "--BANTEXT":   case "/BANTEXT":    case "--BANNERTEXT":    case "/BANNERTEXT":
                        try
                        {
                            if (tokens.Count() > 1)
                                banner = parameter.ToUpper().Split('=').Last(); // Take original parameter because leading or trailing spaces may have been included in banner text
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad banner text string specified. Use " + sep + "bannertext=\"BANNERTEXT\"");
                            errorLevel = 1;
                        }
                        break;

                    case "-BAU":  case "/BAU":  case "--BAU":  case "-BAUDOT":  case "/BAUDOT":  case "--BAUDOT":
                        baudot = true;
                        sprocketPos = 2;    // 5-level tape has the sprocket feed hole between the 2nd and 3rd data holes, starting from the right
                        break;

                    case "-CABLECODE":  case "/CABLECODE":  case "--CABLECODE":
                        cablecode = true;
                        break;

                    case "-CHADLESS":   case "/CHADLESS":   case "--CHADLESS":
                        chadless = true;
                        break;

                    case "-C":  case "/C":  case "--C":  case "-CONTROL":    case "/CONTROL":    case "-CONTROL-CHARS":  case "/CONTROL-CHARS":  case "--CONTROL":
                        showControlChars = true;
                        break;

                    case "-D":  case "/D":  case "--D":   case "-DRY":    case "/DRY":    case "--DRY-RUN":   case "/DRY-RUN":    case "/DRYRUN": case "--DRYRUN":
                        dryRun = true;
                        break;

                    case "-F":  case "--F": case "-FLIP":   case "--FLIP":  case "/F":  case "/FLIP":
                        invertPattern = true;
                        break;

                    case "-G":  case "/G":  case "--G":  case "--GAP":   case "/GAP":    case "--INTER-SEGMENT-GAP": case "/INTER-SEGMENT-GAP":
                        try
                        {
                            if (tokens.Count() > 1)
                                interSegmentGap = int.Parse(tokens[1]);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad inter-segment gap value specified. Use a +ve integer to produce a seperation between tape segment long edges in 0.1 inch increments");
                            errorLevel = 1;
                        }
                        break;

                    case "-H":  case "/H":  case "-?":  case "--?": case "--H":  case "--HELP":  case "/HELP":   case "/?":  case "-WTF":    case "/WTF":
                        Usage();
                        errorLevel = 2;
                        break;

                    case "-I":  case "/I":  case "--I":  case "--IN":    case "/IN": case "--INPUT": case "/INPUT":
                        try
                        {
                            if (tokens.Count() > 1)
                                inputFileName = tokens[1];
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad input filename specified. Use " + sep + "input=\"/path/to/papertapeimagefile\"");
                            errorLevel = 1;
                        }
                        break;

                    case "-J":  case "/J":  case "--J":   case "-JOIN":   case "--JOIN":   case "/JOIN":   case "-JOINER": case "/JOINER":     case "-JOINING":    case "/JOINING":    case "--JOININGTAPE":   case "/JOININGTAPE":   case "--JOINER":
                        joiningTape = true;
                        break;

                    case "-L":  case "/L":  case "--L": case "--LEAD":  case "-LEAD":   case "/LEAD":   case "-LEADER":  case "/LEADER": case "--LEADER": case "-HEADER":  case "/HEADER":  case "--HEADER":
                        try
                        {
                            if (tokens.Count() > 1)
                                leader = int.Parse(tokens[1]);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad tape leader padding value. Use a +ve integer to specify the number of 0.1-inch sprocket feed tape rows to prepend");
                            errorLevel = 1;
                        }
                        break;

                    case "-LEV":  case "/LEV":  case "--LEV":  case "--LEVEL":  case "-LEVEL":  case "/LEVEL":
                        try
                        {
                            if (tokens.Count() > 1)
                                level = int.Parse(tokens[1]);
                            // Verify the value is within punching range. Default is 8-level (byte-width) paper tape
                            if (level < 1 || level > 8)
                                throw new Exception();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad level value:" + level + ". Use a +ve integer between 1 and 8 signifying the data bit width. Default is 8 for byte 8-level. Use 5 for 5-level Baudot.");
                            errorLevel = 1;
                        }
                        // 5-level tape has the sprocket hole between the 2nd and 3rd data holes, starting from the right.
                        // Note this can be overridden by the /SPROCKET= switch, for custom feed hole positioning
                        if (level == 5)
                            sprocketPos = 2;
                        break;

                    case "-MARK":  case "/MARK":  case "--MARK":
                        try
                        {
                            if (tokens.Count() > 1)
                            {
                                string markString = parameter.ToUpper().Split('=').Last();
                                if (!string.IsNullOrEmpty(markString))
                                    mark = markString[0];
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad mark string specified. Use " + sep + "mark=\"O\". Only the first character is used.");
                            errorLevel = 1;
                        }
                        break;

                    case "-M":  case "/M":  case "--M": case "--MIR":  case "-MIR":    case "/MIR":    case "--MIRROR":    case "/MIRROR":
                        mirror = true;
                        break;

                    case "-N":  case "/N":  case "--N": case "--NUM":  case "-NUM":    case "/NUM":    case "--NUMBER":    case "/NUMBER":    case "--NUMBERING":   case "/NUMBERING":
                        try
                        {
                            lineNumbers = true;
                            if (tokens.Count() > 1)
                            {
                                string[] numberingTokens = tokens[1].Split(':');
                                foreach (var numberingToken in numberingTokens)
                                    if (Enum.IsDefined(typeof(Numbering), numberingToken.ToUpper()))
                                        requestedNumbering = requestedNumbering | (Numbering)Enum.Parse(typeof(Numbering), numberingToken, true);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad line numbering specified. Use " + sep + "number={NONE, BANNER, LEADER, CODE, TRAILER, ALL}  CODE is the default");
                            errorLevel = 1;
                        }
                        break;

                    case "-O":  case "--O": case "--OUT":   case "/O":  case "-OUT":   case "/OUT":    case "--OUTPUT":    case "/OUTPUT":
                        try
                        {
                            if (tokens.Count() > 1)
                                outputFileName = tokens[1];
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad output filename specified. Use " + sep + "output=\"/path/to/outputDXFfile\"");
                            errorLevel = 1;
                        }
                        break;

                    case "-PARITY": case "/PARITY": case "--PARITY":
                        try
                        {
                            if (tokens.Count() > 1)
                            {
                                string parityString = parameter.ToUpper().Split('=').Last();
                                if (!string.IsNullOrEmpty(parityString))
                                    switch (parityString.ToUpper())
                                    {
                                        case "EVEN": parity = Parity.EVEN; break;
                                        case "ODD": parity = Parity.ODD; break;
                                        default: parity = Parity.NONE; break;
                                    }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad parity string specified. Use " + sep + "parity={NONE,EVEN,ODD} Default is NONE");
                            errorLevel = 1;
                        }
                        break;


                    case "-Q":  case "/Q":  case "--Q": case "--QUIET": case "/QUIET":
                        quiet = true;
                        break;

                    case "-R":  case "/R":  case "--R": case "-RANGE":  case "/RANGE":  case "--RANGE":
                        try
                        {
                            if (tokens.Count() > 1)
                            {
                                string[] tokens2 = tokens[1].Split(',');
                                if (tokens2.Count() == 2)
                                {
                                    start = int.Parse(tokens2[0]);
                                    if (tokens2[1].StartsWith("+-") || tokens2[1].StartsWith("-+")) // Take bytes from around specified offset (length -ve plus length +ve)
                                    {
                                        int halflength = int.Parse(tokens2[1].Remove(0, 2));
                                        start = start - halflength; // Possible to have -ve start if producing joiner at or around zero (start of data)
                                        length = halflength * 2;
                                    }
                                    else if (tokens2[1].StartsWith("-"))    // Take bytes from specified offset going backwards (-ve length). Possible to have -ve start.
                                    {
                                        start = start - length;
                                        length = Math.Abs(int.Parse(tokens2[1]));
                                    }
                                    else
                                        length = int.Parse(tokens2[1]);     // Take bytes from specified offset going forwards (+ve length)
                                }
                                else
                                {
                                    Console.WriteLine("Bad or missing start,length in " + sep + "RANGE. Use " + sep + "RANGE=offset,length where offset,length are integers. Length can be -ve or +-");
                                    errorLevel = 1;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad range parameters. Use either --range=offset,length  or --range=offset,-length  or --range=offset,+-length");
                            errorLevel = 1;
                        }
                        break;
                    
                    case "-S":  case "/S":  case "--S": case "--SEG":   case "-SEG":    case "/SEG":    case "--SEGMENT":   case "/SEGMENT":    case "--SEGMENTLENGTH": case "/SEGMENTLENGTH":
                        try
                        {
                            if (tokens.Count() > 1)
                                segment = int.Parse(tokens[1]);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad segment length specified. Use " + sep + "segment=length where length is a +ve integer");
                            errorLevel = 1;
                        }
                        break;

                    case "-SPACE":  case "/SPACE":  case "--SPACE":
                        try
                        {
                            if (tokens.Count() > 1)
                            {
                                string spaceString = parameter.ToUpper().Split('=').Last();
                                if (!string.IsNullOrEmpty(spaceString))
                                    space = spaceString[0];
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad space string specified. Use " + sep + "space=\" \". Only the first character is used.");
                            errorLevel = 1;
                        }
                        break;

                    case "-SPROCKET": case "/SPROCKET": case "--SPROCKET":  case "-SPROCKETPOS":  case "/SPROCKETPOS":  case "--SPROCKETPOS":
                        try
                        {
                            if (tokens.Count() > 1)
                                sprocketPos = int.Parse(tokens[1]);
                            // Verify the value is within punching range
                            if (sprocketPos < 0 || sprocketPos > 8)
                                throw new Exception();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad sprocket position value: " + sprocketPos + ". Use a +ve integer between 0 and (level + 1) signifying the position. Default for 8-level tape is 3, ie. between the 3rd and 4th data hole from the right.");
                            errorLevel = 1;
                        }
                        break;

                    case "-TEXT":  case "/TEXT":  case "--TEXT":
                        try
                        {
                            if (tokens.Count() > 1)
                                messageText = parameter.Split('=').Last(); // Take original parameter because leading or trailing spaces may have been included in the input text
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad input text string specified. Use " + sep + "text=\"THE QUICK BROWN FOX\"");
                            errorLevel = 1;
                        }
                        break;

                    case "-T":  case "/T":  case "-TRAIL":  case "--T": case "--TRAIL": case "/TRAIL":    case "-TRAILER":    case "/TRAILER":    case "--TRAILER":
                        try
                        {
                            if (tokens.Count() > 1)
                                trailer = int.Parse(tokens[1]);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Bad tape trailer addition value. Use a +ve integer to specify the number of 0.1-inch sprocket feed tape rows to append");
                            errorLevel = 1;
                        }
                        break;

                    case "--VEE":   case "/VEE":    // Buggy, dont use
                        drawVee = true;
                        break;

                    case "-V":  case "/V":  case "--VERSION":   case "/VERSION":
                        Console.WriteLine(VERSION);
                        return 0;

                    case "-W":  case "/W":  case "--WAIT":  case "/WAIT":
                        waitAtEnd = true;
                        break;

                    case "--WHEATSTONE":
                    case "/WHEATSTONE":
                        wheatstone = true;
                        break;

                    case "-X":  case "/X":  case "--PERDXF":    case "--X": case "/PERDXF":     case "-PERDXF":  case "--SEGMENTSPERDXF":    case "/SEGMENTSPERDXF":
                        try
                        {
                            if (tokens.Count() > 1)
                                segmentsPerDXF = int.Parse(tokens[1]);
                        }
                        catch (Exception e)
                        {
                            string label = "1-inch";
                            if (level == 5)
                                label = "11/16-inch";
                            if (level < 5)
                                label = level + "-level";
                            Console.WriteLine("Bad per-DXF page value specified. Use " + sep + "perdxf=count where count is a +ve integer of the number of " + label + "-wide tape strips L to R across your cutting mat");
                            errorLevel = 1;
                        }
                        break;

                    default:
                        if (!parameter.StartsWith("-") && !parameter.StartsWith(sep))
                            inputFileName = parameter;  // Input file does not have to have a switch qualifier
                        break;
                }
            }
            // Some sanity checks before attempting to generate
            // 1) Check sprocket feed hole position does not exceed bit punching width level
            if (sprocketPos > level)
            {
                Console.WriteLine("Sprocket hole position of " + sprocketPos + " exceeds requested punch level of " + level);
                errorLevel = 1;
            }
            // 2) If Baudot then set to 5-level (11/16" wide) tape
            if (baudot)
                level = 5;

            if (wheatstone || cablecode)
                level = 2;

            // Bail out if some argument was wrong or out of spec, but wait for a key if requested
            if (errorLevel > 0)
            {
                if (waitAtEnd)
                {
                    Console.Write("Hit Enter to end");
                    Console.Read();
                }
                return errorLevel;
            }

            // Arguments all parsed, time to generate the tape
            PTAP2DXF papertape = new PTAP2DXF();
            errorLevel = papertape.Generate(inputFileName, outputFileName, start, length, segment, interSegmentGap, segmentsPerDXF, leader, trailer, mark, space, drawVee, 
                                                joiningTape, level, sprocketPos, mirror, quiet, lineNumbers, showASCIIChars, showControlChars, dryRun, invertPattern, baudot, 
                                                banner, messageText, requestedNumbering, parity, chadless, wheatstone, cablecode);
            if (waitAtEnd)
            {
                Console.Write("Hit Enter to end");
                Console.Read();
            }
            return errorLevel;
        }

        /// <summary>
        /// Program description and options are printed to standard console output if no arguments supplied
        /// </summary>
        public static void Usage()
        {
            Console.WriteLine("PTAP2DXF - Generate DXF files from teletype punched paper tape binary images, suitable for home CNC stencil cutting");
            Console.WriteLine("Written by Steve Malikoff (C) 2017 in Brisbane, Australia. Uses the DxfMaker library written by David S. Tufts");
            Console.Write("Version " + VERSION + "      ");
            Console.WriteLine("See https://github.com/1944GPW for more details");
            Console.WriteLine("Usage:");
            Console.WriteLine("ptap2dxf [inputfilename.ptap]                      (Input ASCII file to be punched. Same as " + sep + "INPUT=\"/path/to/inputfile\")");
            Console.WriteLine("         [" + sep + "ASCII]                                 (Show ASCII character representation for row on console output)");
            Console.WriteLine("         [" + sep + "BANNERFILE=/path/to/bannerfile]        (Generate uppercase punched banner in 8x8 font from ASCII file contents)");
            Console.WriteLine("         [" + sep + "BANNERTEXT=\"YOUR TEXT\"]                (Generate uppercase punched banner in 8x8 font from string)");
            Console.WriteLine("         [" + sep + "BAUDOT]                                (convert ASCII characters to ITA2 Baudot. Forces 5-level output)");
            Console.WriteLine("         [" + sep + "CABLECODE]                             (Generate 2-level Morse tape with Cable Code coding (15/32 inch wide))");
            Console.WriteLine("         [" + sep + "CHADLESS]                              (Half-punch Teletype Corp chadless holes (circa 1975))");
            Console.WriteLine("         [" + sep + "CONTROL-CHARS]                         (Show control characters on console output)");
            Console.WriteLine("         [" + sep + "DRYRUN]                                (Run everything but do not generate DXF file(s))");
            Console.WriteLine("         [" + sep + "FLIP]                                  (Invert bit pattern. Logical NOT)");
            Console.WriteLine("         [" + sep + "GAP=n]                                 (Inter-segment gap in mm between each paper segment on CNC cutting mat. Default is 0, ie. shared edges with no gap)");
            Console.WriteLine("         [" + sep + "HELP]                                  (or ? prints this help)");
            Console.WriteLine("         [" + sep + "INPUT=/path/to/inputfile]              (.ptap or any binary or ASCII input file. Optional switch, does not need to be given with filename)");
            Console.WriteLine("         [" + sep + "JOINER]                                (Make adhesive joiners for paper segments)");
            Console.WriteLine("         [" + sep + "LEADER=n]                              (Prefix output with blank sprocket punch tape in 1/10 inch increments eg. 240 is 2 feet. aka /HEADER=)");
            Console.WriteLine("         [" + sep + "LEVEL=n]                               (The number of data bits in a row of holes. Default is 8 for byte-width ASCII 8-level. Use 5 for 5-level)");
            Console.WriteLine("         [" + sep + "MARK=c]                                (Console output character to represent a mark (data bit = 1). Default is 'O'");
            Console.WriteLine("         [" + sep + "MIRROR]                                (Reverse the output mark/space bit pattern to right-left)");
            Console.WriteLine("         [" + sep + "NUMBER=BANNER|LEADER|CODE|TRAILER|ALL] (NOTE: " + sep + "N defaults to number the code lines only)");
            Console.WriteLine("         [" + sep + "OUTPUT=/path/to/outputfile.dxf]        (output DXF file)");
            Console.WriteLine("         [" + sep + "PARITY=NONE|EVEN|ODD]                  (Parity, if desired. Uses MSB ie. leftmost hole. {NONE, EVEN, ODD}. Default is NONE)");
            Console.WriteLine("         [" + sep + "PERDXF=n]                              (Fill CNC cutting mat with this number of 1 inch wide (for 8-level) segment strips across before starting another. 5-level = 11/16 inch)");
            Console.WriteLine("         [" + sep + "QUIET]                                 (do not write any console output)");
            Console.WriteLine("         [" + sep + "RANGE=n,[L [-p] [+-z]]                 (Start generation at byte n and run for following length L or previous p or prefix/suffix z bytes)");
            Console.WriteLine("         [" + sep + "SEGMENT=n]                             (Length in 0.1 inch increments for one vertical-cut paper strip before generating adjacent segment)");
            Console.WriteLine("         [" + sep + "SPACE=c]                               (Console output character to represent a space (data bit = 0). Default is ' '");
            Console.WriteLine("         [" + sep + "SPROCKET=n]                            (Sprocket feed hole position. Default is 3 for between 3rd and 4th data bit holes starting from right)");
            Console.WriteLine("         [" + sep + "TEXT=\"" +
                "" +
                "YOUR TEXT\"]                      (Input text string to be punched, taken from the command line)");
            Console.WriteLine("         [" + sep + "TRAILER=n]                             (Suffix output with blank sprocket punch tape in 1/10 inch increments eg. 120 is 1 foot)");
            //BROKEN. TODO  Console.WriteLine("         [" + sep + "VEE]                                   (Draws /\\ vee at start of first and at end of last segment)");
            Console.WriteLine("         [" + sep + "VERSION]                               (Version number)");
            Console.WriteLine("         [" + sep + "WAIT]                                  (Pause for Enter on console after running)");
            Console.WriteLine("         [" + sep + "WHEATSTONE]                            (Generate 2-level Morse tape with USN Wheatstone coding (15/32 inch wide))");
        }
    }

    /// <summary>
    /// Helper extensions for string and char classes to perform problem domain-specific operations
    /// </summary>
    public static class PaperTapeExtensions
    {
        /// <summary>
        /// Helper extension to class String to convert an 8-bit array to a string
        /// </summary>
        /// <param name="bits">A bit array of 8 bits. Least Significant Bit is the lowest index in the array</param>
        /// <returns>String representation of the bit array</returns>
        public static string ToBitString(this BitArray bits)
        {
            var sb = new StringBuilder();
            for (int i = bits.Length - 1; i >= 0;  i--)
            {
                char c = bits[i] ? '1' : '0';
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Helper extension to class Char to convert an 8-bit array to a character.
        /// Modified to call a .NET Core equivalent
        /// </summary>
        /// <param name="bits">An array of 8 bits</param>
        /// <returns>A Char representation of the bit array</returns>
        public static char ToChar(this BitArray bits)
        {
#if DOTNETCORE
            return (char)GetByteReversed(bits);
#else
            byte[] b = new byte[2];
            bits.CopyTo(b, 0); //SM 20170630 BitArray.CopyTo(..) DOES NOT EXIST YET IN .NET Core
            return (char)b[0];
#endif
        }

        /// <summary>
        /// A BitArray CopyTo() replacement that works in .NET Core.
        /// https://stackoverflow.com/questions/560123/convert-from-bitarray-to-byte#560131
        /// </summary>
        /// <param name="bits">An array of 8 bits</param>
        /// <returns>Byte representation of the bit array</returns>
        public static byte GetByte(BitArray input)
        {
            int len = input.Length;
            if (len > 8)
                len = 8;
            int output = 0;
            for (int i = 0; i < len; i++)
                if (input.Get(i))
                    output += (1 << (len - 1 - i)); //this part depends on your system (Big/Little).  //output += (1 << i); //depends on system
            return (byte)output;
        }

        /// <summary>
        /// Reverse the pattern of bits in the byte so LSB is now MSB and so on
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte GetByteReversed(BitArray input)
        {
            return GetByte(Reverse(input));
        }

        /// <summary>
        /// The following function is from Stack Overflow:
        /// Most efficient way to reverse the order of a BitArray?  https://stackoverflow.com/questions/4791202/most-efficient-way-to-reverse-the-order-of-a-bitarray
        /// </summary>
        /// <param name="array"></param>
        public static BitArray Reverse(BitArray array)
        {
            int length = array.Length;
            int mid = (length / 2);

            for (int i = 0; i < mid; i++)
            {
                bool bit = array[i];
                array[i] = array[length - i - 1];
                array[length - i - 1] = bit;
            }
            return array;
        }

        // https://stackoverflow.com/questions/560123/convert-from-bitarray-to-byte#560131
        public static byte ConvertToByte(this BitArray bits)
        {
            byte b = 0;
            if (bits.Get(7)) b++;
            if (bits.Get(6)) b += 2;
            if (bits.Get(5)) b += 4;
            if (bits.Get(4)) b += 8;
            if (bits.Get(3)) b += 16;
            if (bits.Get(2)) b += 32;
            if (bits.Get(1)) b += 64;
            if (bits.Get(0)) b += 128;
            return b;
        }

        /// <summary>
        /// Helper extension to class String to convert an 8-bit array to a string for console output.
        /// Used to format a row of 8 bits where a blank space represents zero or no punched hole, and a 'O' represents a binary one, or punched hole.
        /// For 8-level tape and the default sprocket position a dot '.' is printed at the 5.3 position and represents the sprocket hole.
        /// The string is framed with a '|' at the left and right ends, to represent the paper tape edges.
        /// For example the letter 'a' is emitted as '| OO  .  O|'
        /// For other levels (eg. 5-level) the output is shown accordingly.
        /// </summary>
        /// <param name="bits">An array of 8 bits</param>
        /// <returns>A string of length characters of up the maximum width format |bbbbb.bbb| where space is unpunched and O is a punched hole and the sprocket is  a dot.</returns>
        // Allow console output format of any level row. eg. 8-level (ASCII), 5-level (Baudot) etc. 8 being the maximum
        public static string FormatNLevelRow(BitArray bits, int someLevel, int someSprocketFeedHolePosition, char someMark, char someSpace)
        {
            string row = string.Empty;
            row += "|";     // Draw left edge of paper tape
            // If the sprocket hole is desired to be at the extreme left, draw it first
            if (someSprocketFeedHolePosition > someLevel - 1)
                row += ".";
            for (int i = someLevel - 1; i >= 0; i--)
            {
                row += (bits.Get(i) ? someMark : someSpace);
                if (i == someSprocketFeedHolePosition)
                    row += ".";
            }
            row += "|";     // Draw right edge of paper tape
            return row;
        }


        /// <summary>
        /// Helper extension for class String to reverse the input string (for underside row generation) for adhesive joiners
        /// </summary>
        /// <param name="someInput">A string to have its characters reversed</param>
        /// <returns>Reversed string</returns>
        public static string Reverse(this string someInput)
        {
            return new string(someInput.ToCharArray().Reverse().ToArray());
        }

        /// <summary>
        /// Helper extension for class String to remove any characters from input filename that may not be used for filenames 
        /// </summary>
        /// <param name="someInput">Path to filename to be stripped of non-conforming characters</param>
        /// <returns>Parsed filename path</returns>
        public static string CleanFilename(this string someInput)
        {
            Regex regex = new Regex(@"[^a-zA-Z0-9._-]");
            return regex.Replace(someInput, "");
        }

        /// <summary>
        /// Helper extension for class String to convert a control character to a printable ASCII string representation
        /// See https://en.wikipedia.org/wiki/ASCII
        /// </summary>
        /// <param name="someInputChar">A character less than 1F hex or greater than or equal to 7F hex</param>
        /// <returns>A bracketed ASCII code representation of the control character</returns>
        public static string ToASCIIControlCodeName(this char someInputChar)
        {
            string s = string.Empty;
            if (char.IsControl(someInputChar) || someInputChar == 255)
            {
                switch (someInputChar)
                {
                    case '\u0000': s = "<NUL>"; break;
                    case '\u0001': s = "<SOH>"; break;
                    case '\u0002': s = "<STX>"; break;
                    case '\u0003': s = "<ETX>"; break;
                    case '\u0004': s = "<EOT>"; break;
                    case '\u0005': s = "<ENQ>"; break;
                    case '\u0006': s = "<ACK>"; break;
                    case '\u0007': s = "<BEL>"; break;
                    case '\u0008': s = "<BS>"; break;
                    case '\u0009': s = "<HT>"; break;
                    case '\u000A': s = "<LF>"; break;
                    case '\u000B': s = "<VT>"; break;
                    case '\u000C': s = "<FF>"; break;
                    case '\u000D': s = "<CR>"; break;
                    case '\u000E': s = "<SO>"; break;
                    case '\u000F': s = "<SI>"; break;
                    case '\u0010': s = "<DLE>"; break;
                    case '\u0011': s = "<DC1>"; break;
                    case '\u0012': s = "<DC2>"; break;
                    case '\u0013': s = "<DC3>"; break;
                    case '\u0014': s = "<DC4>"; break;
                    case '\u0015': s = "<NAK>"; break;
                    case '\u0016': s = "<SYN>"; break;
                    case '\u0017': s = "<ETB>"; break;
                    case '\u0018': s = "<CAN>"; break;
                    case '\u0019': s = "<EM>"; break;
                    case '\u001A': s = "<SUB>"; break;
                    case '\u001B': s = "<ESC>"; break;
                    case '\u001C': s = "<FS>"; break;
                    case '\u001D': s = "<GS>"; break;
                    case '\u001E': s = "<RS>"; break;
                    case '\u001F': s = "<US>"; break;
                    case '\u007F': s = "<DEL>"; break;
                    case '\u00FF': s = "RUBOUT"; break;
                    default: break;
                }
            }
            return s;
        }

        // TODO FIX THIS BAUDOT CONVERSION. (It is only used for console formatting, not Baudot DXF generation)
        public static string ToBaudotName(this BitArray someBitArray)
        {
            string s = string.Empty;
            byte z1 = someBitArray.ConvertToByte();

            int bsr = z1  & 31;
            char? csr = Baudot.GetLetter(bsr);

            int b = (int)z1 & 31;
            if (b == 27)
                s = " SHIFT TO FIGS ";
            if (b == 31)
                s = " SHIFT TO LETRS ";
            else
                s = Convert.ToString(b);
            return s;
        }
    }

    // The following Baudot class code snippet from https://stackoverflow.com/questions/22568251/how-to-implement-baudot-encoding#22568290
    public class Baudot
    {
        public const char Null = 'n';
        public const char ShiftToFigures = 'f';
        public const char ShiftToLetters = 'l';
        public const char Undefined = 'u';
        public const char Wru = 'w';
        public const char Bell = 'b';
        private const string Letters = "nE\nA SIU\rDRJNFCKTZLWHYPQOBGfMXVu";
        private const string Figures = "n3\n- b87\rw4',!:(5\")2#6019?&u./;l";

        public static char? GetFigure(int key)
        {
            char? c = Figures[key];
            return (c != Undefined) ? c : null;
        }

        public static int? GetFigure(char c)
        {
            int? i = Figures.IndexOf(c);
            return (i >= 0) ? i : null;
        }

        public static char? GetLetter(int key)
        {
            char? c = Letters[key];
            return (c != Undefined) ? c : null;
        }

        public static int? GetLetter(char c)
        {
            int? i = Letters.IndexOf(c);
            return (i >= 0) ? i : null;
        }
    }

    // Morse code
    public class Morse
    {
        public Dictionary<string, string> morse = new Dictionary<string, string>();
        public List<int> pips = new List<int>();

        public Morse()
        {
            // Letters
            morse.Add("A", ".-");
            morse.Add("B", "-...");
            morse.Add("C", "-.-.");
            morse.Add("D", "-..");
            morse.Add("E", ".");
            morse.Add("F", "..-.");
            morse.Add("G", "--.");
            morse.Add("H", "....");
            morse.Add("I", "..");
            morse.Add("J", ".---");
            morse.Add("K", "-.-");
            morse.Add("L", ".-..");
            morse.Add("M", "--");
            morse.Add("N", "-.");
            morse.Add("O", "---");
            morse.Add("P", ".--.");
            morse.Add("Q", "--.-");
            morse.Add("R", ".-.");
            morse.Add("S", "...");
            morse.Add("T", "-");
            morse.Add("U", "..-");
            morse.Add("V", "...-");
            morse.Add("W", ".--");
            morse.Add("X", "-..-");
            morse.Add("Y", "-.--");
            morse.Add("Z", "--..");
            // Numbers
            morse.Add("1", ".----");
            morse.Add("2", "..---");
            morse.Add("3", "...--");
            morse.Add("4", "...-");
            morse.Add("5", ".....");
            morse.Add("6", "-....");
            morse.Add("7", "--...");
            morse.Add("8", "---..");
            morse.Add("9", "----.");
            morse.Add("0", "-----");
            // Punctuation
            morse.Add(".", ".-.-.-");
            morse.Add(",", "--..--");
            morse.Add("?", "..--..");
            morse.Add("\'", ".----.");
            morse.Add("!", "-.-.--");
            morse.Add(":", "---...");
            morse.Add(";", "-.-.-.");
            morse.Add("=", "-...-");
            morse.Add("+", ".-.-.");
            morse.Add("-", "-....-");
            morse.Add("_", "..--.-");
            morse.Add("\"", ".-..-.");
            morse.Add("@", ".--.-.");
        }
    }

    // USN Wheatstone morse encoding
    // see http://www.quadibloc.com/feat.htm
    // and http://telegraphkeys.com/images/straightkeys/cable/cable%20tapes.jpg
    // and http://navy-radio.com/manuals/boehme-man-tm11-377.pdf
    public class Wheatstone : Morse
    {
        public List<int> GetLetter(char someLetter)
        {
            pips.Clear();
            // Space is a special case where the Wheatstone tape is just advanced one sprocket hole
            if (char.IsWhiteSpace(someLetter))
                pips.Add('\u0000');
            else
            {
                foreach (char pip in morse[someLetter.ToString().ToUpper()])
                {
                    if (pip == '-')
                    {
                        pips.Add('\u0001');
                        pips.Add('\u0002');
                    }
                    else
                        pips.Add('\u0003');
                }
            }
            pips.Add('\u0000'); // pips seperator
            return pips;
        }
    }

    // Cable code morse encoding
    // see http://www.quadibloc.com/feat.htm
    public class CableCode : Morse
    {
        public List<int> GetLetter(char someLetter)
        {
            pips.Clear();
            // Space is a special case where the Cable Code tape is just advanced one sprocket hole
            if (char.IsWhiteSpace(someLetter))
                pips.Add('\u0000');
            else
            {
                foreach (char pip in morse[someLetter.ToString().ToUpper()])
                {
                    if (pip == '-')
                        pips.Add('\u0002');
                    else
                        pips.Add('\u0001');
                }
            }
            pips.Add('\u0000'); // pips seperator
            return pips;
        }
    }

}
