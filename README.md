# Donkey Konga 2/3 Texture Editor

This program allows you to edit .nut files([Namco Texture](https://github.com/marco-calautti/Rainbow/wiki/NUT-File-Format#)).

<img width="1177" height="788" alt="image" src="https://github.com/user-attachments/assets/025ca8de-ac16-49fa-bc06-7d7b15b53228" />


Allows you to replace images in a .nut file so long as the new image is the exact same dimensions.

Note that images with more than 256 colors may not work properly. Namco textures supports images with more than 256 images, but that specific format is not implemented here.

The program does have a simple algorithm that attempts to pair together similar colors so that some images with more than 256 colors can function properly.

This project was made specifically for Donkey Konga 2, but Donkey Konga 3 has been tested and all texture files from both games can be viewed in this application. If other games use this file format they should also work.

This project was possible thanks to rainbow, it was a great resource in figuring out how to import textures and much of the code was lifted from the C# application: 
[Rainbow](https://github.com/marco-calautti/Rainbow)

Please feel free to create new issues if you run into problems!

## Used In the Donkey Konga Bongo Troopers Demo
This tool was used to create the Demo For Donkey Konga Bongo Troopers: [Link](https://www.romhacking.net/forum/index.php?topic=37588.0).
Donkey Konga Bongo Trooper features an example custom song, stages, chart and textures.

Bongo Troopers Screenshots:


<img width="483" height="359" alt="Screenshot 2023-08-27 at 4 22 26 PM" src="https://github.com/user-attachments/assets/b3385392-e6b8-4830-aae8-e7ffb2b7b928" />
<img width="483" height="359" alt="Screenshot 2023-08-27 at 4 17 43 PM" src="https://github.com/user-attachments/assets/c1be38b4-a658-40b9-a907-8c591866ce33" />
<img width="483" height="359" alt="Screenshot 2023-08-27 at 4 18 05 PM" src="https://github.com/user-attachments/assets/c0135e89-0cf0-40c7-9e66-32d1516147c9" />
<img width="483" height="359" alt="Screenshot 2023-08-27 at 4 19 04 PM" src="https://github.com/user-attachments/assets/46a5e376-171d-4efe-b611-0c17b3a293da" />

