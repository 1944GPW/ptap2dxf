Last updated 10:34 AM 01-Dec-17

Prerequisite
	* The .NET Core runtime from https://www.microsoft.com/net/core

To build PTAP2DXF firstly get the project from Github and place the solution files somewhere, run a shell and change to that directory.
I am using the Git Bash shell on Windows 10 here.


cd /c/Path/To/ptap2dxf


**** You should see something like

/c/Path/to/ptap2dxf $ ls -l
total 20
-rw-r--r-- 1 you 197121    0 Dec  1 10:04 __readme_Building_and_running.txt
drwxr-xr-x 1 you 197121    0 Dec  1 09:52 Documentation/
drwxr-xr-x 1 you 197121    0 Aug  3 17:22 packages/
drwxr-xr-x 1 you 197121    0 Dec  1 09:43 ptap2dxf/
-rw-r--r-- 1 you 197121 1490 Nov 30 17:09 ptap2dxf.sln
drwxr-xr-x 1 you 197121    0 Nov 30 16:43 Samples/
drwxr-xr-x 1 you 197121    0 Nov 30 17:30 UnitTestProject1/
/c/Path/to/ptap2dxf $

**** Restore and Build with 
**** 	$ dotnet restore
**** 	$ dotnet build
**** as follows.

**** Restore the packages used:

/c/Path/To/ptap2dxf $ dotnet restore
  Restoring packages for C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj...
  Restoring packages for C:\Path\To\ptap2dxf\UnitTestProject1\ptap2dxf.Tests.csproj...
  Lock file has not changed. Skipping lock file write. Path: C:\Path\To\ptap2dxf\ptap2dxf\obj\project.assets.json
  Restore completed in 679.6 ms for C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj.
  Lock file has not changed. Skipping lock file write. Path: C:\Path\To\ptap2dxf\UnitTestProject1\obj\project.assets.json
  Restore completed in 798.53 ms for C:\Path\To\ptap2dxf\UnitTestProject1\ptap2dxf.Tests.csproj.

  NuGet Config files used:
      C:\Users\you\AppData\Roaming\NuGet\NuGet.Config
      C:\Program Files (x86)\NuGet\Config\Microsoft.VisualStudio.Offline.config

  Feeds used:
      https://api.nuget.org/v3/index.json
      C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\


**** Now build the program:

/c/Path/To/ptap2dxf $ dotnet build
Microsoft (R) Build Engine version 15.1.1012.6693
Copyright (C) Microsoft Corporation. All rights reserved.

