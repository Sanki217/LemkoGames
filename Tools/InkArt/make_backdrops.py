"""Activity backdrops, shop layout and house rooms in ink-on-paper style.

Backdrops keep the middle calm because word cards and tokens sit there; the drawing
lives along the top and in the corners.
Usage: python3 make_backdrops.py [preview dir]
"""
import math, sys
from pathlib import Path
from scene import *

ROOT = Path(__file__).resolve().parents[2]
BG = ROOT / 'Assets/Lemko/Sprites/Placeholders/Backgrounds'
ROOMS = ROOT / 'Assets/Lemko/Sprites/Placeholders/Rooms'
PREVIEW = Path(sys.argv[1]) if len(sys.argv) > 1 else None
W, H = 1024, 480


def out(c, folder, name, size):
    c.save(str((PREVIEW or folder) / (name + '.png')), size)


def grid(c, y0, w=W, h=H, step=150, alpha=.5, color=WHITE):
    cr = c.c
    cr.save(); cr.rectangle(0, y0, w, h - y0); cr.clip()
    cr.set_source_rgba(*rgb(color), alpha); cr.set_line_width(1.6)
    for k in range(-20, 30):
        o = k * step / 2
        cr.move_to(-50, o - 25); cr.line_to(w + 50, o + (w + 50) * .5); cr.stroke()
        cr.move_to(-50, o + 25 + 300); cr.line_to(w + 50, o - (w + 50) * .5 + 300); cr.stroke()
    cr.restore()


def outdoor(seed, horizon=118):
    c = Scene(W, H, 2, SKY, seed)
    return c, horizon


def land(c, horizon):
    r = c.r
    # far hills and a forest band soften the horizon
    far = [(0, horizon - 4), (120, horizon - 30), (260, horizon - 12), (400, horizon - 36), (560, horizon - 14), (700, horizon - 34), (860, horizon - 16), (W, horizon - 30)]
    c.flat(far + [(W, horizon + 20), (0, horizon + 20)], GRASS_LIGHT)
    c.line(far, 1.6, PINE, amp=1)
    for i in range(30):
        x = i * 36 + r.uniform(-8, 8)
        c.flat(c.lumpy(x, horizon + r.uniform(-4, 6), r.uniform(20, 30), r.uniform(10, 16), 5, .3), PINE, .5)
    pts = [(0, horizon + 8), (140, horizon - 6), (330, horizon + 6), (520, horizon - 8), (720, horizon + 4), (900, horizon - 6), (W, horizon + 6)]
    c.flat(pts + [(W, H), (0, H)], GRASS)
    for i in range(22):
        c.flat(c.lumpy(r.uniform(-20, W + 20), r.uniform(horizon + 30, H + 10), r.uniform(60, 150), r.uniform(18, 40), 6, .25), (GRASS_LIGHT, GRASS_DARK)[i % 2], .28)
    c.line(pts, 2.6, amp=1.2)
    def edge(x, y):
        return not (130 < x < W - 130 and horizon + 60 < y < H - 44)   # keep the play area clear
    for i in range(150):
        x, y = r.uniform(6, W - 6), r.uniform(horizon + 22, H - 4)
        if edge(x, y) or i % 9 == 0:
            (c.tuft(x, y, r.uniform(.7, 1.1)) if i % 3 else c.tick(x, y, .8, PINE))
    for i in range(34):
        x, y = r.uniform(10, W - 10), r.uniform(horizon + 30, H - 8)
        if edge(x, y):
            c.flowers(x, y, r.randint(4, 8), r.uniform(10, 22), ((WHITE, GOLD), (RED, ORANGE, GOLD), BLOOM)[i % 3])


def sky(c, clouds):
    for x, y, w in clouds:
        c.cloud(x, y, w)
    for x, y in ((420, 40), (446, 50), (700, 30)):
        c.line([(x - 7, y - 2), (x, y + 2), (x + 7, y - 2)], 1.3, amp=.2, step=5)      # birds


