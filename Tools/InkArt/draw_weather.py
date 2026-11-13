"""Weather, seasons and time of day (49 pictures)."""
import math
from sk import *

D = {}


def item(f):
    D[f.__name__] = f
    return f


def P(cx, cy, r, a):
    return (cx + math.cos(math.radians(a)) * r, cy + math.sin(math.radians(a)) * r)


def sun(c, x=128, y=128, r=46, rays=10, face=False, colr='yellow', ray='gold', a0=0, a1=360, long=1.0):
    for i in range(rays):
        a = a0 + (a1 - a0) * i / (rays if a1 - a0 >= 360 else rays - 1)
        c.ln([P(x, y, r + 12, a), P(x, y, r + 12 + 20 * long, a)], 8, 'ink', amp=.3); c.ln([P(x, y, r + 13, a), P(x, y, r + 11 + 20 * long, a)], 3.2, ray, amp=.2)
    c.ell(x, y, r, r, colr)
    if face:
        c.dot(x - r * .32, y - r * .1, r * .1); c.dot(x + r * .32, y - r * .1, r * .1); c.smile(x, y + r * .2, r * .5, 5)
        c.blush(x - r * .55, y + r * .22, r * .14); c.blush(x + r * .55, y + r * .22, r * .14)


def moon(c, x=128, y=128, r=56, colr='cream'):
    # crescent: big circle minus a smaller one shifted to the right, joined exactly at the two horns
    pts = [P(x, y, r, a) for a in range(52, 309, 12)] + [P(x + r * .55, y, r * .78, a) for a in range(274, 85, -14)]
    c.poly(pts, colr, step=9, amp=.6)


def star(c, x, y, r=16, colr='yellow', w=6):
    c.poly(star_pts(x, y, r), colr, w=w, step=9, amp=.5, sx=3, sy=3)


def cloud(c, x, y, w=120, colr='white', w_=LW):
    """Outline of four overlapping puffs with a flat bottom."""
    r = w / 2
    puffs = ((x - r * .55, y + r * .12, r * .34), (x - r * .18, y - r * .16, r * .44), (x + r * .3, y - r * .1, r * .5), (x + r * .66, y + r * .16, r * .3))
    ox, oy = x + r * .05, y + r * .1
    floor = y + r * .44
    pts = []
    for i in range(72):
        a = math.radians(i * 5); dx, dy = math.cos(a), math.sin(a)
        best = 0
        for cx, cy, rr in puffs:
            fx, fy = ox - cx, oy - cy
            bq = fx * dx + fy * dy; cq = fx * fx + fy * fy - rr * rr
            disc = bq * bq - cq
            if disc > 0:
                best = max(best, -bq + math.sqrt(disc))
        px, py = ox + dx * best, oy + dy * best
        if py > floor:
            t = (floor - oy) / (py - oy); px, py = ox + (px - ox) * t if False else px, floor
        pts.append((px, py))
    c.poly(pts, colr, step=9, amp=.8, w=w_)


def hill(c, colr='sage', y=196, snow=False):
    p = [(14, y + 24), (40, y), (96, y - 22), (150, y - 10), (206, y - 20), (242, y + 4), (238, y + 30), (20, y + 34)]
    c.poly(p, 'white' if snow else colr, step=12)


def thermo(c, x=128, level=.5, colr='brick', y0=30, y1=196):
    c.rr(x - 17, y0, 34, y1 - y0, 17, 'white')
    top = y1 - 20 - (y1 - y0 - 44) * level
    c.rr(x - 7, top, 14, y1 - top, 6, colr, w=0, shade=False)
    c.ell(x, y1 + 8, 30, 30, colr)
    c.hi([(x - 10, y1 + 0), (x - 12, y1 + 12)])
    for i in range(5):
        yy = y0 + 22 + i * (y1 - y0 - 60) / 4
        c.ln([(x + 24, yy), (x + 36, yy)], 4.5, amp=.2)


def flake(c, x, y, r=18, w=6, colr='ink'):
    for a in (0, 60, 120):
        c.ln([P(x, y, r, a), P(x, y, r, a + 180)], w, colr, amp=.2)
    if r > 20:
        for a in range(0, 360, 60):
            m = P(x, y, r * .62, a)
            c.ln([P(m[0], m[1], r * .3, a + 50), m, P(m[0], m[1], r * .3, a - 50)], w * .8, colr, amp=.2)


