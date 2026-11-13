"""The 25 farm animals, drawn as chunky side-view doodles."""
import math
from sk import *

G = 216  # ground line


def leg(c, x, top, bottom, fill, hoof='ink', w=17):
    c.rr(x - w / 2, top, w, bottom - top, 6, fill, step=10)
    if hoof:
        c.rr(x - w / 2 - 1, bottom - 11, w + 2, 12, 4, hoof, shade=False, w=5, step=8)


def quad(c, body='white', rx=62, ry=40, legl=44, hr=30, neck=0, head=None, legs=None, hoof='ink', wool=False, spots=None,
         tail='cow', bx=112, far=None, belly=None):
    """Body, four legs and a tail. Returns the head centre and radius for the caller to finish the face."""
    head = head or body; legs = legs or body; far = far or SHADE[legs]
    by = G - legl - ry * .72
    hx, hy = bx + rx * .98, by - ry * .5 - neck
    # tail
    tx, ty = bx - rx * .93, by - ry * .25
    if tail == 'cow':
        c.ln([(tx + 4, ty), (tx - 16, ty + 14), (tx - 14, ty + 52)], 6); c.ell(tx - 14, ty + 60, 7, 11, 'brown', shade=False, w=5)
    elif tail == 'horse':
        c.poly([(tx + 8, ty - 6), (tx - 22, ty + 4), (tx - 30, ty + 50), (tx - 14, ty + 74), (tx - 8, ty + 40), (tx + 6, ty + 14)], 'brown')
    elif tail == 'pig':
        pts = [(tx + 4 - t * 2.2 + math.cos(t * .9) * 9 - 9, ty - 4 + math.sin(t * .9) * 9) for t in range(0, 12)]
        c.ln(pts, 6, amp=.3, step=5)
    elif tail == 'dog':
        c.poly([(tx + 10, ty + 4), (tx - 8, ty - 22), (tx - 2, ty - 50), (tx + 8, ty - 46), (tx + 6, ty - 22), (tx + 20, ty - 8)], body, step=10)
    elif tail == 'cat':
        c.poly([(tx + 12, ty + 12), (tx - 18, ty + 6), (tx - 28, ty - 30), (tx - 14, ty - 62), (tx - 2, ty - 56), (tx - 14, ty - 30), (tx - 6, ty - 8), (tx + 16, ty - 4)], body, step=10)
    elif tail == 'tuft':
        c.lump(tx - 4, ty + 2, 13, 12, body, bumps=5)
    elif tail == 'goat':
        c.poly([(tx + 8, ty), (tx - 10, ty - 20), (tx + 2, ty - 24), (tx + 16, ty - 8)], body, step=10)
    # far legs
    top = by + ry * .3
    leg(c, bx - rx * .5 + 12, top, G - 4, far, hoof)
    leg(c, bx + rx * .55 + 10, top, G - 4, far, hoof)
    # body
    def inner():
        if belly:
            c.fe(bx + 4, by + ry * .8, rx * .8, ry * .5, belly)
        for sx, sy, srx, sry, colr in (spots or []):
            c.fe(bx + sx * rx, by + sy * ry, srx, sry, colr)
    if wool:
        c.lump(bx, by, rx * 1.04, ry * 1.08, body, bumps=11, depth=.2, inner=inner)
    else:
        c.ell(bx, by, rx, ry, body, inner=inner)
    # near legs
    leg(c, bx - rx * .62, top, G, legs, hoof)
    leg(c, bx + rx * .42, top, G, legs, hoof)
    return hx, hy, hr


def cow(c, body='white', spot='brown', horns=True, baby=False, udder=True):
    k = dict(rx=50, ry=34, legl=38, hr=27, bx=110) if baby else dict(rx=64, ry=42, legl=42, hr=30)
    spots = [(-.45, -.3, 22, 18, spot), (.25, .1, 20, 15, spot), (-.1, .65, 16, 10, spot)] if spot else None
    hx, hy, hr = quad(c, body, spots=spots, tail='cow', **k)
    if udder and not baby:
        c.ell(112 + 14, G - 50, 15, 11, 'skin', w=5.5)
    # ears, horns, head, muzzle
    c.poly([(hx - hr * .6, hy - hr * .5), (hx - hr * 1.5, hy - hr * .75), (hx - hr * .9, hy - hr * .05)], spot or body, step=9)
    if horns:
        c.poly([(hx - hr * .5, hy - hr * .8), (hx - hr * .75, hy - hr * 1.55), (hx - hr * .1, hy - hr * .95)], 'cream', step=9)
        c.poly([(hx + hr * .2, hy - hr * .9), (hx + hr * .6, hy - hr * 1.6), (hx + hr * .7, hy - hr * .75)], 'cream', step=9)
    c.ell(hx, hy, hr, hr * .95, body, inner=(lambda: c.fe(hx - hr * .5, hy - hr * .5, hr * .6, hr * .55, spot)) if spot else None)
    c.ell(hx + hr * .45, hy + hr * .45, hr * .72, hr * .5, 'skin')
    c.dot(hx + hr * .25, hy + hr * .45, 3.5); c.dot(hx + hr * .7, hy + hr * .42, 3.5)
    c.eye(hx + hr * .1, hy - hr * .25)


