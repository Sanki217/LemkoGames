"""House things, furniture and rooms (74 pictures)."""
import math
from sk import *

D = {}


def item(f):
    D[f.__name__] = f
    return f


def turned(c, deg, f, cx=128, cy=128):
    cr = c.c; cr.save(); cr.translate(cx, cy); cr.rotate(math.radians(deg)); cr.translate(-cx, -cy); f(); cr.restore()


def scaled(c, f, x, y, s):
    cr = c.c; cr.save(); cr.translate(x, y); cr.scale(s, s); f(c); cr.restore()


def knob(c, x, y, colr='yellow', r=6):
    c.ell(x, y, r, r, colr, w=4.5, shade=False)


# ---- bathroom ---------------------------------------------------------------------------
@item
def bathtub(c):
    c.ln([(196, 108), (196, 70), (172, 70), (172, 84)], 8, 'ink'); c.ln([(196, 106), (196, 72), (173, 72)], 3.5, 'silver')
    for x in (66, 190):
        c.ell(x, 196, 10, 12, 'gold')
    c.poly([(30, 116), (226, 116), (212, 176), (196, 192), (60, 192), (44, 176)], 'white', step=14)
    c.rr(22, 102, 212, 22, 10, 'white')
    for x, y, r in ((80, 88, 14), (104, 78, 10), (122, 92, 9)):
        c.ell(x, y, r, r, 'pale', w=5)


@item
def sink(c):
    c.poly([(104, 150), (152, 150), (146, 226), (110, 226)], 'silver', step=12)
    c.poly([(40, 110), (216, 110), (196, 156), (60, 156)], 'white', step=14)
    c.rr(32, 96, 192, 22, 10, 'white')
    c.ln([(128, 96), (128, 56), (164, 56), (164, 72)], 9, 'ink'); c.ln([(128, 94), (128, 58), (163, 58)], 4, 'silver')
    c.ell(104, 84, 8, 8, 'brick', w=4.5, shade=False); c.ell(150, 84, 8, 8, 'blue', w=4.5, shade=False)


@item
def tap(c):
    c.poly([(60, 150), (60, 112), (150, 112), (186, 130), (192, 176), (160, 176), (158, 146), (140, 140), (92, 140), (92, 150)], 'silver', step=12)
    c.rr(98, 78, 22, 36, 6, 'silver'); c.rr(74, 62, 70, 20, 9, 'brick')
    c.rr(38, 104, 26, 54, 8, 'grey2')
    c.poly([(176, 196), (188, 220), (176, 232), (164, 220)], 'sky', step=8, w=6)


@item
def soap(c):
    c.rr(48, 120, 160, 84, 34, 'mint')
    c.hi([(80, 142), (130, 138)])
    for x, y, r in ((176, 92, 16), (204, 70, 10), (150, 78, 8)):
        c.ell(x, y, r, r, 'white', w=5)


@item
def shampoo(c):
    c.ln([(128, 74), (128, 44), (166, 44), (166, 56)], 9); c.rr(110, 62, 36, 22, 6, 'silver')
    c.rr(72, 82, 112, 150, 26, 'blue')
    c.rr(88, 132, 80, 54, 10, 'cream', w=5.5); c.poly([(128, 144), (140, 162), (128, 174), (116, 162)], 'sky', w=4.5, step=8, shade=False)


@item
def toothbrush(c):
    def f():
        c.rr(34, 114, 190, 22, 10, 'sky')
        c.rr(34, 92, 60, 26, 6, 'white')
        for x in (48, 62, 76):
            c.ln([(x, 96), (x, 114)], 3.5, 'ice', amp=.2)
    turned(c, -35, f)


@item
def toothpaste(c):
    def f():
        c.poly([(40, 96), (176, 106), (176, 150), (40, 160)], 'white', step=14, inner=lambda: c.fill([(90, 90), (130, 90), (130, 170), (90, 170)], 'blue'))
        c.ln([(42, 98), (42, 158)], 9); c.rr(176, 110, 22, 36, 5, 'silver'); c.rr(196, 114, 22, 28, 6, 'brick')
    turned(c, -25, f)


@item
def towel(c):
    c.ln([(34, 62), (222, 62)], 11); c.ln([(36, 62), (220, 62)], 4.5, 'silver')
    c.rr(62, 52, 132, 170, 12, 'mint', inner=lambda: [c.fill([(60, y), (196, y), (196, y + 12), (60, y + 12)], 'white') for y in (160, 184)])
    c.ln([(64, 76), (192, 76)], 5)


@item
def mirror(c):
    c.ell(128, 128, 76, 100, 'wood')
    c.ell(128, 128, 58, 82, 'pale', inner=lambda: c.fill([(90, 190), (150, 50), (176, 50), (116, 190)], 'white'))


@item
def comb(c):
    c.rr(36, 84, 184, 38, 12, 'wood')
    for i in range(11):
        x = 50 + i * 15.6
        c.ln([(x, 122), (x, 176)], 7); c.ln([(x, 122), (x, 172)], 2.5, 'wood', amp=.2)


