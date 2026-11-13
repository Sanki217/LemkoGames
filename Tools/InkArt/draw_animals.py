"""The 25 farm animals as cute big-headed characters: a huge round head, shiny eyes, a small chubby body."""
import math
from sk import *

HX, HY = 128, 104      # head centre
BX, BY = 128, 190      # body centre


def body(c, colr, belly=None, rx=50, ry=40, feet=None, marks=None, wool=False, arms=True):
    feet = feet or colr
    for s in (-1, 1):                                               # back feet
        c.ell(BX + s * 30, BY + ry - 4, 19, 13, feet, w=7)
    def inner():
        if belly:
            c.fe(BX, BY + 12, rx * .62, ry * .7, belly)
        if marks:
            marks()
    if wool:
        c.lump(BX, BY, rx + 6, ry + 4, colr, bumps=9, depth=.2, inner=inner)
    else:
        c.ell(BX, BY, rx, ry, colr, inner=inner)
    if arms:
        for s in (-1, 1):                                           # little front paws
            c.ell(BX + s * 22, BY + 18, 11, 15, feet, w=6.5, rot=s * .25, shine=False)


def head(c, colr, rx=80, ry=66, marks=None, wool=False):
    if wool:
        return c.lump(HX, HY, rx, ry, colr, bumps=11, depth=.16, inner=marks)
    return c.ell(HX, HY, rx, ry, colr, inner=marks)


def face(c, y=112, gap=36, blush='rose', shut=False, r=1.0):
    c.fe(HX - gap - 14, y + 20, 11, 7, blush); c.fe(HX + gap + 14, y + 20, 11, 7, blush)
    c.bigeye(HX - gap, y, 10 * r, 13 * r, shut); c.bigeye(HX + gap, y, 10 * r, 13 * r, shut)


def ear_side(c, colr, inner='pink', y=92, rx=26, ry=13, out=74, droop=.35):
    for s in (-1, 1):
        c.ell(HX + s * (out + rx * .5), y, rx, ry, colr, rot=s * droop, w=7.5, inner=lambda s=s: c.fe(HX + s * (out + rx * .6), y + 2, rx * .55, ry * .5, inner))


def ear_point(c, colr, inner='pink', x=46, top=18, w=30):
    for s in (-1, 1):
        px = HX + s * x
        c.poly([(px - s * w * .7, 70), (px + s * w * .25, top), (px + s * w * .8, 80)], colr, step=10)
        c.poly([(px - s * w * .3, 68), (px + s * w * .2, top + 20), (px + s * w * .5, 74)], inner, w=0, shade=False, shine=False)


def tuft(c, colr, n=3, y=44):
    for i in range(n):
        dx = (i - (n - 1) / 2) * 15
        c.poly([(HX + dx - 9, y + 12), (HX + dx * 1.5, y - 16), (HX + dx + 9, y + 12)], colr, w=6.5, step=8, shine=False)


def nostrils(c, y, gap=11):
    c.fe(HX - gap, y, 4, 5.5, 'ink'); c.fe(HX + gap, y, 4, 5.5, 'ink')


# ---- animals --------------------------------------------------------------------------------------
def cow(c, kind='cow'):
    base = 'wood' if kind == 'bull' else 'white'
    spot = 'brown'
    c.ln([(84, 196), (58, 190), (50, 158)], 7); c.ell(48, 150, 9, 12, spot, w=6, shine=False)                     # tail
    body(c, base, 'cream' if kind != 'bull' else 'sand', feet='brown',
         marks=(lambda: (c.fe(96, 172, 22, 18, spot), c.fe(166, 206, 20, 14, spot))) if kind != 'bull' else None)
    if kind != 'calf':
        for s in (-1, 1):                                                                                              # horns
            h = 30 if kind == 'bull' else 20
            c.poly([(HX + s * 44, 52), (HX + s * 60, 52 - h * .4), (HX + s * (58 + h * .5), 44 - h), (HX + s * 70, 50 - h * .3), (HX + s * 62, 62)], 'cream', step=8, w=7)
    ear_side(c, base if kind == 'bull' else spot, y=84, out=70)
    head(c, base, marks=(lambda: (c.fe(84, 76, 36, 34, spot), c.fe(190, 132, 20, 24, spot))) if kind != 'bull' else None)
    if kind == 'calf':
        tuft(c, spot, 3, 44)
    face(c, 100, 38)
    c.ell(HX, 142, 44, 26, 'pink', w=7.5)
    nostrils(c, 138, 15); c.mouth(HX, 152, 8, 3.5)
    if kind == 'bull':
        c.ln([(HX - 52, 80), (HX - 26, 88)], 6); c.ln([(HX + 52, 80), (HX + 26, 88)], 6)
        c.arc(HX, 160, 13, 20, 160, 6, 'yellow')
    if kind == 'cow':
        c.ell(HX, 232, 9, 8, 'yellow', w=5.5, shine=False); c.ln([(HX - 20, 214), (HX, 224), (HX + 20, 214)], 5, 'brick')   # bell


