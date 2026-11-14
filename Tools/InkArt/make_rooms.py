"""The four rooms of the House game, each a whole decorated picture that fills the screen, and the
furniture bar that lies along the bottom of the screen.

World (wx, wy) is at 960 + wx*120, 540 - wy*120. The game draws on top of a room:
  - the score, the task ("Place X in Z") and the feedback line over the top 290 px, so that strip is plain wall;
  - an arrow button at each side (around px 75 and 1845, py 510);
  - the furniture bar over everything below py 894.
The floor line is at py 600. Pieces the player sets down may stand anywhere between py 336 and 804.
Usage: python3 make_rooms.py [preview dir]
"""
import math, sys
from pathlib import Path
from scene import *

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/Lemko/Resources/Rooms'
PREVIEW = Path(sys.argv[1]) if len(sys.argv) > 1 else None
W, H, FLOOR = 1920, 1080, 600
CREAM, BONE, ICE, PALE, MINT, PINK, ROSE, MAROON = (PAL[k] for k in 'cream bone ice pale mint pink rose maroon'.split())
BLUE, SLATE, NIGHT, SILVER, GREY, SAGE = (PAL[k] for k in 'blue slate night silver grey2 sage'.split())


def out(c, name, size):
    """Save a picture the way every backdrop is saved: pale orange-brown lines and slightly hazy colours,
    so the dark-outlined pieces in front of it stand out."""
    folder = PREVIEW or OUT
    folder.mkdir(parents=True, exist_ok=True)
    path = str(folder / (name + '.png'))
    c.save(path, size)
    import numpy as np
    from PIL import Image
    im = np.asarray(Image.open(path).convert('RGB')).astype(float)
    ink = np.array(rgb(INK)) * 255; line = np.array(rgb('#b9764a')) * 255; haze = np.array(rgb('#f9f0cc')) * 255
    w = np.clip(1 - np.sqrt(((im - ink) ** 2).sum(2)) / 90, 0, 1)[..., None]
    im = im + (line - im) * w
    im = im + (haze - im) * .1
    Image.fromarray(im.round().astype('uint8')).save(path)


def begin(seed, wall):
    c = Scene(W, H, 1.5, wall, seed)
    c.rad = 6; c.wob = 1.0; c.seg = 22
    return c


def box(c, x0, y0, x1, y1, fill, w=2.6, dx=4, dy=0, **k):
    return c.two([(x0, y0), (x1, y0), (x1, y1), (x0, y1)], fill, w, dx, dy, **k)