def indoor(seed, floor=330, planks=True, wall=PAPER, pattern='stripes'):
    c = Scene(W, H, 2, wall, seed)
    d = dk(wall)
    if pattern == 'stripes':
        for i in range(0, 44):
            c.flat([(i * 24, 0), (i * 24 + 9, 0), (i * 24 + 9, floor), (i * 24, floor)], d, .28)
    else:
        for i in range(26):
            for j in range(7):
                x, y = 20 + i * 40 + (j % 2) * 20, 16 + j * 38
                c.flat([(x, y - 5), (x + 4, y), (x, y + 5), (x - 4, y)], d, .5)
    # wooden panelling under a rail
    top = floor - 74
    c.flat([(0, top), (W, top), (W, floor), (0, floor)], d)
    for i in range(0, 20):
        x = i * 54 + 10
        c.shape([(x, top + 14), (x + 40, top + 14), (x + 40, floor - 18), (x, floor - 18)], wall, 1.1, dk(d), amp=.3, step=20)
    c.flat([(0, top - 8), (W, top - 8), (W, top + 4), (0, top + 4)], WHITE)
    c.line([(0, top - 8), (W, top - 8)], 2, amp=.6); c.line([(0, top + 4), (W, top + 4)], 1.6, amp=.6)
    # floor boards with seams and knots
    c.flat([(0, floor), (W, floor), (W, H), (0, H)], TONE1)
    c.flat([(0, floor), (W, floor), (W, floor + 16), (0, floor + 16)], dk(TONE1), .6)
    if planks:
        for i in range(-6, 18):
            x = 512 + (i - 6) * 95
            a, b = (512 + (x - 512) * .82, floor + 2), (512 + (x - 512) * 1.5, H)
            c.line([a, b], 1.5, WOOD, amp=.6)
            for t in (c.r.uniform(.15, .45), c.r.uniform(.55, .9)):
                px, py = a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t
                c.line([(px, py), (px + 60 + t * 50, py)], 1.1, WOOD, amp=.3)
                c.oval(px + 30, py + 12 + t * 10, 3, 1.6, dk(TONE1), .8, WOOD, amp=.1)
    c.flat([(0, floor - 8), (W, floor - 8), (W, floor + 2), (0, floor + 2)], WOOD_DARK)
    c.line([(0, floor - 8), (W, floor - 8)], 2, amp=.8); c.line([(0, floor + 2), (W, floor + 2)], 2.6, amp=.8)
    return c, floor


def window(c, x, y, w=70, h=84, view=True, curtain=RED):
    c.shape([(x - w / 2 - 5, y - h / 2 - 5), (x + w / 2 + 5, y - h / 2 - 5), (x + w / 2 + 5, y + h / 2 + 5), (x - w / 2 - 5, y + h / 2 + 5)], WHITE, 2.8, amp=.6)
    pane = [(x - w / 2, y - h / 2), (x + w / 2, y - h / 2), (x + w / 2, y + h / 2), (x - w / 2, y + h / 2)]
    def outside():
        c.flat([(x - w, y + h), (x - w, y + 10), (x - 14, y - 12), (x + 6, y + 6), (x + w / 2, y - 8), (x + w, y + h)], GRASS)
        c.flat([(x - w, y + h), (x - w, y + 24), (x, y + 14), (x + w, y + 26), (x + w, y + h)], GRASS_DARK)
        c.oval(x + w * .24, y - h * .26, 8, 8, GOLD, 1.6)
        c.flat(c.lumpy(x - w * .2, y - h * .3, 13, 5, 4, .4), WHITE)
    c.flat(pane, PAL['sky'])
    if view:
        c.inside(pane, outside)
    c.stroke(pane, 1.8)
    c.line([(x, y - h / 2), (x, y + h / 2)], 2.4, WHITE, amp=.2); c.line([(x - w / 2, y), (x + w / 2, y)], 2.4, WHITE, amp=.2)
    for s_ in (-1, 1):
        ex = x + s_ * (w / 2 + 3)
        cur = [(ex + s_ * 4, y - h / 2 - 6), (ex - s_ * 20, y - h / 2 - 6), (ex - s_ * 16, y - 6), (ex - s_ * 6, y + 12), (ex - s_ * 12, y + h / 2 + 2), (ex + s_ * 6, y + h / 2 + 2)]
        c.two(cur, curtain, 1.8, 2 * s_, 0, amp=.5, step=12)
        c.line([(ex - s_ * 12, y + 8), (ex + s_ * 4, y + 8)], 1.6, GOLD, amp=.2)
    c.line([(x - w / 2 - 14, y - h / 2 - 8), (x + w / 2 + 14, y - h / 2 - 8)], 2.6, amp=.3)
    c.shape([(x - w / 2 - 10, y + h / 2 + 5), (x + w / 2 + 10, y + h / 2 + 5), (x + w / 2 + 10, y + h / 2 + 12), (x - w / 2 - 10, y + h / 2 + 12)], WHITE, 2, amp=.4)
    plant(c, x - w * .2, y + h / 2 + 5, .5)


