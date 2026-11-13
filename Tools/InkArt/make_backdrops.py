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
    """A whole shop: wall shelf and market table on the left, a tall rack, the shopkeeper behind the counter,
    the shopping list hanging on the right and the basket on the floor. World (wx, wy) is at 960 + wx*120, 540 - wy*120."""
    c = Scene(1920, 1080, 1.5, PAPER, 31)
    r = c.r
    FLOOR = 800
    for i in range(0, 46):
        c.flat([(i * 44, 0), (i * 44 + 16, 0), (i * 44 + 16, FLOOR), (i * 44, FLOOR)], SAND, .3)
    # floorboards
    c.flat([(0, FLOOR), (1920, FLOOR), (1920, 1080), (0, 1080)], TONE1)
    c.flat([(0, FLOOR), (1920, FLOOR), (1920, FLOOR + 26), (0, FLOOR + 26)], dk(TONE1), .6)
    for i in range(-8, 26):
        x = 960 + (i - 8) * 150
        a, b = (960 + (x - 960) * .86, FLOOR + 4), (960 + (x - 960) * 1.4, 1080)
        c.line([a, b], 2, WOOD, amp=.8)
        t = r.uniform(.2, .8); px, py = a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t
        c.line([(px, py), (px + 120, py)], 1.4, WOOD, amp=.4)
    c.flat([(0, FLOOR - 14), (1920, FLOOR - 14), (1920, FLOOR + 4), (0, FLOOR + 4)], WOOD_DARK)
    c.line([(0, FLOOR - 14), (1920, FLOOR - 14)], 2.4, amp=1); c.line([(0, FLOOR + 4), (1920, FLOOR + 4)], 3, amp=1)

    # hanging lamps and strings of garlic and sausages along the ceiling (the middle stays free for the message)
    for x in (150, 520, 1420, 1790):
        c.line([(x, 0), (x, 150)], 2, amp=.2)
        c.two([(x - 34, 196), (x - 14, 150), (x + 14, 150), (x + 34, 196)], PAL['gold'], 2.4, 3, 0, amp=.4); c.oval(x, 200, 12, 8, GOLD, 1.8)
    c.line([(230, 110), (330, 136), (430, 110)], 1.8, amp=.6)
    for i in range(7):
        t = (i + .5) / 7; x = 230 + 200 * t; y = 110 + 26 * 4 * t * (1 - t) * .95
        c.two(c.lumpy(x, y + 16, 9, 12, 4, .2), (WHITE, WHITE, RED)[i % 3], 1.6, 2, 2, amp=.3, step=7)

    # --- wall shelf (products stand at py 354, on the board at 426) ----------------------------
    x0, x1 = 54, 726
    c.shadow([(x0 + 8, 446), (x1 + 8, 446), (x1 + 8, 460), (x0 + 8, 460)])
    back = [(x0 + 14, 262), (x1 - 14, 262), (x1 - 14, 430), (x0 + 14, 430)]
    def panel():
        for i in range(1, 10):
            c.line([(x0 + i * 70, 262), (x0 + i * 70, 430)], 1.4, dk(PAL['pale']), amp=.5)
    c.two(back, PAL['pale'], 2.6, 4, 4, inner=panel)
    c.two([(x0, 426), (x1, 426), (x1, 446), (x0, 446)], WOOD, 3, 3, 0, amp=.8)
    c.two([(x0 + 6, 250), (x1 - 6, 250), (x1 - 6, 266), (x0 + 6, 266)], WOOD_DARK, 2.6, 3, 0, amp=.8)
    for x in (x0 + 40, (x0 + x1) / 2, x1 - 40):
        c.shape([(x - 8, 446), (x + 8, 446), (x + 8, 470), (x, 486), (x - 8, 470)], WOOD_DARK, 2, amp=.3)
    # --- market table with a striped cloth (products stand at py 588) -------------------------
    t0, t1 = 44, 736
    c.sh_oval(390, 812, 360, 16)
    for x in (t0 + 40, t1 - 40, t0 + 110, t1 - 110):
        c.two([(x - 9, 700), (x + 9, 700), (x + 7, 806), (x - 7, 806)], WOOD_DARK, 2.4, 3, 0, amp=.4)
    top = [(t0 + 46, 628), (t1 - 46, 628), (t1, 690), (t0, 690)]
    c.two(top, WOOD, 3, 4, 4, inner=lambda: [c.line([(t0 + 60 + i * 90, 628), (t0 + 30 + i * 100, 690)], 1.4, dk(WOOD), amp=.4) for i in range(8)])
    cloth = [(t0, 690), (t1, 690), (t1, 728), (t0, 728)]
    def stripes():
        for i in range(18):
            if i % 2 == 0:
                c.flat([(t0 + i * 40, 686), (t0 + i * 40 + 40, 686), (t0 + i * 40 + 40, 760), (t0 + i * 40, 760)], RED)
    scallop = [(t0, 690), (t1, 690)] + [(t1 - i * 20, 722 + (8 if i % 2 else 0)) for i in range(0, int((t1 - t0) / 20) + 1)]
    c.flat(scallop, WHITE); c.inside(scallop, stripes); c.stroke(scallop, 2.4)
    # --- tall rack (products stand at py 354 and 588) -------------------------------------------
    r0, r1 = 752, 988
    c.sh_oval(876, 806, 140, 12)
    rack = [(r0, 232), (r1, 232), (r1, 796), (r0, 796)]
    def inside_rack():
        c.flat([(r0 + 16, 248), (r1 - 16, 248), (r1 - 16, 780), (r0 + 16, 780)], PAL['cream'])
        for i in range(1, 6):
            c.line([(r0 + 16 + i * 34, 248), (r0 + 16 + i * 34, 780)], 1.2, SAND, amp=.4)
    c.two(rack, WOOD, 3, 5, 0, inner=inside_rack)
    c.stroke([(r0 + 16, 248), (r1 - 16, 248), (r1 - 16, 780), (r0 + 16, 780)], 1.8)
    for y in (426, 660):
        c.two([(r0 + 8, y), (r1 - 8, y), (r1 - 8, y + 18), (r0 + 8, y + 18)], WOOD, 2.6, 3, 0, amp=.6)
    c.two([(r0 - 10, 214), (r1 + 10, 214), (r1 + 10, 236), (r0 - 10, 236)], WOOD_DARK, 2.6, 3, 0, amp=.6)
    # jars on top of the rack, baskets on its bottom shelf
    for i, col in enumerate((RED, GOLD, PAL['lime'], ORANGE)):
        jx = r0 + 30 + i * 54
        c.two([(jx - 16, 214), (jx - 18, 176), (jx + 18, 176), (jx + 16, 214)], col, 2, 3, 0, amp=.3); c.shape([(jx - 14, 176), (jx + 14, 176), (jx + 14, 166), (jx - 14, 166)], WHITE, 1.6, amp=.2)
    for i in range(2):
        bx = r0 + 70 + i * 100
        for k in range(3):
            c.oval(bx - 16 + k * 16, 716, 11, 11, (RED, LIME, ORANGE)[(i + k) % 3], 1.6, amp=.2)
        c.two([(bx - 40, 722), (bx + 40, 722), (bx + 32, 778), (bx - 32, 778)], PAL['gold'], 2.2, 3, 0, amp=.4, inner=lambda bx=bx: c.hatch(bx - 40, 726, bx + 40, 778, gap=10, w=1.2, color=WOOD))
    # --- the basket on the floor (picked products are shown along it, around py 912) -----------
    b0, b1 = 96, 1000
    c.sh_oval(548, 1004, 470, 14)
    wick = [(b0 - 10, 906), (b1 + 10, 906), (b1 - 16, 996), (b0 + 16, 996)]
    def weave():
        for j in range(5):
            c.line([(b0 - 10, 918 + j * 17), (b1 + 10, 918 + j * 17)], 1.6, WOOD, amp=.6)
        for i in range(48):
            c.line([(b0 + i * 20, 906), (b0 + i * 20 + (6 if i % 2 else -6), 996)], 1.2, WOOD, amp=.3)
    c.two(wick, PAL['gold'], 3, 4, 0, inner=weave)
    c.two([(b0 - 22, 892), (b1 + 22, 892), (b1 + 18, 914), (b0 - 18, 914)], WOOD, 2.6, 3, 0, amp=.6)
    for x in (b0 - 6, b1 + 6):
        c.line([(x, 892), (x + (-26 if x < 500 else 26), 850), (x + (-10 if x < 500 else 10), 826)], 4.5, amp=.5); c.line([(x, 892), (x + (-26 if x < 500 else 26), 850), (x + (-10 if x < 500 else 10), 826)], 2, WOOD, amp=.5)

    # --- the shopkeeper behind the counter ----------------------------------------------------------
    kx = 1104
    c.two([(kx - 66, 760), (kx - 56, 596), (kx - 20, 572), (kx + 20, 572), (kx + 56, 596), (kx + 66, 760)], PAL['sage'], 3, 5, 0)       # shirt
    c.two([(kx - 40, 760), (kx - 34, 610), (kx + 34, 610), (kx + 40, 760)], WHITE, 2.4, 4, 0)                                            # apron
    c.line([(kx - 34, 612), (kx - 12, 574)], 2, amp=.2); c.line([(kx + 34, 612), (kx + 12, 574)], 2, amp=.2)
    c.two([(kx - 12, 580), (kx + 12, 580), (kx + 10, 556), (kx - 10, 556)], SKIN, 2, 2, 0, amp=.2)
    c.two(c.ring(kx, 512, 44, 46), SKIN, 3, 5, 5, amp=.5, step=8)
    c.flat([(kx - 44, 506), (kx - 40, 474), (kx - 14, 458), (kx + 16, 458), (kx + 40, 474), (kx + 44, 506), (kx + 30, 486), (kx, 478), (kx - 30, 488)], WOOD_DARK)
    c.line([(kx - 44, 506), (kx - 40, 474), (kx - 14, 458), (kx + 16, 458), (kx + 40, 474), (kx + 44, 506)], 2.6, amp=.5)
    c.dotc(kx - 15, 514, 4.2, INK); c.dotc(kx + 15, 514, 4.2, INK)
    c.flat(c.ring(kx - 26, 530, 8, 5), ORANGE, .7); c.flat(c.ring(kx + 26, 530, 8, 5), ORANGE, .7)
    c.line([(kx - 12, 532), (kx, 540), (kx + 12, 532)], 2.4, amp=.2, step=6)
    c.line([(kx - 20, 524), (kx - 6, 528)], 3, WOOD_DARK, amp=.1); c.line([(kx + 20, 524), (kx + 6, 528)], 3, WOOD_DARK, amp=.1)        # moustache
    # counter with a bell
    c0 = 1020
    c.sh_oval(1470, 1052, 450, 12)
    front = [(c0, 772), (1920, 772), (1920, 1046), (c0, 1046)]
    def panels():
        for i in range(6):
            px = c0 + 24 + i * 150
            c.shape([(px, 800), (px + 126, 800), (px + 126, 1020), (px, 1020)], WOOD, 1.6, dk(WOOD), amp=.5, step=30)
    c.two(front, WOOD, 3, 6, 0, inner=panels)
    c.two([(c0 - 16, 744), (1920, 744), (1920, 776), (c0 - 16, 776)], WOOD_DARK, 3, 3, 0, amp=.8)
    c.two([(1140, 744), (1146, 722), (1176, 722), (1182, 744)], GOLD, 2.2, 3, 0, amp=.3); c.dotc(1161, 718, 5, INK)       # bell

    # --- the shopping list hanging on the wall (the game writes the words on it) ---------------
    p0, p1, py0, py1 = 1196, 1880, 258, 722
    c.shadow([(p0 + 12, py1 + 12), (p1 + 12, py1 + 12), (p1 + 12, py0 + 20), (p1, py0 + 20), (p1, py1), (p0 + 12, py1)])
    c.two([(p0 - 14, py0 - 6), (p1 + 14, py0 - 6), (p1 + 14, py0 + 22), (p0 - 14, py0 + 22)], WOOD_DARK, 2.6, 3, 0, amp=.6)
    paper = [(p0, py0 + 18), (p1, py0 + 18), (p1, py1 - 14), (p1 - 24, py1), (p0 + 30, py1 - 6), (p0, py1 - 16)]
    def rules():
        for i in range(1, 9):
            c.line([(p0 + 30, py0 + 60 + i * 50), (p1 - 30, py0 + 60 + i * 50)], 1.2, PAL['ice'], amp=.5)
        c.line([(p0 + 62, py0 + 20), (p0 + 62, py1)], 1.4, PAL['skin'], amp=.4)
    c.two(paper, WHITE, 2.8, 4, 5, inner=rules)
    c.line([((p0 + p1) / 2, py0 - 6), ((p0 + p1) / 2, py0 - 44)], 2, amp=.2); c.dotc((p0 + p1) / 2, py0 - 46, 7, RED)
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