@item
def hairbrush(c):
    def f():
        c.rr(34, 116, 96, 24, 10, 'wood')
        c.ell(168, 128, 58, 40, 'brown', inner=lambda: c.fe(168, 128, 42, 26, 'cream'))
        for i in range(4):
            for j in range(3):
                c.dot(140 + i * 18 + (j % 2) * 8, 114 + j * 14, 3.4)
    turned(c, -40, f)


@item
def washing_machine(c):
    c.rr(48, 34, 160, 194, 14, 'white')
    c.ln([(50, 78), (206, 78)], 6)
    knob(c, 78, 56, 'brick', 8); knob(c, 104, 56, 'yellow', 8); c.rr(140, 46, 50, 20, 5, 'slate', w=4.5, shade=False)
    c.ell(128, 152, 54, 54, 'silver')
    c.ell(128, 152, 38, 38, 'sky', inner=lambda: (c.fe(128, 172, 44, 22, 'blue'), c.fe(112, 140, 7, 7, 'white')))


# ---- kitchen ------------------------------------------------------------------------------
@item
def bowl(c):
    c.poly([(30, 106), (226, 106), (200, 178), (166, 200), (90, 200), (56, 178)], 'pale', step=14,
           inner=lambda: [c.fill([(30, y), (226, y), (226, y + 10), (30, y + 10)], 'brick') for y in (140,)])
    c.ell(128, 106, 100, 24, 'white', inner=lambda: c.fe(128, 112, 84, 14, 'pale'))


@item
def plate(c):
    c.ell(128, 134, 106, 66, 'white')
    c.ell(128, 130, 68, 40, 'pale', w=5.5)
    for a in range(0, 360, 30):
        c.dot(128 + math.cos(math.radians(a)) * 88, 132 + math.sin(math.radians(a)) * 53, 3.5, 'brick')


@item
def glass(c):
    c.poly([(60, 44), (196, 44), (176, 220), (80, 220)], 'white', step=14, inner=lambda: c.fill([(40, 110), (216, 110), (216, 240), (40, 240)], 'pale'))
    c.ln([(68, 110), (188, 110)], 5, 'mint'); c.hi([(86, 70), (92, 150)])


@item
def mug(c):
    c.ell(196, 140, 34, 40, 'sand'); c.ell(196, 140, 15, 20, None, w=6)
    c.poly([(48, 78), (190, 78), (178, 214), (60, 214)], 'sand', step=14, inner=lambda: c.fill([(40, 150), (200, 150), (200, 172), (40, 172)], 'brick'))
    c.ell(119, 78, 71, 15, 'brown')
    for x in (96, 128):
        c.ln([(x, 56), (x + 8, 44), (x, 32), (x + 8, 20)], 5.5, 'grey2', amp=.4, step=6)


@item
def jug(c):
    c.ell(190, 138, 34, 48, 'sand'); c.ell(190, 138, 15, 28, None, w=6)
    c.poly([(70, 60), (40, 44), (60, 80), (56, 190), (76, 222), (164, 222), (184, 190), (178, 60)], 'sand', step=14,
           inner=lambda: (c.fill([(40, 120), (200, 120), (200, 136), (40, 136)], 'blue'), c.fill([(40, 150), (200, 150), (200, 158), (40, 158)], 'brick')))
    c.ln([(64, 62), (176, 62)], 5)


@item
def kettle(c):
    c.poly([(190, 120), (232, 92), (240, 108), (204, 150)], 'mint', step=10)
    c.arc(116, 96, 56, 200, 340, 9, ry=60); c.arc(116, 96, 56, 204, 336, 3.5, 'brown', ry=60)
    c.ell(118, 152, 82, 68, 'mint')
    c.rr(78, 78, 80, 20, 9, 'sage'); knob(c, 118, 68, 'brick', 10)
    c.hi([(70, 130), (64, 160)])


@item
def pot(c):
    c.rr(22, 112, 34, 20, 8, 'slate'); c.rr(200, 112, 34, 20, 8, 'slate')
    c.poly([(46, 98), (210, 98), (204, 196), (184, 214), (72, 214), (52, 196)], 'blue', step=14)
    c.poly([(40, 100), (58, 72), (198, 72), (216, 100)], 'sky', step=12); knob(c, 128, 60, 'brick', 12)
    c.hi([(70, 124), (70, 170)], color='sky')


@item
def frying_pan(c):
    def f():
        c.rr(150, 116, 96, 24, 10, 'wood')
        c.ell(92, 128, 82, 62, 'slate', inner=lambda: c.fe(92, 124, 64, 46, 'grey1'))
        c.ell(84, 124, 34, 26, 'white', w=5); c.ell(88, 124, 13, 12, 'yellow', w=4.5)
    turned(c, -25, f)


@item
def stove(c):
    c.rr(44, 40, 168, 190, 12, 'white')
    c.ln([(46, 86), (210, 86)], 6)
    for x in (72, 108, 148, 184):
        knob(c, x, 64, 'slate', 8)
    c.rr(66, 106, 124, 88, 10, 'slate', inner=lambda: (c.fe(128, 186, 50, 26, 'orange'), c.fe(128, 192, 28, 16, 'yellow')))
    c.ln([(78, 206), (178, 206)], 8); c.ln([(80, 206), (176, 206)], 3, 'silver')


