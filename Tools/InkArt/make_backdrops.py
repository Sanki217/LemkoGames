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
    c.oval(x, y - 3, 7, 7, '#ffc58f', 1.6); c.line([(x - 12, y + 12), (x, y + 5), (x + 12, y + 12)], 1.6, amp=.2)


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
    c.shape([(x - 15, y - 12), (x - 15, y - 44), (x + 15, y - 44), (x + 15, y - 12)], '#4b2a20', 2, amp=.3)
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
    sky(c, [(300, 40, 90), (560, 34, 110), (760, 56, 80)])
    for x, y, w, h in [(330, hz + 4, 150, 96), (450, hz, 120, 70), (640, hz - 2, 140, 84), (540, hz, 110, 60)]:
        c.mountain(x, y, w, h)
    land(c, hz)
    c.field(250, hz + 40, 90, 24, GOLD, 4)
    c.house(900, hz + 40, 100, 54, 46, roof=NAVY, wall=RED, side=WOOD_DARK, planks=True, windows=0, door=False, roof_h=48, glass=PAPER)
    c.shape([(880, hz + 40), (880, hz + 4), (920, hz + 4), (920, hz + 40)], PAPER, 2.2, amp=.3)
    c.line([(880, hz + 4), (920, hz + 40)], 1.6, amp=.2); c.line([(920, hz + 4), (880, hz + 40)], 1.6, amp=.2)
    c.two([(992, hz + 22), (992, hz - 38), (1016, hz - 38), (1016, hz + 22)], STONE, 2.2, 4, 0); c.two([(988, hz - 36), (1004, hz - 56), (1020, hz - 36)], RED, 2, 3, 3)
    c.pine(600, hz + 30, 50); c.pine(632, hz + 36, 58); c.tree(700, hz + 36, 18, fruit=RED); c.birch(770, hz + 36, 15)
    c.haybale(820, hz + 44, 17)

    # the pasture in the middle: trodden grass inside a rail fence with an open gate towards the shelter
    tl, tr, br, bl = (372, 182), (850, 182), (902, 392), (318, 392)
    c.shadow([(bl[0] + 6, bl[1] + 8), (br[0] + 10, br[1] + 8), (br[0], br[1] - 4), (bl[0], bl[1] - 4)])
    c.flat([tl, tr, br, bl], GRASS_LIGHT)
    def turf():
        for i in range(16):
            c.flat(c.lumpy(c.r.uniform(330, 890), c.r.uniform(190, 390), c.r.uniform(40, 90), c.r.uniform(10, 22), 6, .25), (GRASS, PAL['pale'], SAND)[i % 3], .4)
        for i in range(40):
            c.tuft(c.r.uniform(340, 880), c.r.uniform(200, 388), c.r.uniform(.6, 1), GRASS_DARK)
        for i in range(8):
            c.flowers(c.r.uniform(350, 870), c.r.uniform(204, 384), c.r.randint(3, 6), 14, (WHITE, GOLD))
    c.inside([tl, tr, br, bl], turf)
    # trough and a haystack along the back rail, a puddle near the front
    c.two([(700, 206), (770, 206), (764, 220), (706, 220)], WOOD, 2, 2, 3, amp=.3); c.flat([(706, 208), (764, 208), (760, 214), (710, 214)], PAL['sky'])
    c.haybale(810, 226, 16)
    c.flat(c.lumpy(470, 360, 34, 9, 5, .2), PAL['sky']); c.line([(452, 358), (464, 355), (476, 358)], 1.2, WHITE, amp=.2, step=6)
    c.fence(tl[0], tl[1], tr[0], tr[1], 11)                          # back rail
    def side(a, b, n, gap=None):
        for i in range(n):
            if gap and i in gap:
                continue
            t0, t1 = i / n, (i + 1) / n
            c.fence(a[0] + (b[0] - a[0]) * t0, a[1] + (b[1] - a[1]) * t0, a[0] + (b[0] - a[0]) * t1, a[1] + (b[1] - a[1]) * t1, 3)
    side(tr, br, 4)
    side(tl, bl, 4, gap=(1,))                                        # the gap is the gateway
    # open gate swung outwards
    g0 = (tl[0] + (bl[0] - tl[0]) * .25, tl[1] + (bl[1] - tl[1]) * .25)
    c.fence(g0[0], g0[1], g0[0] - 44, g0[1] + 30, 3)
    c.fence(bl[0], bl[1], br[0], br[1], 12)                          # front rail
    c.cart(960, H - 18); c.bush(1004, H - 66, 18, RED)

    # the shelter on the left where the animals wait: plank wall, straw floor and a shingled roof
    x0, x1, top, bot = -6, 226, 60, 286
    c.shadow([(x0, bot + 6), (x1 + 16, bot + 6), (x1 + 4, bot - 8), (x0, bot - 8)])
    wall = [(x0, top + 8), (x1, top + 8), (x1, bot - 26), (x0, bot - 26)]
    def boards():
        for i in range(1, 12):
            c.line([(x0 + i * 20, top), (x0 + i * 20, bot)], 1.3, dk(WOOD), amp=.4)
        c.flat([(x0, top), (x1, top), (x1, top + 30), (x0, top + 30)], dk(WOOD), .6)
    c.two(wall, WOOD, 2.6, 5, 0, inner=boards)
    straw = [(x0, bot - 30), (x1 + 6, bot - 30), (x1 + 18, bot), (x0, bot)]
    c.two(straw, GOLD, 2.4, 3, 3, inner=lambda: c.hatch(x0, bot - 28, x1 + 10, bot, gap=9, w=1.2, color=PAL['gold']))
    c.line([(x0, top + 118), (x1, top + 118)], 3.2, amp=.5); c.line([(x0, top + 118), (x1, top + 118)], 1.4, WOOD_DARK, amp=.5)   # rail between the rows
    for px in (4, 112, 220):
        c.two([(px - 5, bot - 2), (px - 5, top + 6), (px + 5, top + 6), (px + 5, bot - 2)], WOOD_DARK, 2, 2, 0, amp=.3)
    roof = [(x0, top - 22), (x1 + 4, top - 22), (x1 + 22, top + 12), (x0, top + 12)]
    def tiles():
        for i in range(1, 3):
            c.line([(x0, top - 22 + i * 11), (x1 + 22, top - 22 + i * 11)], 1.3, dk(RED), amp=.4)
        for i in range(26):
            for j in range(3):
                c.line([(x0 + i * 10 + (j % 2) * 5, top - 22 + j * 11), (x0 + i * 10 + (j % 2) * 5 + 2, top - 11 + j * 11)], 1, dk(RED), amp=0)
    c.two(roof, RED, 2.6, 3, 4, inner=tiles)
    c.hen(60, bot + 26, 1.2); c.hen(100, bot + 38, 1, PAL['wood']); c.barrel(200, bot + 44, 1.1); c.tree(262, hz + 60, 20)
    c.fence(-6, H - 14, 150, H - 30, 6)
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
    # the big blackboard: the letter fields are shown on it by the game
    x0, x1, y0, y1 = 168, 945, 100, 246
    c.shadow([(x0 + 6, y1 + 16), (x1 + 10, y1 + 16), (x1 + 10, y0 + 10), (x1, y0 + 10), (x1, y1 + 6), (x0 + 6, y1 + 6)])
    c.two([(x0 - 12, y0 - 12), (x1 + 12, y0 - 12), (x1 + 12, y1 + 12), (x0 - 12, y1 + 12)], WOOD, 3, 4, 4, amp=.8)
    board = [(x0, y0), (x1, y0), (x1, y1), (x0, y1)]
    def chalk():
        for i in range(9):                                            # wiped chalk smears
            c.flat(c.lumpy(c.r.uniform(x0 + 40, x1 - 40), c.r.uniform(y0 + 16, y1 - 16), c.r.uniform(50, 120), c.r.uniform(8, 18), 5, .3), PINE, .35)
        c.line([(x0 + 26, y1 - 30), (x1 - 26, y1 - 30)], 1.3, PAL['sage'], amp=.8)          # the line the word is written on
        for i, ch in enumerate('Аа Бб Вв'.split()):
            cr = c.c; cr.select_font_face('DejaVu Sans', cairo.FONT_SLANT_NORMAL, cairo.FONT_WEIGHT_NORMAL); cr.set_font_size(13)
            cr.set_source_rgba(*rgb(PAL['pale']), .7); cr.move_to(x0 + 16 + i * 40, y0 + 22); cr.show_text(ch)
    c.flat(board, PINE_DARK); c.inside(board, chalk); c.stroke(board, 2)
    c.two([(x0 - 20, y1 + 12), (x1 + 20, y1 + 12), (x1 + 20, y1 + 22), (x0 - 20, y1 + 22)], WOOD_DARK, 2.2, 3, 0, amp=.5)       # chalk ledge
    for i, col in enumerate((WHITE, PAL['yellow'], WHITE, PAL['skin'])):
        c.shape([(x1 - 150 + i * 30, y1 + 6), (x1 - 130 + i * 30, y1 + 6), (x1 - 130 + i * 30, y1 + 12), (x1 - 150 + i * 30, y1 + 12)], col, 1.2, amp=.2, step=10)
    c.two([(x0 + 30, y1 + 2), (x0 + 66, y1 + 2), (x0 + 66, y1 + 12), (x0 + 30, y1 + 12)], PAL['taupe'], 1.6, 2, 0, amp=.2)       # sponge
    # letter bunting and a clock above the board
    a, b = (x0 + 20, 22), (x1 - 20, 22)
    c.line([a, ((a[0] + b[0]) / 2, 44), b], 1.5, amp=.5)
    for i in range(1, 20):
        if 6 < i < 14:
            continue                                                  # leaves room for the prompt card
        t = i / 20
        fx = a[0] + (b[0] - a[0]) * t; fy = 22 + 22 * 4 * t * (1 - t)
        c.shape([(fx - 9, fy), (fx + 9, fy), (fx, fy + 20)], (GOLD, RED, PAL['sky'], WHITE, ORANGE)[i % 5], 1.6, amp=.3, step=8)
    # a frame on the wall for the picture of the word, a globe, books and a pencil pot
    c.two([(8, 104), (128, 104), (128, 238), (8, 238)], WOOD, 2.6, 4, 4, amp=.6); c.flat([(18, 114), (118, 114), (118, 228), (18, 228)], PAL['cream'])
    c.line([(68, 104), (68, 84)], 1.6, amp=.2); c.dotc(68, 82, 3, INK)
    gx, gy = 966, fl + 126
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
    """A whole shop, after Sanki's sketch: slanted display table (top left), shelf unit on the back wall, two crates on
    the floor, a tall rack, the door and the shopkeeper behind the counter (right), the basket (bottom right) and a notepad
    for the shopping list (bottom left). World (wx, wy) is at 960 + wx*120, 540 - wy*120; products are 162 x 144."""
    c = Scene(1920, 1080, 1.5, PAPER, 31)
    c.rad = 6
    r = c.r
    FLOOR = 700
    CREAM = PAL['cream']
    for i in range(0, 46):
        c.flat([(i * 44, 0), (i * 44 + 16, 0), (i * 44 + 16, FLOOR), (i * 44, FLOOR)], SAND, .3)
    # floorboards
    c.flat([(0, FLOOR), (1920, FLOOR), (1920, 1080), (0, 1080)], TONE1)
    c.flat([(0, FLOOR), (1920, FLOOR), (1920, FLOOR + 30), (0, FLOOR + 30)], dk(TONE1), .6)
    for i in range(-8, 26):
        x = 960 + (i - 8) * 150
        a, b = (960 + (x - 960) * .8, FLOOR + 4), (960 + (x - 960) * 1.5, 1080)
        c.line([a, b], 2, WOOD, amp=.8)
        for t in (r.uniform(.15, .45), r.uniform(.55, .9)):
            px, py = a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t
            c.line([(px, py), (px + 150 * (.8 + t * .7), py)], 1.4, WOOD, amp=.4)
    c.flat([(0, FLOOR - 16), (1920, FLOOR - 16), (1920, FLOOR + 4), (0, FLOOR + 4)], WOOD_DARK)
    c.line([(0, FLOOR - 16), (1920, FLOOR - 16)], 2.4, amp=1); c.line([(0, FLOOR + 4), (1920, FLOOR + 4)], 3, amp=1)

    def jar(x, y, w, h, col, lid=WHITE):
        c.two([(x - w / 2, y), (x - w / 2 - 2, y - h + 8), (x - w / 2 + 6, y - h), (x + w / 2 - 6, y - h), (x + w / 2 + 2, y - h + 8), (x + w / 2, y)], col, 2, 3, 0, amp=.3, step=10)
        c.shape([(x - w / 2 + 3, y - h), (x + w / 2 - 3, y - h), (x + w / 2 - 3, y - h - 9), (x - w / 2 + 3, y - h - 9)], lid, 1.6, amp=.2)
        c.shape([(x - w / 2 + 6, y - h * .62), (x + w / 2 - 6, y - h * .62), (x + w / 2 - 6, y - h * .28), (x - w / 2 + 6, y - h * .28)], WHITE, 1.2, amp=.2)

    def bottle(x, y, h, col):
        c.two([(x - 11, y), (x - 11, y - h * .58), (x - 5, y - h * .74), (x - 5, y - h), (x + 5, y - h), (x + 5, y - h * .74), (x + 11, y - h * .58), (x + 11, y)], col, 2, 3, 0, amp=.2, step=10)
        c.shape([(x - 6, y - h), (x + 6, y - h), (x + 6, y - h - 7), (x - 6, y - h - 7)], WOOD, 1.4, amp=.1)
        c.shape([(x - 8, y - h * .46), (x + 8, y - h * .46), (x + 8, y - h * .16), (x - 8, y - h * .16)], CREAM, 1.2, amp=.1)

    def tin(x, y, w, h, col):
        c.two([(x - w / 2, y), (x - w / 2, y - h), (x + w / 2, y - h), (x + w / 2, y)], col, 2, 3, 0, amp=.2, step=10)
        c.line([(x - w / 2, y - h * .3), (x + w / 2, y - h * .3)], 1.2, amp=.1); c.line([(x - w / 2, y - h * .7), (x + w / 2, y - h * .7)], 1.2, amp=.1)

    def sack(x, y, k=1.0, col=CREAM):
        c.sh_oval(x + 6, y, 46 * k, 8 * k)
        c.two(c.lumpy(x, y - 46 * k, 42 * k, 48 * k, 5, .12), col, 2.4, 4, 3, amp=.6, step=10)
        c.two([(x - 14 * k, y - 90 * k), (x - 22 * k, y - 112 * k), (x, y - 104 * k), (x + 22 * k, y - 114 * k), (x + 14 * k, y - 90 * k)], col, 2, 2, 0, amp=.4, step=8)
        c.line([(x - 15 * k, y - 91 * k), (x + 15 * k, y - 91 * k)], 2.4, WOOD_DARK, amp=.3)
        c.line([(x - 18 * k, y - 50 * k), (x - 8 * k, y - 20 * k)], 1.2, dk(col), amp=.4); c.line([(x + 12 * k, y - 60 * k), (x + 20 * k, y - 30 * k)], 1.2, dk(col), amp=.4)

    # --- ceiling: a lamp and strings of garlic and peppers (the top middle stays free for the message) ---
    lx = 420
    c.line([(lx, 0), (lx, 120)], 2, amp=.2)
    c.two([(lx - 38, 170), (lx - 15, 120), (lx + 15, 120), (lx + 38, 170)], PAL['gold'], 2.4, 3, 0, amp=.4); c.oval(lx, 174, 13, 8, GOLD, 1.8)
    c.line([(40, 96), (140, 128), (250, 100)], 1.8, amp=.6)
    for i in range(7):
        t = (i + .5) / 7; x = 40 + 210 * t; y = 98 + 26 * 4 * t * (1 - t)
        c.two(c.lumpy(x, y + 16, 9, 13, 4, .2), (WHITE, WHITE, RED)[i % 3], 1.6, 2, 2, amp=.3, step=7)

    # --- the door, top right (the counter hides its lower half) -------------------------------------
    d0, d1, dt = 1650, 1884, 132
    c.two([(d0 - 16, dt - 16), (d1 + 16, dt - 16), (d1 + 16, FLOOR - 16), (d0 - 16, FLOOR - 16)], WOOD_DARK, 3, 4, 0, amp=.6)
    def leaf():
        for i in range(1, 4):
            c.line([(d0 + i * 58, dt), (d0 + i * 58, FLOOR)], 1.4, dk(WOOD), amp=.4)
    c.two([(d0, dt), (d1, dt), (d1, FLOOR - 16), (d0, FLOOR - 16)], WOOD, 2.6, 5, 0, amp=.5, inner=leaf)
    g = [(d0 + 34, dt + 34), (d1 - 34, dt + 34), (d1 - 34, dt + 250), (d0 + 34, dt + 250)]
    def outside():
        c.flat([(d0, dt + 170), (d1, dt + 150), (d1, dt + 260), (d0, dt + 260)], GRASS)
        c.flat(c.lumpy(d0 + 80, dt + 150, 40, 46, 6, .2), LEAF); c.flat([(d0 + 76, dt + 180), (d0 + 84, dt + 180), (d0 + 84, dt + 230), (d0 + 76, dt + 230)], WOOD_DARK)
        c.flat(c.lumpy(d1 - 70, dt + 70, 44, 16, 5, .2), WHITE)
    c.flat(g, SKY); c.inside(g, outside); c.stroke(g, 2.4)
    gx = (d0 + d1) / 2
    c.line([(gx, dt + 34), (gx, dt + 250)], 2.4, amp=.2); c.line([(d0 + 34, dt + 142), (d1 - 34, dt + 142)], 2.4, amp=.2)
    c.oval(d0 + 26, dt + 310, 8, 8, GOLD, 1.8)
    c.line([(d0 + 30, dt - 16), (d0 + 30, dt - 46), (d0 + 62, dt - 46)], 2.2, amp=.2)                              # door bell
    c.two([(d0 + 50, dt - 20), (d0 + 54, dt - 42), (d0 + 70, dt - 42), (d0 + 74, dt - 20)], GOLD, 1.8, 2, 0, amp=.2)

    # --- slanted display table, top left (products lie on it around py 342..396) -----------------
    t = [(64, 286), (566, 286), (606, 462), (18, 462)]
    c.sh_oval(312, 722, 300, 14)
    for x, y0 in ((52, 490), (572, 490), (96, 470), (530, 470)):
        c.two([(x - 11, y0), (x + 11, y0), (x + 8, 716), (x - 8, 716)], WOOD_DARK, 2.4, 3, 0, amp=.4)
    c.line([(60, 620), (566, 620)], 5, amp=.6); c.line([(60, 620), (566, 620)], 2.4, WOOD_DARK, amp=.6)
    def cloth():
        for i in range(-2, 16):
            if i % 2 == 0:
                c.flat([(64 + i * 36 - 4, 286), (64 + i * 36 + 32, 286), (18 + i * 42 + 38, 462), (18 + i * 42 - 4, 462)], PAL['pale'])
        c.line([(52, 322), (578, 322)], 1.6, RED, amp=.4); c.line([(26, 436), (600, 436)], 2, RED, amp=.4)
    c.two(t, WHITE, 3, 4, 0, amp=.8, inner=cloth)
    c.two([(64 - 6, 262), (566 + 6, 262), (566 + 6, 288), (64 - 6, 288)], WOOD, 2.6, 3, 0, amp=.6)          # raised back board
    ap0, ap1 = 18, 606
    scallop = [(ap0, 462), (ap1, 462)] + [(ap1 - i * 21, 496 + (9 if i % 2 else 0)) for i in range(0, int((ap1 - ap0) / 21) + 1)]
    def stripes():
        for i in range(16):
            if i % 2 == 0:
                c.flat([(ap0 + i * 42, 458), (ap0 + i * 42 + 42, 458), (ap0 + i * 42 + 42, 520), (ap0 + i * 42, 520)], RED)
    c.flat(scallop, WHITE); c.inside(scallop, stripes); c.stroke(scallop, 2.4)
    sack(548, 716, .95); sack(476, 722, .7, SAND)

    # --- shelf unit on the back wall (boards at py 408 and 606, cupboard below) --------------------
    s0, s1 = 612, 1188
    c.sh_oval(900, 708, 300, 10)
    def back():
        c.flat([(s0 + 14, 258), (s1 - 14, 258), (s1 - 14, 626), (s0 + 14, 626)], PAL['pale'])
        for i in range(1, 9):
            c.line([(s0 + 14 + i * 62, 258), (s0 + 14 + i * 62, 626)], 1.4, dk(PAL['pale']), amp=.5)
        c.flat([(s0 + 14, 258), (s1 - 14, 258), (s1 - 14, 280), (s0 + 14, 280)], dk(PAL['pale']), .7)
        c.flat([(s0 + 14, 426), (s1 - 14, 426), (s1 - 14, 448), (s0 + 14, 448)], dk(PAL['pale']), .7)
    c.two([(s0, 244), (s1, 244), (s1, FLOOR + 4), (s0, FLOOR + 4)], WOOD, 3, 5, 0, inner=back)
    c.stroke([(s0 + 14, 258), (s1 - 14, 258), (s1 - 14, 626), (s0 + 14, 626)], 1.8)
    # background goods on the free parts of the boards
    jar(852, 408, 40, 56, RED); jar(900, 408, 44, 70, GOLD); bottle(940, 408, 92, PAL['sage'])
    jar(1150, 408, 40, 62, ORANGE); bottle(640, 408, 80, PAL['mint'])
    tin(876, 606, 54, 44, PAL['sky']); tin(880, 562, 44, 38, RED); tin(934, 606, 46, 62, GOLD)
    bottle(646, 606, 96, WOOD_DARK); bottle(1154, 606, 100, PAL['sage'])
    for y in (408, 606):
        c.two([(s0 + 6, y), (s1 - 6, y), (s1 - 6, y + 20), (s0 + 6, y + 20)], WOOD, 2.6, 3, 0, amp=.6)
    for i in range(3):
        x = s0 + 22 + i * 180
        c.shape([(x, 640), (x + 172, 640), (x + 172, 690), (x, 690)], WOOD, 1.8, dk(WOOD), amp=.4, step=30)
        c.oval(x + 86, 665, 6, 6, GOLD, 1.4)
    c.two([(s0 - 12, 226), (s1 + 12, 226), (s1 + 12, 248), (s0 - 12, 248)], WOOD_DARK, 2.6, 3, 0, amp=.6)

    # --- barrel with a scoop between the crates and the rack ---------------------------------------
    bx = 1116
    c.sh_oval(bx + 8, 812, 76, 12)
    c.two([(bx - 58, 808), (bx - 70, 740), (bx - 58, 672), (bx + 58, 672), (bx + 70, 740), (bx + 58, 808)], WOOD, 2.8, 5, 0, amp=.5, step=12,
          inner=lambda: [c.line([(bx - 40 + i * 27, 672), (bx - 46 + i * 31, 740), (bx - 40 + i * 27, 808)], 1.4, dk(WOOD), amp=.3) for i in range(4)])
    for y, w in ((694, 62), (786, 62)):
        c.line([(bx - w, y), (bx + w, y)], 6, amp=.3); c.line([(bx - w, y), (bx + w, y)], 3, TONE3, amp=.3)
    c.oval(bx, 672, 58, 13, SAND, 2.2)
    c.line([(bx + 14, 668), (bx + 50, 628)], 5, amp=.3); c.line([(bx + 14, 668), (bx + 50, 628)], 2.4, WOOD_DARK, amp=.3)

    # --- two crates on the floor (products lie in them around py 744) -----------------------------
    for cx, tilt in ((648, -5), (894, 6)):
        c.sh_oval(cx + 8, 906, 126, 13)
        c.two([(cx - 98, 742 + tilt), (cx + 98, 742 - tilt), (cx + 112, 808 - tilt), (cx - 112, 808 + tilt)], WOOD_DARK, 2.4, 0, 0, amp=.5)       # inside
        for i in range(16):                                                                                                                     # straw
            x = cx - 100 + i * 13 + r.uniform(-4, 4)
            c.line([(x, 806), (x + r.uniform(-12, 12), 772 + r.uniform(-8, 8))], 1.8, GOLD, amp=.5)
        fr = [(cx - 112, 806 + tilt), (cx + 112, 806 - tilt), (cx + 106, 900 - tilt * .5), (cx - 106, 900 + tilt * .5)]
        def slats(cx=cx, tilt=tilt):
            for j in (1, 2):
                yy = 806 + j * 31
                c.line([(cx - 112, yy + tilt * .8), (cx + 112, yy - tilt * .8)], 2, WOOD_DARK, amp=.5)
            for i in range(5):
                c.line([(cx - 90 + i * 45, 812 + j * 0), (cx - 60 + i * 45, 812)], 1.2, dk(WOOD), amp=.3)
        c.two(fr, WOOD, 2.8, 4, 0, amp=.6, inner=slats)
        for sx in (-1, 1):
            c.two([(cx + sx * 112 - 9, 800 - sx * tilt), (cx + sx * 112 + 9, 800 - sx * tilt), (cx + sx * 106 + 9, 904), (cx + sx * 106 - 9, 904)], WOOD_DARK, 2, 2, 0, amp=.3)
            c.dotc(cx + sx * 110, 820, 3, INK); c.dotc(cx + sx * 107, 884, 3, INK)

    # --- tall rack (boards at py 444 and 660, products on all three levels) ------------------------
    r0, r1 = 1224, 1452
    c.sh_oval(1344, 880, 136, 12)
    def inside_rack():
        c.flat([(r0 + 16, 264), (r1 - 16, 264), (r1 - 16, 856), (r0 + 16, 856)], CREAM)
        for i in range(1, 6):
            c.line([(r0 + 16 + i * 33, 264), (r0 + 16 + i * 33, 856)], 1.2, SAND, amp=.4)
        for y in (264, 462, 678):
            c.flat([(r0 + 16, y), (r1 - 16, y), (r1 - 16, y + 22), (r0 + 16, y + 22)], SAND, .8)
    c.two([(r0, 248), (r1, 248), (r1, 874), (r0, 874)], WOOD, 3, 5, 0, inner=inside_rack)
    c.stroke([(r0 + 16, 264), (r1 - 16, 264), (r1 - 16, 856), (r0 + 16, 856)], 1.8)
    for y in (444, 660):
        c.two([(r0 + 8, y), (r1 - 8, y), (r1 - 8, y + 18), (r0 + 8, y + 18)], WOOD, 2.6, 3, 0, amp=.6)
    c.two([(r0 + 4, 856), (r1 - 4, 856), (r1 - 4, 876), (r0 + 4, 876)], WOOD_DARK, 2.4, 3, 0, amp=.5)
    c.two([(r0 - 10, 230), (r1 + 10, 230), (r1 + 10, 252), (r0 - 10, 252)], WOOD_DARK, 2.6, 3, 0, amp=.6)
    for i, col in enumerate((RED, GOLD, LIME, ORANGE)):
        jar(r0 + 34 + i * 54, 230, 36, 44, col)

    # --- the shopkeeper behind the counter -----------------------------------------------------------
    kx, ky = 1562, -168
    c.two([(kx - 66, 760 + ky), (kx - 56, 596 + ky), (kx - 20, 572 + ky), (kx + 20, 572 + ky), (kx + 56, 596 + ky), (kx + 66, 760 + ky)], PAL['sage'], 3, 5, 0)       # shirt
    c.two([(kx - 40, 760 + ky), (kx - 34, 610 + ky), (kx + 34, 610 + ky), (kx + 40, 760 + ky)], WHITE, 2.4, 4, 0)                                            # apron
    c.line([(kx - 34, 612 + ky), (kx - 12, 574 + ky)], 2, amp=.2); c.line([(kx + 34, 612 + ky), (kx + 12, 574 + ky)], 2, amp=.2)
    c.two([(kx - 12, 580 + ky), (kx + 12, 580 + ky), (kx + 10, 556 + ky), (kx - 10, 556 + ky)], SKIN, 2, 2, 0, amp=.2)
    c.two(c.ring(kx, 512 + ky, 44, 46), SKIN, 3, 5, 5, amp=.5, step=8)
    c.flat([(kx - 44, 506 + ky), (kx - 40, 474 + ky), (kx - 14, 458 + ky), (kx + 16, 458 + ky), (kx + 40, 474 + ky), (kx + 44, 506 + ky), (kx + 30, 486 + ky), (kx, 478 + ky), (kx - 30, 488 + ky)], WOOD_DARK)
    c.line([(kx - 44, 506 + ky), (kx - 40, 474 + ky), (kx - 14, 458 + ky), (kx + 16, 458 + ky), (kx + 40, 474 + ky), (kx + 44, 506 + ky)], 2.6, amp=.5)
    c.dotc(kx - 15, 514 + ky, 4.2, INK); c.dotc(kx + 15, 514 + ky, 4.2, INK)
    c.flat(c.ring(kx - 26, 530 + ky, 8, 5), ORANGE, .7); c.flat(c.ring(kx + 26, 530 + ky, 8, 5), ORANGE, .7)
    c.line([(kx - 12, 532 + ky), (kx, 540 + ky), (kx + 12, 532 + ky)], 2.4, amp=.2, step=6)
    c.line([(kx - 20, 524 + ky), (kx - 6, 528 + ky)], 3, WOOD_DARK, amp=.1); c.line([(kx + 20, 524 + ky), (kx + 6, 528 + ky)], 3, WOOD_DARK, amp=.1)        # moustache
    # counter with a bell and a till
    c0 = 1478
    c.sh_oval(1710, 824, 240, 12)
    front = [(c0, 600), (1920, 600), (1920, 818), (c0, 818)]
    def panels():
        for i in range(3):
            px = c0 + 22 + i * 146
            c.shape([(px, 628), (px + 124, 628), (px + 124, 796), (px, 796)], WOOD, 1.6, dk(WOOD), amp=.5, step=30)
    c.two(front, WOOD, 3, 6, 0, inner=panels)
    c.two([(c0 - 16, 572), (1920, 572), (1920, 604), (c0 - 16, 604)], WOOD_DARK, 3, 3, 0, amp=.8)
    c.two([(1668, 572), (1674, 550), (1704, 550), (1710, 572)], GOLD, 2.2, 3, 0, amp=.3); c.dotc(1689, 546, 5, INK)       # bell
    tx = 1812                                                                                                           # till
    c.two([(tx - 62, 572), (tx - 62, 520), (tx - 40, 482), (tx + 62, 482), (tx + 62, 572)], RED, 2.6, 4, 0, amp=.4)
    c.shape([(tx - 30, 496), (tx + 48, 496), (tx + 48, 520), (tx - 30, 520)], WHITE, 1.6, amp=.2)
    for i in range(4):
        for j in range(2):
            c.oval(tx - 30 + i * 24, 536 + j * 16, 6, 5, CREAM, 1.2, amp=.1)
    c.two([(tx - 66, 572), (tx + 66, 572), (tx + 66, 560), (tx - 66, 560)], WOOD_DARK, 2, 2, 0, amp=.2)
    c.line([(tx + 62, 500), (tx + 84, 488)], 3, amp=.2); c.dotc(tx + 86, 486, 6, INK)

    # --- the basket, bottom right (picked products show in its mouth around py 866) --------------
    bcx = 1578
    c.sh_oval(bcx + 10, 1062, 220, 14)
    arch = [(bcx - 196, 902)] + [(bcx + math.cos(math.radians(a)) * 196, 902 - math.sin(math.radians(a)) * 150) for a in range(170, 5, -10)] + [(bcx + 196, 902)]
    c.line(arch, 14, amp=.6); c.line(arch, 9, WOOD, amp=.6)
    c.oval(bcx, 902, 204, 34, WOOD_DARK, 2.6)                                                       # mouth
    def lining():
        for i in range(-6, 7):
            if i % 2 == 0:
                c.flat([(bcx + i * 32, 880), (bcx + i * 32 + 32, 880), (bcx + i * 32 + 32, 950), (bcx + i * 32, 950)], RED)
    cl = [(bcx - 190, 914)] + [(bcx - 190 + i * 38, 936 + (10 if i % 2 else -2)) for i in range(11)] + [(bcx + 190, 914)]
    c.flat(cl, WHITE); c.inside(cl, lining); c.stroke(cl, 2)
    body = [(bcx - 204, 906), (bcx - 150, 932), (bcx, 942), (bcx + 150, 932), (bcx + 204, 906), (bcx + 168, 1050), (bcx - 168, 1050)]
    def weave():
        for j in range(7):
            c.line([(bcx - 210, 950 + j * 17), (bcx + 210, 950 + j * 17)], 1.6, WOOD, amp=.6)
        for i in range(24):
            c.line([(bcx - 210 + i * 18, 900), (bcx - 180 + i * 16 + (6 if i % 2 else -6), 1052)], 1.2, WOOD, amp=.3)
    c.two(body, PAL['gold'], 3, 5, 0, amp=.6, inner=weave)
    c.line([(bcx - 204, 906), (bcx - 150, 932), (bcx, 942), (bcx + 150, 932), (bcx + 204, 906)], 6, amp=.5)
    c.line([(bcx - 204, 906), (bcx - 150, 932), (bcx, 942), (bcx + 150, 932), (bcx + 204, 906)], 3, WOOD, amp=.5)

    # --- the shopping list: a notepad leaning in the bottom left corner (the game writes the words on it) ----
    p0, p1, pt = 86, 478, 552
    board = [(p0 - 26, pt - 20), (p1 + 26, pt - 30), (p1 + 40, 1100), (p0 - 36, 1100)]
    c.shadow([(x + 16, y + 14) for x, y in board])
    c.two(board, WOOD, 3, 6, 0)
    paper = [(p0, pt + 12), (p1, pt + 2), (p1 + 10, 1100), (p0 - 8, 1100)]
    def rules():
        for i in range(1, 9):
            c.line([(p0 + 20, pt + 96 + i * 67), (p1 - 14, pt + 94 + i * 67)], 1.4, PAL['ice'])
        c.line([(p0 + 16, pt + 84), (p1 - 12, pt + 80)], 2, PAL['skin'])
    c.two(paper, WHITE, 2.8, 5, 0, inner=rules)
    mx = (p0 + p1) / 2
    c.two([(mx - 70, pt - 34), (mx + 70, pt - 38), (mx + 70, pt + 22), (mx - 70, pt + 26)], PAL['ice'], 2.8, 4, 0)     # clip
    c.oval(mx, pt - 6, 12, 12, PAPER, 2)
    out(c, BG, 'sprite_shop_clean', (1920, 1080))


