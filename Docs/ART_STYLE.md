# Art style and motion — 2026-11-15

The game is drawn in a smooth, painted sticker style: soft round shapes with a dark brown outline. The word pictures and room pieces come from the painted sprite sheets. Backgrounds and the interface art are being replaced in the same style; the pixel-art backdrops still in the project are placeholders.

## Rules the game follows

- **Sizes.** The world is 16 × 9 units, and the interface canvas is 1280 × 720 with 80 interface units to a world unit. A word picture is 192 px at 160 px per unit (1.2 units). A room piece is drawn at 160 px per unit.
- **Smooth drawing.** Pictures use bilinear filtering with mipmaps. The camera fits the whole world to the window at any size, with a dark frame for other window shapes. Nothing snaps to a pixel grid.
- **Interface shapes in code.** `Runtime/Polish/Look.cs` draws cards, badges and icons as distance fields, four texture pixels per design pixel, with smooth edges. Their sizes match the old pixel versions.
- **Motion** (`Runtime/Polish/Motion.cs`, called through `Juice`):
  - Things squash and stretch. They pop in past their size, stretch when they hop and squash when they land.
  - Cards flip by narrowing to their edge.
  - Pieces sway slightly while they wait (`Juice.Idle`) and rise under the pointer (`Juice.Hover`). A held piece leans into the drag (`Juice.Hold`).
  - Everything is layered on the resting position, scale and turn, so game code may keep moving things.
- **Pieces** (`Runtime/Polish/TokenFeel.cs`):
  - Every token casts a soft shadow the shape of its card.
  - A piece dropped where it does not fit floats back home.
- **Effects** (`Juice`):
  - Correct answers burst into twinkles, soft glows and a widening ring.
  - Confetti flutters down.
  - Rain falls as soft streaks and snow as glowing flakes.
  - Faint motes drift up through every scene.
  - All of it respects Reduced motion.

## Earlier pixel-art notes (superseded)


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
