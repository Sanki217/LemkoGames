"""The two card pictures the scenes and prefabs use for panels and buttons, as pixel art nine-slices.

A card is 16 x 16 pixels at the game's 40 pixels per unit with a 6 pixel border: white with a one pixel dark outline
and cut corners, so the game can tint it with any palette colour. The button has a darker lower lip.
The other interface shapes (hearts, stars, badges) are drawn by the game itself, see Runtime/Polish/Look.cs.
Usage: python3 make_ui.py [preview dir]
"""
import sys
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/Lemko/Sprites/UI'
INK, WHITE, LIP = (58, 22, 13, 255), (255, 255, 255, 255), (204, 204, 204, 255)
INSET = (4, 2, 1, 1)          # how far each of the first rows is cut in at a corner


def card(lip=0, size=16):
    im = Image.new('RGBA', (size, size), (0, 0, 0, 0))

    def start(y):
        d = min(y, size - 1 - y)
        return INSET[d] if d < len(INSET) else 0
    for y in range(size):
        a = start(y)
        for x in range(a, size - a):
            edge = y in (0, size - 1) or x in (a, size - 1 - a)
            # the outline also closes the steps of the cut corners
            for ny in (y - 1, y + 1):
                if 0 <= ny < size and (x < start(ny) or x >= size - start(ny)):
                    edge = True
            im.putpixel((x, y), INK if edge else LIP if lip and y >= size - 1 - lip else WHITE)
    return im


if __name__ == '__main__':
    folder = Path(sys.argv[1]) if len(sys.argv) > 1 else OUT
    card().save(str(folder / 'ui_card.png'))
    card(2).save(str(folder / 'ui_button.png'))