# --- house rooms (stretched by the game to the 8 x 3.6 placement area) -------------------
def room(name, seed, kitchen):
    c = Scene(1600, 720, 1.5, PAPER if kitchen else '#d3f3e2', seed)
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
    c.shape([(x - w / 2, y - h / 2), (x + w / 2, y - h / 2), (x + w / 2, y + h / 2), (x - w / 2, y + h / 2)], '#8cc6ee', 3.4, amp=.8)
    c.flat([(x - w / 2 + 8, y + h / 2 - 8), (x - w / 2 + 8, y + 20), (x - 20, y - 20), (x + 20, y + 14), (x + w / 2 - 8, y - 10), (x + w / 2 - 8, y + h / 2 - 8)], GRASS)
    c.oval(x + 36, y - 46, 15, 15, GOLD, 2.4)
    c.line([(x, y - h / 2), (x, y + h / 2)], 2.8, amp=.4); c.line([(x - w / 2, y), (x + w / 2, y)], 2.8, amp=.4)
    c.shape([(x - w / 2 - 12, y + h / 2), (x + w / 2 + 12, y + h / 2), (x + w / 2 + 12, y + h / 2 + 14), (x - w / 2 - 12, y + h / 2 + 14)], WHITE, 2.8, amp=.5)
    if kitchen:
        # tiled strip and a rail of hanging tools
        for i in range(12):
            for j in range(2):
                x0, y0 = 80 + i * 62, fl - 150 + j * 62
                c.shape([(x0, y0), (x0 + 62, y0), (x0 + 62, y0 + 62), (x0, y0 + 62)], WHITE if (i + j) % 2 else '#a5e3c0', 1.8, amp=.5, step=20)
        c.line([(120, 150), (760, 150)], 3.2, amp=.8)
        for i, kind in enumerate('spoon pan spoon pan spoon'.split()):
            hx = 190 + i * 125
            c.line([(hx, 150), (hx, 196)], 2.4, amp=.3)
            if kind == 'pan':
                c.oval(hx, 232, 34, 34, ORANGE if i == 1 else '#7a6a62', 2.8)
            else:
                c.oval(hx, 214, 13, 18, WOOD, 2.4)
    else:
        # wallpaper dots, a picture and a rug
        for i in range(9):
            for j in range(3):
                c.tick(520 + i * 120 + (j % 2) * 60, 90 + j * 120, .8, '#a5e3c0')
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