def horse(c, baby=False):
    base, mane = ('yellow', 'cream') if baby else ('wood', 'brown')
    c.poly([(86, 186), (54, 170), (40, 200), (52, 232), (70, 214), (84, 206)], mane, step=10)                          # tail
    body(c, base, None, feet=mane if not baby else 'wood')
    ear_point(c, base, 'pink', x=44, top=12, w=26)
    # long face: the head is taller, with a pale muzzle
    c.ell(HX, HY + 4, 70, 70, base, inner=(lambda: c.fe(HX, 60, 15, 44, 'white')) if baby else None)
    c.poly([(HX - 30, 34), (HX - 8, 22), (HX + 18, 26), (HX + 30, 44), (HX + 16, 66), (HX + 2, 50), (HX - 14, 68), (HX - 24, 54)], mane, step=8)   # forelock
    if not baby:
        c.poly([(HX + 56, 50), (HX + 78, 70), (HX + 82, 108), (HX + 72, 140), (HX + 62, 104)], mane, step=10)
    face(c, 104, 36)
    c.ell(HX, 150, 40, 27, 'cream', w=7.5)
    nostrils(c, 146, 15); c.mouth(HX, 160, 8, 3.5)


def pig(c, baby=False):
    base = 'pink'
    c.arc(74, 176, 12, 60, 400, 6, 'ink'); c.arc(74, 176, 12, 60, 400, 3, 'rose')
    body(c, base, None, feet='rose')
    for s in (-1, 1):                                                                                                  # folded ears
        px = HX + s * 52
        c.poly([(px - s * 22, 60), (px + s * 4, 24), (px + s * 30, 44), (px + s * 22, 78)], 'rose' if not baby else base, step=10)
    head(c, base)
    if baby:
        c.arc(HX, 40, 9, 180, 400, 5)
    face(c, 100, 40)
    c.ell(HX, 138, 30, 21, 'rose', w=7.5)
    nostrils(c, 138, 10); c.mouth(HX, 166, 8, 3.5)


def sheep(c, kind='sheep'):
    face_c = 'cream' if kind == 'lamb' else 'plum'
    wool = 'white'
    body(c, wool, None, feet=face_c, wool=True)
    ear_side(c, face_c, 'pink' if kind == 'lamb' else 'mauve', y=100, out=62, rx=24, ry=12, droop=.45)
    c.lump(HX, HY - 6, 80, 62, wool, bumps=11, depth=.2)                                                               # wool hood
    c.ell(HX, HY + 14, 56, 48, face_c)
    c.lump(HX, 62, 38, 20, wool, bumps=6, depth=.25)                                                                   # curl on the forehead
    if kind == 'ram':
        for s in (-1, 1):
            cx = HX + s * 70
            pts = [(cx + math.cos(a) * (30 - a * 3.2) * s, 84 + math.sin(a) * (30 - a * 3.2)) for a in [i * .45 for i in range(0, 17)]]
            c.ln(pts, 20, 'ink', step=6); c.ln(pts, 12, 'sand', step=6)
    if kind == 'lamb':
        c.fe(HX - 50, 136, 10, 6, 'rose'); c.fe(HX + 50, 136, 10, 6, 'rose')
        c.bigeye(HX - 26, 116, 9, 12); c.bigeye(HX + 26, 116, 9, 12)
        c.fe(HX, 134, 6, 4, 'rose'); c.mouth(HX, 140, 8, 3.5)
        c.poly([(HX - 24, 170), (HX, 178), (HX + 24, 170), (HX + 18, 190), (HX, 182), (HX - 18, 190)], 'brick', w=6, step=8)   # bow
    else:
        for s in (-1, 1):
            c.fe(HX + s * 26, 116, 12, 15, 'white'); c.fe(HX + s * 26, 118, 7, 10, 'ink'); c.fe(HX + s * 24, 113, 3, 3.5, 'white')
        c.fe(HX, 138, 7, 4.5, 'pink'); c.arc(HX - 4.5, 144, 4.5, 20, 170, 3.5, 'pink', ry=4); c.arc(HX + 4.5, 144, 4.5, 10, 160, 3.5, 'pink', ry=4)


