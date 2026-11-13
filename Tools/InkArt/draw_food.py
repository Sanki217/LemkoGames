"""Shop products (76 pictures)."""
import math
from sk import *

D = {}


def item(f):
    D[f.__name__] = f
    return f


def P(cx, cy, r, a):
    return (cx + math.cos(math.radians(a)) * r, cy + math.sin(math.radians(a)) * r)


def turned(c, deg, f, cx=128, cy=128):
    cr = c.c; cr.save(); cr.translate(cx, cy); cr.rotate(math.radians(deg)); cr.translate(-cx, -cy); f(); cr.restore()


def leaf(c, x, y, rot=0, colr='green', rx=22, ry=10, vein=True, w=5.5):
    c.ell(x, y, rx, ry, colr, rot=rot, w=w, sx=3, sy=3)
    if vein:
        c.ln([P(x, y, rx * .7, math.degrees(rot) + 180), P(x, y, rx * .7, math.degrees(rot))], 3, SHADE[colr], amp=.2)


def stem(c, pts, colr='dgreen', w=7):
    c.ln(pts, w + 4, 'ink', amp=.5); c.ln(pts, w - 2.5, colr, amp=.5)


def tops(c, x, y, colr='green', n=3, h=60, spread=28):
    """Leafy tops of a root vegetable."""
    for i in range(n):
        dx = (i - (n - 1) / 2) * spread
        c.poly([(x - 6, y), (x + dx * .4 - 12, y - h * .5), (x + dx, y - h), (x + dx * .4 + 12, y - h * .5), (x + 6, y)], colr, step=10, w=6)


def fruit(c, x, y, rx, ry, colr, leafc='green', stemc='brown', shine=True, w=LW):
    c.ell(x, y, rx, ry, colr, w=w)
    if shine:
        c.hi([(x - rx * .5, y - ry * .35), (x - rx * .32, y - ry * .58)], 5)
    if stemc:
        c.ln([(x, y - ry + 4), (x + 5, y - ry - 16)], 6.5, stemc, amp=.2)
    if leafc:
        leaf(c, x + 20, y - ry - 12, -.5, leafc, 17, 8)


def bowl(c, fillc, dots=None, y=150, colr='pale'):
    c.ell(128, y - 14, 96, 30, fillc, inner=dots)
    c.poly([(30, y - 12), (40, y + 6), (60, y + 60), (92, y + 76), (164, y + 76), (196, y + 60), (216, y + 6), (226, y - 12), (200, y + 6), (128, y + 16), (56, y + 6)], colr, step=12)


def sack(c, colr='cream', band='brick'):
    c.poly([(84, 60), (104, 44), (128, 54), (152, 44), (172, 60), (160, 84), (206, 140), (204, 210), (180, 230), (76, 230), (52, 210), (50, 140), (96, 84)], colr, step=12)
    c.ln([(92, 84), (164, 84)], 8, band, amp=.5); c.ln([(92, 84), (164, 84)], 0)


def bottle(c, colr='pale', label='cream', cap='wood', neck=20, shoulder=118, liquid=None):
    c.rr(128 - neck / 2 - 3, 18, neck + 6, 22, 5, cap)
    c.poly([(128 - neck / 2, 38), (128 + neck / 2, 38), (128 + neck / 2 + 2, shoulder - 30), (176, shoulder), (176, 220), (164, 234), (92, 234), (80, 220), (80, shoulder), (128 - neck / 2 - 2, shoulder - 30)], colr, step=14,
           inner=(lambda: c.fill([(60, shoulder + 6), (200, shoulder + 6), (200, 250), (60, 250)], liquid)) if liquid else None)
    if label:
        c.rr(90, shoulder + 26, 76, 60, 8, label, w=5.5)
    c.hi([(92, shoulder + 2), (98, shoulder - 8)])


# ---- fruit -------------------------------------------------------------------------------------
@item
def apples(c):
    fruit(c, 170, 136, 62, 60, 'lime', leafc=None); fruit(c, 92, 150, 68, 66, 'brick')
@item
def pears(c):
    def pear(x, y, colr, s=1.0):
        c.poly([(x - 12 * s, y - 76 * s), (x + 12 * s, y - 76 * s), (x + 22 * s, y - 30 * s), (x + 50 * s, y + 20 * s), (x + 40 * s, y + 62 * s), (x, y + 74 * s), (x - 40 * s, y + 62 * s), (x - 50 * s, y + 20 * s), (x - 22 * s, y - 30 * s)], colr, step=10)
        c.ln([(x, y - 74 * s), (x + 6, y - 96 * s)], 7, 'brown', amp=.2); leaf(c, x + 22, y - 90 * s, -.5, 'green', 17, 8)
        c.hi([(x - 26 * s, y + 20 * s), (x - 22 * s, y)], 5)
    pear(176, 132, 'lime', .86); pear(92, 144, 'yellow')
@item
def plums(c):
    fruit(c, 172, 128, 54, 62, 'mauve'); fruit(c, 92, 150, 58, 68, 'navy')
    c.arc(92, 150, 20, 250, 290, 4.5, 'slate', ry=64)
@item
def lemon(c):
    c.poly([(26, 132), (50, 116), (84, 78), (140, 66), (196, 86), (222, 122), (238, 128), (222, 146), (196, 180), (140, 198), (84, 186), (50, 150)], 'yellow', step=10)
    c.hi([(86, 104), (116, 88)], 5.5); leaf(c, 60, 86, .6, 'green', 26, 12)
    for x, y in ((130, 120), (160, 140), (120, 156)):
        c.dot(x, y, 3, 'gold')