def shelf(c, x, y, w=120, books=6, pot=False):
    c.shape([(x, y), (x + w, y), (x + w, y + 8), (x, y + 8)], WOOD, 2.4, amp=.5)
    c.line([(x + 12, y + 8), (x + 12, y + 20)], 2.2, amp=.2); c.line([(x + w - 12, y + 8), (x + w - 12, y + 20)], 2.2, amp=.2)
    bx = x + 10
    for i in range(books):
        bw, bh = c.r.uniform(9, 14), c.r.uniform(26, 38)
        c.shape([(bx, y), (bx, y - bh), (bx + bw, y - bh), (bx + bw, y)], (RED, GOLD, NAVY, LEAF, ORANGE, WATER)[i % 6], 2, amp=.3, step=10)
        bx += bw + 1
    if pot:
        plant(c, x + w - 20, y, .7)


def plant(c, x, y, k=1.0):
    c.line([(x, y - 16 * k), (x - 4 * k, y - 44 * k)], 2.2, amp=.3)
    c.shape([(x - 4 * k, y - 44 * k), (x - 22 * k, y - 50 * k), (x - 12 * k, y - 34 * k)], LEAF, 2, amp=.3, step=8)
    c.shape([(x - 2 * k, y - 32 * k), (x + 18 * k, y - 46 * k), (x + 10 * k, y - 26 * k)], LEAF, 2, amp=.3, step=8)
    c.shape([(x - 12 * k, y - 18 * k), (x + 12 * k, y - 18 * k), (x + 9 * k, y), (x - 9 * k, y)], ORANGE, 2.2, amp=.3, step=8)


def frame(c, x, y, w=46, h=36, fill=WHITE):
    c.line([(x - 8, y - h / 2), (x, y - h / 2 - 12), (x + 8, y - h / 2)], 1.6, amp=.2)
    c.shape([(x - w / 2, y - h / 2), (x + w / 2, y - h / 2), (x + w / 2, y + h / 2), (x - w / 2, y + h / 2)], fill, 2.6, amp=.5)
    c.oval(x, y - 3, 7, 7, '#f2b888', 1.6); c.line([(x - 12, y + 12), (x, y + 5), (x + 12, y + 12)], 1.6, amp=.2)


def text(c, s, x, y, size=22, color=INK, bold=True):
    cr = c.c
    cr.select_font_face('DejaVu Sans', cairo.FONT_SLANT_NORMAL, cairo.FONT_WEIGHT_BOLD if bold else cairo.FONT_WEIGHT_NORMAL)
    cr.set_font_size(size); cr.set_source_rgb(*rgb(color)); cr.move_to(x, y); cr.show_text(s)


def rug(c, x, y, w=220, h=44):
    p = [(x - w / 2 + 24, y - h / 2), (x + w / 2 + 24, y - h / 2), (x + w / 2 - 24, y + h / 2), (x - w / 2 - 24, y + h / 2)]
    def weave():
        for i in range(int(w / 26) + 2):
            cx = x - w / 2 + i * 26
            c.flat([(cx, y - 9), (cx + 9, y), (cx, y + 9), (cx - 9, y)], GOLD)
            c.flat([(cx, y - 4), (cx + 4, y), (cx, y + 4), (cx - 4, y)], NAVY)
        for dy, col in ((-h / 2 + 7, PAPER), (h / 2 - 7, PAPER)):
            c.line([(x - w, y + dy), (x + w, y + dy)], 2, col, amp=.3)
    c.two(p, RED, 2.4, 3, 3, amp=.8, inner=weave)


def stove(c, x, y):
    """Tiled stove for the corners of indoor scenes; x,y is the bottom middle."""
    c.sh_oval(x + 8, y + 2, 46, 8)
    def tiles():
        for i in range(1, 7):
            c.line([(x - 40, y - i * 20), (x + 40, y - i * 20)], 1.2, dk(PAL['pale']), amp=.3)
        for k in (-13, 13):
            c.line([(x + k, y - 130), (x + k, y)], 1.2, dk(PAL['pale']), amp=.3)
    c.two([(x - 36, y), (x - 36, y - 118), (x + 36, y - 118), (x + 36, y)], PAL['pale'], 2.6, 6, 0, inner=tiles)
    c.two([(x - 42, y - 118), (x + 42, y - 118), (x + 38, y - 130), (x - 38, y - 130)], WOOD, 2.2, 3, 0, amp=.4)
    c.shape([(x - 15, y - 12), (x - 15, y - 44), (x + 15, y - 44), (x + 15, y - 12)], '#3d3333', 2, amp=.3)
    c.flat([(x - 9, y - 14), (x - 4, y - 30), (x, y - 20), (x + 5, y - 34), (x + 9, y - 14)], ORANGE)
    c.flat([(x - 4, y - 14), (x, y - 24), (x + 4, y - 14)], GOLD)


