"""Village map in ink-on-paper style. Building positions match the hotspots in Village.unity."""
import math, sys
from pathlib import Path
from ink import *

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/Lemko/Sprites/Placeholders/Village'
W, H = 1920, 864          # the scene shows this sprite at 0.8 height, so draw it flatter


def world(wx, wy):
    return 960 + wx * 120, 348 - wy * 120


def ground(c):
    cr = c.c
    # a few darker and lighter diamond tiles, then the faint tile grid
    step = 240
    r = c.r
    for i in range(-6, 14):
        for j in range(-6, 14):
            cx = (i - j) * step / 2 + 960
            cy = (i + j) * step / 4
            if not (-200 < cx < W + 200 and -100 < cy < H + 100):
                continue
            k = r.random()
            if k < .16 or k > .9:
                c.flat([(cx, cy - step / 4), (cx + step / 2, cy), (cx, cy + step / 4), (cx - step / 2, cy)],
                       GRASS_DARK if k < .16 else GRASS_LIGHT, .35)
    cr.set_source_rgba(*rgb(GRASS_LIGHT), .45); cr.set_line_width(2)
    for k in range(-30, 40):
        o = k * step / 2
        cr.move_to(-100, o + (-100) * .5); cr.line_to(W + 100, o + (W + 100) * .5); cr.stroke()
        cr.move_to(-100, o - (-100) * .5 + 400); cr.line_to(W + 100, o - (W + 100) * .5 + 400); cr.stroke()


def river(c):
    pts = [(-40, 250), (60, 300), (110, 380), (70, 470), (40, 560), (110, 650), (190, 730), (230, 800), (250, 900)]
    cr = c.c
    c._path(pts); cr.set_source_rgb(*rgb(WATER_DARK)); cr.set_line_width(64); cr.stroke()
    c._path(pts); cr.set_source_rgb(*rgb(WATER)); cr.set_line_width(54); cr.stroke()
    n = len(pts)
    for side in (-1, 1):
        edge = []
        for i, (x, y) in enumerate(pts):
            x0, y0 = pts[max(i - 1, 0)]; x1, y1 = pts[min(i + 1, n - 1)]
            d = math.hypot(x1 - x0, y1 - y0)
            edge.append((x - (y1 - y0) / d * 31 * side, y + (x1 - x0) / d * 31 * side))
        c.line(edge, 2.6, amp=1.5)
    for x, y in [(50, 318), (92, 420), (58, 520), (96, 628), (168, 706), (226, 790)]:
        c.line([(x - 12, y), (x - 4, y - 4), (x + 4, y), (x + 12, y - 4)], 2, WHITE, amp=.3, step=6)
    # footbridge
    c.shape([(20, 478), (120, 506), (112, 530), (12, 502)], WOOD, 2.6)
    for i in range(1, 6):
        c.line([(20 + i * 17, 478 + i * 4.7), (12 + i * 17, 502 + i * 4.7)], 1.6, amp=.3)


def paths(c, centres, hub):
    cr = c.c
    for x, y in centres:
        mid = ((hub[0] + x) / 2, (hub[1] + y) / 2 + 26)
        pts = [hub, mid, (x, y + 16)]
        cr.new_path(); cr.move_to(*pts[0]); cr.curve_to(*mid, *mid, *pts[2])
        cr.set_source_rgb(*rgb(TONE1)); cr.set_line_width(36); cr.stroke_preserve()
        cr.set_source_rgb(*rgb(PAPER)); cr.set_line_width(26); cr.stroke()
        # stepping stones along the path
        for t in (.3, .5, .7):
            px = (1 - t) ** 2 * hub[0] + 2 * t * (1 - t) * mid[0] + t * t * x
            py = (1 - t) ** 2 * hub[1] + 2 * t * (1 - t) * mid[1] + t * t * (y + 16)
            c.blob(px, py, 8, 4.5, STONE, 1.6, bumps=4, lump=.15)


def table(c, x, y):
    c.sh_oval(x + 6, y + 30, 96, 26)
    for dx, dy in [(-62, -6), (52, 16), (30, 34), (-84, 12)]:
        c.line([(x + dx, y + dy), (x + dx, y + dy + 24)], 3.4, amp=.3); c.line([(x + dx, y + dy), (x + dx, y + dy + 24)], 1.6, WOOD, amp=.3)
    c.shape([(x - 66, y - 16), (x + 58, y + 6), (x + 34, y + 30), (x - 90, y + 8)], GOLD, 2.8)
    c.line([(x - 54, y - 6), (x + 44, y + 12)], 1.6, amp=.5)
    # bunting between two poles
    a, b = (x - 130, y - 78), (x + 110, y - 52)
    c.line([(a[0], y - 6), a], 2.8, amp=.3); c.line([(b[0], y + 26), b], 2.8, amp=.3)
    c.line([a, ((a[0] + b[0]) / 2, a[1] + 26), b], 1.8, amp=.5)
    for i in range(1, 9):
        t = i / 9
        fx = a[0] + (b[0] - a[0]) * t
        fy = a[1] + (b[1] - a[1]) * t + 26 * 4 * t * (1 - t) * .9
        c.shape([(fx - 7, fy), (fx + 7, fy + 2), (fx, fy + 15)], (GOLD, WHITE, RED)[i % 3], 1.8, amp=.3, step=8)