def goat(c, baby=False):
    base = 'white' if baby else 'taupe'
    c.poly([(84, 176), (62, 158), (72, 150), (92, 164)], base, step=8)
    body(c, base, 'white', feet='grey1')
    for s in (-1, 1):                                                                                                  # horns
        if baby:
            c.poly([(HX + s * 26, 48), (HX + s * 30, 26), (HX + s * 42, 46)], 'sand', w=6.5, step=8)
        else:
            c.poly([(HX + s * 22, 50), (HX + s * 30, 12), (HX + s * 54, -4), (HX + s * 50, 20), (HX + s * 44, 52)], 'sand', step=8)
    ear_side(c, base, 'pink', y=96, out=72, rx=28, ry=12, droop=.5)
    if not baby:
        c.poly([(HX - 14, 160), (HX, 196), (HX + 14, 160)], 'white', step=8)                                          # beard
    head(c, base, rx=76, ry=64, marks=lambda: c.fe(HX, 150, 44, 34, 'white'))
    face(c, 104, 36)
    c.fe(HX, 132, 8, 5, 'rose'); c.ln([(HX, 136), (HX, 144)], 3.5); c.mouth(HX, 146, 9, 3.5)
    # sideways pupils make it a goat
    if baby:
        tuft(c, 'white', 2, 44)


def dog(c, baby=False):
    base, dark = ('sand', 'wood') if baby else ('wood', 'brown')
    c.poly([(86, 184), (58, 168), (48, 136), (62, 132), (74, 156), (96, 168)], base, step=8)                           # tail
    body(c, base, 'cream', feet=base)
    head(c, base, marks=(lambda: c.fe(HX + 40, 96, 34, 40, dark)) if baby else None)
    for s in (-1, 1):                                                                                                  # floppy ears
        px = HX + s * 70
        c.poly([(px - s * 18, 52), (px + s * 14, 54), (px + s * 26, 110), (px + s * 8, 142), (px - s * 10, 116)], dark, step=10)
    face(c, 100, 38)
    c.ell(HX, 140, 36, 26, 'cream', w=7.5)
    c.fe(HX, 130, 10, 7, 'ink'); c.fe(HX - 3, 128, 3, 2, 'white'); c.ln([(HX, 136), (HX, 144)], 3.5); c.mouth(HX, 146, 10, 3.5)
    c.poly([(HX - 7, 152), (HX + 7, 152), (HX + 6, 164), (HX, 168), (HX - 6, 164)], 'rose', w=5, step=6, shine=False)   # tongue
    if baby:
        c.rr(HX - 34, 164, 68, 12, 6, 'brick', w=6); c.ell(HX, 178, 7, 7, 'yellow', w=5, shine=False)


