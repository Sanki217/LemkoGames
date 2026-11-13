"""Activity backdrops, shop layout and house rooms in ink-on-paper style.

Backdrops keep the middle calm because word cards and tokens sit there; the drawing
lives along the top and in the corners.
Usage: python3 make_backdrops.py [preview dir]
"""
import math, sys
from pathlib import Path
from ink import *

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
    c = Canvas(W, H, 2, SKY, seed)
    return c, horizon


def land(c, horizon):
    pts = [(0, horizon + 8), (140, horizon - 6), (330, horizon + 6), (520, horizon - 8), (720, horizon + 4), (900, horizon - 6), (W, horizon + 6)]
    c.flat(pts + [(W, H), (0, H)], GRASS)
    grid(c, horizon + 10, alpha=.4, color=GRASS_LIGHT)
    c.line(pts, 2.6, amp=1.2)
    r = c.r
    for i in range(26):
        x, y = r.uniform(10, W - 10), r.uniform(horizon + 30, H - 8)
        if 150 < x < W - 150 and y < H - 60 and i % 3:
            continue                                   # keep the play area mostly clear
        (c.tick(x, y, .8, PINE) if i % 2 else c.grass(x, y, .8))


def indoor(seed, floor=330, planks=True, wall=PAPER):
    c = Canvas(W, H, 2, wall, seed)
    c.flat([(0, floor), (W, floor), (W, H), (0, H)], TONE1)
    if planks:
        for i in range(-6, 18):
            x = 512 + (i - 6) * 95
            c.line([(512 + (x - 512) * .82, floor + 12), (512 + (x - 512) * 1.5, H)], 1.5, WOOD, amp=.6)
    c.flat([(0, floor - 12), (W, floor - 12), (W, floor), (0, floor)], WOOD)
    c.line([(0, floor - 12), (W, floor - 12)], 2, amp=.8); c.line([(0, floor), (W, floor)], 2.6, amp=.8)
    return c, floor


def window(c, x, y, w=62, h=74, view=True):
    c.shape([(x - w / 2, y - h / 2), (x + w / 2, y - h / 2), (x + w / 2, y + h / 2), (x - w / 2, y + h / 2)], SKY, 2.8, amp=.6)
    if view:
        c.flat([(x - w / 2 + 5, y + h / 2 - 5), (x - w / 2 + 5, y + 6), (x - 6, y - 10), (x + 8, y + 4), (x + w / 2 - 5, y - 6), (x + w / 2 - 5, y + h / 2 - 5)], GRASS)
        c.oval(x + w * .22, y - h * .26, 7, 7, GOLD, 1.8)
    c.line([(x, y - h / 2), (x, y + h / 2)], 2.2, amp=.3); c.line([(x - w / 2, y), (x + w / 2, y)], 2.2, amp=.3)
    c.shape([(x - w / 2 - 6, y + h / 2), (x + w / 2 + 6, y + h / 2), (x + w / 2 + 6, y + h / 2 + 7), (x - w / 2 - 6, y + h / 2 + 7)], WHITE, 2.2, amp=.4)


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
    c.shape([(x - w / 2 + 24, y - h / 2), (x + w / 2 + 24, y - h / 2), (x + w / 2 - 24, y + h / 2), (x - w / 2 - 24, y + h / 2)], RED, 2.4, amp=.8)
    c.line([(x - w / 2 + 14, y - h / 2 + 10), (x + w / 2 + 6, y - h / 2 + 10)], 2.4, PAPER, amp=.5)
    c.line([(x - w / 2 - 8, y + h / 2 - 10), (x + w / 2 - 16, y + h / 2 - 10)], 2.4, GOLD, amp=.5)