@item
def refrigerator(c):
    c.rr(62, 18, 132, 222, 16, 'pale')
    c.ln([(64, 92), (192, 92)], 6)
    c.ln([(84, 50), (84, 76)], 9); c.ln([(84, 110), (84, 160)], 9)
    c.ln([(84, 52), (84, 74)], 3.5, 'silver'); c.ln([(84, 112), (84, 158)], 3.5, 'silver')
    c.rr(140, 40, 30, 22, 4, 'yellow', w=4.5, shade=False)


@item
def fork(c):
    def f():
        c.rr(28, 118, 120, 20, 9, 'wood')
        c.poly([(146, 108), (176, 104), (232, 104), (232, 114), (188, 116), (232, 122), (232, 134), (188, 136), (232, 142), (232, 152), (176, 152), (146, 148)], 'silver', step=10)
    turned(c, -45, f)


@item
def spoon(c):
    def f():
        c.rr(26, 118, 140, 20, 9, 'silver')
        c.ell(194, 128, 42, 30, 'silver', inner=lambda: c.fe(198, 124, 26, 16, 'white'))
    turned(c, -45, f)


@item
def knife(c):
    def f():
        c.poly([(110, 112), (226, 112), (238, 126), (214, 146), (110, 146)], 'silver', step=12, inner=lambda: c.fill([(110, 132), (240, 132), (240, 150), (110, 150)], 'white'))
        c.rr(22, 110, 92, 38, 12, 'brown'); c.dot(46, 129, 4, 'sand'); c.dot(76, 129, 4, 'sand')
    turned(c, -45, f)


@item
def rolling_pin(c):
    def f():
        c.rr(16, 116, 50, 24, 10, 'brown'); c.rr(190, 116, 50, 24, 10, 'brown')
        c.rr(56, 98, 144, 60, 16, 'sand'); c.hi([(76, 112), (170, 112)], color='cream')
    turned(c, -30, f)


@item
def board(c):
    def f():
        c.rr(40, 70, 190, 116, 22, 'wood', inner=lambda: [c.fill([(40, y), (232, y + 6), (232, y + 10), (40, y + 4)], 'brown') for y in (100, 132, 160)])
        c.rr(10, 110, 44, 36, 12, 'wood'); c.ell(28, 128, 7, 7, 'cream', w=4.5, shade=False)
    turned(c, -18, f)


@item
def bottle(c):
    c.rr(108, 22, 40, 26, 6, 'wood')
    c.poly([(110, 46), (146, 46), (150, 90), (182, 122), (182, 216), (166, 232), (90, 232), (74, 216), (74, 122), (106, 90)], 'mint', step=14)
    c.rr(86, 140, 84, 58, 8, 'cream', w=5.5); c.ell(128, 169, 14, 14, 'brick', w=4.5)
    c.hi([(92, 122), (100, 112)])


@item
def can(c):
    c.poly([(60, 60), (196, 60), (196, 204), (60, 204)], 'silver', step=16,
           inner=lambda: (c.fill([(50, 96), (206, 96), (206, 176), (50, 176)], 'brick'), c.fe(128, 136, 30, 30, 'cream')))
    c.ell(128, 204, 68, 18, 'silver', w=LW); c.fill([(62, 190), (194, 190), (194, 204), (62, 204)], 'ice')
    c.ell(128, 60, 68, 18, 'white')
    c.ln([(60, 96), (196, 96)], 5); c.ln([(60, 176), (196, 176)], 5)


# ---- furniture ------------------------------------------------------------------------------
@item
def cabinet(c):
    for x in (64, 180):
        c.rr(x, 196, 14, 30, 4, 'brown')
    c.rr(36, 54, 184, 150, 10, 'wood')
    c.ln([(128, 58), (128, 200)], 6)
    c.rr(52, 72, 62, 114, 6, 'gold', w=5); c.rr(142, 72, 62, 114, 6, 'gold', w=5)
    knob(c, 116, 130); knob(c, 140, 130)


@item
def chest(c):
    for x in (56, 188):
        c.rr(x, 204, 14, 24, 4, 'brown')
    c.rr(36, 44, 184, 168, 10, 'wood')
    for y in (60, 110, 160):
        c.rr(52, y, 152, 38, 6, 'sand', w=5); knob(c, 100, y + 19, 'brick'); knob(c, 156, y + 19, 'brick')


@item
def shelf(c):
    c.rr(40, 30, 176, 200, 8, 'wood')
    for y in (44, 106, 168):
        c.rr(54, y, 148, 48, 4, 'cream', w=5)
    for x, h, colr in ((62, 40, 'brick'), (76, 34, 'blue'), (90, 42, 'green'), (104, 36, 'gold')):
        c.rr(x, 92 - h, 13, h, 2, colr, w=4.5, shade=False, step=8)
    c.ell(168, 80, 14, 12, 'mint', w=5)
    for x, h, colr in ((120, 38, 'navy'), (134, 42, 'orange'), (148, 34, 'sage'), (162, 40, 'mauve'), (176, 36, 'yellow')):
        c.rr(x, 154 - h, 13, h, 2, colr, w=4.5, shade=False, step=8)
    c.rr(66, 190, 44, 26, 4, 'sand', w=4.5); c.ell(160, 200, 16, 14, 'orange', w=5)