@item
def sour_cherries(c):
    stem(c, [(90, 150), (110, 80), (150, 40)], 'dgreen', 6); stem(c, [(176, 160), (166, 90), (150, 40)], 'dgreen', 6)
    leaf(c, 186, 46, .2, 'dgreen', 34, 15)
    fruit(c, 86, 176, 46, 46, 'brown', None, None); fruit(c, 178, 186, 44, 44, 'brown', None, None)
@item
def sweet_cherries(c):
    stem(c, [(70, 160), (96, 86), (128, 34)], 'green', 6); stem(c, [(128, 176), (130, 100), (128, 34)], 'green', 6); stem(c, [(190, 160), (160, 90), (128, 34)], 'green', 6)
    leaf(c, 164, 40, .1, 'lime', 34, 15)
    fruit(c, 64, 180, 40, 42, 'brick', None, None); fruit(c, 192, 180, 40, 42, 'brick', None, None); fruit(c, 128, 196, 40, 42, 'orange', None, None)
@item
def strawberries(c):
    def berry(x, y, s=1.0):
        c.poly([(x - 48 * s, y - 40 * s), (x, y - 56 * s), (x + 48 * s, y - 40 * s), (x + 44 * s, y + 10 * s), (x, y + 70 * s), (x - 44 * s, y + 10 * s)], 'brick', step=10)
        for dx, dy in ((-20, -18), (16, -22), (-4, 4), (22, 8), (-24, 14), (4, 34)):
            c.ell(x + dx * s, y + dy * s, 3, 4.5, 'yellow', w=0, shade=False)
        c.poly(star_pts(x, y - 50 * s, 30 * s, 12 * s, 5), 'green', w=5.5, step=8)
    berry(176, 122, .8); berry(92, 142)
@item
def raspberries(c):
    def berry(x, y, colr, shade):
        pts = []
        c.poly([(x - 40, y - 34), (x + 40, y - 34), (x + 34, y + 20), (x, y + 56), (x - 34, y + 20)], colr, step=10,
               inner=lambda: [c.ell(x + dx, y + dy, 13, 13, colr, w=4, color=shade, sx=3, sy=3) for dy in (-28, -6, 16, 38) for dx in ((-26, 0, 26) if dy in (-28, 16) else (-13, 13))])
        c.poly(star_pts(x, y - 40, 22, 9, 5), 'green', w=5.5, step=8)
    berry(174, 116, 'orange', 'brick'); berry(90, 146, 'brick', 'brown')
@item
def blackberries(c):
    def berry(x, y):
        for dy in (-30, -8, 14, 36):
            for dx in ((-24, 0, 24) if dy in (-30, 14) else (-12, 12)) if dy != 36 else (0,):
                c.ell(x + dx, y + dy, 15, 15, 'night' if (dx + dy) % 3 else 'plum', w=5.5, sx=3, sy=3)
        c.poly(star_pts(x, y - 46, 20, 8, 5), 'green', w=5.5, step=8)
    berry(176, 116); berry(88, 150)
@item
def blueberries(c):
    for x, y, r in ((170, 100, 44), (84, 124, 50), (150, 180, 48)):
        c.ell(x, y, r, r, 'blue')
        c.poly(star_pts(x + 6, y - 10, r * .34, r * .14, 5), 'navy', w=4.5, step=7, shade=False)
    leaf(c, 60, 60, .7, 'green', 28, 12)
@item
def currant(c):
    stem(c, [(128, 26), (120, 90), (132, 150), (124, 210)], 'dgreen', 5)
    leaf(c, 170, 40, -.3, 'green', 34, 16)
    for x, y in ((84, 86), (166, 104), (92, 142), (170, 162), (104, 196), (150, 214), (128, 110)):
        c.ell(x, y, 24, 24, 'brick'); c.hi([(x - 10, y - 6), (x - 4, y - 12)], 4); c.dot(x + 8, y + 12, 3)
@item
def gooseberry(c):
    stem(c, [(128, 20), (128, 60)], 'brown', 6); leaf(c, 164, 34, -.4, 'green', 30, 14)
    c.ell(128, 142, 74, 82, 'lime', inner=lambda: [c.arc(128 + d, 142, abs(d) * .2 + 6, 90, 270, 4, 'cream', ry=80) if d < 0 else c.arc(128 + d, 142, abs(d) * .2 + 6, -90, 90, 4, 'cream', ry=80) for d in (-44, -16, 16, 44)])
    c.ln([(118, 226), (128, 236), (138, 226)], 5.5)
@item
def grapes(c):
    stem(c, [(128, 20), (128, 56)], 'brown', 7); leaf(c, 170, 40, -.3, 'green', 36, 18)
    rows = ((-52, -18, 18, 52), (-36, 0, 36), (-18, 18), (0,))
    for j, row in enumerate(rows):
        for dx in row:
            c.ell(128 + dx, 86 + j * 40, 24, 24, 'mauve' if (j + dx) % 2 else 'plum', sx=4, sy=4)
            c.hi([(120 + dx, 78 + j * 40), (124 + dx, 74 + j * 40)], 4, 'ice')
# ---- vegetables ----------------------------------------------------------------------------------
@item
def tomato(c):
    c.ell(128, 138, 96, 84, 'brick'); c.hi([(70, 110), (84, 90)], 6)
    c.poly(star_pts(128, 66, 44, 15, 5), 'green', w=6, step=9); c.ln([(128, 62), (134, 34)], 8, 'dgreen', amp=.2)