def horse(c, body='wood', mane='brown', baby=False):
    k = dict(rx=48, ry=30, legl=52, hr=24, bx=108) if baby else dict(rx=62, ry=38, legl=50, hr=28, bx=106)
    hx, hy, hr = quad(c, body, tail='horse', neck=30 if not baby else 26, hoof='ink', **k)
    bx, rx, ry = k['bx'], k['rx'], k['ry']
    by = G - k['legl'] - ry * .72
    # neck
    c.poly([(bx + rx * .35, by - ry * .7), (hx - hr * .7, hy - hr * .2), (hx + hr * .3, hy + hr * .6), (bx + rx * .98, by + ry * .1)], body, step=12)
    c.fill([(bx + rx * .3, by - ry * .2), (bx + rx * .8, by - ry * .9), (bx + rx * .9, by + ry * .3)], body)
    # ears
    c.poly([(hx - hr * .5, hy - hr * .7), (hx - hr * .35, hy - hr * 1.55), (hx + hr * .1, hy - hr * .8)], body, step=9)
    # head: long muzzle
    c.ell(hx + hr * .25, hy + hr * .1, hr * 1.25, hr * .78, body, rot=.45)
    c.ell(hx + hr * 1.0, hy + hr * .68, hr * .52, hr * .42, 'cream', rot=.45, w=6)
    c.dot(hx + hr * 1.12, hy + hr * .62, 3.2)
    c.eye(hx + hr * .1, hy - hr * .2)
    # mane
    c.poly([(hx - hr * .25, hy - hr * 1.0), (hx - hr * 1.05, hy - hr * .55), (hx - hr * 1.35, hy + hr * .5), (bx + rx * .4, by - ry * .85),
            (hx - hr * .95, hy + hr * .25), (hx - hr * .55, hy - hr * .35)], mane, step=10)


def pig(c, baby=False):
    k = dict(rx=44, ry=34, legl=22, hr=27, bx=112) if baby else dict(rx=66, ry=46, legl=26, hr=32, bx=110)
    hx, hy, hr = quad(c, 'skin', tail='pig', hoof='brown', **k)
    hy += hr * .45; hx -= 2
    c.poly([(hx - hr * .75, hy - hr * .55), (hx - hr * .7, hy - hr * 1.35), (hx - hr * .05, hy - hr * .85)], 'orange', step=9)
    c.ell(hx, hy, hr, hr * .95, 'skin')
    c.poly([(hx + hr * .2, hy - hr * .8), (hx + hr * .6, hy - hr * 1.35), (hx + hr * .85, hy - hr * .5)], 'orange', step=9)
    c.ell(hx + hr * .7, hy + hr * .2, hr * .45, hr * .38, 'orange', w=6)
    c.dot(hx + hr * .58, hy + hr * .2, 3.2); c.dot(hx + hr * .84, hy + hr * .2, 3.2)
    c.eye(hx + hr * .05, hy - hr * .25)


def sheep(c, kind='sheep'):
    baby = kind == 'lamb'
    k = dict(rx=44, ry=32, legl=30, hr=22, bx=112) if baby else dict(rx=62, ry=44, legl=32, hr=25, bx=108)
    face = 'cream' if baby else 'plum'
    hx, hy, hr = quad(c, 'white', wool=True, legs=face, far='ink' if not baby else 'sand', tail='tuft', hoof=None, **k)
    hy += hr * .5
    c.ell(hx - hr * .75, hy - hr * .1, hr * .5, hr * .28, face, rot=-.4, w=6)
    c.ell(hx + hr * .15, hy + hr * .1, hr * .8, hr, face)
    c.lump(hx - hr * .05, hy - hr * .75, hr * .85, hr * .6, 'white', bumps=6)
    if baby:
        c.eye(hx + hr * .2, hy + hr * .1); c.dot(hx + hr * .55, hy + hr * .55, 3)
    else:
        c.dot(hx + hr * .25, hy + hr * .05, 5, 'white'); c.dot(hx + hr * .3, hy + hr * .07, 2.6, 'ink')
    if kind == 'ram':
        for r, colr in ((hr * .95, 'sand'),):
            cx, cy = hx - hr * .55, hy - hr * .15
            c.ell(cx, cy, r, r, colr)
            c.arc(cx, cy, r * .55, 30, 330, 5.5)
            c.dot(cx, cy, 3.5)