# --- walls and floors -----------------------------------------------------------------------
def stripes(c, col, alpha=.3, step=44, wid=16, y1=FLOOR):
    for i in range(W // step + 2):
        c.flat([(i * step, 0), (i * step + wid, 0), (i * step + wid, y1), (i * step, y1)], col, alpha)


def sprinkle(c, fn, y0=40, y1=FLOOR - 30, dx=120, dy=92):
    for j, y in enumerate(range(y0, y1, dy)):
        for i in range(-1, W // dx + 2):
            fn(i * dx + (j % 2) * dx / 2, y, i + j)


def planks(c, col=TONE1, seam=WOOD):
    r = c.r
    c.flat([(0, FLOOR), (W, FLOOR), (W, H), (0, H)], col)
    c.flat([(0, FLOOR), (W, FLOOR), (W, FLOOR + 30), (0, FLOOR + 30)], dk(col), .6)
    for i in range(-8, 26):
        x = 960 + (i - 8) * 150
        a, b = (960 + (x - 960) * .8, FLOOR + 4), (960 + (x - 960) * 1.5, H)
        c.line([a, b], 2, seam, amp=.8)
        for t in (r.uniform(.15, .45), r.uniform(.55, .9)):
            px, py = a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t
            c.line([(px, py), (px + 150 * (.8 + t * .7), py)], 1.4, seam, amp=.4)


def checker(c, a, b, seam, s0=120, s1=215, rows=5):
    """A tiled floor seen from the front: the tiles widen towards the viewer."""
    ys = [FLOOR + (H - FLOOR) * (j / rows) ** 1.25 for j in range(rows + 1)]
    X = lambda k, y: 960 + k * (s0 + (s1 - s0) * (y - FLOOR) / (H - FLOOR))
    c.flat([(0, FLOOR), (W, FLOOR), (W, H), (0, H)], a)
    for j in range(rows):
        for k in range(-12, 12):
            if (k + j) % 2:
                c.flat([(X(k, ys[j]), ys[j]), (X(k + 1, ys[j]), ys[j]), (X(k + 1, ys[j + 1]), ys[j + 1]), (X(k, ys[j + 1]), ys[j + 1])], b)
    for j in range(1, rows):
        c.line([(0, ys[j]), (W, ys[j])], 1.4, seam, amp=.5)
    for k in range(-12, 13):
        c.line([(X(k, FLOOR), FLOOR), (X(k, H), H)], 1.4, seam, amp=.5)
    c.flat([(0, FLOOR), (W, FLOOR), (W, FLOOR + 26), (0, FLOOR + 26)], seam, .35)


def skirting(c, col=WOOD_DARK, h=18):
    c.flat([(0, FLOOR - h), (W, FLOOR - h), (W, FLOOR + 4), (0, FLOOR + 4)], col)
    c.line([(0, FLOOR - h), (W, FLOOR - h)], 2.4, amp=1); c.line([(0, FLOOR + 4), (W, FLOOR + 4)], 3, amp=1)


def beam(c):
    """A dark ceiling beam along the very top."""
    c.flat([(0, 0), (W, 0), (W, 20), (0, 20)], WOOD_DARK)
    c.line([(0, 20), (W, 20)], 2.6, amp=1)
    for x in range(120, W, 300):
        c.line([(x, 6), (x + 110, 8)], 1.2, WOOD, amp=.4)


# --- shared props ---------------------------------------------------------------------------
def window(c, x, y, w, h, curtain, night=False, drop=None, pots=()):
    """Window with a view. drop is where long curtains end; without it the curtains are short and tied back."""
    frame = [(x - w / 2 - 14, y - h / 2 - 14), (x + w / 2 + 14, y - h / 2 - 14), (x + w / 2 + 14, y + h / 2 + 14), (x - w / 2 - 14, y + h / 2 + 14)]
    c.two(frame, WHITE, 3, 4, 4)
    pane = [(x - w / 2, y - h / 2), (x + w / 2, y - h / 2), (x + w / 2, y + h / 2), (x - w / 2, y + h / 2)]
    def view():
        if night:
            c.flat(pane, PAL['navy'])
            for sx, sy in ((-.34, -.3), (-.1, -.38), (.14, -.12), (.36, -.34), (-.26, .02), (.3, .08), (0, -.2)):
                c.dotc(x + sx * w, y + sy * h, 3.2, CREAM)
            c.oval(x + w * .2, y - h * .2, 30, 30, CREAM, 0); c.oval(x + w * .2 + 14, y - h * .2 - 8, 26, 26, PAL['navy'], 0)     # crescent moon
            c.flat([(x - w, y + h), (x - w, y + h * .3), (x - w * .2, y + h * .12), (x + w * .1, y + h * .3), (x + w * .5, y + h * .16), (x + w, y + h)], SLATE)
        else:
            c.flat(pane, PAL['sky'])
            c.flat(c.lumpy(x - w * .2, y - h * .26, 40, 14, 4, .4), WHITE); c.flat(c.lumpy(x + w * .26, y - h * .1, 30, 11, 4, .4), WHITE)
            c.oval(x + w * .22, y - h * .3, 20, 20, GOLD, 0)
            c.flat([(x - w, y + h), (x - w, y + h * .12), (x - w * .12, y - h * .08), (x + w * .1, y + h * .14), (x + w * .5, y), (x + w, y + h)], GRASS)
            c.flat([(x - w, y + h), (x - w, y + h * .3), (x, y + h * .2), (x + w, y + h * .32), (x + w, y + h)], GRASS_DARK)
    c.inside(pane, view)
    c.stroke(pane, 2.2)
    c.line([(x, y - h / 2), (x, y + h / 2)], 4, WHITE, amp=.3); c.line([(x - w / 2, y), (x + w / 2, y)], 4, WHITE, amp=.3)
    sill = y + h / 2 + 14
    box(c, x - w / 2 - 30, sill, x + w / 2 + 30, sill + 18, WHITE, 2.6, 3, 3)
    for px, col in pots:
        plant(c, x + px, sill, col)
    top = y - h / 2 - 34
    for s in (-1, 1):
        ex = x + s * (w / 2 + 22)
        if drop:
            cur = [(ex + s * 26, top), (ex - s * 62, top), (ex - s * 50, (top + drop) / 2), (ex - s * 58, drop), (ex + s * 30, drop)]
            def folds(ex=ex, s=s):
                for i in range(1, 4):
                    c.line([(ex - s * (62 - i * 22), top + 10), (ex - s * (56 - i * 22), drop - 6)], 1.4, dk(curtain), amp=.6)
            c.two(cur, curtain, 2.6, 4 * s, 0, inner=folds)
        else:
            mid = y + h * .12
            cur = [(ex + s * 22, top), (ex - s * 70, top), (ex - s * 56, y - h * .2), (ex - s * 12, mid), (ex - s * 30, y + h / 2 + 8), (ex + s * 24, y + h / 2 + 8)]
            c.two(cur, curtain, 2.6, 4 * s, 0)
            c.line([(ex - s * 16, mid - 4), (ex + s * 22, mid - 2)], 3, GOLD, amp=.3)
    c.line([(x - w / 2 - 70, top), (x + w / 2 + 70, top)], 3.4, amp=.5)
    c.dotc(x - w / 2 - 74, top, 7, INK); c.dotc(x + w / 2 + 74, top, 7, INK)


def plant(c, x, y, bloom=RED, k=1.0):
    """A small pot of flowers; x,y is the bottom middle."""
    for dx, dy in ((-12, -52), (0, -62), (12, -50)):
        c.line([(x, y - 24 * k), (x + dx * k, y + dy * k)], 2, LEAF_DARK, amp=.3)
        c.oval(x + dx * k, y + dy * k, 9 * k, 9 * k, bloom, 1.6)
    c.shape([(x - 14 * k, y - 36 * k), (x - 28 * k, y - 46 * k), (x - 12 * k, y - 26 * k)], LEAF, 1.6, amp=.3, step=8)
    c.shape([(x + 14 * k, y - 34 * k), (x + 30 * k, y - 42 * k), (x + 12 * k, y - 24 * k)], LEAF, 1.6, amp=.3, step=8)
    c.two([(x - 18 * k, y - 26 * k), (x + 18 * k, y - 26 * k), (x + 13 * k, y), (x - 13 * k, y)], ORANGE, 2.2, 3, 0, amp=.3, step=8)


def jar(c, x, y, w, h, col, lid=WOOD_DARK):
    c.two([(x - w / 2, y), (x - w / 2, y - h + 8), (x - w / 2 + 6, y - h), (x + w / 2 - 6, y - h), (x + w / 2, y - h + 8), (x + w / 2, y)], col, 2.2, 3, 0, amp=.4)
    c.shape([(x - w / 2 + 2, y - h), (x + w / 2 - 2, y - h), (x + w / 2 - 2, y - h - 9), (x - w / 2 + 2, y - h - 9)], lid, 2, amp=.3)
    c.shape([(x - w / 2 + 6, y - h * .62), (x + w / 2 - 6, y - h * .62), (x + w / 2 - 6, y - h * .3), (x - w / 2 + 6, y - h * .3)], WHITE, 1.4, amp=.3)


def basket(c, x, y, w, h, col=None):
    """Wicker basket; x,y is the bottom middle."""
    col = col or PAL['gold']
    c.sh_oval(x + 6, y + 2, w * .58, 10)
    body = [(x - w / 2, y - h), (x + w / 2, y - h), (x + w / 2 - 10, y), (x - w / 2 + 10, y)]
    def weave():
        for j in range(1, int(h / 16) + 1):
            c.line([(x - w, y - h + j * 16), (x + w, y - h + j * 16)], 1.4, WOOD, amp=.5)
        for i in range(int(w / 15) + 2):
            c.line([(x - w / 2 + i * 15, y - h), (x - w / 2 + i * 15 + (5 if i % 2 else -5), y)], 1.1, WOOD, amp=.3)
    c.two(body, col, 2.6, 4, 0, inner=weave)
    box(c, x - w / 2 - 6, y - h - 8, x + w / 2 + 6, y - h + 6, WOOD, 2.4, 3, 0)


# --- the rooms ---------------------------------------------------------------------------------
def living_room():
    c = begin(51, PAPER)
    stripes(c, SAND, .3)
    # white panelling under a rail
    top = FLOOR - 136
    c.flat([(0, top), (W, top), (W, FLOOR), (0, FLOOR)], BONE)
    for i in range(14):
        x = i * 142 - 20
        c.shape([(x, top + 30), (x + 112, top + 30), (x + 112, FLOOR - 36), (x, FLOOR - 36)], WHITE, 1.4, SAND, amp=.5, step=30)
    box(c, -10, top - 12, W + 10, top + 10, WHITE, 2.4, 0, 3)
    planks(c); skirting(c); beam(c)

    window(c, 540, 330, 250, 240, RED, drop=FLOOR - 26, pots=((-70, RED), (60, GOLD)))

    # two little oval portraits and a pair of candle sconces between the window and the corner
    for fx, fy, hair in ((960, 366, WOOD_DARK), (1100, 382, PAL['gold'])):
        c.line([(fx, fy - 76), (fx, fy - 58)], 1.8, amp=.2)
        c.two(c.ring(fx, fy, 46, 56), WOOD, 2.8, 4, 4, amp=.5, step=9)
        c.oval(fx, fy, 34, 44, CREAM, 1.8)
        def face(fx=fx, fy=fy, hair=hair):
            c.flat(c.ring(fx, fy + 44, 30, 26), (PAL['sage'], MAROON)[hair == WOOD_DARK])
            c.flat(c.ring(fx, fy - 4, 17, 19), SKIN); c.flat(c.ring(fx, fy - 16, 19, 12), hair)
        c.inside(c.ring(fx, fy, 34, 44), face)
    for sx in (850,):
        box(c, sx - 16, 392, sx + 16, 404, WOOD_DARK, 2.2, 2, 0)
        c.line([(sx, 404), (sx, 428)], 3, amp=.2)
        box(c, sx - 7, 352, sx + 7, 392, WHITE, 2, 2, 0)
        c.shape([(sx, 322), (sx + 8, 340), (sx, 352), (sx - 8, 340)], GOLD, 1.6, amp=.2, step=8)

    # hat stand in the corner with a hat, a scarf and an umbrella
    hx = 1270
    c.sh_oval(hx + 6, 652, 60, 10)
    c.line([(hx, 300), (hx, 640)], 9, amp=.4); c.line([(hx, 300), (hx, 640)], 5, WOOD, amp=.4)
    for dx in (-52, 52):
        c.line([(hx, 632), (hx + dx, 650)], 7, amp=.3); c.line([(hx, 632), (hx + dx, 650)], 3.6, WOOD, amp=.3)
        c.line([(hx, 330), (hx + dx * .8, 306)], 6, amp=.3); c.line([(hx, 330), (hx + dx * .8, 306)], 3, WOOD, amp=.3)
    c.two([(hx - 86, 304), (hx - 64, 296), (hx - 58, 262), (hx - 26, 262), (hx - 20, 296), (hx + 2, 304)], PAL['pine'], 2.6, 3, 3)       # hat
    c.line([(hx - 60, 290), (hx - 22, 290)], 4, GOLD, amp=.2)
    c.two([(hx + 30, 312), (hx + 56, 312), (hx + 62, 470), (hx + 36, 470)], RED, 2.4, 3, 0)                                             # scarf
    for i in range(3):
        c.line([(hx + 32, 350 + i * 44), (hx + 58, 350 + i * 44)], 3, CREAM, amp=.3)
    for i in range(4):
        c.line([(hx + 38 + i * 7, 470), (hx + 37 + i * 7, 486)], 1.6, amp=.2)
    c.two([(hx - 36, 640), (hx - 26, 470), (hx - 14, 470), (hx - 4, 640)], PAL['navy'], 2.4, 3, 0)                                      # umbrella
    c.line([(hx - 20, 470), (hx - 20, 440), (hx - 34, 432)], 4, amp=.3)

    # armchair with a sleeping cat, and a basket of wool beside it
    ax, ay = 1590, 792
    c.sh_oval(ax + 10, ay + 6, 170, 16)
    for lx in (ax - 112, ax + 112):
        box(c, lx - 9, ay - 22, lx + 9, ay + 4, WOOD_DARK, 2.2, 2, 0)
    c.two([(ax - 108, ay - 120), (ax - 112, ay - 268), (ax - 84, ay - 304), (ax + 84, ay - 304), (ax + 112, ay - 268), (ax + 108, ay - 120)], SAGE, 3, 6, 4)     # back
    for bx in (-46, 0, 46):
        c.dotc(ax + bx, ay - 236, 5, PAL['pine'])
    box(c, ax - 124, ay - 96, ax + 124, ay - 14, SAGE, 3, 5, 0)                                                                         # base
    c.two([(ax - 100, ay - 150), (ax + 100, ay - 150), (ax + 106, ay - 96), (ax - 106, ay - 96)], PAL['mint'], 2.6, 4, 3)                   # cushion
    for s in (-1, 1):
        c.two([(ax + s * 96, ay - 14), (ax + s * 96, ay - 168), (ax + s * 108, ay - 190), (ax + s * 142, ay - 190), (ax + s * 154, ay - 168), (ax + s * 154, ay - 14)], SAGE, 3, 5 * s, 0)
    cx, cy = ax - 8, ay - 166                                                                                                           # the cat
    c.line([(cx + 62, cy + 10), (cx + 84, cy - 6), (cx + 74, cy - 26)], 12, amp=.4); c.line([(cx + 62, cy + 10), (cx + 84, cy - 6), (cx + 74, cy - 26)], 7, ORANGE, amp=.4)
    c.two(c.ring(cx, cy, 70, 34), ORANGE, 2.8, 5, 4, amp=.6, step=9)
    c.two([(cx - 78, cy - 34), (cx - 70, cy - 60), (cx - 54, cy - 44), (cx - 34, cy - 44), (cx - 20, cy - 60), (cx - 12, cy - 30), (cx - 16, cy - 6), (cx - 46, cy + 6), (cx - 74, cy - 6)], ORANGE, 2.8, 4, 3)
    for ex in (-58, -32):
        c.line([(cx + ex - 7, cy - 22), (cx + ex, cy - 17), (cx + ex + 7, cy - 22)], 2.2, amp=.2, step=6)
    c.dotc(cx - 45, cy - 12, 3.4, MAROON)
    for i in range(3):
        c.line([(cx + 8 + i * 22, cy - 28), (cx + 14 + i * 22, cy - 6)], 2.4, RED, amp=.3)
    bx = 1330
    basket(c, bx, 806, 130, 70)
    for wx, wy, col in ((bx - 30, 728, RED), (bx + 26, 722, PAL['sky']), (bx - 2, 706, GOLD)):
        c.two(c.ring(wx, wy, 26, 26), col, 2.4, 4, 4, amp=.5, step=8)
        for i in range(3):
            c.line([(wx - 20, wy - 8 + i * 9), (wx, wy - 14 + i * 9), (wx + 20, wy - 6 + i * 9)], 1.4, dk(col), amp=.4)
    c.line([(bx + 40, 736), (bx + 80, 800), (bx + 60, 836), (bx - 10, 842)], 2.4, PAL['sky'], amp=.8)

    # a tall jug of dry grasses by the left wall
    vx = 236
    c.sh_oval(vx + 6, 706, 54, 10)
    for i, (dx, hh) in enumerate(((-34, 250), (-14, 300), (8, 270), (30, 310), (46, 240))):
        c.line([(vx, 560), (vx + dx, 700 - hh - 150)], 2, WOOD, amp=.6)
        c.two(c.ring(vx + dx, 700 - hh - 162, 7, 22), (GOLD, WOOD_DARK)[i % 2], 1.8, 2, 2, amp=.3, step=8)
    c.two([(vx - 22, 540), (vx + 22, 540), (vx + 28, 570), (vx + 48, 620), (vx + 40, 700), (vx - 40, 700), (vx - 48, 620), (vx - 28, 570)], PAL['blue'], 2.8, 5, 0)
    c.line([(vx - 44, 628), (vx + 44, 628)], 4, CREAM, amp=.4); c.line([(vx - 40, 650), (vx + 40, 650)], 2, CREAM, amp=.4)
    out(c, 'room_living_room', (W, H))


def bedroom():
    c = begin(52, ICE)
    def star(x, y, i):
        if i % 3 == 0:
            c.oval(x, y, 9, 9, WHITE, 0); c.oval(x + 5, y - 3, 8, 8, ICE, 0)        # little moon
        else:
            c.flat([(x, y - 8), (x + 3, y - 3), (x + 8, y), (x + 3, y + 3), (x, y + 8), (x - 3, y + 3), (x - 8, y), (x - 3, y - 3)], WHITE)
    sprinkle(c, star)
    planks(c, BONE, SAND); skirting(c, WHITE, 26); beam(c)

    window(c, 1490, 350, 240, 232, PINK, night=True)

    # a string of little flags across the wall
    pts = [(250 + i * 30, 306 + 46 * math.sin(math.pi * i / 33)) for i in range(34)]
    c.line(pts, 2.2, amp=.4)
    for i in range(2, 33, 4):
        x, y = pts[i]
        c.two([(x - 20, y + 2), (x + 20, y + 2), (x, y + 44)], (ROSE, GOLD, WHITE, PAL['sky'])[(i // 4) % 4], 2, 3, 2, amp=.3, step=10)

    # woven wall hanging
    kx0, kx1, ky0, ky1 = 660, 1000, 392, 550
    c.line([(kx0 - 24, ky0 - 10), (kx1 + 24, ky0 - 10)], 7, amp=.4); c.line([(kx0 - 24, ky0 - 10), (kx1 + 24, ky0 - 10)], 3.6, WOOD, amp=.4)
    def weave():
        for y, col, wd in ((ky0 + 18, CREAM, 5), (ky1 - 18, CREAM, 5), (ky0 + 34, PAL['navy'], 3), (ky1 - 34, PAL['navy'], 3)):
            c.line([(kx0 - 10, y), (kx1 + 10, y)], wd, col, amp=.4)
        for i in range(5):
            mx, my = kx0 + 46 + i * 62, (ky0 + ky1) / 2
            c.flat([(mx, my - 30), (mx + 26, my), (mx, my + 30), (mx - 26, my)], GOLD)
            c.flat([(mx, my - 14), (mx + 12, my), (mx, my + 14), (mx - 12, my)], PAL['navy'])
    box(c, kx0, ky0 - 8, kx1, ky1, RED, 2.8, 5, 0, inner=weave)
    for i in range(18):
        c.line([(kx0 + 8 + i * 19, ky1), (kx0 + 7 + i * 19, ky1 + 20)], 1.8, amp=.2)

    # dressing gown on a hook
    gx, gy = 1160, 372
    c.dotc(gx, gy - 6, 9, INK)
    for s_ in (-1, 1):
        c.two([(gx + s_ * 44, gy + 26), (gx + s_ * 84, gy + 58), (gx + s_ * 80, gy + 150), (gx + s_ * 52, gy + 150), (gx + s_ * 46, gy + 80)], PAL['mauve'], 2.6, 3 * s_, 0)
    c.two([(gx - 14, gy), (gx + 14, gy), (gx + 50, gy + 24), (gx + 58, gy + 222), (gx - 58, gy + 222), (gx - 50, gy + 24)], PAL['mauve'], 2.8, 5, 0)
    c.two([(gx - 14, gy), (gx, gy + 84), (gx + 14, gy), (gx + 30, gy + 10), (gx + 4, gy + 110), (gx - 4, gy + 110), (gx - 30, gy + 10)], PINK, 2.2, 2, 0)
    c.line([(gx, gy + 110), (gx - 2, gy + 220)], 2, PAL['plum'], amp=.4)
    c.line([(gx - 54, gy + 118), (gx + 54, gy + 114)], 7, amp=.4); c.line([(gx - 54, gy + 118), (gx + 54, gy + 114)], 3.6, PINK, amp=.4)
    c.line([(gx + 10, gy + 118), (gx + 18, gy + 170)], 6, amp=.3); c.line([(gx + 10, gy + 118), (gx + 18, gy + 170)], 3, PINK, amp=.3)

    # bedside table under the window, with a candle and a book
    bx0, bx1, bt = 1400, 1580, 596
    c.sh_oval((bx0 + bx1) / 2 + 6, 742, 104, 10)
    for lx in (bx0 + 14, bx1 - 14):
        box(c, lx - 8, bt + 60, lx + 8, 740, WOOD_DARK, 2.2, 2, 0)
    box(c, bx0 + 4, bt + 14, bx1 - 4, bt + 66, WOOD, 2.6, 4, 0)
    c.dotc((bx0 + bx1) / 2, bt + 40, 6, GOLD)
    box(c, bx0 - 8, bt, bx1 + 8, bt + 16, WOOD, 2.6, 0, 3)
    box(c, bx0 + 86, bt - 22, bx1 - 4, bt, RED, 2.2, 3, 0); c.line([(bx0 + 92, bt - 6), (bx1 - 10, bt - 6)], 2, CREAM, amp=.3)                  # book
    c.two([(bx0 + 14, bt), (bx0 + 70, bt), (bx0 + 62, bt - 14), (bx0 + 22, bt - 14)], PAL['blue'], 2.2, 3, 0)                                  # candle holder
    box(c, bx0 + 34, bt - 62, bx0 + 50, bt - 14, WHITE, 2, 2, 0)
    c.shape([(bx0 + 42, bt - 94), (bx0 + 51, bt - 74), (bx0 + 42, bt - 62), (bx0 + 33, bt - 74)], GOLD, 1.6, amp=.2, step=8)

    # a mobile of moon and stars above the cradle
    mx = 420
    c.line([(mx, 20), (mx, 214)], 1.6, amp=.3)
    c.line([(mx - 86, 222), (mx + 86, 214)], 5, amp=.3); c.line([(mx - 86, 222), (mx + 86, 214)], 2.6, WOOD, amp=.3)
    for dx, ln, kind in ((-78, 44, 'star'), (0, 70, 'moon'), (76, 40, 'star')):
        c.line([(mx + dx, 218), (mx + dx, 218 + ln)], 1.4, amp=.2)
        yy = 218 + ln + 20
        if kind == 'moon':
            c.oval(mx + dx, yy, 22, 22, GOLD, 2); c.oval(mx + dx + 12, yy - 6, 18, 18, ICE, 0)
        else:
            c.shape([(mx + dx, yy - 20), (mx + dx + 7, yy - 7), (mx + dx + 20, yy), (mx + dx + 7, yy + 7), (mx + dx, yy + 20), (mx + dx - 7, yy + 7), (mx + dx - 20, yy), (mx + dx - 7, yy - 7)], WHITE, 1.8, amp=.2, step=8)

    # wooden cradle on rockers, with a pillow and a dotted blanket
    x0, x1, yb = 250, 600, 800
    mid = (x0 + x1) / 2
    c.sh_oval(mid + 8, yb + 4, 220, 16)
    rock = [(mid + math.cos(math.radians(a)) * 214, yb - 46 + math.sin(math.radians(a)) * 40) for a in range(176, 3, -12)]
    c.line(rock, 15, amp=.5); c.line(rock, 9, WOOD_DARK, amp=.5)
    def dots():
        for i in range(12):
            c.dotc(x0 + 130 + (i % 6) * 40 + (i // 6) * 20, yb - 196 + (i // 6) * 24, 5.5, WHITE)
    c.two([(x0 + 96, yb - 150), (x0 + 110, yb - 210), (x1 - 60, yb - 216), (x1 - 24, yb - 150)], PAL['sky'], 2.6, 4, 3, inner=dots)             # blanket
    c.two(c.ring(x0 + 84, yb - 190, 50, 26), WHITE, 2.4, 3, 3, amp=.5, step=9)                                                             # pillow
    def slats():
        for i in range(1, 9):
            c.line([(x0 + i * 40, yb - 160), (x0 + i * 40, yb - 40)], 1.6, WOOD_DARK, amp=.4)
        c.flat([(x0, yb - 160), (x1, yb - 160), (x1, yb - 138), (x0, yb - 138)], WOOD_DARK, .35)
    box(c, x0 + 10, yb - 160, x1 - 10, yb - 44, WOOD, 3, 6, 0, inner=slats)
    for px_, ht in ((x0, 270), (x1, 214)):                                                                                              # end posts
        box(c, px_ - 16, yb - ht, px_ + 16, yb - 30, WOOD, 2.8, 4, 0)
        c.two(c.ring(px_, yb - ht - 12, 17, 17), WOOD, 2.6, 3, 3, amp=.4, step=8)
    c.shape([(x0, yb - 232), (x0 + 9, yb - 220), (x0, yb - 204), (x0 - 9, yb - 220)], GOLD, 1.4, amp=.2, step=8)

    # slippers on a round mat
    mx, my = 1010, 806
    c.two(c.ring(mx, my, 150, 40), ROSE, 2.6, 5, 4, amp=.6, step=10)
    c.stroke(c.ring(mx, my, 120, 28), 1.4, CREAM)
    for sx in (-52, 30):
        c.two([(mx + sx - 38, my + 10), (mx + sx - 36, my - 6), (mx + sx - 8, my - 10), (mx + sx + 6, my - 28), (mx + sx + 30, my - 26), (mx + sx + 42, my - 8), (mx + sx + 42, my + 10)], MAROON, 2.4, 3, 2)
        c.line([(mx + sx - 34, my - 4), (mx + sx - 8, my - 8)], 4, CREAM, amp=.3)
        c.two(c.ring(mx + sx + 22, my - 24, 11, 10), WHITE, 1.8, 2, 2, amp=.3, step=7)
    out(c, 'room_bedroom', (W, H))


def kitchen():
    c = begin(53, PAPER)
    stripes(c, WHITE, .5, 60, 30, 400)
    # tiled wall under a rail
    t0 = 400
    c.flat([(0, t0), (W, t0), (W, FLOOR), (0, FLOOR)], WHITE)
    for j in range(4):
        for i in range(W // 50 + 1):
            if (i + j) % 2:
                c.flat([(i * 50, t0 + j * 50), (i * 50 + 50, t0 + j * 50), (i * 50 + 50, t0 + j * 50 + 50), (i * 50, t0 + j * 50 + 50)], MINT)
    for j in range(1, 4):
        c.line([(0, t0 + j * 50), (W, t0 + j * 50)], 1.2, SAGE, amp=.4)
    for i in range(1, W // 50 + 1):
        c.line([(i * 50, t0), (i * 50, FLOOR)], 1.2, SAGE, amp=.4)
    box(c, -10, t0 - 14, W + 10, t0 + 6, WOOD, 2.4, 0, 3)
    checker(c, CREAM, SAND, TONE2); skirting(c)

    window(c, 370, 296, 240, 160, RED, pots=((-80, LEAF), (0, GOLD), (80, LEAF)))
    # the curtains are gingham: a few pale checks over the red
    for s in (-1, 1):
        for i in range(3):
            c.line([(370 + s * (146 + i * 16), 196), (370 + s * (142 + i * 14), 280)], 1.6, PINK, amp=.4)

    # a rail of hanging tools: ladle, whisk, garlic, sieve, oven glove, onions
    ry = 300
    c.line([(790, ry), (1310, ry)], 6, amp=.5); c.line([(790, ry), (1310, ry)], 3, GREY, amp=.5)
    for hx in (790, 1310):
        c.dotc(hx, ry, 8, INK)
    lx = 850                                                                         # ladle
    c.line([(lx, ry), (lx, ry + 96)], 5, amp=.3); c.line([(lx, ry), (lx, ry + 96)], 2.4, GREY, amp=.3)
    c.two(c.ring(lx, ry + 116, 26, 22), SILVER, 2.4, 4, 3, amp=.4, step=8)
    wx = 930                                                                         # whisk
    c.line([(wx, ry), (wx, ry + 56)], 6, amp=.3); c.line([(wx, ry), (wx, ry + 56)], 3, WOOD, amp=.3)
    for dx in (-18, -8, 0, 8, 18):
        c.line([(wx, ry + 56), (wx + dx, ry + 100), (wx, ry + 136)], 1.8, amp=.3)
    gx = 1010                                                                        # garlic braid
    c.line([(gx, ry), (gx, ry + 30)], 2.4, WOOD, amp=.3)
    for i in range(4):
        c.two(c.ring(gx + (10 if i % 2 else -10), ry + 46 + i * 30, 19, 17), WHITE, 2, 3, 3, amp=.4, step=7)
    sx = 1100                                                                        # sieve
    c.line([(sx, ry), (sx, ry + 40)], 5, amp=.3); c.line([(sx, ry), (sx, ry + 40)], 2.4, WOOD, amp=.3)
    c.two(c.ring(sx, ry + 84, 44, 44), SILVER, 2.6, 4, 4, amp=.5, step=9)
    c.oval(sx, ry + 84, 30, 30, CREAM, 1.6)
    c.inside(c.ring(sx, ry + 84, 30, 30), lambda: c.hatch(sx - 40, ry + 44, sx + 40, ry + 124, gap=8, w=.9, color=GREY))
    ox = 1196                                                                        # oven glove
    c.line([(ox, ry), (ox, ry + 26)], 2.4, amp=.3)
    c.two([(ox - 26, ry + 26), (ox + 22, ry + 26), (ox + 26, ry + 96), (ox + 44, ry + 84), (ox + 52, ry + 104), (ox + 22, ry + 136), (ox - 22, ry + 136)], RED, 2.6, 4, 0)
    c.line([(ox - 24, ry + 42), (ox + 22, ry + 42)], 4, CREAM, amp=.3)
    nx = 1272                                                                        # onions
    c.line([(nx, ry), (nx, ry + 30)], 2.4, WOOD, amp=.3)
    for i in range(3):
        c.two(c.ring(nx + (9 if i % 2 else -9), ry + 50 + i * 36, 21, 20), ORANGE, 2, 3, 3, amp=.4, step=7)

    # a shelf of jars above the worktop
    box(c, 1420, 344, 1880, 360, WOOD, 2.6, 0, 3)
    for bx_ in (1460, 1840):
        c.line([(bx_, 360), (bx_, 384), (bx_ + 20, 360)], 2.6, amp=.3)
    for i, (jw, jh, col) in enumerate(((54, 70, RED), (46, 56, GOLD), (58, 76, PAL['lime']), (44, 52, ORANGE), (54, 66, PAL['sky']), (48, 58, CREAM))):
        jar(c, 1466 + i * 74, 344, jw, jh, col)

    # the worktop along the right wall, with a loaf on a board and a bowl of apples
    k0, top, bot = 1380, 520, 800
    c.sh_oval(1650, bot + 6, 300, 14)
    def doors():
        for i in range(3):
            dx = k0 + 26 + i * 178
            c.shape([(dx, top + 96), (dx + 150, top + 96), (dx + 150, bot - 30), (dx, bot - 30)], WHITE, 1.6, SAND, amp=.5, step=30)
            c.shape([(dx, top + 30), (dx + 150, top + 30), (dx + 150, top + 78), (dx, top + 78)], WHITE, 1.6, SAND, amp=.5, step=30)
            c.dotc(dx + 75, top + 54, 7, WOOD_DARK); c.dotc(dx + 130, top + 180, 7, WOOD_DARK)
    box(c, k0, top, W + 10, bot, PAL['mint'], 3, 6, 0, inner=doors)
    box(c, k0 - 18, top - 26, W + 10, top + 8, WOOD, 3, 0, 4)
    box(c, k0 + 6, bot - 14, W + 10, bot + 6, WOOD_DARK, 2.4, 0, 3)
    c.two([(1410, top - 26), (1600, top - 26), (1592, top - 38), (1418, top - 38)], PAL['taupe'], 2.2, 3, 2)                                  # board
    c.two([(1436, top - 38), (1440, top - 84), (1466, top - 104), (1546, top - 104), (1572, top - 84), (1576, top - 38)], PAL['gold'], 2.8, 5, 4)   # loaf
    for i in range(3):
        c.line([(1466 + i * 34, top - 98), (1482 + i * 34, top - 70)], 2.2, WOOD_DARK, amp=.3)
    c.two([(1620, top - 70), (1780, top - 70), (1758, top - 26), (1642, top - 26)], PAL['blue'], 2.8, 5, 0)                                  # bowl
    for axx, col in ((1652, RED), (1698, GOLD), (1744, RED), (1676, PAL['lime']), (1722, RED)):
        c.two(c.ring(axx, top - 84 - (10 if col != RED else 0), 22, 20), col, 2.2, 3, 3, amp=.4, step=7)
    box(c, 1620, top - 76, 1780, top - 64, PAL['blue'], 2.2, 0, 3)

    # a sack of potatoes and a crate of cabbages in the left corner
    px = 160
    c.sh_oval(px + 8, 800, 90, 12)
    c.two([(px - 70, 794), (px - 78, 700), (px - 44, 650), (px + 44, 650), (px + 78, 700), (px + 70, 794)], PAL['taupe'], 2.8, 6, 0)
    c.two([(px - 52, 656), (px + 52, 656), (px + 60, 638), (px - 60, 638)], PAL['taupe'], 2.4, 3, 2)
    for i, (dx, dy) in enumerate(((-30, -8), (2, -16), (32, -6), (-10, 4))):
        c.two(c.ring(px + dx, 632 + dy, 20, 15), PAL['wood'], 2, 3, 3, amp=.5, step=7)
    c.line([(px - 30, 720), (px - 16, 740), (px - 30, 760)], 1.8, WOOD, amp=.4); c.line([(px + 20, 730), (px + 34, 752)], 1.8, WOOD, amp=.4)
    out(c, 'room_kitchen', (W, H))


def bathroom():
    c = begin(54, BONE)
    # tiles up to shoulder height with a patterned row on top
    t0 = 300
    c.flat([(0, t0), (W, t0), (W, FLOOR), (0, FLOOR)], ICE)
    for j in range(1, 5):
        c.line([(0, t0 + j * 60), (W, t0 + j * 60)], 1.4, PAL['sky'], amp=.4)
    for i in range(W // 60 + 1):
        c.line([(i * 60, t0), (i * 60, FLOOR)], 1.4, PAL['sky'], amp=.4)
    for i in range(W // 60 + 1):
        x = i * 60 + 30
        c.flat([(x, t0 + 12), (x + 18, t0 + 30), (x, t0 + 48), (x - 18, t0 + 30)], (PAL['sky'], WHITE)[i % 2])
    box(c, -10, t0 - 16, W + 10, t0 + 6, PAL['blue'], 2.4, 0, 3)
    checker(c, WHITE, PAL['sky'], PAL['blue'], s0=90, s1=160, rows=6); skirting(c, PAL['blue'], 14)

    # a small frosted window
    wx, wy, ww, wh = 290, 196, 200, 150
    box(c, wx - ww / 2 - 14, wy - wh / 2 - 14, wx + ww / 2 + 14, wy + wh / 2 + 14, WHITE, 3, 4, 4)
    pane = [(wx - ww / 2, wy - wh / 2), (wx + ww / 2, wy - wh / 2), (wx + ww / 2, wy + wh / 2), (wx - ww / 2, wy + wh / 2)]
    c.flat(pane, PAL['sky'])
    def frost():
        for i in range(-4, 8):
            c.flat([(wx - ww / 2 + i * 60, wy + wh / 2), (wx - ww / 2 + i * 60 + 22, wy + wh / 2), (wx - ww / 2 + i * 60 + 122, wy - wh / 2), (wx - ww / 2 + i * 60 + 100, wy - wh / 2)], WHITE, .5)
    c.inside(pane, frost); c.stroke(pane, 2.2)
    c.line([(wx, wy - wh / 2), (wx, wy + wh / 2)], 4, WHITE, amp=.3)

    # a towel rail with a bath robe, and soap bubbles drifting up
    c.line([(1000, 356), (1270, 356)], 7, amp=.4); c.line([(1000, 356), (1270, 356)], 3.6, SILVER, amp=.4)
    for hx in (1000, 1270):
        c.dotc(hx, 356, 9, INK)
    c.two([(1040, 352), (1140, 352), (1150, 540), (1030, 540)], PAL['mauve'], 2.8, 5, 0)
    c.line([(1034, 500), (1148, 500)], 4, PINK, amp=.3); c.line([(1034, 518), (1148, 518)], 2, PINK, amp=.3)
    c.two([(1176, 352), (1250, 352), (1254, 480), (1172, 480)], WHITE, 2.6, 4, 0)
    c.line([(1174, 452), (1252, 452)], 4, PAL['sky'], amp=.3)
    for bx_, by_, br in ((880, 440, 20), (924, 392, 12), (848, 364, 9), (1430, 380, 14), (1466, 334, 9)):
        c.oval(bx_, by_, br, br, WHITE, 1.4, PAL['blue'])
        c.line([(bx_ - br * .5, by_ - br * .2), (bx_ - br * .2, by_ - br * .55)], 1.2, PAL['sky'], amp=.2, step=5)

    # the toilet, with its cistern high on the wall and the paper beside it
    tx, fy = 1580, 800
    c.sh_oval(tx + 20, fy + 6, 130, 14)
    c.line([(tx - 70, 300), (tx - 70, fy - 150)], 10, amp=.3); c.line([(tx - 70, 300), (tx - 70, fy - 150)], 5.5, SILVER, amp=.3)        # pipe
    box(c, tx - 140, 196, tx, 300, WHITE, 3, 5, 0)                                                                             # cistern
    box(c, tx - 148, 184, tx + 8, 204, WHITE, 2.6, 4, 0)
    c.line([(tx - 14, 300), (tx - 12, 410)], 2, amp=.4); c.two(c.ring(tx - 12, 426, 10, 17), WOOD, 2, 2, 2, amp=.3, step=7)       # chain
    c.two(c.ring(tx - 74, fy - 250, 26, 84), WOOD, 2.8, 4, 3, amp=.5, step=9)                                                    # the lid, raised
    c.two([(tx - 44, fy), (tx - 38, fy - 80), (tx + 44, fy - 80), (tx + 56, fy)], WHITE, 3, 6, 0)                                 # foot
    c.two([(tx - 100, fy - 160), (tx + 120, fy - 160), (tx + 112, fy - 116), (tx + 60, fy - 72), (tx - 48, fy - 70), (tx - 92, fy - 104)], WHITE, 3, 6, 4)   # bowl
    c.two([(tx - 106, fy - 178), (tx + 120, fy - 178), (tx + 132, fy - 166), (tx + 120, fy - 154), (tx - 106, fy - 154)], WOOD, 2.8, 4, 3)                  # seat
    rx = 1360
    c.line([(rx - 34, 470), (rx + 34, 470)], 6, amp=.3); c.line([(rx - 34, 470), (rx + 34, 470)], 3, SILVER, amp=.3)
    c.two(c.ring(rx, 492, 32, 32), WHITE, 2.6, 4, 4, amp=.4, step=8); c.oval(rx, 492, 10, 10, BONE, 1.6)
    c.two([(rx + 14, 520), (rx + 32, 494), (rx + 34, 580), (rx + 12, 584)], WHITE, 2.2, 3, 0)

    # a little shelf with a fern and a sponge
    box(c, 560, 420, 780, 436, WOOD, 2.6, 0, 3)
    for bx_ in (584, 756):
        c.line([(bx_, 436), (bx_, 456)], 2.6, amp=.3)
    for i, (dx, dy) in enumerate(((-44, -52), (-26, -74), (0, -84), (26, -72), (44, -50))):
        c.two([(620, 384), (620 + dx * .5 - 8, 384 + dy * .5), (620 + dx, 384 + dy), (620 + dx * .5 + 8, 384 + dy * .5 + 6)], (LEAF, LEAF_DARK)[i % 2], 1.8, 2, 2, amp=.3, step=8)
    c.two([(596, 380), (644, 380), (638, 420), (602, 420)], ORANGE, 2.2, 3, 0, amp=.3, step=8)
    c.two(c.ring(716, 402, 36, 18), GOLD, 2.2, 3, 3, amp=.6, step=7)
    for dx in (-18, 0, 16):
        c.dotc(716 + dx, 400 + (dx % 5), 3, PAL['gold'])

    # a laundry basket in the left corner and a duck by the mat
    basket(c, 190, 800, 190, 170)
    c.two([(110, 630), (150, 590), (200, 612), (250, 584), (280, 630)], PAL['sky'], 2.6, 4, 3)
    c.two([(210, 630), (240, 600), (276, 606), (284, 630)], ROSE, 2.4, 3, 3)
    c.two([(250, 640), (300, 632), (308, 720), (262, 726)], WHITE, 2.4, 3, 0); c.line([(256, 700), (304, 694)], 3, RED, amp=.3)

    mx, my = 900, 806
    c.two([(mx - 190, my - 34), (mx + 190, my - 34), (mx + 210, my + 34), (mx - 210, my + 34)], PAL['blue'], 2.8, 5, 4)
    c.stroke([(mx - 160, my - 18), (mx + 160, my - 18), (mx + 174, my + 18), (mx - 174, my + 18)], 1.6, ICE)
    for i in range(14):
        c.line([(mx - 206 + i * 32, my + 34), (mx - 208 + i * 32, my + 50)], 1.8, amp=.2)
    dxk, dyk = 1300, 800
    c.two([(dxk - 44, dyk - 6), (dxk - 60, dyk - 36), (dxk - 30, dyk - 30), (dxk + 20, dyk - 34), (dxk + 40, dyk - 10), (dxk + 20, dyk + 12), (dxk - 30, dyk + 12)], GOLD, 2.6, 4, 3)
    c.two(c.ring(dxk + 26, dyk - 50, 26, 24), GOLD, 2.6, 4, 3, amp=.4, step=8)
    c.two([(dxk + 46, dyk - 54), (dxk + 74, dyk - 48), (dxk + 46, dyk - 40)], ORANGE, 2, 2, 2, amp=.3, step=8)
    c.dotc(dxk + 32, dyk - 56, 4, INK)
    out(c, 'room_bathroom', (W, H))


def bar():
    """The furniture bar: a long wooden tray. Only its top 186 px show on a 16:9 screen; the rest covers taller windows.
    2048 px is the widest texture the project imports, a little wider than the 1920 px screen."""
    BW, BH = 2048, 432
    c = Scene(BW, BH, 1.5, WHITE, 55)
    c.rad = 6; c.wob = 1.0; c.seg = 22
    c.flat([(0, 0), (BW, 0), (BW, BH), (0, BH)], WHITE)
    c.flat([(0, 30), (BW, 30), (BW, 66), (0, 66)], BONE, .8)                       # the rim's shadow on the tray
    for i in range(1, 16):
        c.line([(i * 150 + 20, 40), (i * 150 + 14, BH)], 1.4, BONE, amp=.6)
    for i in range(40):
        x, y = c.r.uniform(0, BW), c.r.uniform(70, BH)
        c.line([(x, y), (x + c.r.uniform(40, 110), y + c.r.uniform(-2, 2))], 1.2, BONE, amp=.4)
    c.flat([(0, 0), (BW, 0), (BW, 30), (0, 30)], WOOD)
    c.flat([(0, 18), (BW, 18), (BW, 30), (0, 30)], WOOD_DARK)
    c.line([(0, 3), (BW, 3)], 3, amp=1); c.line([(0, 30), (BW, 30)], 3, amp=1)
    for i in range(0, 17):
        c.dotc(i * 150 + 60, 10, 3.4, WOOD_DARK)
    out(c, 'furniture_bar', (BW, BH))


for make in (living_room, bedroom, kitchen, bathroom, bar):
    make()