def cat(c, baby=False):
    base, stripe = ('taupe', 'grey1') if baby else ('orange', 'brick')
    tail = [(170, 206), (200, 196), (212, 164), (198, 138), (184, 148), (194, 166), (186, 184), (166, 190)]
    c.poly(tail, base, step=8)
    def bstripes():
        for i in range(3):
            c.ln([(BX - 44, 170 + i * 16), (BX - 26, 174 + i * 16)], 6, stripe); c.ln([(BX + 44, 170 + i * 16), (BX + 26, 174 + i * 16)], 6, stripe)
    body(c, base, 'white', feet='white', marks=bstripes)
    ear_point(c, base, 'pink', x=48, top=12, w=34)
    def hstripes():
        for dx in (-16, 0, 16):
            c.ln([(HX + dx, 36), (HX + dx * .8, 62 - abs(dx) * .4)], 7, stripe)
        for s in (-1, 1):
            for i in range(2):
                c.ln([(HX + s * 82, 104 + i * 16), (HX + s * 62, 108 + i * 14)], 6, stripe)
        c.fe(HX, 150, 40, 28, 'white')
    head(c, base, marks=hstripes)
    face(c, 104, 38)
    c.fe(HX, 128, 7, 5, 'rose'); c.mouth(HX, 134, 10, 3.8)
    for s in (-1, 1):
        for i in range(2):
            c.ln([(HX + s * 62, 132 + i * 10), (HX + s * 92, 128 + i * 16)], 3.2)


def feet2(c, y, colr='orange', gap=22):
    for s in (-1, 1):
        x = BX + s * gap
        c.ln([(x, y - 18), (x, y)], 7); c.poly([(x - 14, y + 4), (x - 4, y - 8), (x + 4, y - 8), (x + 14, y + 4), (x, y + 8)], colr, w=6, step=6, shine=False)


def bird(c, kind='hen'):
    """One round body with the face on it, wings at the sides, tail behind."""
    colr = dict(hen='white', rooster='orange', duck='white', goose='white', turkey='brown')[kind]
    cy, rx, ry = 120, 82, 80
    if kind == 'goose':
        cy, rx, ry = 140, 70, 62
    if kind == 'rooster':                                                                                              # tail feathers
        for i, (col2, a) in enumerate((('dgreen', -150), ('navy', -128), ('brick', -106), ('yellow', -168))):
            x, y = 128 + math.cos(math.radians(a)) * 100, 150 + math.sin(math.radians(a)) * 110
            c.poly([(100, 150), (x - 16, y + 10), (x, y - 12), (x + 14, y + 16), (128, 160)], col2, step=10)
    if kind == 'turkey':                                                                                               # fan
        for i in range(9):
            a = math.radians(-170 + i * 20)
            x, y = 128 + math.cos(a) * 104, 130 + math.sin(a) * 104
            c.ell(128 + math.cos(a) * 78, 130 + math.sin(a) * 78, 40, 15, ('sand', 'wood', 'cream')[i % 3], rot=a, w=7)
            c.fe(x - math.cos(a) * 12, y - math.sin(a) * 12, 8, 8, 'brick')
    if kind in ('hen', 'duck', 'goose'):
        c.poly([(196, 150), (232, 120), (226, 160), (236, 178), (200, 186)], colr, step=10)                             # tail
    feet2(c, 226, 'orange' if kind in ('duck', 'goose') else 'yellow')
    if kind == 'goose':
        c.rr(98, 22, 60, 120, 28, colr)                                                                                # long neck and head
    if kind in ('hen', 'rooster'):
        n = 4 if kind == 'rooster' else 3
        for i in range(n):                                                                                             # comb
            dx = (i - (n - 1) / 2) * 20
            c.ell(128 + dx, 40 - (6 if kind == 'rooster' else 0), 13, 17 if kind == 'hen' else 22, 'brick', w=7)
    c.ell(128, cy, rx, ry, colr, inner=(lambda: c.fe(128, cy + 50, 54, 40, 'yellow')) if kind == 'rooster' else (lambda: c.fe(128, cy + 46, 52, 36, 'wood')) if kind == 'turkey' else None)
    for s in (-1, 1):                                                                                                  # wings
        wc = dict(hen='silver', rooster='brick', duck='silver', goose='silver', turkey='wood')[kind]
        c.ell(128 + s * (rx - 8), cy + 26, 20, 32, wc, rot=-s * .3, w=7)
    ey = 96 if kind != 'goose' else 62
    if kind == 'turkey':
        c.ell(128, 92, 44, 42, 'sky')
    gap = 30 if kind != 'goose' else 18
    c.fe(128 - gap - 12, ey + 20, 10, 6.5, 'rose'); c.fe(128 + gap + 12, ey + 20, 10, 6.5, 'rose')
    c.bigeye(128 - gap, ey, 9, 12); c.bigeye(128 + gap, ey, 9, 12)
    if kind in ('duck', 'goose'):
        c.ell(128, ey + 22, 26 if kind == 'duck' else 22, 12, 'orange', w=7); c.fe(128 - 6, ey + 19, 2.5, 2, 'ink'); c.fe(128 + 6, ey + 19, 2.5, 2, 'ink')
    else:
        c.poly([(114, ey + 12), (142, ey + 12), (128, ey + 34)], 'yellow', w=7, step=8, shine=False)
        w_y = ey + 38
        for s in (-1, 1):                                                                                              # wattles
            c.ell(128 + s * 9, w_y + (8 if kind == 'turkey' else 4), 8, 12 if kind != 'turkey' else 18, 'brick', w=6, shine=False)


