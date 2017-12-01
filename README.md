# ptap2dxf
PTAP2DXF - Generate CNC-cut paper tapes from .PTAP or other binaries on a home stencil cutting machine 

* Do you have a vintage computer and paper tape reader, but no punch?
* Do you need to make some repairs to an existing paper tape?
* Interested in punching other materials but concerned they might damage your punch?
* Are you looking for a quick way to visualise a paper tape (.ptap) file for SIMH?
* Would you like to produce paper tape banners from ASCII text?
* Do you want to make 5-level Baudot-encoded RTTY paper tape?
* Want to make your own custom-punched paper tape?

If your answer is 'yes' to any of these questions then PTAP2DXF is for you.

PTAP2DXF is a small open source command line utility that allows the user to make up to 8-level ASCII paper tapes similar to those 
punched by an ASR33 Teletype, using a common home CNC stencil cutter from binary images available on the internet, or your own 
files. For the default 8-level tape it makes, the output is one inch wide with a pattern of up to 8 data mark/space holes across 
as well as a smaller sprocket feed hole. The rows are spaced at 0.1 inch apart.

Its primary use is to make small tapes for loading software such as Absolute Loaders, MAINDECs (diagnostics)  or other programs 
into vintage computers such as the Digital Equipment Corporation PDP-11, Altair, IMSAI or the like. These images are commonly 
found with a .PTAP, .PTP,, .TAP, .BIN or other extension and are often used with the SIMH simulator. PTAP2DXF does not actually 
care about file types as it treats everything as binary. In some situations the binary may first need to be converted using a 
special utility to a paper tape image. In addition PTAP2DXF can be used for fun purposes such as punching letters and words 
along the tape in an upper case 8x8 font, or as a text label punched as a prefixed description at the start of a binary tape. 
It can make 11/16”-wide 5-level tape for an old Baudot machine. You could even experiment making tapes from various types of 
plastic film or sheet.