@item
def wardrobe(c):
    for x in (62, 180):
        c.rr(x, 222, 14, 18, 4, 'brown')
    c.rr(44, 16, 168, 212, 12, 'wood')
    c.ln([(128, 20), (128, 180)], 6); c.ln([(46, 180), (210, 180)], 6)
    c.rr(58, 32, 58, 136, 6, 'gold', w=5); c.rr(140, 32, 58, 136, 6, 'gold', w=5)
    knob(c, 116, 104); knob(c, 140, 104); knob(c, 128, 204, 'yellow', 7)


@item
def bed(c):
    c.rr(22, 70, 26, 150, 8, 'wood'); c.rr(210, 124, 24, 96, 8, 'wood')
    c.rr(40, 130, 180, 62, 10, 'white')
    c.rr(50, 106, 66, 40, 16, 'cream')
    c.poly([(104, 128), (222, 128), (222, 196), (104, 196), (110, 160)], 'blue', step=14,
           inner=lambda: [c.fe(x, y, 8, 8, 'sky') for x, y in ((140, 150), (180, 146), (160, 176), (204, 174))])


@item
def chair(c):
    c.rr(70, 26, 18, 200, 6, 'wood'); c.rr(176, 132, 16, 96, 6, 'wood')
    c.rr(70, 40, 100, 20, 6, 'wood'); c.rr(70, 78, 100, 20, 6, 'wood')
    c.rr(152, 40, 18, 100, 6, 'wood')
    c.poly([(62, 128), (198, 128), (204, 152), (62, 152)], 'brick', step=14)


@item
def table(c):
    for x in (46, 194):
        c.rr(x, 110, 18, 122, 6, 'brown')
    c.rr(20, 86, 216, 30, 10, 'wood')
    c.rr(54, 112, 148, 18, 4, 'brown', w=5.5)


@item
def small_table(c):
    for x in (74, 166):
        c.rr(x, 150, 16, 80, 6, 'brown')
    c.ell(128, 146, 84, 22, 'wood')
    c.poly([(108, 136), (104, 96), (152, 96), (148, 136)], 'pale', step=10, w=6)
    c.ln([(128, 96), (120, 60)], 5.5, 'dgreen'); c.ell(118, 52, 12, 12, 'brick', w=5); c.ln([(134, 96), (146, 70)], 5.5, 'dgreen'); c.ell(150, 62, 10, 10, 'yellow', w=5)


@item
def stool(c):
    c.poly([(84, 110), (64, 226), (82, 226), (100, 112)], 'brown', step=12)
    c.poly([(172, 110), (192, 226), (174, 226), (156, 112)], 'brown', step=12)
    c.rr(120, 110, 16, 100, 5, 'brown'); c.ln([(78, 176), (178, 176)], 8)
    c.ell(128, 100, 78, 24, 'wood')


@item
def sofa(c):
    for x in (54, 190):
        c.rr(x, 198, 16, 26, 4, 'brown')
    c.rr(40, 64, 176, 100, 26, 'sage')
    c.rr(48, 132, 160, 60, 14, 'green'); c.ln([(128, 134), (128, 188)], 5.5)
    c.rr(16, 112, 44, 92, 18, 'sage'); c.rr(196, 112, 44, 92, 18, 'sage')
    c.rr(86, 96, 46, 40, 10, 'yellow', w=5.5)


@item
def carpet(c):
    c.poly([(60, 76), (236, 76), (196, 190), (20, 190)], 'brick', step=14,
           inner=lambda: (c.fill([(84, 98), (206, 98), (178, 168), (56, 168)], 'cream'), c.fill([(112, 118), (176, 118), (160, 150), (96, 150)], 'navy')))
    for i in range(9):
        c.ln([(26 + i * 21, 190), (22 + i * 21, 206)], 5, amp=.2); c.ln([(66 + i * 21, 76), (70 + i * 21, 60)], 5, amp=.2)


@item
def pillow(c):
    c.poly([(36, 84), (60, 72), (128, 82), (196, 72), (220, 84), (212, 128), (220, 176), (196, 190), (128, 180), (60, 190), (36, 176), (44, 128)], 'cream', step=12,
           inner=lambda: [c.fe(x, y, 7, 7, 'orange') for x, y in ((90, 110), (128, 132), (166, 110), (90, 156), (166, 156))])


# ---- living things and light ------------------------------------------------------------------
@item
def lamp(c):
    c.ell(128, 226, 54, 12, 'brown'); c.rr(120, 96, 16, 130, 5, 'brown')
    c.poly([(84, 26), (172, 26), (206, 108), (50, 108)], 'yellow', step=14)
    c.ln([(176, 110), (176, 136)], 4.5); c.dot(176, 140, 5.5, 'brick')