@item
def pumpkin(c):
    c.rr(116, 30, 24, 40, 6, 'dgreen')
    c.ell(128, 148, 104, 80, 'orange', inner=lambda: [c.arc(128, 148, rx, 90, 270, 5, 'brick', ry=78) or c.arc(128, 148, rx, -90, 90, 5, 'brick', ry=78) for rx in (26, 66)])
@item
def onion(c):
    tops(c, 128, 70, 'lime', 2, 48, 30)
    c.poly([(128, 52), (158, 86), (212, 150), (190, 208), (128, 226), (66, 208), (44, 150), (98, 86)], 'gold', step=10,
           inner=lambda: [c.arc(128, 150, rx, 200, 340, 4.5, 'wood', ry=90) for rx in (30, 60)])
    for dx in (-12, 0, 12):
        c.ln([(128 + dx * .4, 224), (128 + dx, 242)], 4.5, amp=.2)
@item
def garlic(c):
    c.poly([(118, 26), (138, 26), (140, 70), (190, 110), (206, 164), (176, 214), (128, 226), (80, 214), (50, 164), (66, 110), (116, 70)], 'white', step=10,
           inner=lambda: [c.arc(128, 150, rx, 100, 260, 4.5, 'ice', ry=76) or c.arc(128, 150, rx, -80, 80, 4.5, 'ice', ry=76) for rx in (30,)])
    c.fe(100, 190, 20, 12, 'mauve') if False else None
@item
def potatoes(c):
    for x, y, rx, ry, rot, colr in ((166, 96, 60, 44, .3, 'sand'), (90, 130, 62, 48, -.3, 'wood'), (160, 176, 66, 46, .1, 'sand')):
        c.ell(x, y, rx, ry, colr, rot=rot)
        for dx, dy in ((-20, -8), (14, 6), (-4, 18)):
            c.dot(x + dx, y + dy, 3.4, 'brown')
@item
def beetroots(c):
    tops(c, 128, 96, 'dgreen', 3, 80, 44)
    for dx in (-22, 0, 22):
        c.ln([(128, 96), (128 + dx, 36)], 4, 'mauve', amp=.3)
    c.poly([(128, 84), (186, 104), (204, 150), (176, 198), (128, 238), (80, 198), (52, 150), (70, 104)], 'plum', step=10); c.hi([(78, 140), (88, 120)], 5, 'mauve')
@item
def turnip(c):
    tops(c, 128, 92, 'green', 3, 76, 44)
    c.poly([(128, 78), (190, 96), (210, 144), (180, 196), (128, 240), (76, 196), (46, 144), (66, 96)], 'white', step=10,
           inner=lambda: c.fill([(40, 60), (216, 60), (216, 128), (128, 140), (40, 128)], 'mauve'))
@item
def radish(c):
    tops(c, 128, 92, 'green', 3, 76, 40)
    c.poly([(128, 80), (182, 96), (198, 140), (170, 186), (136, 214), (128, 246), (120, 214), (86, 186), (58, 140), (74, 96)], 'brick', step=10,
           inner=lambda: c.fill([(90, 196), (166, 196), (140, 250), (116, 250)], 'white'))
    c.hi([(84, 132), (94, 112)], 5)
@item
def carrot(c):
    def f():
        tops(c, 60, 128, 'green', 3, 0, 0)
        for dy in (-26, 0, 26):
            c.poly([(64, 128), (30, 128 + dy - 10), (6, 128 + dy), (30, 128 + dy + 10)], 'green', step=9, w=6)
        c.poly([(60, 96), (150, 104), (246, 128), (150, 152), (60, 160), (50, 128)], 'orange', step=12)
        for x in (96, 136, 176):
            c.ln([(x, 110), (x + 4, 124)], 4.5, 'brick', amp=.2)
    turned(c, -35, f)
@item
def parsley_root(c):
    def f():
        for dy in (-28, 0, 28):
            c.lump(24, 128 + dy, 26, 18, 'green', bumps=5, depth=.3, w=6)
            c.ln([(64, 128), (40, 128 + dy)], 6, 'dgreen', amp=.3)
        c.poly([(60, 100), (150, 108), (248, 128), (150, 148), (60, 156), (52, 128)], 'cream', step=12)
        for x in (96, 136, 176):
            c.ln([(x, 112), (x + 4, 124)], 4.5, 'sand', amp=.2)
    turned(c, -35, f)
@item
def horseradish(c):
    def f():
        for dy in (-22, 12):
            c.ell(30, 128 + dy, 34, 13, 'dgreen', rot=dy * .02, w=6)
        c.poly([(56, 104), (110, 112), (150, 100), (200, 118), (246, 122), (200, 140), (150, 156), (110, 144), (56, 152), (48, 128)], 'sand', step=10)
        for x in (90, 130, 170, 206):
            c.ln([(x, 116), (x + 6, 140)], 4.5, 'wood', amp=.3)
    turned(c, -30, f)
@item
def cucumbers(c):
    def cu(deg, colr, cy):
        def f():
            c.rr(30, cy - 26, 196, 52, 26, colr)
            for x in (64, 100, 136, 172):
                c.dot(x, cy - 8 + (x % 3) * 6, 3.5, SHADE[colr]); c.dot(x + 16, cy + 8 - (x % 3) * 4, 3.5, SHADE[colr])
            c.hi([(56, cy - 12), (120, cy - 14)], 5, 'lime')
        turned(c, deg, f)
    cu(-38, 'dgreen', 100); cu(-22, 'green', 160)