def drop(c, x, y, r=11, colr='sky'):
    c.poly([(x, y - r * 1.7), (x + r * .9, y + r * .1), (x + r * .6, y + r * .85), (x, y + r * 1.1), (x - r * .6, y + r * .85), (x - r * .9, y + r * .1)], colr, step=7, amp=.5, w=5.5, sx=3, sy=3)


def bolt(c, x, y, s=1.0, colr='yellow'):
    c.poly([(x + 14 * s, y - 50 * s), (x - 24 * s, y + 6 * s), (x - 2 * s, y + 6 * s), (x - 16 * s, y + 52 * s), (x + 28 * s, y - 10 * s), (x + 4 * s, y - 10 * s), (x + 22 * s, y - 50 * s)], colr, step=10, amp=.6)


def wind(c, y, x0=30, x1=220, curl=True, colr='ink', w=7):
    pts = [(x0, y), ((x0 + x1) / 2, y - 6), (x1 - 24, y)]
    if curl:
        pts += [P(x1 - 24, y - 16, 16, a) for a in range(90, -200, -30)]
    c.ln(pts, w, colr, amp=.5, step=9)


def leaf(c, x, y, rot=0, colr='orange', s=1.0):
    c.ell(x, y, 15 * s, 8 * s, colr, rot=rot, w=5)


def tree(c, x, y, colr='green', bare=False, r=44):
    c.poly([(x - 9, y), (x - 6, y - 62), (x + 6, y - 62), (x + 9, y)], 'brown', step=10)
    if bare:
        for a, l in ((-130, 50), (-100, 64), (-60, 52), (-30, 40)):
            c.ln([(x, y - 58), P(x, y - 58, l, a)], 7, amp=.6)
    else:
        c.lump(x, y - 92, r, r * .92, colr, bumps=7)


def pine(c, x, y, h=120, snow=False):
    c.rr(x - 8, y - 24, 16, 26, 4, 'brown')
    for i in range(3):
        base = y - 18 - i * h * .24; top = base - h * .42; ww = h * .62 * (1 - .2 * i)
        c.poly([(x - ww / 2, base), (x, top), (x + ww / 2, base)], 'pine', step=10,
               inner=(lambda base=base, top=top, ww=ww: c.fill([(x - ww * .3, base - (base - top) * .45), (x, top - 4), (x + ww * .3, base - (base - top) * .45), (x + ww * .14, base - (base - top) * .3), (x, base - (base - top) * .5), (x - ww * .14, base - (base - top) * .3)], 'white')) if snow else None)


def house(c, x, y, s=1.0, lit=False):
    c.rr(x - 30 * s, y - 40 * s, 60 * s, 42 * s, 3, 'cream')
    c.poly([(x - 40 * s, y - 38 * s), (x, y - 74 * s), (x + 40 * s, y - 38 * s)], 'brick', step=10)
    c.rr(x - 9 * s, y - 24 * s, 18 * s, 24 * s, 2, 'yellow' if lit else 'wood', w=5, shade=False)


def sky(c, colr, r=104, y=120):
    c.ell(128, y, r, r * .92, colr, shade=False)


# ---- sky and time of day ----------------------------------------------------------------------
@item
def sun_(c): sun(c, 128, 128, 58, 12, face=True)
@item
def bright(c):
    sun(c, 128, 132, 50, 12, long=1.5)
    for x, y, r in ((40, 46, 15), (214, 60, 12), (220, 206, 14)):
        c.poly(star_pts(x, y, r, r * .3, 4), 'white', w=5, step=8, amp=.3, shade=False)
@item
def sunny(c):
    sun(c, 150, 86, 44, 10, face=True); hill(c, 'green')
    for x, colr in ((60, 'brick'), (96, 'white')):
        c.ln([(x, 196), (x, 172)], 5, 'dgreen', amp=.2); c.ell(x, 166, 9, 9, colr, w=4.5, shade=False)
@item
def day(c):
    sky(c, 'pale'); sun(c, 128, 96, 42, 10); hill(c, 'green')
@item
def in_daytime(c):
    sun(c, 180, 76, 36, 10); cloud(c, 70, 70, 80); hill(c, 'green'); house(c, 96, 190, 1.1)
@item
def getting_light(c):
    sky(c, 'yellow', 104, 124)
    sun(c, 128, 186, 54, 7, colr='orange', a0=200, a1=340, long=1.3)
    hill(c, 'sage')
@item
def getting_dark(c):
    sky(c, 'navy', 104, 124)
    sun(c, 128, 196, 50, 0, colr='orange'); star(c, 70, 60, 12); star(c, 190, 80, 9); moon(c, 170, 54, 22, 'cream')
    hill(c, 'pine')
