"""The 25 farm animals as small round characters in three-quarter view: they stand on four short legs and look to the
front left, so the far eye, ear and legs are smaller and partly hidden. Eyes are plain dark ovals."""
import math
from sk import *

G = 228                # ground line
HX, HY = 96, 100       # head centre
BX, BY = 160, 168      # body centre


def epts(cx, cy, rx, ry, n=40):
    return [(cx + math.cos(6.2832 * i / n) * rx, cy + math.sin(6.2832 * i / n) * ry) for i in range(n)]


def leg(c, x, top, bottom, colr, hoof=None, w=22):
    c.rr(x - w / 2, top, w, bottom - top, 9, colr, shine=False)
    if hoof:
        c.rr(x - w / 2, bottom - 13, w, 13, 6, hoof, shade=False, shine=False, w=6)


def quad(c, colr, legs=None, far=None, hoof=None, marks=None, wool=False, rx=54, ry=40, belly=None, short=False):
    """Body seen at an angle: the two far legs stand further back and higher, the near legs in front."""
    legs = legs or colr
    far = far or SHADE[legs]
    bx = BX - (10 if short else 0)
    if short:
        rx -= 8
    leg(c, bx - 14, BY + 6, G - 9, far, hoof); leg(c, bx + rx - 14, BY + 6, G - 9, far, hoof)
    def inner():
        if belly:
            c.fe(bx - 12, BY + 32, rx * .8, ry * .45, belly)
        if marks:
            marks()
    if wool:
        c.lump(bx, BY, rx + 4, ry + 4, colr, bumps=9, depth=.2, inner=inner)
    else:
        c.ell(bx, BY, rx, ry, colr, inner=inner)
    leg(c, bx - 38, BY + 10, G, legs, hoof); leg(c, bx + rx - 34, BY + 10, G, legs, hoof)


def head(c, colr, marks=None, rx=62, ry=55):
    def inner():
        if marks:
            marks()
        c.blush(42, 121, 8)                    # far cheek, cut by the edge of the head
    return c.ell(HX, HY, rx, ry, colr, inner=inner)


def face(c, y=104, shut=False, blush=True):
    c.bigeye(57, y, 6.5, 8.5, shut); c.bigeye(101, y + 2, 8, 10.5, shut)
    if blush:
        c.blush(121, y + 21, 9)


def ears_side(c, colr, inner='pink', y=78, near=True):
    c.ell(37, y + 3, 20, 11, colr, rot=.5, w=6.5, shine=False)
    if near:
        c.ell(156, y, 25, 13, colr, rot=-.4, shine=False, inner=lambda: c.fe(158, y + 2, 13, 6, inner))


def ears_point(c, colr, inner='pink'):
    c.poly([(42, 72), (48, 22), (84, 50)], colr, shine=False)
    c.poly([(104, 50), (138, 16), (152, 70)], colr, shine=False)
    c.poly([(118, 52), (135, 32), (143, 64)], inner, w=0, shade=False, shine=False)


def tuft(c, colr, x=90, y=50, n=3):
    for i in range(n):
        dx = (i - (n - 1) / 2) * 13
        c.poly([(x + dx - 8, y + 10), (x + dx * 1.5 - 3, y - 14), (x + dx + 8, y + 10)], colr, w=6, shine=False)


def fur(c, x, y, colr, n=3, dx=10, h=8):
    """A few short tone-on-tone dashes."""
    for i in range(n):
        c.ln([(x + i * dx, y - h / 2 - (i % 2) * 2), (x + i * dx - 2, y + h / 2)], 2.2, colr)


def snout(c, colr, x=55, y=130, rx=30, ry=21, holes=True):
    c.ell(x, y, rx, ry, colr, w=6.5, shine=False)
    if holes:
        c.fe(x - rx * .42, y - 2, 3.4, 5, 'ink'); c.fe(x + rx * .12, y, 4, 5.6, 'ink')