def shop(c, x, y):
    a = c.house(x, y, 124, 76, 54, roof=LEAF_DARK, wall=PAPER)
    # striped awning over the front
    for i in range(6):
        x0 = x - 66 + i * 22
        c.shape([(x0, y - 50), (x0 + 22, y - 50), (x0 + 22 - 6, y - 28), (x0 - 6, y - 28)], RED if i % 2 == 0 else WHITE, 2.2, amp=.4)
    # crates and a barrel
    c.shape([(x - 92, y + 12), (x - 60, y + 16), (x - 60, y - 12), (x - 92, y - 16)], WOOD, 2.4, amp=.5)
    c.line([(x - 92, y - 2), (x - 60, y + 2)], 1.6, amp=.3)
    for i in range(3):
        c.oval(x - 86 + i * 10, y - 20, 6, 6, RED if i != 1 else GOLD, 1.8)
    c.shape([(x + 78, y + 24), (x + 76, y - 2), (x + 100, y - 2), (x + 98, y + 24)], WOOD_DARK, 2.4, amp=.5)
    c.line([(x + 77, y + 8), (x + 99, y + 8)], 1.6, amp=.3)


def farm(c, x, y):
    c.house(x, y, 132, 70, 60, roof=NAVY, wall=RED, side=WOOD_DARK, planks=True, windows=0, door=False, roof_h=64)
    # big barn door with a cross brace
    c.shape([(x - 26, y), (x - 26, y - 46), (x + 26, y - 46), (x + 26, y)], PAPER, 2.6, amp=.4)
    c.line([(x - 26, y - 46), (x + 26, y)], 2, amp=.3); c.line([(x + 26, y - 46), (x - 26, y)], 2, amp=.3)
    c.oval(x, y - 66, 9, 9, PAPER, 2)
    # haystacks
    c.blob(x + 118, y - 14, 26, 22, GOLD, 2.6, bumps=5, lump=.12)
    c.hatch(x + 102, y - 28, x + 130, y, gap=8, w=1.5)
    c.fence(x + 70, y + 50, x + 190, y + 30, 6)
    # a sheep
    c.blob(x + 150, y + 64, 15, 11, WHITE, 2.2, bumps=6, lump=.3)
    c.oval(x + 164, y + 60, 6, 5, INK, 1.5)


def cottage(c, x, y):
    c.house(x, y, 116, 72, 52, roof=ORANGE, wall=WHITE, side=STONE, chimney=True)
    c.bush(x - 78, y + 8, 15); c.bush(x + 88, y + 2, 12)
    c.line([(x - 100, y + 30), (x - 60, y + 36)], 2, amp=.6)


def family(c, x, y):
    c.house(x - 10, y, 104, 68, 48, roof=NAVY, wall=PAPER)
    # the big family tree with golden fruit
    tx, ty = x + 104, y + 20
    c.sh_oval(tx + 10, ty + 4, 44, 12)
    c.shape([(tx - 9, ty), (tx - 5, ty - 62), (tx + 5, ty - 62), (tx + 9, ty)], WOOD, 2.6, amp=.5)
    c.line([(tx, ty - 50), (tx - 26, ty - 80)], 2.6, amp=.5); c.line([(tx, ty - 54), (tx + 26, ty - 84)], 2.6, amp=.5)
    c.blob(tx, ty - 108, 52, 46, LEAF, 2.8, bumps=8, lump=.2)
    for dx, dy in [(-24, -118), (4, -132), (26, -108), (-8, -96), (18, -84), (-30, -92)]:
        c.oval(tx + dx, ty + dy, 5.5, 5.5, GOLD, 1.8)
    c.flag(x - 10, y - 126, 30, RED)