@item
def dark(c):
    sky(c, 'night'); moon(c, 150, 86, 36, 'ice'); cloud(c, 112, 122, 150, 'slate'); hill(c, 'forest')
@item
def night(c):
    sky(c, 'navy'); moon(c, 116, 118, 54); star(c, 186, 70, 17); star(c, 196, 150, 11); star(c, 62, 60, 10)
@item
def at_night(c):
    sky(c, 'navy'); moon(c, 180, 70, 30); star(c, 70, 60, 12); star(c, 116, 40, 8)
    hill(c, 'pine'); house(c, 100, 190, 1.15, lit=True)
@item
def moon_(c): moon(c, 118, 128, 92)
@item
def stars(c): star(c, 104, 156, 62, w=LW); star(c, 196, 64, 34); star(c, 46, 54, 22)
# ---- clouds, rain, storm -----------------------------------------------------------------------
@item
def clouds(c): cloud(c, 150, 86, 150, 'silver'); cloud(c, 104, 152, 190)
@item
def dark_clouds(c): cloud(c, 156, 84, 150, 'slate'); cloud(c, 104, 152, 190, 'navy')
@item
def becoming_cloudy(c): sun(c, 170, 86, 46, 10, a0=170, a1=380); cloud(c, 110, 150, 190)
@item
def became_cloudy(c): cloud(c, 170, 70, 140, 'ice'); cloud(c, 80, 100, 150, 'silver'); cloud(c, 136, 160, 200, 'sky')
@item
def rain(c):
    for i, (x, y) in enumerate(((66, 178), (106, 204), (146, 178), (186, 204))):
        drop(c, x, y, 13)
    cloud(c, 128, 96, 200, 'sky')
@item
def rainy(c):
    for x, y in ((40, 40), (90, 26), (150, 30), (210, 46), (226, 110), (26, 110)):
        c.ln([(x, y), (x - 8, y + 22)], 7, 'ink', amp=.2); c.ln([(x, y + 1), (x - 7, y + 20)], 2.8, 'sky', amp=.2)
    c.ln([(128, 120), (128, 214), (112, 226), (100, 212)], 8, amp=.3)
    c.poly([P(128, 132, 92, a) for a in range(180, 361, 15)] + [(196, 132), (174, 118), (150, 132), (128, 118), (106, 132), (82, 118), (60, 132)], 'brick', step=10,
           inner=lambda: [c.fill([(128, 40), P(128, 132, 96, a), P(128, 132, 96, a + 22)], 'orange') for a in (202, 248, 294)])
@item
def moisture(c): drop(c, 112, 132, 58); drop(c, 196, 84, 22); drop(c, 204, 176, 16); c.hi([(92, 140), (100, 164)], 6)
@item
def fog(c):
    hill(c, 'sage'); house(c, 128, 188, 1.3)
    for y, x0, x1 in ((92, 40, 200), (124, 20, 236), (156, 44, 216), (188, 16, 190)):
        c.rr(x0, y - 11, x1 - x0, 22, 11, 'silver', sx=3, sy=3)
@item
def lightning(c): bolt(c, 128, 128, 2.1)
@item
def lightning_strike(c):
    tree(c, 80, 226, 'dgreen', r=36); bolt(c, 150, 150, 1.25); cloud(c, 140, 62, 190, 'navy')
    for a in (200, 250, 300, 350):
        c.ln([P(136, 218, 16, a), P(136, 218, 30, a)], 5, 'gold', amp=.2)
@item
def storm(c):
    drop(c, 64, 196, 12); drop(c, 196, 190, 12); drop(c, 100, 222, 10)
    bolt(c, 138, 176, 1.2); cloud(c, 128, 82, 204, 'navy')
@item
def thunder(c):
    cloud(c, 108, 112, 180, 'slate')
    for r in (34, 58, 82):
        c.arc(180, 150, r, -30, 60, 7.5)
@item
def thundering(c):
    cloud(c, 128, 84, 200, 'slate'); bolt(c, 76, 178, .9)
    for r in (30, 52, 74):
        c.arc(150, 170, r, 10, 80, 7.5)