@item
def small_lamp(c):
    c.ell(128, 210, 46, 22, 'sage'); c.rr(118, 130, 20, 70, 6, 'pine')
    c.poly([(92, 56), (164, 56), (196, 136), (60, 136)], 'green', step=14, inner=lambda: c.fill([(50, 116), (206, 116), (206, 124), (50, 124)], 'yellow'))
    for a in (-150, -90, -30):
        c.ln([(128 + math.cos(math.radians(a)) * 70, 70 + math.sin(math.radians(a)) * 50), (128 + math.cos(math.radians(a)) * 92, 70 + math.sin(math.radians(a)) * 70)], 5.5, 'gold', amp=.3)


@item
def light_switch(c):
    c.rr(60, 34, 136, 188, 20, 'cream')
    c.rr(98, 76, 60, 104, 12, 'white', inner=lambda: c.fill([(90, 128), (166, 128), (166, 190), (90, 190)], 'silver'))
    c.ln([(100, 128), (156, 128)], 5); knob(c, 128, 54, 'grey2', 4); knob(c, 128, 204, 'grey2', 4)


@item
def plant(c):
    c.ln([(128, 150), (124, 96)], 7, 'dgreen')
    for (x, y), rot, colr in (((92, 86), .7, 'green'), ((166, 74), -.7, 'dgreen'), ((120, 50), 1.5, 'lime'), ((84, 126), .25, 'dgreen'), ((174, 118), -.3, 'green')):
        c.ell(x, y, 38, 17, colr, rot=rot)
    c.poly([(74, 150), (182, 150), (170, 226), (86, 226)], 'orange', step=12); c.rr(66, 140, 124, 24, 8, 'orange')


@item
def window(c):
    c.rr(40, 30, 176, 180, 8, 'wood')
    c.rr(56, 46, 144, 148, 4, 'pale', w=5.5, inner=lambda: (c.fill([(56, 160), (110, 110), (140, 140), (170, 116), (200, 150), (200, 200), (56, 200)], 'sage'), c.fe(168, 80, 15, 15, 'yellow')))
    c.ln([(128, 46), (128, 194)], 7); c.ln([(56, 120), (200, 120)], 7)
    c.rr(28, 206, 200, 20, 6, 'wood')


@item
def door(c):
    c.rr(60, 16, 136, 224, 8, 'wood')
    c.rr(78, 34, 100, 80, 6, 'sand', w=5.5); c.rr(78, 130, 100, 90, 6, 'gold', w=5.5)
    knob(c, 168, 128, 'yellow', 9)


@item
def stairs(c):
    pts = [(30, 226)]
    x, y = 30, 190
    for i in range(4):
        pts += [(x, y), (x + 50, y)]; x += 50; y -= 40
    pts += [(230, y + 40), (230, 226)]
    c.poly(pts, 'wood', step=14, inner=lambda: [c.fill([(30 + i * 50, 190 - i * 40), (80 + i * 50, 190 - i * 40), (80 + i * 50, 202 - i * 40), (30 + i * 50, 202 - i * 40)], 'sand') for i in range(4)])
    c.ln([(30, 150), (180, 30)], 8); c.ln([(30, 150), (30, 190)], 7); c.ln([(180, 30), (180, 70)], 7)


@item
def wall(c):
    def bricks():
        for r in range(5):
            y = 44 + r * 34
            c.fill([(30, y - 3), (226, y - 3), (226, y + 3), (30, y + 3)], 'cream')
            for x in range(30 + (r % 2) * 32, 230, 64):
                c.fill([(x - 3, y), (x + 3, y), (x + 3, y + 34), (x - 3, y + 34)], 'cream')
    c.rr(30, 44, 196, 170, 6, 'orange', inner=bricks, shade='brick')


@item
def floor(c):
    c.poly([(74, 70), (240, 104), (182, 196), (16, 162)], 'wood', step=14,
           inner=lambda: [c.fill([(74 - k * 14.5 + 0, 70 + k * 23), (240 - k * 14.5, 104 + k * 23), (240 - k * 14.5, 108 + k * 23), (74 - k * 14.5, 74 + k * 23)], 'brown') for k in range(1, 4)])
    c.ln([(150, 86), (136, 108)], 4.5); c.ln([(90, 122), (76, 144)], 4.5); c.ln([(176, 138), (162, 162)], 4.5)


@item
def ceiling(c):
    c.poly([(20, 30), (236, 30), (200, 78), (56, 78)], 'white', step=14)
    c.ln([(56, 78), (56, 110)], 6); c.ln([(200, 78), (200, 110)], 6)
    c.ln([(128, 60), (128, 136)], 6)
    c.poly([(96, 136), (160, 136), (184, 196), (72, 196)], 'yellow', step=12)
    c.ell(128, 200, 20, 16, 'cream', w=5.5)
    for a in (40, 90, 140):
        c.ln([(128 + math.cos(math.radians(a)) * 34, 204 + math.sin(math.radians(a)) * 26), (128 + math.cos(math.radians(a)) * 52, 204 + math.sin(math.radians(a)) * 44)], 5.5, 'gold', amp=.3)