def dresser(c, x, y, w=96, h=70):
    c.sh_oval(x + 8, y + 2, w * .6, 8)
    def drawers():
        for i in range(1, 3):
            c.line([(x - w, y - h * i / 3), (x + w, y - h * i / 3)], 1.6, amp=.3)
        for i in range(3):
            c.dotc(x - w * .2, y - h * (i + .5) / 3, 2.6, GOLD); c.dotc(x + w * .2, y - h * (i + .5) / 3, 2.6, GOLD)
    c.two([(x - w / 2, y), (x - w / 2, y - h), (x + w / 2, y - h), (x + w / 2, y)], WOOD, 2.6, 5, 0, inner=drawers)
    c.shape([(x - w / 2 - 5, y - h), (x + w / 2 + 5, y - h), (x + w / 2 + 5, y - h - 7), (x - w / 2 - 5, y - h - 7)], WOOD_DARK, 2, amp=.3)


# --- the seven activity backdrops ------------------------------------------------------
def farm():
    c, hz = outdoor(21)
    sky(c, [(90, 40, 90), (560, 34, 110), (840, 52, 80)])
    for x, y, w, h in [(120, hz + 4, 150, 96), (240, hz, 120, 70), (640, hz - 2, 140, 84), (520, hz, 110, 60)]:
        c.mountain(x, y, w, h)
    land(c, hz)
    c.field(300, hz + 44, 110, 30, GOLD, 5); c.field(600, hz + 40, 90, 26, LIME, 4, crop=LEAF_DARK)
    c.house(880, hz + 48, 100, 54, 46, roof=NAVY, wall=RED, side=WOOD_DARK, planks=True, windows=0, door=False, roof_h=48, glass=PAPER)
    c.shape([(860, hz + 48), (860, hz + 12), (900, hz + 12), (900, hz + 48)], PAPER, 2.2, amp=.3)
    c.line([(860, hz + 12), (900, hz + 48)], 1.6, amp=.2); c.line([(900, hz + 12), (860, hz + 48)], 1.6, amp=.2)
    c.two([(972, hz + 30), (972, hz - 30), (998, hz - 30), (998, hz + 30)], STONE, 2.2, 4, 0); c.two([(968, hz - 28), (985, hz - 48), (1002, hz - 28)], RED, 2, 3, 3)
    c.haybale(790, hz + 44, 20); c.haybale(760, hz + 52, 15)
    c.fence(10, hz + 50, 260, hz + 36, 10)
    c.pine(420, hz + 26, 50); c.pine(455, hz + 34, 60); c.tree(540, hz + 34, 18, fruit=RED); c.birch(700, hz + 34, 15)
    c.cow(150, hz + 30, .7); c.sheep(210, hz + 26, .6)
    # the corners of the foreground: fence, trough, hens, a cart and bushes
    c.fence(-6, H - 14, 150, H - 30, 6); c.hen(60, H - 50, 1.2); c.hen(96, H - 40, 1, PAL['wood'])
    c.two([(18, H - 62), (64, H - 66), (60, H - 52), (22, H - 48)], WOOD, 2, 2, 3, amp=.3); c.flat([(24, H - 60), (58, H - 63), (56, H - 57), (26, H - 54)], PAL['sky'])
    c.cart(950, H - 22); c.bush(1004, H - 70, 18, RED); c.pine(1004, H - 96, 56)
    c.tree(20, hz + 70, 22)
    out(c, BG, 'scene_Farm', (2048, 960))


def family():
    c, hz = outdoor(22)
    sky(c, [(420, 36, 100), (640, 60, 80), (950, 30, 90)])
    for x, y, w, h in [(880, hz + 2, 160, 100), (760, hz + 4, 120, 70), (640, hz + 2, 100, 54)]:
        c.mountain(x, y, w, h)
    land(c, hz)
    c.house(150, hz + 50, 96, 56, 44, roof=ORANGE, wall=WHITE, chimney=True, shutters=PAL['blue'])
    c.fence(60, hz + 76, 120, hz + 84, 4, WHITE); c.fence(180, hz + 84, 250, hz + 72, 4, WHITE)
    c.laundry(262, hz + 20, 330, hz + 10)
    c.tree(46, hz + 56, 20); c.tree(520, hz + 30, 22, LIME); c.pine(600, hz + 30, 52); c.pine(380, hz + 30, 56); c.pine(412, hz + 38, 46); c.birch(470, hz + 34, 16)
    c.well(700, hz + 50)
    # the family tree in the corner, with golden fruit, a swing and a bench
    tx, ty = 962, H - 14
    c.tree(tx, ty, 58, LEAF, fruit=GOLD)
    c.line([(tx - 34, ty - 84), (tx - 34, ty - 30)], 1.3, amp=.2); c.line([(tx - 16, ty - 80), (tx - 16, ty - 30)], 1.3, amp=.2)
    c.shape([(tx - 40, ty - 30), (tx - 10, ty - 30), (tx - 10, ty - 25), (tx - 40, ty - 25)], WOOD, 1.6, amp=.1, step=10)
    c.bush(36, H - 12, 22, WHITE); c.rock(84, H - 8, 12); c.rock(106, H - 6, 8, STONE); c.bench(150, H - 12, 60)
    out(c, BG, 'scene_FamilyTree', (2048, 960))