def church(c, x, y):
    """Memory: a small wooden church with shingled tower, in the Carpathian style."""
    c.house(x + 18, y, 100, 60, 50, roof=WOOD_DARK, wall=WOOD, side=WOOD_DARK, windows=1, roof_h=52)
    # tower in front, left
    tx = x - 46
    c.shadow([(tx - 30, y + 6), (tx + 30, y + 10), (tx + 46, y + 2)])
    c.shape([(tx - 26, y + 4), (tx - 20, y - 92), (tx + 20, y - 92), (tx + 26, y + 4)], WOOD, 2.8)
    c.flat([(tx + 8, y - 90), (tx + 19, y - 90), (tx + 25, y + 2), (tx + 12, y + 2)], WOOD_DARK)
    for yy in (y - 24, y - 48, y - 70):
        c.line([(tx - 23, yy), (tx + 23, yy)], 1.5, amp=.4)
    c.shape([(tx - 30, y - 90), (tx, y - 134), (tx + 30, y - 90)], '#593e47', 2.8)
    c.oval(tx, y - 142, 11, 12, GOLD, 2.4)
    c.line([(tx, y - 154), (tx, y - 174)], 2.4, amp=.2); c.line([(tx - 7, y - 166), (tx + 7, y - 166)], 2.4, amp=.2)
    c.shape([(tx - 8, y + 4), (tx - 8, y - 22), (tx + 8, y - 22), (tx + 8, y + 4)], INK, 2, amp=.3)
    c.shape([(tx - 6, y - 60), (tx + 6, y - 60), (tx + 6, y - 78), (tx - 6, y - 78)], GOLD, 2, amp=.3)


def numbers(c, x, y):
    c.house(x, y, 118, 70, 52, roof=WATER_DARK, wall=WHITE, side=STONE)
    # hanging number sign
    sx, sy = x - 92, y - 56
    c.line([(sx, y + 14), (sx, sy - 34)], 3, amp=.3); c.line([(sx, sy - 28), (sx + 34, sy - 28)], 2.6, amp=.3)
    c.shape([(sx + 6, sy - 20), (sx + 46, sy - 20), (sx + 46, sy + 12), (sx + 6, sy + 12)], GOLD, 2.4, amp=.5)
    cr = c.c; cr.set_source_rgb(*rgb(INK)); cr.select_font_face('DejaVu Sans', cairo.FONT_SLANT_NORMAL, cairo.FONT_WEIGHT_BOLD)
    cr.set_font_size(20); cr.move_to(sx + 9, sy + 4); cr.show_text('123')
    # market tickets on a line
    c.line([(x + 70, y - 30), (x + 150, y - 44)], 1.8, amp=.5)
    for i in range(3):
        c.shape([(x + 82 + i * 22, y - 30 - i * 4), (x + 98 + i * 22, y - 33 - i * 4), (x + 98 + i * 22, y - 13 - i * 4), (x + 82 + i * 22, y - 10 - i * 4)],
                (WHITE, GOLD, ORANGE)[i], 1.8, amp=.3, step=8)
    c.line([(x + 150, y + 16), (x + 150, y - 46)], 3, amp=.3)


def school(c, x, y):
    a = c.house(x, y, 136, 74, 56, roof=RED, wall=GOLD, side='#de9f47', roof_h=54)
    # bell turret on the ridge
    bx, by = x + 26, a['apex'][1] - 6
    c.shape([(bx - 12, by + 12), (bx - 12, by - 20), (bx + 12, by - 20), (bx + 12, by + 8)], WHITE, 2.4, amp=.4)
    c.shape([(bx - 17, by - 20), (bx, by - 42), (bx + 17, by - 20)], GOLD, 2.4, amp=.4)
    c.oval(bx, by - 6, 5, 6, INK, 1.5)
    # letter board
    sx, sy = x - 104, y - 34
    c.line([(sx - 16, y + 18), (sx - 10, sy - 4)], 2.6, amp=.3); c.line([(sx + 22, y + 22), (sx + 16, sy - 4)], 2.6, amp=.3)
    c.shape([(sx - 24, sy - 30), (sx + 28, sy - 26), (sx + 28, sy + 12), (sx - 24, sy + 8)], WHITE, 2.6, amp=.5)
    cr = c.c; cr.set_source_rgb(*rgb(INK)); cr.select_font_face('DejaVu Sans', cairo.FONT_SLANT_NORMAL, cairo.FONT_WEIGHT_BOLD)
    cr.set_font_size(24); cr.move_to(sx - 17, sy + 2); cr.show_text('Аб')
    c.flag(x - 50, a['apex'][1] + 26, 34, RED, right=False)