@item
def bell_pepper(c):
    c.poly([(120, 34), (140, 30), (144, 66), (116, 66)], 'dgreen', step=9)
    c.poly([(70, 76), (104, 62), (128, 72), (152, 62), (186, 76), (206, 124), (190, 196), (160, 228), (128, 216), (96, 228), (66, 196), (50, 124)], 'brick', step=10,
           inner=lambda: [c.arc(x, 150, 10, 90 if x < 128 else -90, 270 if x < 128 else 90, 5, 'brown', ry=70) for x in (104, 152)])
    c.hi([(74, 120), (82, 96)], 6, 'orange')
@item
def corn(c):
    def f():
        def kern():
            for i in range(9):
                for j in range(5):
                    c.ell(78 + i * 17, 100 + j * 14 + (i % 2) * 7, 8, 7, 'yellow', w=3, color='gold', shade=False)
        c.rr(60, 92, 180, 72, 34, 'gold', inner=kern)
        c.poly([(20, 128), (70, 84), (150, 96), (96, 128)], 'lime', step=10); c.poly([(20, 128), (70, 172), (150, 160), (96, 128)], 'green', step=10)
    turned(c, -35, f)
@item
def cabbage(c):
    c.ell(128, 136, 100, 92, 'green')
    c.ell(120, 132, 72, 68, 'lime', w=6)
    c.arc(150, 150, 60, 150, 280, 5.5, 'green'); c.arc(100, 120, 40, -40, 80, 5.5, 'green'); c.ln([(120, 196), (124, 110)], 5.5, 'cream', amp=.6)
@item
def lettuce(c):
    c.lump(128, 140, 104, 86, 'green', bumps=10, depth=.2)
    c.lump(124, 136, 74, 60, 'lime', bumps=8, depth=.22, w=6)
    c.lump(122, 134, 40, 32, 'yellow', bumps=6, depth=.24, w=5.5)
    c.ln([(124, 220), (124, 150)], 5, 'cream', amp=.6)
@item
def peas(c):
    def f():
        c.poly([(20, 128), (70, 86), (186, 86), (236, 128), (186, 170), (70, 170)], 'green', step=10)
        c.poly([(40, 128), (76, 106), (180, 106), (216, 128), (180, 150), (76, 150)], 'dgreen', step=10, w=5, shade=False)
        for x in (76, 110, 144, 178):
            c.ell(x, 128, 17, 17, 'lime', w=5.5)
        c.ln([(236, 128), (250, 116)], 6, 'dgreen')
    turned(c, -25, f)
@item
def fava_bean(c):
    def f():
        c.poly([(16, 128), (50, 92), (96, 82), (128, 94), (160, 82), (206, 92), (240, 128), (206, 164), (160, 174), (128, 162), (96, 174), (50, 164)], 'sage', step=10)
        c.poly([(40, 128), (64, 108), (192, 108), (216, 128), (192, 148), (64, 148)], 'pale', step=10, w=5, shade=False)
        for x in (84, 128, 172):
            c.ell(x, 128, 20, 15, 'lime', w=5.5); c.arc(x, 122, 8, 200, 340, 3.5, 'dgreen', ry=4)
    turned(c, -25, f)
@item
def beans(c):
    def bean(x, y, rot, colr):
        c.ell(x, y, 34, 22, colr, rot=rot, w=6.5); c.arc(x, y, 8, 0, 360, 3.5, 'cream' if colr != 'white' else 'brick', ry=4)
    for x, y, rot, colr in ((84, 80, .4, 'brick'), (170, 76, -.3, 'white'), (128, 128, .1, 'brown'), (60, 150, -.5, 'white'), (196, 146, .5, 'brick'), (110, 190, -.2, 'brick'), (176, 200, .3, 'brown')):
        bean(x, y, rot, colr)
@item
def lentil(c):
    def dots():
        for i in range(34):
            x = 50 + (i * 53) % 156; y = 122 + (i * 29) % 26
            c.ell(x, y, 9, 6, 'orange' if i % 3 else 'gold', w=2.5, color='brick', shade=False)
    bowl(c, 'orange', dots)
@item
def mushrooms(c):
    def m(x, y, s, cap='white'):
        c.poly([(x - 16 * s, y), (x - 20 * s, y + 60 * s), (x + 20 * s, y + 60 * s), (x + 16 * s, y)], 'cream', step=10)
        c.poly([P(x, y, 56 * s, a) for a in range(180, 361, 15)] + [(x + 44 * s, y + 12 * s), (x - 44 * s, y + 12 * s)], cap, step=9)
    m(174, 120, .8); m(96, 136, 1.15)
@item
def porcini_mushrooms(c):
    def m(x, y, s):
        c.poly([(x - 20 * s, y), (x - 36 * s, y + 40 * s), (x - 26 * s, y + 66 * s), (x + 26 * s, y + 66 * s), (x + 36 * s, y + 40 * s), (x + 20 * s, y)], 'cream', step=10)
        c.poly([P(x, y, 62 * s, a) for a in range(180, 361, 15)] + [(x + 52 * s, y + 14 * s), (x - 52 * s, y + 14 * s)], 'wood', step=9); c.hi([(x - 34 * s, y - 28 * s), (x - 20 * s, y - 42 * s)], 5, 'sand')
    m(184, 138, .7); m(96, 130, 1.1)
# ---- herbs ---------------------------------------------------------------------------------------
@item
def bay_leaf(c):
    leaf(c, 84, 150, -1.0, 'dgreen', 86, 30, w=LW); leaf(c, 168, 132, -.6, 'green', 86, 30, w=LW)