@item
def fireplace(c):
    c.rr(22, 34, 212, 28, 8, 'wood')
    def bricks():
        for r in range(6):
            y = 60 + r * 28
            c.fill([(30, y - 2.5), (226, y - 2.5), (226, y + 2.5), (30, y + 2.5)], 'cream')
            for x in range(36 + (r % 2) * 24, 230, 48):
                c.fill([(x - 2.5, y), (x + 2.5, y), (x + 2.5, y + 28), (x - 2.5, y + 28)], 'cream')
    c.rr(36, 58, 184, 170, 6, 'brick', inner=bricks, shade='brown')
    c.poly([(76, 226), (76, 136), (100, 108), (156, 108), (180, 136), (180, 226)], 'slate', step=12)
    c.poly([(100, 222), (92, 180), (112, 190), (124, 140), (146, 184), (160, 166), (162, 222)], 'orange', step=10, w=6)
    c.poly([(116, 222), (118, 196), (130, 178), (142, 200), (144, 222)], 'yellow', step=8, w=0, shade=False)
    c.rr(88, 214, 82, 14, 5, 'brown', w=5)


@item
def house(c):
    c.rr(160, 34, 24, 50, 4, 'brick')
    c.rr(50, 112, 156, 116, 6, 'cream')
    c.poly([(26, 122), (128, 30), (230, 122)], 'brick', step=14, inner=lambda: [c.fill([(20, y), (240, y), (240, y + 5), (20, y + 5)], 'brown') for y in (70, 96)])
    c.rr(110, 156, 38, 72, 6, 'wood'); knob(c, 138, 194, 'yellow', 5)
    c.rr(64, 140, 34, 34, 4, 'pale', w=5.5); c.rr(160, 140, 34, 34, 4, 'pale', w=5.5)
    c.ln([(81, 140), (81, 174)], 4); c.ln([(177, 140), (177, 174)], 4)


# ---- things -----------------------------------------------------------------------------------
@item
def books(c):
    c.rr(40, 164, 180, 44, 8, 'green', inner=lambda: c.fill([(180, 160), (200, 160), (200, 212), (180, 212)], 'cream'))
    c.rr(56, 116, 164, 44, 8, 'brick', inner=lambda: c.fill([(76, 110), (94, 110), (94, 164), (76, 164)], 'cream'))
    c.rr(36, 68, 172, 44, 8, 'blue', inner=lambda: c.fill([(170, 64), (188, 64), (188, 116), (170, 116)], 'cream'))


@item
def notebook(c):
    c.rr(56, 28, 152, 204, 10, 'sand')
    c.rr(86, 60, 96, 40, 6, 'white', w=5)
    for y in range(44, 224, 22):
        c.ell(56, y, 12, 7, 'silver', w=4.5, shade=False)
    c.ln([(96, 130), (180, 130)], 4.5, 'wood'); c.ln([(96, 156), (180, 156)], 4.5, 'wood'); c.ln([(96, 182), (150, 182)], 4.5, 'wood')


@item
def paper(c):
    c.poly([(54, 22), (160, 22), (204, 66), (204, 234), (54, 234)], 'white', step=16)
    c.poly([(160, 22), (160, 66), (204, 66)], 'silver', step=10, w=6)
    for y in (100, 130, 160, 190):
        c.ln([(78, y), (180 if y < 190 else 140, y)], 5, 'ice', amp=.5)


@item
def calendar(c):
    c.rr(40, 46, 176, 184, 12, 'white')
    c.rr(40, 46, 176, 50, 12, 'brick')
    for x in (84, 172):
        c.rr(x - 7, 26, 14, 40, 6, 'silver', w=5)
    c.text('17', 128, 196, 92)


@item
def clock(c):
    c.ell(128, 128, 104, 104, 'wood')
    c.ell(128, 128, 82, 82, 'cream')
    for a in range(0, 360, 30):
        r0 = 64 if a % 90 else 58
        c.ln([(128 + math.cos(math.radians(a)) * r0, 128 + math.sin(math.radians(a)) * r0), (128 + math.cos(math.radians(a)) * 72, 128 + math.sin(math.radians(a)) * 72)], 4.5, amp=0)
    c.ln([(128, 128), (128, 74)], 7); c.ln([(128, 128), (164, 146)], 7, 'brick'); c.dot(128, 128, 8)


@item
def painting(c):
    c.ln([(88, 44), (128, 14), (168, 44)], 5)
    c.rr(24, 44, 208, 172, 8, 'wood')
    c.rr(44, 64, 168, 132, 4, 'pale', w=5.5,
         inner=lambda: (c.fe(170, 96, 17, 17, 'yellow'), c.fill([(44, 196), (44, 150), (92, 104), (128, 150), (156, 126), (212, 176), (212, 196)], 'green'), c.fill([(80, 116), (92, 104), (104, 118), (92, 122)], 'white')))