def weather():
    c, hz = outdoor(23, 150)
    c.two(c.ring(150, 62, 30, 30), GOLD, 2.8, 4, 4, amp=.5, step=8)
    for k in range(12):
        a = k * math.pi / 6
        r0, r1 = 40, 56 if k % 2 == 0 else 50
        c.line([(150 + math.cos(a) * r0, 62 + math.sin(a) * r0), (150 + math.cos(a) * r1, 62 + math.sin(a) * r1)], 2.6, amp=.3)
    c.cloud(330, 54, 110); c.cloud(600, 84, 90); c.cloud(900, 50, 120, PAL['ice'])
    for i in range(6):
        c.line([(862 + i * 15, 76), (856 + i * 15, 92)], 1.8, NAVY, amp=.2)
    for i in range(4):
        c.line([(872 + i * 15, 100), (868 + i * 15, 110)], 1.8, NAVY, amp=.2)
    # a rainbow from the rain cloud to the hills
    for i, col in enumerate((RED, ORANGE, GOLD, LEAF, PAL['sky'])):
        rr = 150 - i * 8
        pts = [(760 + math.cos(math.radians(a)) * rr, hz + 30 - math.sin(math.radians(a)) * rr * .8) for a in range(20, 170, 8)]
        c.c.set_line_width(8); c._path(pts); c.c.set_source_rgb(*rgb(col)); c.c.stroke()
    for x, y, w, h in [(470, hz + 2, 170, 100), (590, hz, 130, 72), (60, hz + 4, 150, 70), (300, hz + 2, 110, 60)]:
        c.mountain(x, y, w, h)
    land(c, hz)
    # weather station: vane, windsock and a thermometer box
    x, y = 960, hz + 64
    c.line([(x, y), (x, y - 80)], 3, amp=.3); c.line([(x - 22, y - 64), (x + 22, y - 64)], 2.2, amp=.2)
    c.shape([(x - 20, y - 90), (x + 14, y - 84), (x - 6, y - 78)], RED, 2, amp=.3, step=8)
    c.line([(880, hz + 66), (880, hz + 6)], 2.6, amp=.2)
    for i, col in enumerate((RED, WHITE, RED, WHITE)):
        c.shape([(880 + i * 11, hz + 8 + i), (891 + i * 11, hz + 10 + i), (891 + i * 11, hz + 22 - i), (880 + i * 11, hz + 24 - i)], col, 1.4, amp=.2, step=10)
    c.two([(808, hz + 40), (808, hz + 14), (838, hz + 14), (838, hz + 40)], WHITE, 2, 3, 0, amp=.3); c.line([(823, hz + 40), (823, hz + 66)], 2.6, amp=.2)
    for i in range(3):
        c.line([(811, hz + 21 + i * 6), (835, hz + 21 + i * 6)], 1, amp=0)
    c.pine(720, hz + 34, 56); c.pine(752, hz + 40, 44); c.tree(260, hz + 38, 20); c.birch(200, hz + 40, 16)
    c.pond(90, H - 30, 60, 16); c.duck(70, H - 34); c.bush(1000, H - 10, 20, GOLD); c.rock(956, H - 8, 10)
    out(c, BG, 'scene_Weather', (2048, 960))


def house():
    c, fl = indoor(24)
    window(c, 120, 100); window(c, 904, 100)
    frame(c, 290, 70); frame(c, 734, 74, 40, 46)
    shelf(c, 450, 84, 130, 6, pot=True)
    # wall clock above the shelf
    c.two(c.ring(380, 60, 18, 18), WHITE, 2.2, 2, 2, amp=.3, step=7); c.line([(380, 60), (380, 48)], 1.8, amp=0); c.line([(380, 60), (389, 64)], 1.8, RED, amp=0)
    rug(c, 512, fl + 110, 320, 54)
    stove(c, 48, fl + 60); dresser(c, 972, fl + 62, 90, 74)
    plant(c, 972, fl - 12 - 0, 1.0); plant(c, 28, H - 4, 1.4)
    out(c, BG, 'scene_House', (2048, 960))


