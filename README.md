![alt text](https://github.com/1944GPW/ptap2dxf/blob/master/Photos%20and%20screenshots/PTAP2DXF_logo.png?raw=true)

# ptap2dxf
PTAP2DXF - Generate CNC-cut paper tapes from .PTAP or other binaries on a home stencil cutting machine 

* Do you have a vintage computer and paper tape reader, <b><i>but no punch</i></b>?
* Do you need to make some repairs to an existing paper tape?
* Interested in punching other materials but concerned they might damage a real punch?
* Are you looking for a quick way to visualise a paper tape (.ptap) file for SIMH?
* Would you like to produce paper tape banners from ASCII text?
* Do you want to make 5-level Baudot-encoded RTTY paper tape?
* Want to make your own custom-punched or chadless n-level paper tape?
* Want 15/32" Morse tape for demonstrating your pre-war Creed or wartime USN equipment?

If your answer is 'yes' to any of these questions then PTAP2DXF is for you.

This tape was created with PTAP2DXF, a 2016 model Silhouette CAMEO vinyl/stencil cutter and a used large yellow business envelope:

![alt text](https://github.com/1944GPW/ptap2dxf/blob/master/Photos%20and%20screenshots/19_finished_tape_2_small.jpg?raw=true)

PTAP2DXF is a small open source command line utility that allows the user to make up to 8-level ASCII paper tapes similar to those 
punched by an ASR33 Teletype, <b>using only a common home CNC vinyl/stencil cutter</b> (eg. Silhouette Cameo, Cricut etc) from binary images available on the internet, or your own files. 
For the default 8-level tape it makes, the output is one inch wide with a pattern of up to 8 data mark/space holes across 
as well as a smaller sprocket feed hole. The rows are spaced at 0.1 inch apart. 
The sprocket hole can be moved to any position desired, even outside the data bits.

Its primary use is to make small tapes for loading software such as Absolute Loaders, MAINDECs (diagnostics) or other programs 
into vintage computers such as the Digital Equipment Corporation PDP-11, Altair, IMSAI or the like. These images are commonly 
found with a .PTAP, .PTP, .TAP, .BIN or other extension and are often used with the SIMH simulator. PTAP2DXF does not actually 
care about file types as it treats everything as binary. In some situations the binary may first need to be converted using a 
special utility to a paper tape image. 

In addition PTAP2DXF can be used for fun purposes such as punching letters and words 
along the tape in an upper case 8x8 font, or as a text label punched as a prefixed description at the start of a binary tape. 

It can make 11/16”-wide 5-level tape for an old Baudot machine, and Teletype Corp's unusual half-punched chadless tape from the mid 70s. 
It can now act like a Morse perforator and produce 15/32" wide Morse tape in both USN Wheatstone and Cable Code protocols.
You can use it to experiment making tapes from various types of plastic film or sheet.

* For more information see the User Manual PDF in the Documentation folder.
* For a ready-to-run program for .NET Core 3.1 look in the Pre-built_binary folder for PTAP2DXF.EXE
* This is a .NET Core application, for Linux and Mac platforms follow the instructions in __readme_Building_and_running.txt or try the pre-built exe
* IN THE WORKS: more exotic historical paper tape formats coming!