@item
def rainbow(c):
    for r, colr in ((104, 'brick'), (88, 'orange'), (72, 'yellow'), (56, 'green'), (40, 'blue')):
        c.poly([P(128, 176, r + 9, a) for a in range(180, 361, 10)] + [P(128, 176, r - 9, a) for a in range(360, 179, -10)], colr, shade=False, w=0)
    c.arc(128, 176, 113, 180, 360); c.arc(128, 176, 31, 180, 360, 6)
    cloud(c, 44, 186, 84); cloud(c, 212, 186, 84)
@item
def weather(c):
    sun(c, 84, 84, 40, 10, a0=120, a1=330); cloud(c, 140, 120, 170)
    drop(c, 96, 196, 12); flake(c, 150, 206, 15); bolt(c, 200, 200, .5)
# ---- wind -------------------------------------------------------------------------------------
@item
def air(c):
    wind(c, 80, 40, 200, w=8); wind(c, 128, 60, 230, w=8); wind(c, 176, 34, 180, w=8)
    for x, y in ((40, 128), (214, 176), (226, 76)):
        c.dot(x, y, 5, 'sky')
@item
def wind_(c):
    cloud(c, 76, 110, 130, 'ice'); c.dot(92, 104, 4.5); c.ell(118, 124, 7, 9, 'ink', w=0, shade=False)
    wind(c, 96, 140, 236); wind(c, 136, 136, 226); wind(c, 170, 120, 200, curl=False)
    leaf(c, 200, 190, .5); leaf(c, 150, 206, -.4, 'gold')
@item
def windstorm(c):
    c.poly([(60, 230), (74, 150), (120, 96), (128, 104), (92, 160), (84, 230)], 'brown', step=10)
    c.lump(160, 84, 56, 34, 'dgreen', bumps=6)
    wind(c, 150, 20, 236); wind(c, 190, 60, 240); wind(c, 40, 20, 150, curl=False)
    leaf(c, 216, 124, .4, 'green'); leaf(c, 184, 218, -.3, 'lime'); leaf(c, 36, 96, .8, 'green')
# ---- cold --------------------------------------------------------------------------------------
@item
def snow(c): flake(c, 110, 140, 78, 9); flake(c, 208, 60, 30, 6.5); flake(c, 216, 206, 18, 6)
@item
def snowy(c):
    hill(c, snow=True); flake(c, 60, 150, 15); flake(c, 128, 164, 15); flake(c, 196, 150, 15); cloud(c, 128, 76, 196, 'silver')
@item
def blizzard(c):
    cloud(c, 128, 70, 200, 'sky')
    for x, y in ((50, 140), (110, 150), (170, 140), (80, 196), (150, 200), (214, 190)):
        flake(c, x, y, 13, 5.5)
    wind(c, 172, 16, 226, curl=False); wind(c, 228, 40, 240, curl=False)
@item
def snow_melted(c):
    c.ell(128, 200, 108, 26, 'sky', inner=lambda: c.fe(100, 196, 40, 8, 'white'))
    c.lump(128, 172, 56, 30, 'white', bumps=5, depth=.25)
    c.dot(110, 164, 5); c.dot(140, 164, 5); c.poly([(124, 174), (158, 180), (124, 186)], 'orange', w=5, step=8)
    sun(c, 196, 60, 30, 9)
    c.ln([(44, 190), (44, 160)], 5.5, 'dgreen'); leaf(c, 54, 156, -.6, 'lime', .8)
@item
def frost(c):
    c.ln([(30, 220), (120, 130), (216, 40)], 10, 'ink'); c.ln([(120, 130), (200, 150)], 9); c.ln([(84, 166), (60, 100)], 9)
    c.ln([(32, 218), (120, 130), (214, 42)], 4.5, 'ice'); c.ln([(120, 130), (198, 150)], 4, 'ice'); c.ln([(84, 166), (61, 102)], 4, 'ice')
    for x, y, r in ((216, 40, 22), (204, 152, 18), (58, 96, 18), (150, 76, 13), (150, 200, 15)):
        flake(c, x, y, r, 6, 'navy')
@item
def ice(c):
    c.poly([(128, 30), (220, 76), (220, 180), (128, 230), (36, 180), (36, 76)], 'pale', step=14,
           inner=lambda: (c.fill([(36, 76), (128, 122), (128, 232), (36, 182)], 'mint'), c.fill([(128, 30), (220, 76), (128, 122), (36, 76)], 'white')))
    c.ln([(36, 76), (128, 122), (220, 76)], 6); c.ln([(128, 122), (128, 228)], 6); c.hi([(150, 150), (196, 126)])