@item
def mint(c):
    stem(c, [(128, 236), (128, 60)], 'dgreen', 6)
    for y, s in ((190, 1.0), (140, .85), (92, .7)):
        for sg in (-1, 1):
            x = 128 + sg * 46 * s
            pts = [(128, y), (x - sg * 18 * s, y - 30 * s), (x + sg * 12 * s, y - 34 * s), (x + sg * 36 * s, y - 20 * s), (x + sg * 30 * s, y + 4 * s), (x, y + 14 * s)]
            c.poly(pts, 'green' if sg < 0 else 'lime', step=8, amp=2.2, w=6); c.ln([(128, y), (x + sg * 24 * s, y - 16 * s)], 3.5, 'dgreen', amp=.3)
    c.lump(128, 48, 22, 18, 'lime', bumps=5, depth=.3, w=6)
@item
def parsley(c):
    for x0, x1 in ((128, 70), (128, 128), (128, 186)):
        stem(c, [(128, 240), (x0, 180), (x1, 110)], 'dgreen', 5)
    for x, y, colr in ((66, 96, 'green'), (190, 96, 'green'), (128, 74, 'lime')):
        c.lump(x, y, 46, 40, colr, bumps=7, depth=.34, w=6.5)
    c.ln([(104, 206), (152, 206)], 7, 'brick', amp=.3)
@item
def marjoram(c):
    stem(c, [(128, 240), (124, 40)], 'brown', 5)
    for i, y in enumerate((200, 164, 128, 94, 62)):
        for sg in (-1, 1):
            c.ell(126 + sg * (30 - i * 2), y - 6, 20 - i * 1.5, 12 - i, 'sage' if (i + sg) % 2 else 'green', rot=-sg * .5, w=5.5)
    for x, y in ((112, 36), (136, 30), (126, 20)):
        c.ell(x, y, 8, 8, 'mauve', w=4.5, shade=False)
@item
def rosemary(c):
    stem(c, [(100, 240), (128, 140), (150, 30)], 'brown', 5)
    for i in range(11):
        t = i / 10; x = 104 + t * 44; y = 218 - t * 180
        for sg in (-1, 1):
            c.ln([(x, y), (x + sg * 40, y - 24)], 9, 'ink', amp=.3); c.ln([(x, y), (x + sg * 38, y - 23)], 4, 'pine' if i % 2 else 'sage', amp=.3)
@item
def chive(c):
    stem(c, [(128, 240), (128, 86)], 'green', 8)
    c.lump(128, 62, 40, 38, 'mauve', bumps=9, depth=.3)
    stem(c, [(128, 240), (170, 150), (196, 120)], 'lime', 6)
@item
def chives(c):
    for i, x in enumerate((60, 84, 108, 128, 150, 174, 198)):
        stem(c, [(128 + (x - 128) * .25, 236), (128 + (x - 128) * .4, 140), (x, 24 + (i % 3) * 12)], 'green' if i % 2 else 'lime', 7)
    c.rr(98, 150, 60, 20, 8, 'brick')
# ---- pantry ---------------------------------------------------------------------------------------
@item
def flour(c):
    sack(c, 'white', 'blue')
    stem(c, [(128, 214), (128, 130)], 'gold', 4)
    for i in range(4):
        for sg in (-1, 1):
            c.ell(128 + sg * 12, 140 + i * 16, 11, 6, 'yellow', rot=-sg * .7, w=4, shade=False)
@item
def sugar(c):
    sack(c, 'pale', 'brick')
    for x, y in ((100, 150), (140, 140), (118, 184), (158, 180)):
        c.rr(x - 15, y - 15, 30, 30, 5, 'white', w=5, sx=3, sy=3)
@item
def salt(c):
    c.poly([(84, 76), (172, 76), (188, 220), (176, 234), (80, 234), (68, 220)], 'white', step=12, inner=lambda: c.fill([(60, 150), (200, 150), (200, 240), (60, 240)], 'ice'))
    c.poly([(84, 78), (92, 40), (164, 40), (172, 78)], 'silver', step=10)
    for x, y in ((112, 54), (128, 62), (144, 54), (128, 46)):
        c.dot(x, y, 3.5)
    c.text('S', 128, 150, 60, 'blue')
@item
def black_pepper(c):
    c.ell(128, 44, 26, 22, 'brown'); c.rr(112, 58, 32, 26, 4, 'silver')
    c.poly([(92, 82), (164, 82), (176, 130), (160, 150), (178, 212), (166, 234), (90, 234), (78, 212), (96, 150), (80, 130)], 'brown', step=10)
    for x, y in ((40, 208), (56, 228), (216, 214), (204, 232), (34, 232)):
        c.ell(x, y, 9, 9, 'slate', w=4.5, shade=False)
@item
def barley_groats(c):
    def dots():
        for i in range(30):
            c.ell(50 + (i * 47) % 156, 122 + (i * 31) % 26, 8, 5, 'cream', rot=i, w=2.5, color='wood', shade=False)
    bowl(c, 'sand', dots, colr='sky')
@item
def buckwheat_groats(c):
    def dots():
        for i in range(30):
            x, y = 50 + (i * 47) % 156, 122 + (i * 31) % 26
            c.poly([(x, y - 7), (x + 7, y + 5), (x - 7, y + 5)], 'wood', w=2.5, color='brown', shade=False, step=8, amp=.2)
    bowl(c, 'brown', dots, colr='cream')
