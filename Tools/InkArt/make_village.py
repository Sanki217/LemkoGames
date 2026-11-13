"""Village map: a busy hand-drawn valley. Building positions match the hotspots in Village.unity."""
import math, sys
from pathlib import Path
from scene import *

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/Lemko/Sprites/Placeholders/Village'
W, H = 1920, 864          # the scene shows this sprite at 0.8 height, so draw it flatter
HUB = (950, 440)
RIVER = [(-40, 236), (60, 290), (116, 372), (76, 462), (44, 552), (112, 644), (192, 724), (232, 796), (252, 900)]


def world(wx, wy):
    return 960 + wx * 120, 348 - wy * 120


def bez(a, m, b, t):
    return ((1 - t) ** 2 * a[0] + 2 * t * (1 - t) * m[0] + t * t * b[0], (1 - t) ** 2 * a[1] + 2 * t * (1 - t) * m[1] + t * t * b[1])


def near_river(x, y, d=52):
    return any(math.hypot(x - rx, y - ry) < d for (ax, ay), (bx, by) in zip(RIVER, RIVER[1:]) for rx, ry in [((ax + bx) / 2, (ay + by) / 2), (ax, ay)])


def ground(c):
    r = c.r
    # soft meadow patches instead of a flat fill
    for i in range(46):
        x, y = r.uniform(-40, W + 40), r.uniform(120, H + 30)
        c.flat(c.lumpy(x, y, r.uniform(70, 190), r.uniform(26, 62), 6, .25), (GRASS_LIGHT, GRASS_DARK, GRASS_LIGHT)[i % 3], .3)
    # distant forest band under the mountains
    for i in range(48):
        x = i * 41 + r.uniform(-8, 8)
        c.flat(c.lumpy(x, 150 + r.uniform(-10, 10), r.uniform(26, 40), r.uniform(16, 26), 5, .3), PINE, .55)


def river(c):
    cr = c.c
    def band(w, color):
        c._path(RIVER); cr.set_source_rgb(*rgb(color)); cr.set_line_width(w); cr.stroke()
    band(86, SAND); band(72, INK); band(62, WATER_DARK); band(50, WATER)
    # lighter current in the middle, ripples, stones and lily pads
    c._path([(x + 6, y) for x, y in RIVER]); cr.set_source_rgba(*rgb(PAL['ice']), .5); cr.set_line_width(14); cr.stroke()
    for x, y in [(50, 300), (96, 404), (62, 506), (92, 612), (166, 698), (226, 780), (236, 842)]:
        c.line([(x - 12, y), (x - 4, y - 4), (x + 4, y), (x + 12, y - 4)], 1.5, WHITE, amp=.3, step=6)
    for x, y in [(108, 352), (58, 560), (150, 676)]:
        c.rock(x, y, 7, STONE)
    for x, y in [(84, 450), (206, 752)]:
        c.oval(x, y, 6, 3.6, LEAF, 1, amp=.1); c.dotc(x + 1, y - 1, 1.6, WHITE)
    for x, y in [(20, 318), (150, 420), (16, 500), (150, 610), (246, 700), (190, 800)]:
        c.reeds(x, y, 4)
    # footbridge with planks and rails
    a, b, wd = (2, 476), (132, 512), 30
    c.shadow([(a[0], a[1] + wd + 4), (b[0], b[1] + wd + 4), (b[0] + 10, b[1] + wd - 8), (a[0] + 10, a[1] + wd - 8)])
    p = [a, b, (b[0] - 8, b[1] + wd), (a[0] - 8, a[1] + wd)]
    c.two(p, WOOD, 2.4, 3, 3)
    for i in range(1, 9):
        t = i / 9
        c.line([(a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t), (a[0] - 8 + (b[0] - a[0]) * t, a[1] + wd + (b[1] - a[1]) * t)], 1.1, WOOD_DARK, amp=.2)
    for off in (0, wd):
        x0, y0, x1, y1 = a[0] - off * .27, a[1] + off, b[0] - off * .27, b[1] + off
        for t in (0, .5, 1):
            px, py = x0 + (x1 - x0) * t, y0 + (y1 - y0) * t
            c.line([(px, py), (px, py - 16)], 2.4, amp=.1)
        c.line([(x0, y0 - 15), (x1, y1 - 15)], 2.6, amp=.4); c.line([(x0, y0 - 15), (x1, y1 - 15)], 1.1, WOOD, amp=.4)
    # a rowing boat tied up below the bridge
    bx, by = 106, 596
    c.two([(bx - 22, by - 6), (bx + 22, by - 2), (bx + 14, by + 9), (bx - 16, by + 6)], RED, 2, 2, 3, amp=.3, step=8)
    c.line([(bx - 12, by - 1), (bx + 12, by + 2)], 1.2, PAPER, amp=.1)