# --- the seven activity backdrops ------------------------------------------------------
def farm():
    c, hz = outdoor(21)
    for x, y, w, h in [(120, hz + 4, 150, 96), (240, hz, 120, 70), (640, hz - 2, 140, 84)]:
        c.mountain(x, y, w, h)
    land(c, hz)
    c.house(880, hz + 44, 96, 52, 44, roof=NAVY, wall=RED, side=WOOD_DARK, planks=True, windows=0, door=False, roof_h=46)
    c.shape([(862, hz + 44), (862, hz + 10), (898, hz + 10), (898, hz + 44)], PAPER, 2.4, amp=.3)
    c.line([(862, hz + 10), (898, hz + 44)], 1.8, amp=.2); c.line([(898, hz + 10), (862, hz + 44)], 1.8, amp=.2)
    c.blob(790, hz + 34, 22, 18, GOLD, 2.4, bumps=5, lump=.12); c.hatch(776, hz + 22, 800, hz + 46, 7, 1.4)
    c.fence(20, hz + 46, 250, hz + 34, 9)
    c.pine(420, hz + 26, 50); c.pine(455, hz + 34, 60); c.tree(540, hz + 30, 18)
    c.fence(30, H - 14, 190, H - 26, 6); c.bush(980, H - 20, 18); c.pine(1000, H - 60, 56)
    out(c, BG, 'scene_Farm', (2048, 960))


def family():
    c, hz = outdoor(22)
    for x, y, w, h in [(880, hz + 2, 160, 100), (760, hz + 4, 120, 70)]:
        c.mountain(x, y, w, h)
    land(c, hz)
    c.house(150, hz + 46, 92, 54, 42, roof=ORANGE, wall=WHITE, side=STONE, chimney=True)
    c.tree(60, hz + 52, 20); c.tree(520, hz + 28, 22); c.pine(600, hz + 30, 52); c.pine(300, hz + 30, 56); c.pine(332, hz + 38, 46)
    # the family tree in the corner, with golden fruit
    tx, ty = 962, H - 14
    c.shape([(tx - 10, ty), (tx - 6, ty - 70), (tx + 6, ty - 70), (tx + 10, ty)], WOOD, 2.6, amp=.5)
    c.blob(tx, ty - 116, 56, 50, LEAF, 2.8, bumps=8, lump=.2)
    for dx, dy in [(-26, -126), (2, -142), (26, -116), (-8, -100), (20, -92), (-32, -100)]:
        c.oval(tx + dx, ty + dy, 5.5, 5.5, GOLD, 1.8)
    c.bush(40, H - 12, 20); c.stones(84, H - 14, 3)
    out(c, BG, 'scene_FamilyTree', (2048, 960))


def weather():
    c, hz = outdoor(23, 150)
    c.oval(150, 62, 30, 30, GOLD, 2.8)
    for k in range(10):
        a = k * math.pi / 5
        c.line([(150 + math.cos(a) * 40, 62 + math.sin(a) * 40), (150 + math.cos(a) * 54, 62 + math.sin(a) * 54)], 2.6, amp=.3)
    c.cloud(330, 54, 110); c.cloud(600, 80, 90); c.cloud(900, 50, 120, '#bbc3d0')
    for i in range(5):
        c.line([(868 + i * 16, 74), (862 + i * 16, 90)], 2, NAVY, amp=.2)
    for x, y, w, h in [(470, hz + 2, 170, 100), (590, hz, 130, 72), (60, hz + 4, 150, 70)]:
        c.mountain(x, y, w, h)
    land(c, hz)
    # weather vane post
    x, y = 960, hz + 60
    c.line([(x, y), (x, y - 76)], 3, amp=.3); c.line([(x - 22, y - 62), (x + 22, y - 62)], 2.2, amp=.2)
    c.shape([(x - 20, y - 86), (x + 14, y - 80), (x - 6, y - 74)], RED, 2.2, amp=.3, step=8)
    c.pine(760, hz + 34, 56); c.pine(800, hz + 40, 44); c.tree(260, hz + 34, 20)
    out(c, BG, 'scene_Weather', (2048, 960))


def house():
    c, fl = indoor(24)
    window(c, 90, 96); window(c, 934, 96)
    frame(c, 250, 70); frame(c, 770, 74, 40, 46)
    shelf(c, 450, 70, 130, 6, pot=True)
    rug(c, 512, fl + 110, 300, 50)
    plant(c, 40, fl + 4, 1.2)
    out(c, BG, 'scene_House', (2048, 960))