@item
def pasta(c):
    def bow(x, y, rot):
        def f():
            c.poly([(x - 44, y - 26), (x - 8, y - 8), (x + 8, y - 8), (x + 44, y - 26), (x + 44, y + 26), (x + 8, y + 8), (x - 8, y + 8), (x - 44, y + 26)], 'yellow', step=9,
                   inner=lambda: [c.fill([(x + sg * 44, y - 30 + k * 12), (x + sg * 30, y - 26 + k * 12), (x + sg * 44, y - 22 + k * 12)], 'gold') for sg in (-1, 1) for k in range(5)])
            c.ln([(x - 6, y - 8), (x - 6, y + 8)], 4.5); c.ln([(x + 6, y - 8), (x + 6, y + 8)], 4.5)
        turned(c, rot, f, x, y)
    bow(84, 76, -20); bow(176, 110, 25); bow(90, 170, 15); bow(180, 196, -25)
@item
def yeast(c):
    c.poly([(40, 96), (150, 70), (220, 100), (220, 180), (112, 214), (40, 176)], 'sand', step=12,
           inner=lambda: c.fill([(40, 96), (112, 128), (220, 100), (150, 70)], 'cream'))
    c.ln([(40, 96), (112, 128), (220, 100)], 6); c.ln([(112, 128), (112, 212)], 6)
    c.poly([(150, 70), (220, 100), (236, 60), (176, 40)], 'white', step=10, w=6); c.poly([(128, 150), (204, 126), (204, 160), (128, 186)], 'brick', w=5, step=10, shade=False)
@item
def bread(c):
    c.poly([(26, 150), (40, 100), (90, 72), (166, 72), (216, 100), (230, 150), (220, 196), (36, 196)], 'gold', step=10,
           inner=lambda: c.fill([(20, 170), (240, 170), (240, 210), (20, 210)], 'wood'))
    for x in (84, 128, 172):
        c.poly([(x - 14, 96), (x + 2, 86), (x + 16, 132), (x, 142)], 'cream', w=5, step=8, shade=False)
@item
def buns(c):
    def bun(x, y, s=1.0):
        c.poly([P(x, y, 56 * s, a) for a in range(180, 361, 15)] + [(x + 52 * s, y + 22 * s), (x - 52 * s, y + 22 * s)], 'gold', step=9)
        for dx, dy in ((-20, -24), (0, -34), (22, -22), (-8, -12), (14, -8)):
            c.ell(x + dx * s, y + dy * s, 4.5, 2.6, 'cream', rot=dx, w=0, shade=False)
    bun(176, 104, .8); bun(84, 124, .9); bun(134, 190, 1.0)
@item
def challah(c):
    def f():
        c.poly([(20, 128), (44, 92), (212, 92), (236, 128), (212, 164), (44, 164)], 'gold', step=10)
        for i in range(5):
            x = 52 + i * 38
            c.arc(x + 4, 128, 22, -70, 70, 6, 'wood', ry=40); c.fe(x - 4, 112, 8, 5, 'yellow')
        for x, y in ((70, 104), (120, 150), (170, 104), (200, 144), (96, 124)):
            c.dot(x, y, 2.6, 'cream')
    turned(c, -15, f)
@item
def croissant(c):
    pts = [(20, 170), (40, 110), (92, 70), (128, 60), (164, 70), (216, 110), (236, 170), (204, 178), (176, 140), (128, 124), (80, 140), (52, 178)]
    c.poly(pts, 'gold', step=10, inner=lambda: [c.fill([(x, 40), (x + 9, 40), (x + 9 + d, 200), (x + d, 200)], 'wood') for x, d in ((70, -34), (104, -12), (144, 12), (178, 34))])
    c.hi([(100, 84), (124, 78)], 5, 'yellow')
@item
def apple_pie(c):
    def lattice():
        for i in range(-4, 8):
            c.fill([(30 + i * 34, 60), (44 + i * 34, 60), (124 + i * 34, 170), (110 + i * 34, 170)], 'gold')
            c.fill([(120 + i * 34, 60), (134 + i * 34, 60), (54 + i * 34, 170), (40 + i * 34, 170)], 'yellow')
    c.poly([(26, 140), (50, 200), (206, 200), (230, 140)], 'silver', step=12)
    c.ell(128, 126, 106, 54, 'brick', inner=lattice, shade=False)
    c.ell(128, 126, 106, 54, None)
    for a in range(0, 360, 24):
        x, y = P(128, 126, 100, a); c.dot(x, 126 + (y - 126) * .5, 0)
@item
def poppy_seed_cake(c):
    c.poly([(30, 190), (30, 110), (70, 76), (200, 76), (228, 110), (228, 190)], 'gold', step=12)
    def swirl():
        for r in (44, 26, 10):
            c.fe(98, 150, r + 5, r * .8 + 5, 'slate'); c.fe(98, 150, r - 5, r * .8 - 5, 'cream')
    c.ell(98, 150, 66, 52, 'cream', inner=swirl, shade=False)
    for x, y in ((186, 100), (204, 124), (176, 132), (196, 156), (212, 176), (180, 176)):
        c.dot(x, y, 2.6, 'slate')
@item
def butter(c):
    c.poly([(16, 170), (60, 134), (240, 150), (204, 200)], 'white', step=12)
    c.poly([(56, 106), (176, 90), (212, 108), (212, 156), (92, 176), (56, 156)], 'yellow', step=12,
           inner=lambda: c.fill([(56, 106), (92, 124), (212, 108), (176, 90)], 'cream'))
    c.ln([(56, 106), (92, 124), (212, 108)], 6); c.ln([(92, 124), (92, 174)], 6)