def paths(c, ends):
    cr = c.c
    curves = []
    for x, y in ends:
        mid = ((HUB[0] + x) / 2, (HUB[1] + y) / 2 + 26)
        low = y > HUB[1] + 100                     # places below the square are reached from the side facing it
        end = (x + (70 if x < HUB[0] else -70), y + 18) if low else (x, y + 14)
        curves.append((HUB, mid, end))
    for width, color in ((40, INK), (33, SAND), (25, PAPER)):
        for a, m, b in curves:
            cr.new_path(); cr.move_to(*a); cr.curve_to(*m, *m, *b)
            cr.set_source_rgb(*rgb(color)); cr.set_line_width(width); cr.stroke()
    for a, m, b in curves:
        for i in range(1, 12):
            t = i / 12
            px, py = bez(a, m, b, t)
            if math.hypot(px - HUB[0], py - HUB[1]) < 96:
                continue
            if i % 2:
                c.oval(px + c.r.uniform(-7, 7), py + c.r.uniform(-4, 4), c.r.uniform(3, 5.5), c.r.uniform(2, 3.2), STONE, 1, amp=.2)
            else:
                c.line([(px - 5, py + c.r.uniform(-5, 5)), (px + 4, py + c.r.uniform(-5, 5))], 1, SAND, amp=.2)
    return curves


def plaza(c):
    x, y = HUB[0], HUB[1] + 8
    c.oval(x, y, 132, 56, SAND, 2.4, amp=1)
    c.flat(c.ring(x, y - 3, 124, 50), PAPER)
    for i in range(46):
        a = c.r.uniform(0, 6.28); d = c.r.uniform(.25, .95)
        px, py = x + math.cos(a) * 118 * d, y - 3 + math.sin(a) * 46 * d
        c.oval(px, py, c.r.uniform(5, 8), c.r.uniform(3, 4.4), (STONE, SAND, WHITE)[i % 3], .9, amp=.2)


