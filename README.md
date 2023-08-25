# Donkey-Konga-2-Texture-Editor

This is a simple program to edit .nut or namco texture.
[Namco Texture Editor]([https://duckduckgo.com](https://github.com/marco-calautti/Rainbow/wiki/NUT-File-Format#))

Allows you to replace an image in a .nut file with another image of the exact same size.

Note that images with more than 256 colors may not work properly. Namco textures do support images with more than 256 images, but that specific format is not implemented here.

The program does have a simple algorithm that attempts to pair together similar colors so that some images with more than 256 colors can function properly.

This project was made specifically for Donkey Konga 2 and tested using that game. However, if another game uses this this version of Namco Textures it should work.

This project was possible thanks to rainbow, it was a great resource in figuring out how to import textures and much of the code was lifted from the C# application: 
[Rainbow]([https://duckduckgo.com]([https://github.com/marco-calautti/Rainbow/wiki/NUT-File-Format#)](https://github.com/marco-calautti/Rainbow)https://github.com/marco-calautti/Rainbow)

I do not intend to update this further unless other people begin using it. This was created before I had much expierence with WPF so the UI is pretty terrible. 

Exceptions should hopefully appear in a pop-up box or at least common ones I've seen while using this tool. If you do begin using this and are running into issues I'd be happy to make updates!