# ---- animals --------------------------------------------------------------------------------------
def cow(c, kind='cow'):
    bull, calf = kind == 'bull', kind == 'calf'
    base, spot = ('wood' if bull else 'white'), 'brown'
    ox = -10 if calf else 0
    c.ln([(208 + ox, 150), (232 + ox, 160), (236 + ox, 190)], 3.4); c.ell(236 + ox, 198, 8, 11, spot, w=6, shine=False)      # tail
    quad(c, base, hoof='plum' if bull else 'brown', short=calf,
         marks=None if bull else (lambda: (c.fe(186 + ox, 148, 24, 19, spot), c.fe(150 + ox, 200, 18, 12, spot))))
    if bull:
        fur(c, 150, 142, 'brown')
    if not calf:
        h = 34 if bull else 20
        c.poly([(60, 60), (44, 44 - h * .3), (50, 32 - h), (62, 40 - h * .4), (76, 54)], 'cream', w=6.5, shine=False)
        c.poly([(110, 52), (122, 38 - h * .4), (136, 30 - h), (140, 44 - h * .3), (128, 60)], 'cream', w=6.5, shine=False)
    ears_side(c, base if bull else spot, y=80)
    head(c, base, marks=None if bull else (lambda: c.fe(116, 76, 36, 34, spot)))
    if calf:
        tuft(c, spot, 86, 50)
    if bull:
        fur(c, 84, 56, 'brown', 3, 9)
    snout(c, 'pink', 55, 130, 32 if not calf else 27, 22 if not calf else 19)
    c.arc(56, 141, 7, 20, 160, 2.2)
    face(c, 100)
    if bull:
        c.ln([(44, 86), (66, 92)], 3.4); c.ln([(90, 92), (116, 86)], 3.4)                                              # brows
        c.arc(52, 150, 11, 10, 170, 3, 'yellow')                                                                       # nose ring
    if kind == 'cow':
        c.ln([(112, 150), (132, 156), (150, 146)], 2.8, 'brick'); c.ell(132, 164, 9, 9, 'yellow', w=5.5, shine=False)      # bell


def horse(c, baby=False):
    base, mane = ('gold', 'cream') if baby else ('wood', 'brown')
    ox = -10 if baby else 0
    c.poly([(204 + ox, 146), (230 + ox, 138), (246 + ox, 168), (240 + ox, 208), (224 + ox, 198), (222 + ox, 170)], mane)   # tail
    quad(c, base, hoof='wood' if baby else 'brown', short=baby)
    c.poly([(118, 46), (150, 60), (168, 98), (166, 138), (142, 128), (136, 90)], mane)                                  # mane down the neck
    ears_point(c, base)
    head(c, base, marks=(lambda: c.fe(74, 78, 13, 34, 'white')) if baby else None)
    snout(c, 'white' if baby else 'cream', 54, 132, 34, 24)
    c.arc(56, 145, 7, 20, 160, 2.2)
    face(c, 96)
    c.poly([(68, 46), (94, 34), (118, 44), (108, 70), (94, 54), (82, 72)], mane, shine=False)                           # forelock


def pig(c, baby=False):
    ox = -10 if baby else 0
    c.arc(214 + ox, 152, 10, 100, 440, 3.2)                                                                             # curly tail
    quad(c, 'pink', hoof='rose', short=baby)
    c.poly([(38, 74), (40, 28), (80, 48)], 'rose', shine=False)
    c.poly([(106, 48), (146, 22), (152, 72)], 'rose', shine=False)
    head(c, 'pink')
    if baby:
        c.arc(92, 42, 8, 180, 420, 2.6)
    snout(c, 'rose', 52, 126, 27, 20)
    c.mouth(62, 150, 8, 2.2)
    face(c, 98)


def sheep(c, kind='sheep'):
    fc = 'cream' if kind == 'lamb' else 'taupe'
    c.lump(214, 158, 13, 12, 'white', bumps=5, shine=False)                                                             # tail
    quad(c, 'white', legs=fc, far='wood' if fc == 'taupe' else 'sand', hoof='brown', wool=True, short=kind == 'lamb')
    if kind == 'ram':
        c.ell(40, 86, 20, 20, 'sand', w=6.5, shine=False)                                                               # far horn
    c.lump(HX, HY - 6, 64, 54, 'white', bumps=10, depth=.18)                                                            # wool hood
    c.ell(36, 104, 19, 10, fc, rot=.5, w=6.5, shine=False); c.ell(150, 102, 23, 12, fc, rot=-.45, shine=False, inner=lambda: c.fe(152, 104, 12, 5.5, 'pink'))
    c.ell(88, 110, 46, 40, fc, shine=False, inner=lambda: c.blush(52, 126, 8))
    c.lump(94, 64, 32, 17, 'white', bumps=6, depth=.25, shine=False)                                                    # curl on the forehead
    if kind == 'ram':
        pts = [(150 + math.cos(a) * (26 - a * 3.6), 82 + math.sin(a) * (26 - a * 3.6)) for a in [i * .5 for i in range(-3, 12)]]
        c.ln(pts, 11.5); c.ln(pts, 6.4, 'sand')
    c.bigeye(60, 108, 6, 8); c.bigeye(100, 110, 7.5, 10)
    c.blush(118, 128, 8)
    c.fe(74, 124, 6, 4.2, 'rose'); c.mouth(74, 130, 8, 2.2)
    if kind == 'lamb':
        c.poly([(104, 152), (126, 162), (148, 150), (144, 172), (126, 166), (110, 174)], 'brick', w=6, shine=False)      # bow