def table(c):
    x, y = HUB
    c.sh_oval(x + 8, y + 32, 92, 20)
    for dx, dy in [(-62, -6), (52, 16), (30, 34), (-84, 12)]:
        c.line([(x + dx, y + dy), (x + dx, y + dy + 24)], 3.2, amp=.3); c.line([(x + dx, y + dy), (x + dx, y + dy + 24)], 1.4, WOOD, amp=.3)
    top = [(x - 66, y - 16), (x + 58, y + 6), (x + 34, y + 30), (x - 90, y + 8)]
    c.two(top, WOOD, 2.6, 3, 3)
    # a runner, plates, a loaf and a jug on the table
    c.shape([(x - 44, y - 9), (x + 40, y + 6), (x + 30, y + 17), (x - 54, y + 2)], WHITE, 1.4, amp=.3)
    c.line([(x - 46, y - 3), (x + 34, y + 12)], 1.6, RED, amp=.2)
    for dx, dy in ((-40, -3), (-6, 4), (24, 10)):
        c.oval(x + dx, y + dy, 7, 3.6, WHITE, 1.1, amp=.1); c.oval(x + dx, y + dy - 1, 3.6, 2, (GOLD, RED, ORANGE)[(dx + 40) // 30 % 3], .8, amp=.1)
    c.oval(x - 22, y - 4, 6, 4, PAL['gold'], 1.2, amp=.1)
    c.shape([(x + 8, y - 2), (x + 6, y - 14), (x + 14, y - 14), (x + 13, y - 1)], PAL['sky'], 1.3, amp=.1, step=8)
    c.bench(x - 96, y + 40, 46); c.bench(x + 74, y + 54, 44)
    # bunting between two poles
    a, b = (x - 136, y - 84), (x + 118, y - 58)
    c.line([(a[0], y - 6), a], 2.8, amp=.3); c.line([(b[0], y + 26), b], 2.8, amp=.3)
    c.dotc(a[0], a[1] - 3, 4, GOLD); c.dotc(b[0], b[1] - 3, 4, GOLD)
    c.line([a, ((a[0] + b[0]) / 2, a[1] + 28), b], 1.6, amp=.5)
    for i in range(1, 11):
        t = i / 11
        fx = a[0] + (b[0] - a[0]) * t
        fy = a[1] + (b[1] - a[1]) * t + 28 * 4 * t * (1 - t) * .88
        c.shape([(fx - 7, fy), (fx + 7, fy + 2), (fx, fy + 15)], (GOLD, WHITE, RED, PAL['sky'], ORANGE)[i % 5], 1.6, amp=.3, step=8)


# ---- the eight places ---------------------------------------------------------------------
def shop(c, x, y):
    c.house(x, y, 128, 78, 56, roof=LEAF_DARK, wall=PAPER, windows=0, door=True, door_fill=RED)
    for s in (-1, 1):
        c.pane(x + s * 40, y - 34, 26, 24, PAL['pale'], box=False)
    # striped awning with a scalloped edge, and a hanging sign
    for i in range(7):
        x0 = x - 70 + i * 20
        c.shape([(x0 + 4, y - 60), (x0 + 24, y - 60), (x0 + 20, y - 44), (x0 + 10, y - 39), (x0, y - 44)], RED if i % 2 == 0 else WHITE, 1.8, amp=.3, step=10)
    c.shape([(x - 22, y - 84), (x + 22, y - 84), (x + 22, y - 68), (x - 22, y - 68)], GOLD, 1.8, amp=.3)
    c.line([(x - 14, y - 76), (x + 14, y - 76)], 1.6, amp=.2)
    c.crate(x - 88, y + 14, (RED, GOLD, ORANGE)); c.crate(x - 58, y + 22, (LEAF, LIME, LEAF), .9)
    c.barrel(x + 86, y + 20); c.barrel(x + 108, y + 12, .85)
    for i, col in enumerate((ORANGE, PAL['gold'], ORANGE)):
        c.oval(x - 112 + i * 13, y + 30 - i * 2, 6.5, 5.5, col, 1.3, amp=.2)       # pumpkins
    c.person(x + 44, y + 22, PAL['pale'], WOOD_DARK, .95, skirt=True)
    c.flowers(x - 20, y + 30, 5, 14)


def farm(c, x, y):
    c.field(x + 150, y - 70, 84, 44, GOLD, 5)
    c.field(x - 214, y - 46, 80, 46, LIME, 5, crop=LEAF_DARK)
    a = c.house(x, y, 136, 72, 62, roof=NAVY, wall=RED, side=WOOD_DARK, planks=True, windows=0, door=False, roof_h=66, glass=PAPER)
    # big double barn door with cross braces and white trim
    c.shape([(x - 30, y), (x - 30, y - 50), (x + 30, y - 50), (x + 30, y)], PAPER, 2.4, amp=.3)
    for s in (-1, 1):
        c.line([(x + s * 28, y - 48), (x + s * 2, y - 2)], 1.6, amp=.2); c.line([(x + s * 2, y - 48), (x + s * 28, y - 2)], 1.6, amp=.2)
    c.line([(x, y - 50), (x, y)], 2, amp=.1)
    # silo behind the barn
    sx, sy = x + 112, y - 34
    c.two([(sx - 17, sy + 30), (sx - 17, sy - 50), (sx + 17, sy - 50), (sx + 17, sy + 30)], STONE, 2.4, 5, 0)
    for yy in (sy - 30, sy - 8, sy + 12):
        c.line([(sx - 16, yy), (sx + 16, yy)], 1.1, amp=.2)
    c.two([(sx - 20, sy - 48), (sx, sy - 72), (sx + 20, sy - 48)], RED, 2.2, 3, 3)
    c.haybale(x + 100, y + 30, 20); c.haybale(x + 134, y + 22, 16)
    c.fence(x + 60, y + 70, x + 214, y + 44, 8)
    c.fence(x + 214, y + 44, x + 236, y - 6, 3)
    c.cow(x + 150, y + 4, .8); c.sheep(x + 190, y + 26, .85); c.sheep(x + 118, y + 78, .8, PAPER)
    c.hen(x - 58, y + 24, .9); c.hen(x - 40, y + 34, .8, PAL['wood']); c.hen(x + 44, y + 28, .8)
    c.cart(x - 104, y - 8)
    c.person(x + 66, y + 44, PAL['sky'], PAL['gold'], .95, hat=PAL['sand'])


def cottage(c, x, y):
    c.garden(x - 150, y + 44, 64, 34, LEAF)
    c.house(x, y, 118, 74, 54, roof=ORANGE, wall=WHITE, chimney=True, shutters=PAL['blue'])
    c.fence(x - 88, y + 30, x - 28, y + 40, 4, WHITE); c.fence(x + 30, y + 40, x + 96, y + 26, 4, WHITE)
    c.bush(x - 78, y + 6, 14, RED); c.bush(x + 100, y - 4, 12, WHITE)
    c.flowers(x - 50, y + 22, 7, 20); c.flowers(x + 56, y + 20, 6, 18)
    c.laundry(x + 116, y - 22, x + 176, y - 36)
    c.well(x + 150, y + 44)
    c.logs(x - 118, y + 6)
    c.person(x + 28, y + 18, RED, PAL['grey2'], .92, skirt=True)


def family(c, x, y):
    c.house(x - 12, y, 106, 70, 50, roof=NAVY, wall=PAPER, shutters=RED, chimney=True)
    # the big family tree with golden fruit and a swing
    tx, ty = x + 108, y + 22
    c.tree(tx, ty, 44, LEAF, fruit=GOLD)
    c.line([(tx - 26, ty - 62), (tx - 26, ty - 22)], 1.2, amp=.2); c.line([(tx - 12, ty - 60), (tx - 12, ty - 22)], 1.2, amp=.2)
    c.shape([(tx - 30, ty - 22), (tx - 8, ty - 22), (tx - 8, ty - 18), (tx - 30, ty - 18)], WOOD, 1.4, amp=.1, step=10)
    c.flag(x - 12, y - 128, 30, RED)
    c.bench(x - 96, y + 22, 38)
    c.flowers(x - 40, y + 26, 8, 22); c.flowers(tx + 30, ty + 10, 6, 16)
    c.fence(x - 120, y + 46, x - 30, y + 56, 5)
    c.person(x + 30, y + 22, PAL['mint'], WOOD_DARK, .9); c.person(x + 50, y + 30, GOLD, PAL['gold'], .7, skirt=True)
    c.beehive(x + 170, y + 34); c.beehive(x + 194, y + 26)


def church(c, x, y):
    """Memory: a small wooden church with a shingled tower, in the Carpathian style."""
    c.house(x + 20, y, 104, 62, 52, roof=WOOD_DARK, wall=WOOD, windows=1, roof_h=54, planks=True, box=False, glass=GOLD, door=False)
    tx = x - 46
    c.shadow([(tx - 30, y + 6), (tx + 30, y + 12), (tx + 54, y + 2)])
    body = [(tx - 27, y + 4), (tx - 20, y - 96), (tx + 20, y - 96), (tx + 27, y + 4)]
    def boards():
        for i in range(1, 8):
            c.line([(tx - 28, y - i * 13), (tx + 28, y - i * 13)], 1.2, WOOD_DARK, amp=.3)
        for i in range(-2, 3):
            c.line([(tx + i * 10, y - 96), (tx + i * 12, y + 4)], .9, WOOD_DARK, amp=.2)
    c.two(body, WOOD, 2.8, 7, 0, inner=boards)
    c.two([(tx - 34, y - 62), (tx - 24, y - 72), (tx + 24, y - 72), (tx + 34, y - 62)], WOOD_DARK, 2, 3, 3)       # skirt roof
    c.two([(tx - 30, y - 94), (tx, y - 138), (tx + 30, y - 94)], PLUM, 2.8, 5, 4)
    c.two(c.ring(tx, y - 146, 11, 12), GOLD, 2.2, 3, 3, amp=.4, step=7)
    c.line([(tx, y - 158), (tx, y - 180)], 2.4, amp=.2); c.line([(tx - 7, y - 171), (tx + 7, y - 171)], 2.4, amp=.2)
    c.door(tx, y + 4, 18, 30, PLUM)
    c.shape([(tx - 6, y - 40), (tx + 6, y - 40), (tx + 6, y - 56), (tx, y - 62), (tx - 6, y - 56)], GOLD, 1.8, amp=.2)
    # graveyard-free churchyard: stone wall, lanterns and flowers
    for i in range(6):
        c.rock(x - 110 + i * 15, y + 34 + i * 3, 8, STONE)
    c.lamp(x - 92, y + 18, 34); c.lamp(x + 96, y + 30, 34)
    c.flowers(x - 10, y + 26, 8, 26, (WHITE, GOLD, WHITE))
    c.bush(x + 120, y + 4, 13)
    c.person(x + 4, y + 16, PLUM, PAL['silver'], .9, skirt=True)


def numbers(c, x, y):
    c.house(x, y, 120, 72, 54, roof=WATER_DARK, wall=WHITE, shutters=PAL['gold'])
    # hanging number sign
    sx, sy = x - 96, y - 56
    c.line([(sx, y + 14), (sx, sy - 36)], 3, amp=.3); c.line([(sx, sy - 30), (sx + 38, sy - 30)], 2.6, amp=.3)
    c.line([(sx + 12, sy - 30), (sx + 12, sy - 22)], 1.2, amp=0); c.line([(sx + 40, sy - 30), (sx + 40, sy - 22)], 1.2, amp=0)
    c.two([(sx + 4, sy - 22), (sx + 50, sy - 22), (sx + 50, sy + 10), (sx + 4, sy + 10)], GOLD, 2.2, 3, 3, amp=.4)
    cr = c.c; cr.set_source_rgb(*rgb(INK)); cr.select_font_face('DejaVu Sans', cairo.FONT_SLANT_NORMAL, cairo.FONT_WEIGHT_BOLD)
    cr.set_font_size(20); cr.move_to(sx + 7, sy + 1); cr.show_text('123')
    # market stall with numbered tickets
    mx, my = x + 118, y + 26
    c.sh_oval(mx + 6, my + 4, 40, 8)
    for dx in (-30, 30):
        c.line([(mx + dx, my), (mx + dx, my - 56)], 2.4, amp=.2)
    for i in range(5):
        x0 = mx - 36 + i * 14.4
        c.shape([(x0, my - 62), (x0 + 14.4, my - 62), (x0 + 14.4, my - 48), (x0 + 7.2, my - 43), (x0, my - 48)], PAL['blue'] if i % 2 == 0 else WHITE, 1.6, amp=.2, step=10)
    c.two([(mx - 32, my - 6), (mx - 32, my - 24), (mx + 32, my - 24), (mx + 32, my - 6)], WOOD, 2, 3, 0, amp=.3)
    for i, n in enumerate('147'):
        c.shape([(mx - 24 + i * 17, my - 40), (mx - 10 + i * 17, my - 40), (mx - 10 + i * 17, my - 26), (mx - 24 + i * 17, my - 26)], (WHITE, GOLD, ORANGE)[i], 1.3, amp=.2, step=10)
        cr.set_source_rgb(*rgb(INK)); cr.set_font_size(11); cr.move_to(mx - 21 + i * 17, my - 29); cr.show_text(n)
    c.person(mx + 4, my + 20, PAL['blue'], WOOD_DARK, .9, hat=PAL['sand'])
    c.barrel(x - 60, y + 22, .85)
    c.flowers(x + 40, y + 24, 6, 18)


def school(c, x, y):
    a = c.house(x, y, 138, 76, 58, roof=RED, wall=GOLD, roof_h=56, shutters=WHITE, glass=PAL['pale'])
    # bell turret on the ridge
    bx, by = x + 28, a['apex'][1] - 4
    c.two([(bx - 12, by + 12), (bx - 12, by - 20), (bx + 12, by - 20), (bx + 12, by + 8)], WHITE, 2.2, 4, 0, amp=.4)
    c.two([(bx - 17, by - 20), (bx, by - 42), (bx + 17, by - 20)], RED, 2.2, 3, 3, amp=.4)
    c.oval(bx, by - 6, 5, 6, PAL['gold'], 1.5)
    # letter board on an easel
    sx, sy = x - 108, y - 30
    c.line([(sx - 16, y + 20), (sx - 10, sy - 4)], 2.6, amp=.3); c.line([(sx + 22, y + 24), (sx + 16, sy - 4)], 2.6, amp=.3)
    c.two([(sx - 26, sy - 32), (sx + 30, sy - 28), (sx + 30, sy + 12), (sx - 26, sy + 8)], PINE_DARK, 2.4, 0, 0, amp=.5, dark=PINE_DARK)
    cr = c.c; cr.set_source_rgb(*rgb(WHITE)); cr.select_font_face('DejaVu Sans', cairo.FONT_SLANT_NORMAL, cairo.FONT_WEIGHT_BOLD)
    cr.set_font_size(24); cr.move_to(sx - 18, sy + 1); cr.show_text('Аб')
    c.flag(x - 52, a['apex'][1] + 28, 34, PAL['blue'], right=False)
    c.fence(x + 76, y + 44, x + 150, y + 30, 5)
    c.person(x - 64, y + 30, RED, WOOD_DARK, .72); c.person(x - 44, y + 36, PAL['sky'], PAL['gold'], .7, skirt=True); c.person(x + 34, y + 24, PAL['mauve'], PLUM, .92, skirt=True)
    c.bush(x + 104, y + 12, 13, GOLD)
    c.flowers(x - 4, y + 30, 6, 18)


def windmill(c, x, y):
    """Weather: a windmill with a weather vane and a little weather station."""
    c.field(x + 70, y + 70, 80, 40, GOLD, 5)
    c.sh_oval(x + 16, y + 6, 66, 16)
    body = [(x - 36, y + 4), (x - 22, y - 98), (x + 22, y - 98), (x + 36, y + 4)]
    def bricks():
        for i in range(1, 8):
            yy = y - i * 13
            c.line([(x - 40, yy), (x + 40, yy)], 1.1, STONE, amp=.3)
            for k in range(-3, 4):
                c.line([(x + k * 12 + (i % 2) * 6, yy), (x + k * 12 + (i % 2) * 6, yy + 13)], .9, STONE, amp=0)
        c.flat([(x - 40, y + 6), (x + 40, y + 6), (x + 40, y - 9), (x - 40, y - 9)], TONE2)
    c.two(body, WHITE, 2.8, 9, 0, inner=bricks)
    c.two([(x - 28, y - 96), (x, y - 132), (x + 28, y - 96)], RED, 2.8, 5, 4)
    c.door(x, y + 4, 18, 32, WOOD_DARK)
    c.pane(x, y - 60, 12, 14, GOLD, box=False)
    hx, hy = x, y - 102
    for k in range(4):
        a = .5 + k * math.pi / 2
        ux, uy = math.cos(a), math.sin(a); vx, vy = -uy, ux
        p = [(hx + ux * 16, hy + uy * 16), (hx + ux * 78, hy + uy * 78), (hx + ux * 78 + vx * 21, hy + uy * 78 + vy * 21), (hx + ux * 24 + vx * 21, hy + uy * 24 + vy * 21)]
        c.shape(p, PAPER, 2, amp=.4)
        for t in (.3, .44, .58, .72, .86):
            c.line([(hx + ux * 78 * t, hy + uy * 78 * t), (hx + ux * 78 * t + vx * 21, hy + uy * 78 * t + vy * 21)], 1.1, WOOD, amp=.2)
        c.line([(hx + ux * 24 + vx * 10.5, hy + uy * 24 + vy * 10.5), (hx + ux * 78 + vx * 10.5, hy + uy * 78 + vy * 10.5)], 1.1, WOOD, amp=.2)
        c.line([(hx, hy), (hx + ux * 80, hy + uy * 80)], 2.8, amp=.4)
    c.oval(hx, hy, 6, 6, GOLD, 1.6)
    # weather vane, a rain gauge and sacks of flour
    c.line([(x, y - 132), (x, y - 150)], 1.8, amp=.1); c.shape([(x - 12, y - 152), (x + 10, y - 148), (x - 2, y - 143)], GOLD, 1.4, amp=.2, step=8)
    for i in range(3):
        c.two(c.lumpy(x + 56 + i * 15, y + 14 + i * 3, 9, 11, 4, .15), PAPER, 1.6, 2, 2, amp=.3, step=7)
    c.person(x - 62, y + 18, WHITE, PAL['gold'], .9, hat=PAL['sand'])
    c.flowers(x - 20, y + 30, 6, 20)


def main():
    c = Scene(W, H, scale=2, bg=GRASS, seed=11, ysquash=1.25)
    r = c.r
    ground(c)
    spots = {k: world(*v) for k, v in dict(Shop=(-5.083, .233), FamilyTree=(4.333, -.033), Farm=(-3, .967), House=(2.25, 1.1),
                                           Memory=(-4.5, -2.033), Numbers=(-1.417, -1.967), Alphabet=(2.917, -2.033),
                                           Weather=(5.583, -1.767)).items()}
    # building ground line sits a little under the hotspot centre
    base = {k: (x + 24, y + 42) for k, (x, y) in spots.items()}
    river(c)
    curves = paths(c, list(base.values()))
    plaza(c)

    def clear(x, y, pad=1.0):
        """True when a spot is free of buildings, the square, the river and the paths."""
        if any(abs(x - bx) < 250 * pad and -190 * pad < y - by < 100 * pad for bx, by in base.values()):
            return False
        if math.hypot((x - HUB[0]) / 175, (y - HUB[1]) / 105) < pad or near_river(x, y, 62 * pad):
            return False
        return all(math.hypot(x - px, y - py) > 34 * pad for a, m, b in curves for px, py in (bez(a, m, b, t / 8) for t in range(9)))

    # meadow marks and flower patches go under everything else
    for i in range(170):
        x, y = r.uniform(10, W - 10), r.uniform(176, H - 6)
        if clear(x, y, .8):
            (c.tuft(x, y, r.uniform(.7, 1.1)) if i % 4 else c.tick(x, y, .9, PINE))
    for i in range(70):
        x, y = r.uniform(30, W - 30), r.uniform(190, H - 14)
        if clear(x, y, .9):
            c.flowers(x, y, r.randint(4, 9), r.uniform(12, 26), ((WHITE, GOLD), (RED, ORANGE, GOLD), (WHITE, MAUVE), BLOOM)[i % 4])

    def big(f, x, y, k):
        """Draw something enlarged around its foot point: places and people are chunky, not map symbols."""
        cr = c.c; cr.save(); cr.translate(x, y); cr.scale(k, k); cr.translate(-x, -y); f(); cr.restore()

    things = []                       # (sort y, draw function)
    def add(y, f): things.append((y, f))

    # mountain range along the top, taller towards the corners
    for x, y, w, h in [(110, 96, 190, 150), (250, 120, 170, 170), (30, 150, 150, 130), (395, 86, 150, 120), (560, 70, 140, 96), (720, 80, 150, 110),
                       (880, 78, 170, 118), (1010, 96, 140, 100), (1180, 74, 150, 104), (1340, 84, 160, 116), (1560, 92, 180, 140),
                       (1700, 126, 190, 180), (1850, 112, 170, 150), (1830, 190, 150, 130)]:
        add(y - 400, lambda x=x, y=y, w=w, h=h: c.mountain(x, y, w, h))
    # pine woods, thick along the edges
    woods = [(430, 160, 7), (640, 150, 6), (770, 140, 6), (1120, 130, 6), (1300, 140, 5), (1420, 150, 6), (1820, 330, 8), (1790, 760, 9), (1500, 800, 6),
             (1090, 806, 7), (640, 800, 7), (330, 806, 7), (300, 440, 4), (1040, 296, 3), (1700, 430, 4), (60, 806, 4), (300, 196, 5), (1600, 240, 5),
             (1250, 300, 3), (860, 800, 4), (1330, 806, 4)]
    for wx, wy, n in woods:
        for i in range(n):
            if i % 2:
                continue
            x = wx + (i % 4) * 36 + r.uniform(-10, 10) + (i // 4) * 26
            y = wy + (i // 4) * 34 + r.uniform(-8, 8)
            if clear(x, y, .62) or y < 180:
                add(y, lambda x=x, y=y, h=r.uniform(66, 92): c.pine(x, y, h))
    for i, (x, y) in enumerate([(560, 420), (1140, 476), (1400, 250), (1560, 660), (990, 262), (240, 250), (1250, 720), (700, 690), (180, 400),
                                (520, 250), (1180, 220), (760, 330), (1660, 560), (420, 640), (1010, 700), (1480, 420)]):
        if clear(x, y, .7):
            add(y, lambda x=x, y=y, i=i, rr=r.uniform(27, 36): (c.birch(x, y, rr) if i % 4 == 3 else c.tree(x, y, rr, (LEAF, LIME, LEAF)[i % 3], fruit=RED if i % 5 == 1 else None)))
    for i in range(40):
        x, y = r.uniform(40, W - 40), r.uniform(200, H - 20)
        if clear(x, y, .85):
            kind = i % 5
            add(y, lambda x=x, y=y, kind=kind: (c.bush(x, y, r.uniform(10, 15), (None, RED, WHITE)[kind % 3]) if kind < 3 else c.rock(x, y, r.uniform(7, 12))))
    # a duck pond, a hay meadow and odds and ends between the places
    add(300, lambda: (c.pond(900, 318, 52, 22), c.duck(886, 314), c.duck(916, 322, .8), c.duck(960, 336, .9)))
    add(254, lambda: c.haybale(1460, 300, 18)); add(262, lambda: c.haybale(1500, 316, 15))
    add(640, lambda: c.cart(1218, 560, None))
    add(470, lambda: c.signpost(HUB[0] + 150, HUB[1] + 34)); add(430, lambda: c.lamp(HUB[0] - 150, HUB[1] + 24)); add(500, lambda: c.lamp(HUB[0] + 100, HUB[1] + 78))
    add(470, lambda: c.well(HUB[0] - 196, HUB[1] + 70))
    add(560, lambda: big(lambda: c.sheep(480, 560, .8), 480, 560, 1.5)); add(574, lambda: big(lambda: c.sheep(520, 576, .7, PAPER), 520, 576, 1.5))
    add(516, lambda: big(lambda: c.person(HUB[0] - 60, HUB[1] + 70, PAL['sky'], WOOD_DARK, .92), HUB[0] - 60, HUB[1] + 70, 1.5)); add(520, lambda: big(lambda: c.person(HUB[0] + 36, HUB[1] + 82, RED, PAL['gold'], .9, skirt=True), HUB[0] + 36, HUB[1] + 82, 1.5))
    add(380, lambda: big(lambda: c.person(1240, 396, GOLD, PLUM, .85), 1240, 396, 1.5)); add(600, lambda: big(lambda: c.person(660, 548, PAL['mint'], PAL['grey2'], .85, hat=PAL['sand']), 660, 548, 1.5))

    draw = dict(Shop=shop, Farm=farm, House=cottage, FamilyTree=family, Memory=church, Numbers=numbers, Alphabet=school, Weather=windmill)
    for k, (x, y) in base.items():
        add(y, lambda k=k, x=x, y=y: big(lambda: draw[k](c, x, y), x, y + 20, 1.3))
    add(HUB[1] + 10, lambda: table(c))
    for _, f in sorted(things, key=lambda t: t[0]):
        f()
    out = sys.argv[1] if len(sys.argv) > 1 else str(OUT / 'sprite_village_map.png')
    c.save(out, (1920, 1080))
    # hover ring: a hand-drawn golden loop
    ring = Canvas(128, 64, scale=4, bg=None, seed=3)
    ring.oval(64, 32, 56, 24, None, 5, GOLD, amp=.5)
    if len(sys.argv) <= 1:
        ring.save(str(OUT / 'sprite_location_ring.png'), (128, 64))


main()