def memory():
    c, fl = indoor(25, wall='#cae6d9')
    shelf(c, 60, 78, 150, 8); shelf(c, 814, 78, 150, 8, pot=True)
    frame(c, 330, 66, 50, 40); frame(c, 512, 60, 44, 50, GOLD); frame(c, 694, 66, 50, 40)
    # a trunk of keepsakes in the corner
    x, y = 950, fl + 120
    c.shape([(x - 44, y), (x - 44, y - 34), (x + 44, y - 34), (x + 44, y)], WOOD, 2.6, amp=.5)
    c.shape([(x - 46, y - 34), (x - 40, y - 54), (x + 40, y - 54), (x + 46, y - 34)], WOOD_DARK, 2.6, amp=.5)
    c.shape([(x - 6, y - 40), (x + 6, y - 40), (x + 6, y - 26), (x - 6, y - 26)], GOLD, 2, amp=.2, step=8)
    plant(c, 50, fl + 6, 1.1)
    out(c, BG, 'scene_Memory', (2048, 960))


def numbers():
    c, fl = indoor(26)
    # ticket board along the wall
    c.shape([(360, 34), (664, 34), (664, 112), (360, 112)], WOOD, 2.8, amp=.8)
    for i, n in enumerate(('1', '2', '3', '4', '5')):
        x = 376 + i * 57
        c.shape([(x, 50), (x + 44, 48), (x + 44, 96), (x, 98)], GOLD if i == 2 else WHITE, 2.2, amp=.5, step=12)
        text(c, n, x + 13, 84, 28)
    # wall clock
    c.oval(120, 80, 34, 34, WHITE, 2.8); c.line([(120, 80), (120, 58)], 2.6, amp=.2); c.line([(120, 80), (136, 88)], 2.6, RED, amp=.2)
    for k in range(12):
        a = k * math.pi / 6
        c.line([(120 + math.cos(a) * 27, 80 + math.sin(a) * 27), (120 + math.cos(a) * 31, 80 + math.sin(a) * 31)], 1.6, amp=0)
    shelf(c, 800, 84, 140, 5, pot=True)
    # a counter corner with a bell
    c.sh_oval(950, fl + 110, 22, 9)
    c.shape([(932, fl + 108), (936, fl + 88), (964, fl + 88), (968, fl + 108)], GOLD, 2.4, amp=.3)
    c.oval(950, fl + 84, 5, 5, INK, 1.5)
    out(c, BG, 'scene_Numbers', (2048, 960))


def alphabet():
    c, fl = indoor(27, wall='#cae6d9')
    # blackboard with chalk letters
    c.shape([(332, 26), (692, 26), (692, 122), (332, 122)], PINE_DARK, 3, amp=.8)
    c.shape([(326, 122), (698, 122), (698, 132), (326, 132)], WOOD, 2.4, amp=.5)
    text(c, 'А Б В Г Ґ Д', 356, 88, 40, WHITE)
    c.shape([(600, 124), (626, 124), (626, 130), (600, 130)], WHITE, 1.6, amp=.2, step=10)
    window(c, 110, 90); window(c, 914, 90)
    # a pile of books and a pencil pot in the corner
    x, y = 60, fl + 124
    for i in range(3):
        c.shape([(x - 34 + i * 4, y - i * 13), (x + 34 + i * 4, y - i * 13), (x + 34 + i * 4, y - 13 - i * 13), (x - 34 + i * 4, y - 13 - i * 13)],
                (NAVY, GOLD, RED)[i], 2.2, amp=.4)
    c.shape([(940, fl + 124), (944, fl + 96), (972, fl + 96), (976, fl + 124)], WATER, 2.4, amp=.3)
    for i, col in enumerate((GOLD, RED, LEAF)):
        c.line([(950 + i * 8, fl + 96), (946 + i * 11, fl + 66)], 4, INK, amp=.2); c.line([(950 + i * 8, fl + 95), (946 + i * 11, fl + 68)], 2, col, amp=.2)
    out(c, BG, 'scene_Alphabet', (2048, 960))


# --- shop layout (1920x1080, matches the product slots and panels in Shop.unity) ---------
def shop():
    c = Canvas(1920, 1080, 1.5, PAPER, 31)
    grid(c, 0, 1920, 1080, 240, .35)
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
    c = Canvas(1600, 720, 1.5, PAPER if kitchen else '#cae6d9', seed)
    fl = 470
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