@item
def cheese(c):
    c.poly([(24, 150), (176, 60), (232, 120), (232, 196), (24, 196)], 'yellow', step=12,
           inner=lambda: (c.fill([(24, 150), (176, 60), (232, 120)], 'cream'), [c.fe(x, y, r, r * .8, 'gold') for x, y, r in ((80, 174), (150, 166), (200, 176))] if False else None))
    c.ln([(24, 150), (232, 120)], 6)
    for x, y, r in ((74, 172, 13), (140, 162, 17), (198, 168, 11), (156, 104, 10)):
        c.ell(x, y, r, r * .8, 'gold', w=5, shade=False)
@item
def eggs(c):
    c.ell(166, 112, 50, 62, 'cream', rot=.2); c.ell(90, 130, 54, 68, 'white', rot=-.15)
    c.poly([(20, 176), (236, 176), (220, 232), (36, 232)], 'taupe', step=12)
    for x in (74, 128, 182):
        c.arc(x, 176, 24, 0, 180, 5.5, ry=14)
@item
def milk(c):
    bottle(c, 'white', 'sky', 'brick', neck=34, shoulder=100, liquid=None)
    c.text('M', 128, 170, 44, 'white')
@item
def cream(c):
    c.poly([(60, 96), (196, 96), (184, 222), (72, 222)], 'white', step=12, inner=lambda: c.fill([(40, 140), (216, 140), (216, 190), (40, 190)], 'sky'))
    c.lump(128, 76, 64, 34, 'cream', bumps=5, depth=.3); c.poly([(118, 52), (132, 24), (146, 54)], 'cream', step=8, w=6)
    c.rr(52, 90, 152, 18, 8, 'silver')
@item
def oil(c):
    bottle(c, 'pale', 'cream', 'dgreen', neck=22, shoulder=120, liquid='yellow')
    c.ell(128, 176, 12, 17, 'lime', w=4.5)
@item
def vinegar(c):
    bottle(c, 'pale', 'white', 'wood', neck=20, shoulder=110, liquid='sand')
    c.text('%', 128, 180, 34, 'brick')
@item
def vodka(c):
    bottle(c, 'white', 'brick', 'silver', neck=24, shoulder=96, liquid='pale')
    c.ln([(100, 150), (156, 150)], 5, 'cream', amp=.2); c.ln([(100, 166), (156, 166)], 5, 'cream', amp=.2)
@item
def water(c):
    bottle(c, 'pale', None, 'blue', neck=30, shoulder=92, liquid='sky')
    c.rr(80, 130, 96, 44, 4, 'blue', w=5.5)
    c.poly([(128, 138), (140, 154), (128, 166), (116, 154)], 'white', w=0, shade=False)
@item
def beer(c):
    c.ell(196, 150, 36, 46, 'silver'); c.ell(196, 150, 17, 26, None, w=6)
    c.poly([(52, 80), (188, 80), (184, 226), (56, 226)], 'gold', step=12, inner=lambda: [c.fill([(x, 100), (x + 12, 100), (x + 12, 240), (x, 240)], 'yellow') for x in (78, 114, 150)])
    c.lump(120, 70, 80, 36, 'white', bumps=6, depth=.3); c.lump(70, 104, 20, 22, 'white', bumps=4, depth=.2, w=6)
@item
def tea(c):
    c.ell(198, 146, 32, 34, 'white'); c.ell(198, 146, 14, 16, None, w=6)
    c.poly([(40, 100), (200, 100), (180, 190), (150, 214), (90, 214), (60, 190)], 'white', step=12, inner=lambda: c.fill([(40, 150), (200, 150), (200, 162), (40, 162)], 'brick'))
    c.ell(120, 100, 80, 18, 'orange')
    c.ln([(150, 100), (160, 64), (150, 44)], 4.5); c.rr(132, 26, 34, 26, 4, 'lime', w=5)
    c.ell(120, 224, 100, 14, 'silver', w=6)
@item
def coffee(c):
    c.ell(198, 146, 32, 34, 'cream'); c.ell(198, 146, 14, 16, None, w=6)
    c.poly([(40, 100), (200, 100), (180, 190), (150, 214), (90, 214), (60, 190)], 'cream', step=12)
    c.ell(120, 100, 80, 18, 'brown')
    for x in (96, 136):
        c.ln([(x, 76), (x + 10, 60), (x, 44), (x + 10, 28)], 5.5, 'grey2', amp=.4, step=6)
    for x, y, rot in ((44, 226, .4), (206, 226, -.4)):
        c.ell(x, y, 17, 11, 'brown', rot=rot, w=5); c.ln([P(x, y, 12, math.degrees(rot) + 180), P(x, y, 12, math.degrees(rot))], 3, 'ink', amp=.2)
# ---- meat and fish --------------------------------------------------------------------------------
@item
def ham(c):
    c.rr(180, 52, 54, 20, 9, 'white'); c.ell(232, 50, 13, 13, 'white'); c.ell(232, 74, 13, 13, 'white')
    c.poly([(40, 150), (70, 96), (140, 70), (196, 76), (190, 120), (176, 176), (120, 214), (60, 204)], 'brick', step=10)
    c.ell(92, 156, 50, 46, 'skin', rot=-.5, inner=lambda: c.fe(92, 156, 14, 12, 'white'))