def goat(c, baby=False):
    base = 'white' if baby else 'taupe'
    ox = -10 if baby else 0
    c.poly([(204 + ox, 150), (222 + ox, 126), (232 + ox, 134), (216 + ox, 160)], base, shine=False)                     # tail
    quad(c, base, hoof='grey1', belly=None if baby else 'cream', short=baby)
    if baby:
        c.poly([(66, 54), (64, 32), (80, 48)], 'bone', w=6, shine=False); c.poly([(108, 48), (118, 28), (126, 52)], 'bone', w=6, shine=False)
    else:
        c.poly([(58, 60), (50, 24), (66, -2), (74, 8), (66, 30), (78, 52)], 'bone', w=6.5, shine=False)
        c.poly([(106, 50), (114, 18), (140, -6), (146, 8), (130, 28), (126, 56)], 'bone', w=6.5, shine=False)
        c.poly([(46, 146), (58, 190), (76, 150)], 'cream', shine=False)                                                 # beard
    ears_side(c, base, y=92)
    head(c, base, marks=lambda: c.fe(58, 140, 42, 30, 'cream' if not baby else 'bone'))
    c.fe(64, 124, 7, 4.6, 'rose'); c.ln([(64, 128), (64, 134)], 2); c.mouth(64, 136, 9, 2.2)
    face(c, 100)
    if baby:
        tuft(c, 'white', 92, 48, 2)


def dog(c, baby=False):
    base, dark = ('sand', 'wood') if baby else ('wood', 'brown')
    ox = -10 if baby else 0
    c.poly([(204 + ox, 152), (226 + ox, 128), (236 + ox, 96), (248 + ox, 102), (242 + ox, 136), (218 + ox, 166)], base, shine=False)   # tail
    quad(c, base, legs=base, belly='cream', short=baby, marks=(lambda: c.fe(176, 144, 22, 18, dark)) if baby else None)
    if not baby:
        fur(c, 152, 142, dark)
    c.poly([(36, 82), (20, 112), (30, 142), (50, 122), (50, 88)], dark, shine=False)                                    # far ear
    head(c, base, marks=(lambda: c.fe(112, 86, 30, 34, dark)) if baby else None)
    c.poly([(130, 56), (160, 64), (172, 106), (160, 142), (138, 118), (128, 82)], dark, shine=False)                    # near ear
    snout(c, 'cream', 56, 130, 31, 23, holes=False)
    c.fe(40, 121, 9, 7, 'ink'); c.ln([(42, 128), (44, 136)], 2); c.arc(37, 137, 7, 10, 160, 2.2); c.arc(51, 138, 7, 20, 170, 2.2)
    c.poly([(40, 144), (54, 145), (52, 158), (46, 162), (40, 156)], 'rose', w=5, shine=False)                           # tongue
    face(c, 96)
    if baby:
        c.ln([(110, 152), (132, 158), (150, 146)], 3.4, 'brick'); c.ell(132, 166, 8, 8, 'yellow', w=5.5, shine=False)


