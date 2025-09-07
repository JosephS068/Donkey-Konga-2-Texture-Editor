# Donkey Konga 2/3 Texture Editor

This is a simple program to edit .nut or [Namco Texture](https://github.com/marco-calautti/Rainbow/wiki/NUT-File-Format#).

<img width="880" height="788" alt="image" src="https://github.com/user-attachments/assets/89f048ab-7dd3-4121-91d3-a6ee837a9ff3" />

Allows you to replace an image in a .nut file with another image of the exact same size.

Note that images with more than 256 colors may not work properly. Namco textures supports images with more than 256 images, but that specific format is not implemented here.

The program does have a simple algorithm that attempts to pair together similar colors so that some images with more than 256 colors can function properly.

This project was made specifically for Donkey Konga 2 and tested using that game. However, if another game uses this this version of Namco Textures it should work.

This project was possible thanks to rainbow, it was a great resource in figuring out how to import textures and much of the code was lifted from the C# application: 
[Rainbow](https://github.com/marco-calautti/Rainbow)

Exceptions should hopefully appear in a pop-up box or at least common ones I've seen while using this tool. If you do begin using this and are running into issues I'd be happy to make updates!

This tool was used to create the Demo For Donkey Konga Bongo Troopers: [Link](https://www.romhacking.net/forum/index.php?topic=37588.0).
Donkey Konga Bongo Trooper features an example custom song, chart and textures.