ptap2dxf.cs(712,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(725,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(755,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(773,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(790,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(806,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(827,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(850,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(863,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(904,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(917,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(934,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(950,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(963,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(976,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(1001,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
  ptap2dxf -> C:\Path\To\ptap2dxf\ptap2dxf\bin\Debug\netcoreapp1.1\ptap2dxf.dll
  ptap2dxf.Tests -> C:\Path\To\ptap2dxf\UnitTestProject1\bin\Debug\netcoreapp1.1\ptap2dxf.Tests.dll

Build succeeded.

ptap2dxf.cs(712,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(725,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(755,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(773,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(790,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(806,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(827,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(850,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(863,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(904,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(917,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(934,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(950,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(963,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(976,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
ptap2dxf.cs(1001,42): warning CS0168: The variable 'e' is declared but never used [C:\Path\To\ptap2dxf\ptap2dxf\ptap2dxf.csproj]
    16 Warning(s)
    0 Error(s)

Time Elapsed 00:00:07.31



**** Now change to where the compiled assembly is. You should see a ptap2dxf.dll there:

/c/Path/To/ptap2dxf $ cd ptap2dxf/bin/Debug/netcoreapp1.1/
/c/Path/To/ptap2dxf/ptap2dxf/bin/Debug/netcoreapp1.1 $ ls -l
total 87
-rw-r--r-- 1 you 197121   456 Dec  1 10:16 ptap2dxf.deps.json
-rwxr-xr-x 1 you 197121 56832 Dec  1 10:16 ptap2dxf.dll*
-rw-r--r-- 1 you 197121 12372 Dec  1 10:16 ptap2dxf.pdb
-rw-r--r-- 1 you 197121   120 Dec  1 10:16 ptap2dxf.runtimeconfig.dev.json
-rw-r--r-- 1 you 197121   125 Dec  1 10:16 ptap2dxf.runtimeconfig.json
/c/Path/To/ptap2dxf/ptap2dxf/bin/Debug/netcoreapp1.1 $


**** You should now be able to run the program:

/c/Path/To/ptap2dxf/ptap2dxf/bin/Debug/netcoreapp1.1 $ dotnet ptap2dxf.dll
PTAP2DXF - Generate DXF files from teletype punched paper tape binary images, suitable for home CNC stencil cutting
Written in C# by Steve Malikoff 2017 in Brisbane, Australia. Uses the DxfMaker library written by David S. Tufts
See https://github.com/1944GPW for more details
Usage:
ptap2dxf [inputfilename.ptap]                      (Input ASCII file to be punched. Same as --INPUT="/path/to/inputfile")
         [--ASCII]                                 (Show ASCII character representation for row on console output)
         [--BANNERFILE=/path/to/bannerfile]        (Generate uppercase punched banner in 8x8 font from ASCII file contents)
         [--BANNERTEXT="YOUR TEXT"]                (Generate uppercase punched banner in 8x8 font from string)
         [--BAUDOT]                                (convert ASCII characters to Baudot. Forces 5-level output)
         [--CONTROL-CHARS]                         (Show control characters on console output)
         [--DRYRUN]                                (Run everything but do not generate DXF file(s))
         [--FLIP]                                  (Invert bit pattern. Logical NOT)
         [--GAP=n]                                 (Inter-segment gap in mm between each paper segment on CNC cutting mat. Default is 0, ie. shared edges with no gap)
         [--HELP]                                  (or ? prints this help)
         [--INPUT=/path/to/inputfile]              (.ptap or any binary or ASCII input file. Optional switch, does not need to be given with filename)
         [--JOINER]                                (Make adhesive joiners for paper segments)
         [--LEADER=n]                              (Prefix output with blank sprocket punch tape in 1/10 inch increments eg. 240 is 2 feet. aka /HEADER=)
         [--LEVEL=n]                               (The number of data bits in a row of holes. Default is 8 for byte-width ASCII 8-level. Use 5 for 5-level)
         [--MARK=c]                                (Console output character to represent a mark (data bit = 1). Default is 'O'
         [--MIRROR]                                (Reverse the output mark/space bit pattern to right-left)
         [--NUMBER=BANNER|LEADER|CODE|TRAILER|ALL] (NOTE: --N defaults to number the code lines only)
         [--OUTPUT=/path/to/outputfile.dxf]        (output DXF file)
         [--PERDXF=n]                              (Fill CNC cutting mat with this number of 1-inch-wide (for 8-level) segment strips across before starting another. 5-level = 11/16-inch)
         [--QUIET]                                 (do not write any console output)
         [--RANGE=n,[L [-p] [+-z]]                 (Start generation at byte n and run for following length L or previous p or prefix/suffix z bytes)
         [--SEGMENT=n]                             (Length in 0.1 inch increments for one vertical-cut paper strip before generating adjacent segment)
         [--SPACE=c]                               (Console output character to represent a space (data bit = 0). Default is ' '
         [--SPROCKET=n]                            (Sprocket feed hole position. Default is 3 for between 3rd and 4th data bit holes starting from right)
         [--TEXT="YOUR TEXT"]                      (Input text string to be punched, taken from the command line)
         [--TRAILER=n]                             (Suffix output with blank sprocket punch tape in 0.1 inch increments eg. 120 is 1 foot)
         [--VERSION]                               (Version number)
         [--WAIT]                                  (Pause for Enter on console after running)
/c/Path/To/ptap2dxf/ptap2dxf/bin/Debug/netcoreapp1.1 $