@item
def meat(c):
    c.poly([(30, 120), (70, 70), (150, 60), (214, 96), (230, 150), (196, 198), (110, 206), (50, 176)], 'brick', step=10,
           inner=lambda: (c.fe(160, 110, 20, 16, 'white'), c.fill([(40, 150), (100, 136), (150, 160), (210, 150), (212, 158), (150, 170), (100, 146), (40, 160)], 'skin')))
    c.ell(160, 110, 20, 16, None, w=5); c.fe(160, 110, 7, 6, 'mauve')
@item
def sausage(c):
    def link(x0, y0, x1, y1):
        c.ln([(x0, y0), ((x0 + x1) / 2, (y0 + y1) / 2 + 26), (x1, y1)], 52, 'ink', amp=.4, step=10)
        c.ln([(x0, y0), ((x0 + x1) / 2, (y0 + y1) / 2 + 26), (x1, y1)], 38, 'brick', amp=.4, step=10)
        c.ln([(x0 + 4, y0 - 8), ((x0 + x1) / 2, (y0 + y1) / 2 + 14)], 5, 'orange', amp=.4)
    c.ln([(36, 90), (20, 70)], 6); c.ln([(220, 90), (238, 70)], 6)
    link(48, 104, 128, 110); link(128, 110, 208, 104)
    c.ln([(128, 86), (128, 136)], 7, 'cream', amp=.2)
    link(70, 176, 190, 176)
@item
def fish(c):
    c.poly([(196, 128), (244, 84), (236, 128), (244, 172)], 'navy', step=10)
    c.poly([(110, 78), (150, 50), (160, 84)], 'navy', step=9)
    c.poly([(20, 128), (60, 92), (120, 76), (176, 96), (206, 128), (176, 160), (120, 180), (60, 164)], 'blue', step=10,
           inner=lambda: c.fill([(20, 140), (210, 140), (210, 190), (20, 190)], 'sky'))
    c.eye(64, 118, 7); c.arc(92, 128, 22, -60, 60, 5, ry=36)
    for x in (126, 152):
        c.arc(x, 128, 10, -70, 70, 4, 'navy', ry=20)
@item
def canned_herring(c):
    c.poly([(30, 110), (226, 110), (226, 190), (30, 190)], 'silver', step=14, inner=lambda: c.fill([(20, 140), (240, 140), (240, 176), (20, 176)], 'navy'))
    c.ell(128, 190, 98, 22, 'silver'); c.fill([(32, 170), (224, 170), (224, 190), (32, 190)], 'ice'); c.ln([(30, 176), (226, 176)], 0)
    c.ell(128, 110, 98, 26, 'slate')
    for y, x in ((104, 84), (116, 150)):
        c.ell(x, y, 40, 9, 'sky', w=4.5, shade=False); c.dot(x - 28, y - 1, 2.6)
    c.poly([(190, 92), (236, 50), (214, 44), (176, 84)], 'silver', step=9, w=6); c.ell(232, 42, 12, 12, None, w=6)
@item
def aspic(c):
    c.ell(128, 196, 110, 28, 'white')
    c.poly([(48, 190), (60, 110), (92, 80), (164, 80), (196, 110), (208, 190)], 'sand', step=10,
           inner=lambda: [c.ell(x, y, rx, ry, colr, w=3.5, color='wood', shade=False) for x, y, rx, ry, colr in ((96, 130, 16, 12, 'orange'), (150, 120, 14, 14, 'white'), (150, 120, 6, 6, 'yellow'), (120, 166, 18, 10, 'skin'), (172, 160, 10, 8, 'green'))])
    c.hi([(74, 120), (82, 100)], 5, 'cream')
# ---- nuts ----------------------------------------------------------------------------------------
@item
def hazelnuts(c):
    def nut(x, y, s=1.0):
        c.poly([(x - 44 * s, y - 20 * s), (x, y - 34 * s), (x + 44 * s, y - 20 * s), (x + 40 * s, y + 20 * s), (x, y + 56 * s), (x - 40 * s, y + 20 * s)], 'wood', step=9)
        c.lump(x, y - 28 * s, 46 * s, 20 * s, 'sand', bumps=6, depth=.3, w=6); c.hi([(x - 22 * s, y + 8 * s), (x - 16 * s, y - 4 * s)], 4.5, 'gold')
    nut(172, 110, .85); nut(88, 150, 1.05)
@item
def walnuts(c):
    def nut(x, y, r, colr='sand'):
        c.ell(x, y, r, r * .9, colr)
        c.ln([(x, y - r * .86), (x, y + r * .86)], 5.5, 'wood', amp=1.2, step=8)
        for sg in (-1, 1):
            c.arc(x + sg * r * .45, y, r * .28, 60, 300, 4.5, 'wood', ry=r * .5)
    nut(170, 104, 54); nut(92, 156, 60, 'wood' if False else 'sand')
@item
def missing(c):
    c.rr(36, 36, 184, 184, 26, 'white'); c.text('?', 128, 176, 140, 'taupe')


RENAME = {}


def make():
    for i, (name, f) in enumerate(D.items()):
        c = Sp(700 + i); f(c)
        if name == 'carrot':
            c.out(ROOT / 'Assets/Lemko/Sprites/Products', 'carrot')
        else:
            c.out(PROD, name)


if __name__ == '__main__':
    import sk
    if len(sys.argv) > 1:
        sk.PREVIEW = sys.argv[1]
    make()
    if len(sys.argv) > 1:
        n = list(D)
        sheet(n[:40], PROD, sys.argv[1] + '/_food1.png', cols=8, cell=170); sheet(n[40:], PROD, sys.argv[1] + '/_food2.png', cols=8, cell=170)