@item
def cold(c): thermo(c, 96, .28, 'blue'); flake(c, 196, 110, 34, 7)
@item
def very_cold(c):
    thermo(c, 96, .08, 'navy'); flake(c, 196, 70, 26, 6.5); flake(c, 208, 140, 18); flake(c, 176, 196, 22, 6)
    c.poly([(70, 34), (122, 34), (118, 60), (110, 46), (102, 70), (94, 48), (86, 62), (78, 44)], 'pale', w=5.5, step=8)
@item
def cool(c): thermo(c, 96, .42, 'sky'); cloud(c, 190, 100, 96, 'ice'); wind(c, 160, 150, 236, w=6)
@item
def warm(c): thermo(c, 96, .62, 'orange'); sun(c, 196, 104, 26, 9)
@item
def hot(c): thermo(c, 92, .85, 'brick'); sun(c, 194, 100, 34, 10, face=True)
@item
def heat(c):
    thermo(c, 92, .92, 'brick')
    for x in (160, 192, 224):
        c.ln([(x, 190), (x + 12, 164), (x - 6, 136), (x + 12, 108), (x - 6, 80), (x + 6, 54)], 8, 'ink', amp=.4); c.ln([(x, 188), (x + 12, 164), (x - 6, 136), (x + 12, 108), (x - 6, 80), (x + 5, 56)], 3.2, 'orange', amp=.3)
@item
def scorching_heat(c):
    pts = []
    for i in range(24):
        pts.append(P(128, 100, 84 if i % 2 == 0 else 58, i * 15 + 4))
    c.poly(pts, 'orange', step=10, amp=.6)
    c.ell(128, 100, 46, 46, 'yellow'); c.dot(112, 94, 5); c.dot(144, 94, 5); c.arc(128, 116, 10, 200, 340, 5)
    c.poly([(20, 206), (236, 206), (246, 240), (10, 240)], 'sand', step=14)
    c.ln([(70, 208), (84, 224), (72, 240)], 5); c.ln([(150, 208), (140, 222), (160, 238)], 5); c.ln([(200, 208), (206, 226)], 5)
@item
def negative_temperature_question(c): thermo(c, 84, .15, 'blue'); c.text('−?', 190, 150, 78)
@item
def positive_temperature_question(c): thermo(c, 84, .8, 'brick'); c.text('+?', 190, 150, 78)
# ---- seasons -----------------------------------------------------------------------------------
@item
def spring(c):
    hill(c, 'lime'); sun(c, 200, 56, 24, 8)
    c.ln([(120, 190), (124, 110)], 7, 'dgreen'); leaf(c, 142, 150, -.5, 'green', 1.3); leaf(c, 102, 164, .5, 'green', 1.3)
    for a in range(0, 360, 60):
        x, y = P(124, 90, 26, a); c.ell(x, y, 15, 15, 'white', w=5.5)
    c.ell(124, 90, 15, 15, 'yellow', w=5.5)
    leaf(c, 50, 178, .9, 'green'); leaf(c, 198, 180, -.9, 'green')
@item
def summer(c):
    sun(c, 186, 66, 38, 10, face=True); hill(c, 'green'); tree(c, 84, 200, 'dgreen', r=50)
    for x, y in ((66, 96), (104, 120), (92, 76)):
        c.dot(x, y, 6, 'brick')
@item
def autumn(c):
    hill(c, 'sand'); tree(c, 112, 200, 'orange', r=56)
    for x, y, r, colr in ((190, 110, .5, 'brick'), (206, 160, -.4, 'gold'), (40, 150, .8, 'orange'), (176, 196, .2, 'brick')):
        leaf(c, x, y, r, colr, 1.1)
    c.fe(96, 96, 14, 12, 'gold'); c.fe(134, 120, 12, 10, 'brick')
@item
def winter(c):
    hill(c, snow=True); pine(c, 100, 204, 150, snow=True)
    flake(c, 196, 60, 22, 6); flake(c, 206, 134, 15); flake(c, 36, 70, 15); flake(c, 170, 100, 11, 5)


RENAME = dict(sun_='sun', moon_='moon', wind_='wind')


def make():
    for i, (name, f) in enumerate(D.items()):
        c = Sp(500 + i); f(c); c.out(ACT, 'weather_' + RENAME.get(name, name))


if __name__ == '__main__':
    import sk
    if len(sys.argv) > 1:
        sk.PREVIEW = sys.argv[1]
    make()
    if len(sys.argv) > 1:
        sheet(['weather_' + RENAME.get(n, n) for n in D], ACT, sys.argv[1] + '/_weather.png', cols=9, cell=170)