def goat(c, baby=False):
    k = dict(rx=44, ry=28, legl=36, hr=22, bx=112) if baby else dict(rx=58, ry=36, legl=44, hr=25, bx=108)
    body = 'white' if baby else 'taupe'
    hx, hy, hr = quad(c, body, tail='goat', hoof='ink', belly='white' if not baby else None, **k)
    c.poly([(hx - hr * .6, hy - hr * .3), (hx - hr * 1.5, hy - hr * .1), (hx - hr * .8, hy + hr * .35)], body, step=9)
    if not baby:
        c.poly([(hx - hr * .4, hy - hr * .8), (hx - hr * 1.1, hy - hr * 2.0), (hx - hr * .55, hy - hr * 1.9), (hx + hr * .1, hy - hr * .9)], 'sand', step=9)
    else:
        c.poly([(hx - hr * .3, hy - hr * .85), (hx - hr * .25, hy - hr * 1.3), (hx + hr * .1, hy - hr * .9)], 'sand', step=8, w=6)
    c.ell(hx + hr * .15, hy + hr * .05, hr * 1.05, hr * .85, body, rot=.35)
    if not baby:
        c.poly([(hx + hr * .3, hy + hr * .75), (hx + hr * .45, hy + hr * 1.55), (hx + hr * .8, hy + hr * .7)], 'white', step=9, w=6)
    c.dot(hx + hr * .95, hy + hr * .35, 3.2)
    c.eye(hx + hr * .1, hy - hr * .15)


def dog(c, baby=False):
    k = dict(rx=40, ry=28, legl=24, hr=28, bx=112) if baby else dict(rx=56, ry=34, legl=40, hr=29, bx=108)
    hx, hy, hr = quad(c, 'wood', tail='dog', hoof=None, belly='cream', **k)
    hy += 4
    c.ell(hx, hy, hr, hr * .92, 'wood')
    c.ell(hx + hr * .55, hy + hr * .35, hr * .62, hr * .45, 'cream', w=6)
    c.ell(hx + hr * .98, hy + hr * .18, 6.5, 5.5, 'ink', shade=False, w=0)
    c.arc(hx + hr * .6, hy + hr * .45, 9, 20, 150, 4.5)
    c.ell(hx - hr * .7, hy + hr * .05, hr * .38, hr * .8, 'brown', rot=.35, w=6.5)
    c.eye(hx + hr * .15, hy - hr * .3)
    if baby:
        c.poly([(hx + hr * .45, hy + hr * .7), (hx + hr * .5, hy + hr * 1.05), (hx + hr * .8, hy + hr * .7)], 'brick', w=5, step=8)


def cat(c, baby=False):
    k = dict(rx=38, ry=27, legl=20, hr=28, bx=114) if baby else dict(rx=54, ry=32, legl=34, hr=28, bx=110)
    body = 'gold' if not baby else 'taupe'
    stripe = 'wood' if not baby else 'grey2'
    spots = [(-.5, -.75, 6, 16, stripe), (-.1, -.85, 6, 16, stripe), (.3, -.8, 6, 16, stripe)]
    hx, hy, hr = quad(c, body, tail='cat', hoof=None, spots=spots, **k)
    hy += 2
    for sgn in (-1, 1):
        c.poly([(hx + sgn * hr * .2, hy - hr * .75), (hx + sgn * hr * .75, hy - hr * 1.45), (hx + sgn * hr * .95, hy - hr * .35)], body, step=9,
               inner=lambda sgn=sgn: c.fill([(hx + sgn * hr * .45, hy - hr * .7), (hx + sgn * hr * .72, hy - hr * 1.15), (hx + sgn * hr * .8, hy - hr * .55)], 'skin'))
    c.ell(hx, hy, hr * 1.05, hr * .9, body, inner=lambda: [c.fe(hx + d, hy - hr * .85, 4, 11, stripe) for d in (-10, 0, 10)])
    c.eye(hx - hr * .35, hy - hr * .1); c.eye(hx + hr * .4, hy - hr * .1)
    c.poly([(hx - 5, hy + hr * .22), (hx + 7, hy + hr * .22), (hx + 1, hy + hr * .4)], 'brick', w=0, shade=False)
    c.arc(hx - 5, hy + hr * .45, 6, 10, 170, 4); c.arc(hx + 7, hy + hr * .45, 6, 10, 170, 4)
    for sgn in (-1, 1):
        c.ln([(hx + sgn * hr * .6, hy + hr * .3), (hx + sgn * hr * 1.35, hy + hr * .15)], 3.5, amp=.3)
        c.ln([(hx + sgn * hr * .6, hy + hr * .45), (hx + sgn * hr * 1.35, hy + hr * .55)], 3.5, amp=.3)