def memory():
    c, fl = indoor(25, wall=PAL['pale'], pattern='dots')
    shelf(c, 40, 84, 150, 8); shelf(c, 834, 84, 150, 8, pot=True)
    frame(c, 290, 70, 54, 44); frame(c, 400, 60, 40, 50, GOLD); frame(c, 512, 74, 60, 46); frame(c, 624, 60, 40, 50, PAL['sky']); frame(c, 734, 70, 54, 44)
    # a trunk of keepsakes, a dresser and a lamp in the corners
    x, y = 950, fl + 126
    c.sh_oval(x + 6, y + 2, 52, 8)
    c.two([(x - 46, y), (x - 46, y - 36), (x + 46, y - 36), (x + 46, y)], WOOD, 2.6, 4, 0, amp=.5)
    c.two([(x - 48, y - 36), (x - 42, y - 58), (x + 42, y - 58), (x + 48, y - 36)], WOOD_DARK, 2.6, 3, 3, amp=.5)
    for k in (-30, 30):
        c.line([(x + k, y - 57), (x + k, y - 1)], 2.2, GOLD, amp=.2)
    c.shape([(x - 6, y - 42), (x + 6, y - 42), (x + 6, y - 28), (x - 6, y - 28)], GOLD, 2, amp=.2, step=8)
    dresser(c, 56, fl + 60, 92, 70)
    c.line([(56, fl - 17), (56, fl - 44)], 2.4, amp=.1); c.two([(40, fl - 44), (72, fl - 44), (64, fl - 66), (48, fl - 66)], GOLD, 2, 2, 2, amp=.3)
    plant(c, 30, H - 4, 1.3)
    rug(c, 512, fl + 128, 260, 34)
    out(c, BG, 'scene_Memory', (2048, 960))


def numbers():
    c, fl = indoor(26)
    # ticket board along the wall
    c.two([(350, 30), (674, 30), (674, 116), (350, 116)], WOOD, 2.8, 4, 4, amp=.8)
    for i, n in enumerate(('1', '2', '3', '4', '5')):
        x = 371 + i * 58
        c.two([(x, 50), (x + 46, 48), (x + 46, 98), (x, 100)], GOLD if i == 2 else WHITE, 2, 2, 3, amp=.5, step=12)
        c.dotc(x + 23, 54, 2.4, RED)
        text(c, n, x + 13, 88, 28)
    # wall clock, scales and an abacus
    c.two(c.ring(150, 80, 36, 36), WHITE, 2.8, 4, 4, amp=.5, step=8); c.line([(150, 80), (150, 56)], 2.4, amp=.2); c.line([(150, 80), (168, 90)], 2.4, RED, amp=.2)
    for k in range(12):
        a = k * math.pi / 6
        c.line([(150 + math.cos(a) * 28, 80 + math.sin(a) * 28), (150 + math.cos(a) * 33, 80 + math.sin(a) * 33)], 1.5, amp=0)
    shelf(c, 800, 84, 150, 5, pot=True)
    ax, ay = 250, 120
    c.shape([(ax, ay - 60), (ax + 64, ay - 60), (ax + 64, ay), (ax, ay)], None, 3, amp=.3)
    for j in range(4):
        yy = ay - 50 + j * 13
        c.line([(ax, yy), (ax + 64, yy)], 1, amp=0)
        for i in range(5):
            c.dotc(ax + 8 + i * 8 + (20 if i > 2 + j % 2 else 0), yy, 3.6, (RED, GOLD, PAL['sky'], LEAF)[j])
    # counter with a till, a bell and sacks in the corners
    cx, cy = 946, fl + 128
    c.sh_oval(cx, cy + 2, 76, 8)
    c.two([(cx - 70, cy), (cx - 70, cy - 60), (cx + 70, cy - 60), (cx + 70, cy)], WOOD, 2.6, 5, 0)
    c.shape([(cx - 76, cy - 60), (cx + 76, cy - 60), (cx + 76, cy - 68), (cx - 76, cy - 68)], WOOD_DARK, 2, amp=.3)
    c.two([(cx - 20, cy - 68), (cx - 16, cy - 92), (cx + 14, cy - 92), (cx + 18, cy - 68)], GOLD, 2.2, 3, 0, amp=.3); c.oval(cx - 1, cy - 96, 5, 5, INK, 1.4)
    c.two([(cx - 62, cy - 68), (cx - 62, cy - 96), (cx - 30, cy - 96), (cx - 30, cy - 68)], PAL['sky'], 2, 3, 0, amp=.3)
    for i in range(3):
        c.two(c.lumpy(40 + i * 30, H - 22 - (i % 2) * 6, 17, 22, 4, .15), PAPER, 2, 3, 3, amp=.3, step=8)
    c.barrel(132, H - 6, 1.5)
    out(c, BG, 'scene_Numbers', (2048, 960))