@item
def radio(c):
    c.ln([(170, 70), (214, 20)], 6); c.dot(214, 20, 6)
    c.rr(24, 68, 208, 150, 20, 'wood')
    c.ell(88, 142, 44, 44, 'sand', inner=lambda: [c.fill([(40, y), (136, y), (136, y + 5), (40, y + 5)], 'brown') for y in range(104, 186, 14)])
    c.rr(148, 96, 66, 28, 6, 'cream', w=5); c.ln([(176, 98), (176, 122)], 4.5, 'brick', amp=0)
    knob(c, 162, 176, 'yellow', 12); knob(c, 200, 176, 'brick', 12)


@item
def television(c):
    c.ln([(128, 52), (92, 16)], 6); c.ln([(128, 52), (170, 20)], 6)
    c.poly([(84, 196), (72, 232), (88, 232), (100, 198)], 'brown', step=10); c.poly([(172, 196), (184, 232), (168, 232), (156, 198)], 'brown', step=10)
    c.rr(22, 50, 212, 154, 20, 'wood')
    c.rr(40, 68, 136, 118, 14, 'pale', w=5.5, inner=lambda: c.fill([(60, 190), (110, 60), (136, 60), (86, 190)], 'white'))
    knob(c, 204, 92, 'yellow', 10); knob(c, 204, 128, 'brick', 10)
    for y in (158, 170):
        c.ln([(192, y), (216, y)], 4, amp=.2)


@item
def computer(c):
    c.rr(108, 150, 40, 36, 4, 'silver'); c.rr(80, 180, 96, 16, 6, 'silver')
    c.rr(28, 24, 200, 134, 12, 'slate')
    c.rr(44, 40, 168, 102, 6, 'pale', w=5, inner=lambda: c.fill([(60, 150), (110, 30), (140, 30), (90, 150)], 'white'))
    c.poly([(40, 208), (216, 208), (232, 238), (24, 238)], 'silver', step=14)
    for i in range(6):
        c.dot(64 + i * 26, 222, 4, 'grey2')


@item
def speaker(c):
    c.rr(56, 18, 144, 222, 14, 'brown')
    c.ell(128, 76, 28, 28, 'slate', inner=lambda: c.fe(128, 76, 10, 10, 'silver'))
    c.ell(128, 164, 50, 50, 'slate', inner=lambda: (c.fe(128, 164, 30, 30, 'grey1'), c.fe(128, 164, 12, 12, 'silver')))


@item
def game(c):
    c.poly([(60, 76), (196, 76), (232, 110), (240, 176), (212, 200), (174, 160), (82, 160), (44, 200), (16, 176), (24, 110)], 'blue', step=12)
    c.rr(62, 104, 16, 44, 4, 'slate', w=4.5, shade=False); c.rr(48, 118, 44, 16, 4, 'slate', w=4.5, shade=False)
    knob(c, 176, 112, 'yellow', 9); knob(c, 198, 132, 'brick', 9); knob(c, 156, 132, 'lime', 9)


@item
def toys(c):
    c.poly([(128, 26), (176, 110), (80, 110)], 'green', step=12)
    c.rr(28, 124, 96, 96, 10, 'orange'); c.text('A', 76, 196, 60, 'cream')
    c.ell(184, 174, 54, 54, 'yellow', inner=lambda: c.fill([(120, 160), (250, 160), (250, 188), (120, 188)], 'brick'))


@item
def clothes(c):
    c.poly([(86, 40), (108, 52), (148, 52), (170, 40), (236, 86), (208, 126), (184, 110), (184, 222), (72, 222), (72, 110), (48, 126), (20, 86)], 'sage', step=14,
           inner=lambda: [c.fill([(60, y), (200, y), (200, y + 12), (60, y + 12)], 'white') for y in (140, 172)])
    c.arc(128, 46, 22, 10, 170, 6, ry=16)


@item
def shoes(c):
    def shoe(x, y):
        c.poly([(x, y), (x + 44, y - 4), (x + 60, y + 30), (x + 124, y + 44), (x + 136, y + 70), (x, y + 70)], 'brown', step=12)
        c.rr(x - 4, y + 64, 144, 16, 6, 'cream', w=5.5)
        for i in range(3):
            c.ln([(x + 56 + i * 14, y + 24 + i * 5), (x + 70 + i * 14, y + 14 + i * 5)], 4.5, 'cream', amp=.2)
    shoe(84, 60); shoe(28, 126)


@item
def broom(c):
    def f():
        c.rr(20, 120, 150, 16, 7, 'wood')
        c.poly([(166, 96), (234, 80), (240, 176), (166, 160)], 'yellow', step=12, inner=lambda: [c.fill([(180, 84 + i * 16), (244, 80 + i * 18), (244, 84 + i * 18), (180, 88 + i * 16)], 'gold') for i in range(1, 5)])
        c.rr(158, 94, 22, 68, 5, 'brick')
    turned(c, -50, f)