def windmill(c, x, y):
    """Weather: a windmill with a weather vane."""
    c.sh_oval(x + 14, y + 6, 64, 16)
    c.shape([(x - 34, y + 4), (x - 22, y - 96), (x + 22, y - 96), (x + 34, y + 4)], WHITE, 2.8)
    c.flat([(x + 8, y - 94), (x + 21, y - 94), (x + 33, y + 2), (x + 14, y + 2)], STONE)
    for yy in (y - 26, y - 52, y - 76):
        c.line([(x - 29, yy), (x + 29, yy)], 1.5, amp=.4)
    c.shape([(x - 28, y - 94), (x, y - 128), (x + 28, y - 94)], RED, 2.8)
    c.shape([(x - 9, y + 4), (x - 9, y - 26), (x + 9, y - 26), (x + 9, y + 4)], INK, 2, amp=.3)
    c.shape([(x - 7, y - 50), (x + 7, y - 50), (x + 7, y - 66), (x - 7, y - 66)], GOLD, 2, amp=.3)
    # sails
    hx, hy = x, y - 100
    for k in range(4):
        a = .5 + k * math.pi / 2
        ux, uy = math.cos(a), math.sin(a); vx, vy = -uy, ux
        p = [(hx + ux * 14, hy + uy * 14), (hx + ux * 74, hy + uy * 74), (hx + ux * 74 + vx * 20, hy + uy * 74 + vy * 20),
             (hx + ux * 22 + vx * 20, hy + uy * 22 + vy * 20)]
        c.shape(p, PAPER, 2.2, amp=.5)
        for t in (.4, .6, .8):
            c.line([(hx + ux * 74 * t, hy + uy * 74 * t), (hx + ux * 74 * t + vx * 20, hy + uy * 74 * t + vy * 20)], 1.4, amp=.2)
        c.line([(hx, hy), (hx + ux * 76, hy + uy * 76)], 2.8, amp=.4)
    c.oval(hx, hy, 6, 6, INK, 1.5)
    # weather vane and a small cloud
    c.cloud(x + 96, y - 150, 70)


def main():
    c = Canvas(W, H, scale=2, bg=GRASS, seed=11, ysquash=1.25)
    ground(c)
    river(c)
    spots = {k: world(*v) for k, v in dict(Shop=(-5.083, .233), FamilyTree=(4.333, -.033), Farm=(-3, .967), House=(2.25, 1.1),
                                           Memory=(-4.5, -2.033), Numbers=(-1.417, -1.967), Alphabet=(2.917, -2.033),
                                           Weather=(5.583, -1.767)).items()}
    hub = (950, 440)
    # building ground line sits a little under the hotspot centre, shifted like the label
    base = {k: (x + 24, y + 42) for k, (x, y) in spots.items()}
    paths(c, list(base.values()), hub)

    things = []                       # (sort y, draw function)
    def add(y, f): things.append((y, f))

    # mountain range along the top, taller towards the corners
    for x, y, w, h in [(110, 96, 190, 150), (250, 120, 170, 170), (30, 150, 150, 130), (395, 86, 150, 120), (880, 78, 170, 118),
                       (1010, 96, 140, 100), (1560, 92, 180, 140), (1700, 126, 190, 180), (1850, 112, 170, 150), (1830, 190, 150, 130)]:
        add(y, lambda x=x, y=y, w=w, h=h: c.mountain(x, y, w, h))
    r = c.r
    # pine woods
    woods = [(430, 150, 5), (770, 130, 4), (1120, 110, 4), (1420, 120, 4), (1820, 330, 6), (1790, 760, 7), (1500, 790, 5), (1090, 800, 5),
             (640, 790, 5), (330, 800, 5), (300, 440, 3), (1040, 300, 2), (1700, 420, 3), (60, 800, 4), (980, 640, 2)]
    for wx, wy, n in woods:
        for i in range(n):
            x = wx + (i % 3) * 40 + r.uniform(-10, 10) + (i // 3) * 20
            y = wy + (i // 3) * 34 + r.uniform(-8, 8)
            add(y, lambda x=x, y=y, h=r.uniform(50, 68): c.pine(x, y, h))
    for x, y in [(560, 420), (1140, 470), (1400, 250), (1560, 660), (890, 250), (240, 250), (1250, 720), (700, 690), (180, 400)]:
        add(y, lambda x=x, y=y, rr=r.uniform(20, 27): c.tree(x, y, rr))

    draw = dict(Shop=shop, Farm=farm, House=cottage, FamilyTree=family, Memory=church, Numbers=numbers, Alphabet=school, Weather=windmill)
    for k, (x, y) in base.items():
        add(y, lambda k=k, x=x, y=y: draw[k](c, x, y))
    add(hub[1] + 10, lambda: table(c, *hub))

    # meadow marks first, kept off the paths' ends
    for i in range(70):
        x, y = r.uniform(20, W - 20), r.uniform(150, H - 10)
        if all(abs(x - bx) > 150 or abs(y - by + 40) > 110 for bx, by in base.values()) and math.hypot(x - hub[0], y - hub[1]) > 150:
            (c.tick(x, y, color=PINE) if i % 3 else c.grass(x, y))
    for i in range(8):
        c.stones(r.uniform(100, W - 100), r.uniform(200, H - 30), r.randint(2, 3))
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