def cat(c, baby=False):
    base, stripe = ('grey2', 'grey1') if baby else ('orange', 'brick')
    ox = -10 if baby else 0
    tail = [(202 + ox, 162), (230 + ox, 152), (244 + ox, 120), (236 + ox, 86), (218 + ox, 82), (212 + ox, 98), (226 + ox, 106), (228 + ox, 126), (216 + ox, 142), (200 + ox, 146)]
    c.poly(tail, base, shine=False, inner=lambda: [c.ln([(226 + ox + d, 112 + i * 16), (242 + ox + d, 108 + i * 16)], 3.2, stripe) for i, d in enumerate((2, 2, -4))])
    quad(c, base, hoof='white', belly='cream', short=baby,
         marks=lambda: [c.ln([(150 + ox + i * 16, 124), (148 + ox + i * 16, 142)], 3.4, stripe) for i in range(3)])
    ears_point(c, base)
    def marks():
        for i, dx in enumerate((-16, 0, 16)):
            c.ln([(88 + dx, 42), (88 + dx * .8, 62 - abs(dx) * .3)], 3.6, stripe)
        c.ln([(160, 96), (140, 100)], 3.2, stripe); c.ln([(160, 112), (142, 114)], 3.2, stripe)
        c.fe(64, 136, 36, 24, 'cream')
    head(c, base, marks=marks)
    c.fe(68, 121, 6.5, 4.6, 'rose'); c.mouth(68, 128, 10, 2.3)
    face(c, 102)
    for i in range(2):
        c.ln([(126, 128 + i * 9), (154, 124 + i * 15)], 1.7)
    c.ln([(40, 128), (20, 124)], 1.7); c.ln([(42, 136), (22, 138)], 1.7)


def feet2(c, xs=(116, 150), colr='orange'):
    for x in xs:
        c.ln([(x, G - 24), (x, G - 4)], 3.2)
        c.poly([(x - 18, G + 2), (x - 6, G - 9), (x + 6, G - 9), (x + 9, G + 2), (x - 4, G + 7)], colr, w=5.5, shine=False)


def bird(c, kind='hen'):
    """A round bird turned to the front left: beak on the left, one wing on the near side, tail at the back."""
    colr = dict(hen='white', rooster='orange', duck='white', goose='white', turkey='brown')[kind]
    wing = dict(hen='bone', rooster='brick', duck='bone', goose='bone', turkey='wood')[kind]
    if kind == 'rooster':
        for a, col2 in ((-88, 'gold'), (-62, 'dgreen'), (-36, 'navy'), (-10, 'maroon')):
            tx, ty = 186 + math.cos(math.radians(a)) * 92, 138 + math.sin(math.radians(a)) * 92
            nx, ny = -math.sin(math.radians(a)) * 13, math.cos(math.radians(a)) * 13
            c.poly([(180 - nx, 140 - ny), ((180 + tx) / 2 - nx * 1.5, (140 + ty) / 2 - ny * 1.5), (tx, ty), ((180 + tx) / 2 + nx * 1.5, (140 + ty) / 2 + ny * 1.5), (180 + nx, 140 + ny)], col2, shine=False)
    if kind == 'turkey':
        for i in range(8):
            a = math.radians(-172 + i * 26)
            c.ell(160 + math.cos(a) * 70, 130 + math.sin(a) * 70, 42, 16, ('sand', 'wood', 'cream')[i % 3], rot=a, w=6.5, shine=False)
            c.fe(160 + math.cos(a) * 94, 130 + math.sin(a) * 94, 7, 7, 'brick')
    if kind in ('hen', 'duck'):
        c.poly([(186, 124), (224, 88), (236, 108), (246, 102), (246, 132), (230, 154), (198, 162)] if kind == 'hen' else [(190, 132), (236, 106), (230, 142), (204, 164)], colr, shine=False)
    if kind == 'goose':
        feet2(c, (138, 170))
        c.poly([(204, 150), (240, 132), (234, 164), (210, 178)], colr, shine=False)
        c.merged([(epts(156, 164, 62, 50), colr, {}), ([(80, 70), (114, 66), (132, 150), (100, 168)], colr, dict(shade=False)), (epts(94, 58, 33, 29), colr, dict(shade=False))])
        c.ell(180, 168, 30, 21, wing, rot=.25, w=6.5, shine=False); c.ln([(168, 172), (192, 178)], 2, 'sand')
        c.ell(60, 66, 20, 9, 'orange', rot=.12, w=6.5, shine=False)
        c.bigeye(82, 52, 4.6, 6.2); c.bigeye(106, 54, 5.6, 7.6); c.blush(118, 68, 6.5)
        return
    feet2(c)
    if kind in ('hen', 'rooster'):
        n = 4 if kind == 'rooster' else 3
        for i in range(n):                                                                                              # comb
            c.ell(86 + i * 17, 84 - (i % 2) * 6 - (10 if kind == 'rooster' else 0), 12, 17 if kind == 'hen' else 22, 'brick', rot=-.15, w=6.5, shine=False)
    body = dict(inner=(lambda: c.fe(118, 190, 52, 36, 'yellow')) if kind == 'rooster' else None)
    c.ell(134, 146, 70, 64, colr, **body)
    c.ell(164, 158, 32, 23, wing, rot=.28, w=6.5, shine=False)
    c.ln([(150, 160), (176, 168)], 2, SHADE[wing]); c.ln([(154, 170), (172, 176)], 2, SHADE[wing])
    if kind == 'turkey':
        c.ell(90, 106, 32, 32, 'sky', shine=False)                                                                      # bare head
        c.poly([(64, 100), (42, 110), (64, 120)], 'yellow', w=6, shine=False)
        c.ell(66, 128, 7, 15, 'brick', w=5.5, shine=False)                                                              # snood
        c.bigeye(78, 98, 4.4, 6); c.bigeye(100, 100, 5.4, 7.4)
        return
    if kind == 'duck':
        c.ell(60, 128, 24, 11, 'orange', rot=.1, w=6.5, shine=False); c.fe(50, 125, 2, 1.6, 'ink')
    else:
        c.poly([(70, 114), (40, 126), (70, 138)], 'yellow', w=6.5, shine=False)
        for dx in (0, 12):
            c.ell(72 + dx, 150, 7, 12 if kind == 'hen' else 16, 'brick', w=5.5, shine=False)                              # wattles
    c.bigeye(84, 106, 5.6, 7.6); c.bigeye(116, 108, 7, 9.6)
    c.blush(134, 128, 8.5)


