# Pixel art — 2026-11-14

The whole game is pixel art on one grid. The 16 × 9 world is 640 × 360 picture pixels, so there are 40 pixels to a world unit, and every picture is shown at exactly that size.

## Rules the game follows

- Every sprite is imported at 40 pixels per unit, point filtered, uncompressed, without mipmaps. `Tools/InkArt/pixel_meta.py` sets this on every picture the drawing kit makes.
- Nothing is scaled or turned. A picture that should look bigger is a bigger picture (the house pieces in `Resources/Pieces` are drawn at room size next to their 48 pixel word pictures).
- Positions are whole pixels. `Runtime/Polish/Pixels.cs` has the helpers: `Snap`, `Even`, `Native`, `Card`.
- Motion is whole pixels too: a hover lifts a picture by one pixel, a pop drops it in from four pixels up (`Runtime/Polish/Motion.cs`).
- The interface uses the same pixels. The canvas is 1280 × 720 units, so one picture pixel is two interface units; buttons, tags and icons are generated in `Runtime/Polish/Look.cs`.
- `WorldCameraFit` enlarges the picture by a whole number (3× on a 1920 × 1080 screen, 2× from 1280 × 720) and leaves a dark frame around it when the window is not an exact multiple. A window smaller than 1280 × 720 falls back to fitting the picture, with uneven pixels.
- Text stays in the smooth interface font.

## Looking at it in the Editor

Set the Game view to Full HD (1920 × 1080) or another fixed size of at least 1280 × 720. With "Free Aspect" the view is usually an odd size and the picture sits inside a dark frame.

## Adding or changing pictures

- Word pictures are 48 × 48 pixels, backdrops 640 × 360. Keep sizes even so a centred picture stays on the grid.
- The drawing kit in `Tools/InkArt` draws every picture: run a `draw_*.py` or `make_*.py` script with no arguments to write into the project, then `python3 Tools/InkArt/pixel_meta.py`. `ink.PIX = False` in `ink.py` brings back the earlier smooth drawings.
- A hand-drawn replacement works the same way: save it over the generated file at the same size, or give a new file the import settings above.