def feet(c, x, y0, colr='gold', n=2, gap=22):
    for i in range(n):
        xx = x + i * gap
        c.ln([(xx, y0), (xx, G - 4)], 6.5, colr)
        c.ln([(xx - 10, G), (xx, G - 6), (xx + 12, G)], 6.5, colr, amp=.3)
        c.ln([(xx, y0), (xx, G - 4)], 2.5, 'yellow' if colr == 'gold' else colr)


def bird(c, body='white', kind='hen'):
    """Hen, rooster, duck, goose, turkey: plump body, wing, head, beak."""
    long_neck = kind in ('goose', 'duck')
    bx, by, rx, ry = 112, 146, 58, 44
    hx, hy, hr = (182, 70, 22) if kind == 'goose' else (180, 96, 23) if kind == 'duck' else (176, 92, 24)
    feet(c, bx - 6, by + ry - 8, 'orange' if long_neck else 'gold')
    # tail
    if kind == 'rooster':
        for i, colr in enumerate(('navy', 'dgreen', 'brick', 'navy')):
            a = math.radians(205 + i * 22)
            ex, ey = bx - 44 + math.cos(a) * 74, by - 20 + math.sin(a) * 74
            c.poly([(bx - 46, by - 26), ((bx - 46 + ex) / 2 - 10, (by - 26 + ey) / 2 - 22), (ex, ey), ((bx - 30 + ex) / 2 + 8, (by - 4 + ey) / 2 - 4), (bx - 30, by - 4)], colr, step=10)
    elif kind == 'turkey':
        fan = [(bx - 20 + math.cos(math.radians(a)) * 92, by + 6 + math.sin(math.radians(a)) * 92) for a in range(150, 300, 10)]
        c.poly([(bx - 10, by + 10)] + fan, 'wood', step=12,
               inner=lambda: [c.fe(bx - 20 + math.cos(math.radians(a)) * 78, by + 6 + math.sin(math.radians(a)) * 78, 11, 11, 'cream') for a in range(158, 300, 22)])
        for a in range(170, 290, 22):
            c.ln([(bx - 14, by + 4), (bx - 20 + math.cos(math.radians(a)) * 88, by + 6 + math.sin(math.radians(a)) * 88)], 4, amp=.5)
    else:
        c.poly([(bx - 40, by - 24), (bx - 86, by - 50), (bx - 78, by - 16), (bx - 50, by + 6)], body, step=10)
    # neck
    if long_neck:
        c.poly([(bx + 26, by - 30), (hx - hr * .8, hy + 4), (hx + hr * .6, hy + hr * .8), (bx + 60, by + 2)], body, step=12)
    # body and wing
    c.ell(bx, by, rx, ry, body, rot=-.12)
    if long_neck:
        c.fill([(bx + 30, by - 24), (hx - hr * .5, hy + 14), (hx + hr * .3, hy + hr * .8), (bx + 54, by - 4)], body)
    wing = dict(hen='sand', rooster='brick', duck='taupe', goose='silver', turkey='brown').get(kind, 'sand')
    c.ell(bx - 8, by + 4, 34, 21, wing, rot=.25, w=6)
    # head
    if kind in ('hen', 'rooster'):
        big = kind == 'rooster'
        c.lump(hx - 2, hy - hr * (1.05 if big else .95), 15 if big else 11, 13 if big else 9, 'brick', bumps=3, depth=.5, w=6)
        c.ell(hx + hr * .45, hy + hr * 1.0, 7 if big else 5, 12 if big else 8, 'brick', w=5.5)
    if kind == 'turkey':
        c.poly([(bx + 34, by - 26), (hx - hr * .7, hy + 4), (hx + hr * .5, hy + hr * .7), (bx + 58, by)], 'sky', step=12)
    c.ell(hx, hy, hr, hr, 'sky' if kind == 'turkey' else body)
    if long_neck:
        c.poly([(hx + hr * .7, hy - 7), (hx + hr * 2.1, hy + 2), (hx + hr * 2.0, hy + 10), (hx + hr * .7, hy + 11)], 'orange', step=9, w=6)
    else:
        c.poly([(hx + hr * .75, hy - 8), (hx + hr * 1.65, hy + 3), (hx + hr * .75, hy + 11)], 'gold', step=9, w=6)
    if kind == 'turkey':
        c.ell(hx + hr * .75, hy + hr * .95, 6, 15, 'brick', w=5.5, rot=-.2)
    c.eye(hx + hr * .15, hy - hr * .2)