def alphabet():
    c, fl = indoor(27, wall=PAL['pale'], pattern='dots')
    # blackboard with chalk letters
    c.two([(322, 20), (702, 20), (702, 128), (322, 128)], WOOD, 3, 4, 4, amp=.8)
    c.shape([(334, 30), (690, 30), (690, 118), (334, 118)], PINE_DARK, 2, amp=.6)
    text(c, 'А Б В Г Ґ Д', 356, 88, 40, WHITE)
    c.line([(360, 100), (560, 100)], 1.2, PAL['sage'], amp=.6)
    c.shape([(316, 128), (708, 128), (708, 138), (316, 138)], WOOD_DARK, 2.2, amp=.5)
    c.shape([(600, 122), (626, 122), (626, 128), (600, 128)], WHITE, 1.4, amp=.2, step=10); c.shape([(560, 120), (584, 120), (584, 128), (560, 128)], PAL['taupe'], 1.4, amp=.2, step=10)
    window(c, 130, 100, curtain=PAL['blue']); window(c, 894, 100, curtain=PAL['blue'])
    # a letter banner, a globe, books and a pencil pot
    gx, gy = 962, fl + 124
    dresser(c, gx, gy, 96, 60)
    c.line([(gx, gy - 67), (gx, gy - 78)], 2.6, amp=.1)
    c.two(c.ring(gx, gy - 98, 22, 22), PAL['sky'], 2.4, 3, 3, amp=.4, step=8, inner=lambda: (c.flat(c.lumpy(gx - 6, gy - 102, 10, 8, 4, .4), LEAF), c.flat(c.lumpy(gx + 10, gy - 90, 6, 5, 4, .4), LEAF)))
    x, y = 60, H - 6
    for i in range(4):
        c.two([(x - 38 + i * 5, y - i * 14), (x + 38 + i * 5, y - i * 14), (x + 38 + i * 5, y - 14 - i * 14), (x - 38 + i * 5, y - 14 - i * 14)], (NAVY, GOLD, RED, LEAF)[i], 2, 3, 0, amp=.4)
        c.line([(x + 26 + i * 5, y - i * 14 - 2), (x + 26 + i * 5, y - i * 14 - 12)], 1.2, PAPER, amp=0)
    c.two([(128, H - 4), (132, H - 34), (160, H - 34), (164, H - 4)], PAL['sky'], 2.2, 3, 0, amp=.3)
    for i, col in enumerate((GOLD, RED, LEAF)):
        c.line([(138 + i * 8, H - 34), (134 + i * 11, H - 66)], 4, INK, amp=.2); c.line([(138 + i * 8, H - 35), (134 + i * 11, H - 64)], 2, col, amp=.2)
    out(c, BG, 'scene_Alphabet', (2048, 960))


# --- shop layout (1920x1080, matches the product slots and panels in Shop.unity) ---------
def shop():
    c = Scene(1920, 1080, 1.5, PAPER, 31)
    for i in range(0, 46):
        c.flat([(i * 44, 0), (i * 44 + 16, 0), (i * 44 + 16, 1080), (i * 44, 1080)], SAND, .3)
    # shelf unit behind the products
    c.flat([(58, 296), (1124, 296), (1124, 786), (58, 786)], TONE1)
    c.shape([(45, 280), (1110, 280), (1110, 770), (45, 770)], WOOD, 3.4, amp=1.2, step=30)
    for row in range(2):
        for col in range(4):
            x0, y0 = 80 + col * 255, 315 + row * 220
            c.shape([(x0, y0), (x0 + 225, y0), (x0 + 225, y0 + 195), (x0, y0 + 195)], PAPER, 2.4, amp=1, step=26)
            c.hatch(x0 + 150, y0 + 150, x0 + 215, y0 + 188, 11, 1.5, TONE1)
    # striped awning along the top of the unit
    for i in range(16):
        x0 = 30 + i * 68.5
        c.shape([(x0, 232), (x0 + 68.5, 232), (x0 + 68.5, 280), (x0 + 34, 298), (x0, 280)], RED if i % 2 == 0 else WHITE, 2.8, amp=.8, step=20)
    c.line([(24, 232), (1132, 232)], 3.4, amp=1)
    # basket box and the list panel
    c.flat([(58, 821), (1124, 821), (1124, 1021), (58, 1021)], TONE1)
    c.shape([(45, 805), (1110, 805), (1110, 1005), (45, 1005)], '#cae6d9', 3.4, amp=1.2, step=30)
    c.flat([(1173, 296), (1889, 296), (1889, 1021), (1173, 1021)], TONE1)
    c.shape([(1160, 280), (1875, 280), (1875, 1005), (1160, 1005)], WHITE, 3.4, amp=1.2, step=30)
    # a pin holding the shopping list
    c.oval(1517, 280, 12, 12, RED, 2.6)
    out(c, BG, 'sprite_shop_clean', (1920, 1080))