@item
def vacuum_cleaner(c):
    c.ln([(176, 150), (214, 120), (214, 44), (186, 30)], 13); c.ln([(176, 150), (214, 120), (214, 44), (188, 31)], 6, 'silver')
    c.rr(152, 18, 44, 22, 8, 'slate')
    c.ell(74, 208, 20, 20, 'slate'); c.ell(170, 208, 20, 20, 'slate')
    c.rr(28, 124, 176, 80, 34, 'brick'); c.rr(60, 144, 60, 20, 8, 'cream', w=5); knob(c, 160, 156, 'yellow', 9)
    c.ln([(30, 226), (70, 226)], 9)


# ---- rooms: a cutaway corner with the room's key thing --------------------------------------
def room(c, wall_col, floor_col, things, tiles=False):
    def wall_in():
        if tiles:
            for i in range(1, 6):
                c.fill([(22 + i * 36, 30), (26 + i * 36, 30), (26 + i * 36, 170), (22 + i * 36, 170)], 'white')
            for j in range(1, 4):
                c.fill([(20, 30 + j * 36), (240, 30 + j * 36), (240, 34 + j * 36), (20, 34 + j * 36)], 'white')
    c.rr(22, 30, 212, 140, 6, wall_col, inner=wall_in, shade=False)
    c.poly([(22, 168), (234, 168), (252, 232), (4, 232)], floor_col, step=14)
    for f, x, y, s in things:
        scaled(c, f, x, y, s)


def make():
    for i, (name, f) in enumerate(D.items()):
        c = Sp(300 + i); f(c)
        c.out(ACT, ('furniture_' if name in ('bed', 'chair', 'table', 'wardrobe') else 'house_') + name)
    rooms = dict(
        bathroom=('pale', 'sky', [(bathtub, 30, 86, .62), (mirror, 170, 40, .34)], True),
        bedroom=('cream', 'wood', [(bed, 30, 76, .66), (window, 164, 40, .32)], False),
        hallway=('sand', 'wood', [(door, 40, 40, .64), (carpet, 128, 150, .44), (lamp, 150, 66, .4)], False),
        kitchen=('yellow', 'taupe', [(stove, 30, 70, .56), (refrigerator, 112, 56, .58), (pot, 44, 54, .26)], False),
        living_room=('pale', 'wood', [(sofa, 24, 82, .6), (painting, 60, 38, .3), (lamp, 140, 70, .46)], False))
    for i, (name, (w, fl, things, tiles)) in enumerate(rooms.items()):
        c = Sp(400 + i); room(c, w, fl, things, tiles); c.out(ACT, 'room_' + name)


# Sizes of the pieces once they stand in a room (the House game lists the same groups): a bed is not the size of a spoon.
LONG = {'furniture_bed', 'house_sofa', 'house_bathtub', 'house_carpet'}
LARGE = {'furniture_wardrobe', 'furniture_table', 'house_refrigerator', 'house_fireplace', 'house_stove', 'house_cabinet', 'house_washing_machine',
         'house_shelf', 'house_sink'}
SMALL = {'house_mug', 'house_plate', 'house_bowl', 'house_glass', 'house_jug', 'house_spoon', 'house_fork', 'house_knife', 'house_soap', 'house_shampoo',
         'house_toothbrush', 'house_toothpaste', 'house_hairbrush', 'house_comb', 'house_can', 'house_bottle', 'house_pot', 'house_frying_pan',
         'house_kettle', 'house_rolling_pin', 'house_pillow', 'house_tap', 'house_shoes', 'house_game', 'house_books', 'house_small_lamp', 'house_radio',
         'house_clock', 'house_towel'}
PIECES = ROOT / 'Assets/Lemko/Resources/Pieces'


def pieces(folder=None):
    """Pixel art only. Every piece once more at the size it has in a room, drawn at that size (never a scaled word
    picture), so its pixels are as big as everyone else's: 36, 58, 88 or 106 pixels across."""
    folder = Path(folder) if folder else PIECES
    folder.mkdir(parents=True, exist_ok=True)
    for i, (name, f) in enumerate(D.items()):
        wid = ('furniture_' if name in ('bed', 'chair', 'table', 'wardrobe') else 'house_') + name
        k = 2.3 if wid in LONG else 1.9 if wid in LARGE else .78 if wid in SMALL else 1.25
        box = int(round(46 * k / 2)) * 2
        c = Sp(300 + i); f(c)
        c.px(box, box + 2).image().save(str(folder / (wid + '.png')), optimize=True)


NAMES = None
if __name__ == '__main__':
    import sk
    if len(sys.argv) > 1:
        sk.PREVIEW = sys.argv[1]
    make()
    if ink.PIX:
        pieces(sys.argv[1] + '/pieces' if len(sys.argv) > 1 else None)
    if len(sys.argv) > 1:
        names = [('furniture_' if n in ('bed', 'chair', 'table', 'wardrobe') else 'house_') + n for n in D] + ['room_' + r for r in 'bathroom bedroom hallway kitchen living_room'.split()]
        sheet(names[:40], ACT, sys.argv[1] + '/_house1.png', cols=8, cell=170)
        sheet(names[40:], ACT, sys.argv[1] + '/_house2.png', cols=8, cell=170)