def chick(c, body='yellow', beak='gold', bill=False, tuft=True):
    """Fluffy ball with a head: chick, duckling, gosling, poult."""
    bx, by = 118, 160
    feet(c, bx - 12, by + 30, 'orange' if bill else 'gold', gap=24)
    c.ell(bx, by, 46, 40, body)
    c.ell(bx - 8, by + 6, 20, 14, SHADE[body], rot=.3, w=6, shade=False)
    hx, hy, hr = bx + 34, by - 46, 30
    if tuft:
        c.ln([(hx - 6, hy - hr + 2), (hx - 12, hy - hr - 14)], 5.5); c.ln([(hx + 2, hy - hr + 1), (hx + 4, hy - hr - 16)], 5.5)
    c.ell(hx, hy, hr, hr * .95, body)
    if bill:
        c.poly([(hx + hr * .7, hy - 4), (hx + hr * 1.7, hy + 4), (hx + hr * 1.6, hy + 12), (hx + hr * .7, hy + 13)], 'orange', step=9, w=6)
    else:
        c.poly([(hx + hr * .8, hy - 5), (hx + hr * 1.45, hy + 4), (hx + hr * .8, hy + 12)], beak, step=9, w=6)
    c.eye(hx + hr * .2, hy - hr * .15, 6); c.blush(hx - hr * .1, hy + hr * .35, 7, 'orange' if body == 'yellow' else SHADE[body])


BABY = 'calf foal piglet lamb kid puppy kitten chick duckling gosling turkey_poult'.split()


def make():
    def S(name, f, *a, seed=None, **k):
        c = Sp(seed or 1); f(c, *a, **k); c.out(ACT, 'animal_' + name, .8 if name in BABY else .94)
    S('cow', cow, seed=3); S('bull', cow, 'wood', None, True, False, False, seed=4); S('calf', cow, 'white', 'wood', False, True, seed=5)
    S('horse', horse, seed=6); S('foal', horse, 'gold', 'wood', True, seed=7)
    S('pig', pig, seed=8); S('piglet', pig, True, seed=9)
    S('sheep', sheep, seed=10); S('ram', sheep, 'ram', seed=11); S('lamb', sheep, 'lamb', seed=12)
    S('goat', goat, seed=13); S('kid', goat, True, seed=14)
    S('dog', dog, seed=15); S('puppy', dog, True, seed=16); S('cat', cat, seed=17); S('kitten', cat, True, seed=18)
    S('hen', bird, 'white', 'hen', seed=19); S('rooster', bird, 'gold', 'rooster', seed=20); S('duck', bird, 'white', 'duck', seed=21)
    S('goose', bird, 'white', 'goose', seed=22); S('turkey', bird, 'brown', 'turkey', seed=23)
    S('chick', chick, seed=24); S('duckling', chick, 'yellow', 'orange', True, False, seed=25)
    S('gosling', chick, 'sand', 'orange', True, False, seed=26); S('turkey_poult', chick, 'wood', 'gold', False, True, seed=27)


NAMES = 'cow bull calf horse foal pig piglet sheep ram lamb goat kid dog puppy cat kitten hen rooster duck goose turkey chick duckling gosling turkey_poult'.split()
if __name__ == '__main__':
    import sk
    if len(sys.argv) > 1:
        sk.PREVIEW = sys.argv[1]
    make()
    if len(sys.argv) > 1:
        sheet(['animal_' + n for n in NAMES], ACT, sys.argv[1] + '/_animals.png', cols=7, cell=180)