# --- house rooms (stretched by the game to the 8 x 3.6 placement area) -------------------
def room(name, seed, kitchen):
    c = Scene(1600, 720, 1.5, PAPER if kitchen else '#cae6d9', seed)
    fl = 470
    wall_dark = dk(PAPER if kitchen else PAL['pale'])
    for i in range(0, 40):
        c.flat([(i * 42, 0), (i * 42 + 15, 0), (i * 42 + 15, fl), (i * 42, fl)], wall_dark, .28)
    c.flat([(0, fl), (1600, fl), (1600, 720), (0, 720)], TONE1)
    for i in range(0, 18):
        x = 800 + (i - 8.5) * 150
        c.line([(800 + (x - 800) * .8, fl + 14), (800 + (x - 800) * 1.35, 720)], 2, WOOD, amp=.8)
    c.flat([(0, fl - 16), (1600, fl - 16), (1600, fl), (0, fl)], WOOD)
    c.line([(0, fl - 16), (1600, fl - 16)], 2.6, amp=1); c.line([(0, fl), (1600, fl)], 3.2, amp=1)
    # window with a view
    x, y, w, h = (1300, 190, 150, 170) if kitchen else (300, 190, 150, 170)
    c.shape([(x - w / 2, y - h / 2), (x + w / 2, y - h / 2), (x + w / 2, y + h / 2), (x - w / 2, y + h / 2)], '#96a9c1', 3.4, amp=.8)
    c.flat([(x - w / 2 + 8, y + h / 2 - 8), (x - w / 2 + 8, y + 20), (x - 20, y - 20), (x + 20, y + 14), (x + w / 2 - 8, y - 10), (x + w / 2 - 8, y + h / 2 - 8)], GRASS)
    c.oval(x + 36, y - 46, 15, 15, GOLD, 2.4)
    c.line([(x, y - h / 2), (x, y + h / 2)], 2.8, amp=.4); c.line([(x - w / 2, y), (x + w / 2, y)], 2.8, amp=.4)
    c.shape([(x - w / 2 - 12, y + h / 2), (x + w / 2 + 12, y + h / 2), (x + w / 2 + 12, y + h / 2 + 14), (x - w / 2 - 12, y + h / 2 + 14)], WHITE, 2.8, amp=.5)
    if kitchen:
        # tiled strip and a rail of hanging tools
        for i in range(12):
            for j in range(2):
                x0, y0 = 80 + i * 62, fl - 150 + j * 62
                c.shape([(x0, y0), (x0 + 62, y0), (x0 + 62, y0 + 62), (x0, y0 + 62)], WHITE if (i + j) % 2 else '#a4c5af', 1.8, amp=.5, step=20)
        c.line([(120, 150), (760, 150)], 3.2, amp=.8)
        for i, kind in enumerate('spoon pan spoon pan spoon'.split()):
            hx = 190 + i * 125
            c.line([(hx, 150), (hx, 196)], 2.4, amp=.3)
            if kind == 'pan':
                c.oval(hx, 232, 34, 34, ORANGE if i == 1 else '#636663', 2.8)
            else:
                c.oval(hx, 214, 13, 18, WOOD, 2.4)
    else:
        # wallpaper dots, a picture and a rug
        for i in range(9):
            for j in range(3):
                c.tick(520 + i * 120 + (j % 2) * 60, 90 + j * 120, .8, '#a4c5af')
        c.shape([(760, 120), (900, 120), (900, 230), (760, 230)], WHITE, 3.2, amp=.6)
        c.shape([(780, 214), (820, 160), (846, 190), (866, 170), (884, 214)], GRASS, 2.2, amp=.4)
        c.oval(866, 148, 10, 10, GOLD, 2)
        c.shape([(560, 560), (1200, 560), (1140, 680), (500, 680)], RED, 3, amp=1)
        c.line([(590, 580), (1170, 580)], 3.4, PAPER, amp=.6); c.line([(540, 660), (1120, 660)], 3.4, GOLD, amp=.6)
    c.shape([(4, 4), (1596, 4), (1596, 716), (4, 716)], None, 4.5, amp=1, step=40)
    out(c, ROOMS, name, (1280, 720))


for f in (farm, family, weather, house, memory, numbers, alphabet, shop):
    f()
room('sprite_empty_room_bedroom', 41, False)
room('sprite_empty_room_kitchen', 42, True)