def chick(c, colr='yellow', kind='chick'):
    feet2(c, (114, 146))
    c.poly([(188, 142), (222, 124), (216, 158)], colr, shine=False)                                                     # tail
    if kind in ('chick', 'poult'):
        tuft(c, colr if kind == 'chick' else 'brown', 104, 84, 3 if kind == 'chick' else 2)
    spots = (lambda: [c.fe(150 + dx, 188 + dy, 7, 5, 'brown') for dx, dy in ((-30, 0), (0, 10), (30, 0), (-14, -18), (16, -18))]) if kind == 'poult' else None
    c.ell(130, 150, 68, 64, colr, inner=spots)
    c.ell(162, 164, 24, 18, SHADE[colr], rot=.3, w=6.5, shine=False)
    if kind in ('duckling', 'gosling'):
        c.ell(62, 140, 21, 9.5, 'orange', rot=.1, w=6.5, shine=False)
    else:
        c.poly([(70, 128), (46, 138), (70, 148)], 'orange', w=6.5, shine=False)
    c.bigeye(84, 120, 6, 8); c.bigeye(116, 122, 7.5, 10)
    c.blush(134, 142, 8.5)
    if kind == 'gosling':
        c.arc(118, 84, 7, 180, 420, 2.4)


def make():
    def S(name, f, *a, seed=1):
        c = Sp(seed); f(c, *a); c.out(ACT, 'animal_' + name)
    S('cow', cow, seed=2); S('bull', cow, 'bull', seed=3); S('calf', cow, 'calf', seed=4)
    S('horse', horse, seed=5); S('foal', horse, True, seed=6)
    S('pig', pig, seed=7); S('piglet', pig, True, seed=8)
    S('sheep', sheep, seed=10); S('ram', sheep, 'ram', seed=11); S('lamb', sheep, 'lamb', seed=12)
    S('goat', goat, seed=13); S('kid', goat, True, seed=14)
    S('dog', dog, seed=15); S('puppy', dog, True, seed=16); S('cat', cat, seed=17); S('kitten', cat, True, seed=18)
    S('hen', bird, 'hen', seed=19); S('rooster', bird, 'rooster', seed=20); S('duck', bird, 'duck', seed=21)
    S('goose', bird, 'goose', seed=22); S('turkey', bird, 'turkey', seed=23)
    S('chick', chick, seed=24); S('duckling', chick, 'yellow', 'duckling', seed=25)
    S('gosling', chick, 'bone', 'gosling', seed=26); S('turkey_poult', chick, 'wood', 'poult', seed=27)


NAMES = 'cow bull calf horse foal pig piglet sheep ram lamb goat kid dog puppy cat kitten hen rooster duck goose turkey chick duckling gosling turkey_poult'.split()
if __name__ == '__main__':
    import sk
    if len(sys.argv) > 1:
        sk.PREVIEW = sys.argv[1]
    make()
    if len(sys.argv) > 1:
        sheet(['animal_' + n for n in NAMES], ACT, sys.argv[1] + '/_animals.png', cols=7, cell=180)