def chick(c, colr='yellow', kind='chick'):
    feet2(c, 228, 'orange', 20)
    if kind == 'chick':
        tuft(c, colr, 3, 36)
    if kind == 'poult':
        tuft(c, 'brown', 2, 36)
    c.ell(128, 132, 80, 78, colr, inner=(lambda: [c.fe(128 + dx, 190 + dy, 7, 5, 'brown') for dx, dy in ((-30, 0), (0, 10), (30, 0), (-14, -18), (16, -18))]) if kind == 'poult' else None)
    for s in (-1, 1):
        c.ell(128 + s * 76, 150, 16, 26, SHADE[colr] if colr != 'yellow' else 'gold', rot=-s * .35, w=7)
    c.fe(128 - 46, 136, 11, 7, 'rose'); c.fe(128 + 46, 136, 11, 7, 'rose')
    c.bigeye(128 - 32, 112, 10, 13); c.bigeye(128 + 32, 112, 10, 13)
    if kind in ('duckling', 'gosling'):
        c.ell(128, 136, 24, 11, 'orange', w=7)
    else:
        c.poly([(116, 126), (140, 126), (128, 144)], 'orange', w=7, step=8, shine=False)
    if kind == 'gosling':
        c.arc(128, 50, 8, 180, 400, 5)


def make():
    def S(name, f, *a, seed=1):
        c = Sp(seed); f(c, *a); c.out(ACT, 'animal_' + name)
    S('cow', cow); S('bull', cow, 'bull'); S('calf', cow, 'calf')
    S('horse', horse); S('foal', horse, True)
    S('pig', pig); S('piglet', pig, True)
    S('sheep', sheep, seed=10); S('ram', sheep, 'ram', seed=11); S('lamb', sheep, 'lamb', seed=12)
    S('goat', goat); S('kid', goat, True)
    S('dog', dog); S('puppy', dog, True); S('cat', cat); S('kitten', cat, True)
    S('hen', bird, 'hen'); S('rooster', bird, 'rooster'); S('duck', bird, 'duck')
    S('goose', bird, 'goose'); S('turkey', bird, 'turkey')
    S('chick', chick); S('duckling', chick, 'yellow', 'duckling')
    S('gosling', chick, 'sand', 'gosling'); S('turkey_poult', chick, 'wood', 'poult')


NAMES = 'cow bull calf horse foal pig piglet sheep ram lamb goat kid dog puppy cat kitten hen rooster duck goose turkey chick duckling gosling turkey_poult'.split()
if __name__ == '__main__':
    import sk
    if len(sys.argv) > 1:
        sk.PREVIEW = sys.argv[1]
    make()
    if len(sys.argv) > 1:
        sheet(['animal_' + n for n in NAMES], ACT, sys.argv[1] + '/_animals.png', cols=7, cell=180)
