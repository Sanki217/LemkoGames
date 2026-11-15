"""Detailed scenery kit: two-tone shading, textured roofs and walls, and the small props
(flowers, fences, wells, carts, animals, villagers) that make the map and backdrops feel lived in.
Only palette colours are used; the darker tone of a colour is its neighbour in sk.SHADE."""
import math
from ink import *
from sk import PAL, SHADE

_NAME = {v.lower(): k for k, v in PAL.items()}
SKIN = PAL['skin']
PLUM = PAL['plum']
MAUVE = PAL['mauve']
LIME = PAL['lime']
SAND = PAL['sand']
BLOOM = (WHITE, GOLD, ORANGE, RED, MAUVE, WHITE, GOLD)


def dk(c):
    """Darker palette neighbour of a palette colour."""
    return PAL[SHADE[_NAME[c.lower()]]]


class Scene(Canvas):
    # ---- drawing helpers -----------------------------------------------------------
    def stroke(self, p, w=2.6, color=INK, closed=True):
        self.pen(p, w * LWK, color, closed)

    def inside(self, p, fn):
        self._path(p, True); self.c.save(); self.c.clip(); fn(); self.c.restore()

    def two(self, pts, fill, w=2.6, dx=4, dy=5, amp=1.3, step=16, dark=None, inner=None):
        """Outlined shape with a darker band along its bottom-right edge."""
        p = self.wobble(pts, amp, step, closed=True)
        self.flat(p, dark or dk(fill))
        def light():
            self.flat([(x - dx, y - dy) for x, y in p], fill)
            if inner:
                inner()
        self.inside(p, light)
        if w:
            self.edge(p, w * LWK)
        return p

    def ring(self, cx, cy, rx, ry, n=28):
        return [(cx + math.cos(6.2832 * i / n) * rx, cy + math.sin(6.2832 * i / n) * ry) for i in range(n)]

    def ball(self, cx, cy, rx, ry=None, fill=WHITE, w=2.2, **k):
        return self.two(self.ring(cx, cy, rx, ry or rx), fill, w, amp=.5, step=7, **k)

    def lumpy(self, cx, cy, rx, ry, bumps=7, lump=.2):
        ph = self.r.uniform(0, 6.28)
        n = bumps * 6
        return [(cx + math.cos(6.2832 * i / n) * rx * (1 + lump * abs(math.sin(3.1416 * i / 6 + ph)) - lump * .5),
                 cy + math.sin(6.2832 * i / n) * ry * (1 + lump * abs(math.sin(3.1416 * i / 6 + ph)) - lump * .5)) for i in range(n)]

    def dotc(self, x, y, r, color):
        self.c.set_source_rgb(*rgb(color)); self.c.arc(x, y, r, 0, 6.3); self.c.fill()

    # ---- plants ----------------------------------------------------------------------
    def tree(self, x, y, r=24, fill=LEAF, fruit=None, trunk=WOOD):
        self.sh_oval(x + r * .4, y + 2, r * .95, r * .26)
        self.two([(x - 4.5, y), (x - 3, y - r * 1.15), (x + 3, y - r * 1.15), (x + 4.5, y)], trunk, 2.2, 2, 0, amp=.3, step=8)
        self.line([(x + 1, y - r * .8), (x + r * .45, y - r * 1.2)], 2.2, amp=.2, step=8)
        cy = y - r * 1.75
        p = self.lumpy(x, cy, r, r * .92, 7, .22)
        def marks():
            self.flat(self.ring(x - r * .3, cy - r * .35, r * .42, r * .3), GRASS_LIGHT if fill == LEAF else fill, .55 if fill == LEAF else 0)
            for i in range(5):
                a = self.r.uniform(0, 6.28); d = self.r.uniform(.15, .65) * r
                mx, my = x + math.cos(a) * d, cy + math.sin(a) * d * .9
                self.line([(mx - 4, my - 1), (mx, my + 3), (mx + 4, my - 1)], 1.3, dk(fill), amp=.2, step=5)
        self.two(p, fill, 2.4, r * .2, r * .22, amp=.7, step=9, inner=marks)
        if fruit:
            for i in range(6):
                a = i * 1.05 + .4; d = r * (.35 + .3 * (i % 2))
                self.oval(x + math.cos(a) * d, cy + math.sin(a) * d * .85, 3.6, 3.6, fruit, 1.3, amp=.2)

    def birch(self, x, y, r=20):
        self.sh_oval(x + r * .4, y + 2, r * .8, r * .22)
        self.shape([(x - 3, y), (x - 2, y - r * 1.6), (x + 2, y - r * 1.6), (x + 3, y)], WHITE, 2, amp=.3, step=8)
        for t in (.25, .5, .8):
            self.line([(x - 2.5, y - r * 1.5 * t), (x + 1, y - r * 1.5 * t - 1)], 1.3, amp=0)
        self.two(self.lumpy(x, y - r * 2.1, r * .72, r * 1.05, 6, .2), LIME, 2.4, r * .16, r * .2, amp=.6, step=9)

    def pine(self, x, y, h=58, shade=PINE_DARK):
        """Chubby fir: three soft rounded tiers."""
        w = h * .72
        self.sh_oval(x + w * .2, y + 2, w * .5, h * .07)
        self.two([(x - 4, y), (x - 4, y - h * .2), (x + 4, y - h * .2), (x + 4, y)], WOOD_DARK, 2, 2, 0)
        old = self.rad; self.rad = h * .12
        for i in range(3):
            base = y - h * (.1 + .25 * i)
            top = base - h * (.44 if i < 2 else .5)
            ww = w * (1 - .2 * i)
            p = [(x - ww / 2, base), (x - ww * .2, top + (base - top) * .45), (x, top), (x + ww * .2, top + (base - top) * .45), (x + ww / 2, base), (x, base + h * .05)]
            self.two(p, PINE, 2.3, ww * .16, 0, dark=shade)
            self.line([(x - ww * .22, base - (base - top) * .3), (x - ww * .1, base - (base - top) * .5)], 1.6, GRASS_LIGHT)
        self.rad = old

    def bush(self, x, y, r=16, flowers=None):
        p = self.lumpy(x, y - r * .6, r, r * .7, 5, .24)
        self.two(p, LEAF, 2.2, r * .2, r * .2, amp=.5, step=8)
        if flowers:
            for i in range(4):
                self.oval(x + (i - 1.5) * r * .42, y - r * (.55 + .25 * (i % 2)), 2.6, 2.6, flowers, 1, amp=.1)

    def flowers(self, x, y, n=6, spread=16, colors=BLOOM):
        for i in range(n):
            fx, fy = x + self.r.uniform(-spread, spread), y + self.r.uniform(-spread * .5, spread * .5)
            self.line([(fx, fy + 5), (fx, fy)], 1.1, LEAF_DARK, amp=0)
            self.oval(fx, fy, 2.7, 2.7, colors[self.r.randrange(len(colors))], 1, amp=.1)

    def tuft(self, x, y, k=1.0, color=PINE):
        for dx in (-5, -1.5, 2, 5.5):
            self.line([(x + dx * k, y), (x + dx * k * 1.5, y - (5 + abs(dx)) * k * .8)], 1.4, color, amp=.2, step=5)

    def reeds(self, x, y, n=4):
        for i in range(n):
            rx = x + i * 5 + self.r.uniform(-1, 1); hh = self.r.uniform(14, 24)
            self.line([(rx, y), (rx + 2, y - hh)], 1.4, LEAF_DARK, amp=.2, step=8)
            self.line([(rx + 2, y - hh), (rx + 2.6, y - hh - 6)], 2.4, WOOD_DARK, amp=0)

    def rock(self, x, y, r=10, fill=TONE2):
        self.two(self.lumpy(x, y - r * .5, r, r * .65, 4, .18), fill, 2, r * .25, r * .3, amp=.4, step=7)

    def field(self, x, y, w, h, color=GOLD, rows=6, slant=.5, crop=None):
        """Crop field as a slanted plot; x,y is the near-left corner."""
        p = [(x, y), (x + w, y - w * .1), (x + w + h * slant, y - w * .1 - h), (x + h * slant, y - h)]
        def furrows():
            for i in range(1, rows):
                t = i / rows
                a = (x + h * slant * t, y - h * t); b = (x + w + h * slant * t, y - w * .1 - h * t)
                self.line([a, b], 1.6, dk(color), amp=.6, step=14)
                if crop:
                    for k in range(1, int(w / 13)):
                        u = k * 13 / w
                        self.oval(a[0] + (b[0] - a[0]) * u, a[1] + (b[1] - a[1]) * u - 2 + h / rows * .5, 3, 2.6, crop, .9, amp=.1)
        self.two(p, color, 2.4, 3, 3, inner=furrows)

    def pond(self, x, y, rx=46, ry=20):
        self.oval(x, y, rx + 5, ry + 4, SAND, 0)
        p = self.lumpy(x, y, rx, ry, 5, .12)
        self.two(p, WATER, 2.2, -4, -4, dark=WATER_DARK, amp=.6, step=9)
        self.line([(x - rx * .4, y - 2), (x - rx * .15, y - 5), (x + rx * .1, y - 2)], 1.4, WHITE, amp=.2, step=6)
        for dx, dy in ((rx * .35, 4), (-rx * .1, 7)):
            self.oval(x + dx, y + dy, 5, 3, LEAF, 1, amp=.1)
        self.reeds(x + rx * .7, y - 2, 3)

    # ---- animals and people -------------------------------------------------------------
    def duck(self, x, y, k=1.0):
        self.oval(x, y, 7 * k, 5 * k, WHITE, 1.4, amp=.2); self.oval(x + 6 * k, y - 5 * k, 3.8 * k, 3.8 * k, WHITE, 1.4, amp=.2)
        self.flat([(x + 9 * k, y - 5.5 * k), (x + 14 * k, y - 4 * k), (x + 9 * k, y - 3 * k)], ORANGE); self.dotc(x + 6.5 * k, y - 5.8 * k, .9 * k, INK)

    def sheep(self, x, y, k=1.0, wool=WHITE):
        for dx in (-7, 6):
            self.line([(x + dx * k, y + 6 * k), (x + dx * k, y + 14 * k)], 2, amp=0)
        self.two(self.lumpy(x, y, 15 * k, 10 * k, 6, .3), wool, 2, 3, 3, amp=.4, step=6)
        self.oval(x + 15 * k, y - 3 * k, 5.5 * k, 5 * k, PLUM, 1.4, amp=.2)

    def cow(self, x, y, k=1.0):
        for dx in (-11, -5, 7, 12):
            self.line([(x + dx * k, y + 6 * k), (x + dx * k, y + 16 * k)], 2.2, amp=0)
        p = self.ring(x, y, 18 * k, 10 * k)
        self.two(p, WHITE, 2, 3, 3, amp=.4, step=7, inner=lambda: (self.flat(self.ring(x - 5 * k, y - 3 * k, 6 * k, 5 * k), WOOD_DARK), self.flat(self.ring(x + 8 * k, y + 4 * k, 5 * k, 4 * k), WOOD_DARK)))
        self.oval(x + 20 * k, y - 6 * k, 7 * k, 6 * k, WHITE, 1.6, amp=.2); self.oval(x + 24 * k, y - 3 * k, 4 * k, 3 * k, SKIN, 1.2, amp=.1)
        self.dotc(x + 19 * k, y - 8 * k, 1 * k, INK)

    def hen(self, x, y, k=1.0, fill=WHITE):
        self.line([(x - 1 * k, y + 4 * k), (x - 1 * k, y + 9 * k)], 1.2, ORANGE, amp=0); self.line([(x + 3 * k, y + 4 * k), (x + 3 * k, y + 9 * k)], 1.2, ORANGE, amp=0)
        self.oval(x, y, 7 * k, 5.5 * k, fill, 1.4, amp=.2); self.oval(x + 6 * k, y - 6 * k, 3.6 * k, 3.6 * k, fill, 1.4, amp=.2)
        self.dotc(x + 6 * k, y - 10 * k, 1.8 * k, RED); self.flat([(x + 9 * k, y - 7 * k), (x + 13 * k, y - 5.5 * k), (x + 9 * k, y - 4 * k)], GOLD)

    def person(self, x, y, shirt=RED, hair=WOOD_DARK, k=1.0, skirt=False, hat=None):
        """Small villager; x,y is between the feet."""
        self.sh_oval(x + 4 * k, y + 1, 9 * k, 3 * k)
        if skirt:
            self.shape([(x - 5 * k, y - 22 * k), (x + 5 * k, y - 22 * k), (x + 9 * k, y - 3 * k), (x - 9 * k, y - 3 * k)], shirt, 1.8, amp=.3, step=8)
            self.line([(x - 3 * k, y - 3 * k), (x - 3 * k, y)], 1.8, amp=0); self.line([(x + 3 * k, y - 3 * k), (x + 3 * k, y)], 1.8, amp=0)
        else:
            self.shape([(x - 5 * k, y - 10 * k), (x + 5 * k, y - 10 * k), (x + 5 * k, y), (x + 1 * k, y), (x, y - 6 * k), (x - 1 * k, y), (x - 5 * k, y)], NAVY, 1.6, amp=.2, step=8)
            self.shape([(x - 6 * k, y - 23 * k), (x + 6 * k, y - 23 * k), (x + 7 * k, y - 9 * k), (x - 7 * k, y - 9 * k)], shirt, 1.8, amp=.3, step=8)
        self.oval(x, y - 29 * k, 6.5 * k, 6.5 * k, SKIN, 1.8, amp=.2)
        self.flat([(x - 6.5 * k, y - 30 * k), (x - 5 * k, y - 35 * k), (x, y - 37 * k), (x + 5 * k, y - 35 * k), (x + 6.5 * k, y - 30 * k), (x, y - 32.5 * k)], hair)
        if hat:
            self.shape([(x - 9 * k, y - 33 * k), (x + 9 * k, y - 33 * k), (x + 5 * k, y - 36 * k), (x + 4 * k, y - 41 * k), (x - 4 * k, y - 41 * k), (x - 5 * k, y - 36 * k)], hat, 1.5, amp=.2, step=8)
        self.dotc(x - 2.2 * k, y - 28.5 * k, .9 * k, INK); self.dotc(x + 2.2 * k, y - 28.5 * k, .9 * k, INK)

    # ---- props -----------------------------------------------------------------------
    def haybale(self, x, y, r=22):
        self.sh_oval(x + 6, y + 2, r, r * .28)
        self.two(self.lumpy(x, y - r * .62, r, r * .78, 5, .1), GOLD, 2.4, 4, 4, amp=.5, step=8)
        self.hatch(x - r * .6, y - r * 1.1, x + r * .5, y - r * .15, gap=7, w=1.3, color=PAL['gold'])

    def well(self, x, y):
        self.sh_oval(x + 8, y + 3, 22, 7)
        self.two([(x - 16, y), (x - 16, y - 18), (x + 16, y - 18), (x + 16, y)], STONE, 2.2, 3, 0, amp=.4, step=10)
        for i, yy in enumerate((y - 6, y - 12)):
            self.line([(x - 15, yy), (x + 15, yy)], 1.1, amp=.2)
            for xx in range(-10 + i * 5, 16, 10):
                self.line([(x + xx, yy), (x + xx, yy + 6)], 1.1, amp=0)
        self.oval(x, y - 18, 16, 5, NAVY, 2, amp=.3)
        self.line([(x - 13, y - 18), (x - 13, y - 44)], 2.4, amp=.2); self.line([(x + 13, y - 18), (x + 13, y - 44)], 2.4, amp=.2)
        self.two([(x - 22, y - 42), (x, y - 58), (x + 22, y - 42)], RED, 2.2, 3, 3, amp=.4, step=10)
        self.line([(x, y - 44), (x, y - 30)], 1.2, amp=0); self.shape([(x - 4, y - 30), (x + 4, y - 30), (x + 3, y - 23), (x - 3, y - 23)], WOOD, 1.4, amp=.1, step=8)

    def wheel(self, x, y, r=9):
        self.oval(x, y, r, r, WOOD, 2, amp=.2)
        for a in (0, 1.05, 2.1):
            self.line([(x - math.cos(a) * r, y - math.sin(a) * r), (x + math.cos(a) * r, y + math.sin(a) * r)], 1.1, amp=0)
        self.dotc(x, y, 1.8, INK)

    def cart(self, x, y, load=GOLD):
        self.sh_oval(x + 6, y + 3, 30, 6)
        if load:
            self.two(self.lumpy(x, y - 30, 24, 12, 5, .2), load, 2, 3, 3, amp=.4, step=7)
        self.two([(x - 26, y - 26), (x + 26, y - 26), (x + 22, y - 10), (x - 22, y - 10)], WOOD, 2.2, 3, 3, amp=.4, step=10)
        self.line([(x - 24, y - 18), (x + 24, y - 18)], 1.2, amp=.2)
        self.line([(x + 24, y - 14), (x + 46, y - 4)], 2.2, amp=.3)
        self.wheel(x - 13, y - 6); self.wheel(x + 13, y - 6)

    def beehive(self, x, y):
        for i, ww in enumerate((12, 10.5, 8, 5)):
            self.oval(x, y - 5 - i * 7, ww, 4.6, GOLD, 1.6, amp=.2)
        self.dotc(x, y - 6, 2.2, INK)
        self.shape([(x - 14, y), (x + 14, y), (x + 12, y + 5), (x - 12, y + 5)], WOOD, 1.6, amp=.2, step=8)

    def logs(self, x, y, n=3):
        for row in range(2):
            for i in range(n - row):
                cx, cy = x + i * 13 + row * 6.5, y - 6 - row * 11
                self.oval(cx, cy, 6.5, 6.5, WOOD, 1.6, amp=.2); self.oval(cx, cy, 3, 3, PAL['skin'], .9, amp=.1)

    def lamp(self, x, y, h=44):
        self.line([(x, y), (x, y - h)], 2.6, amp=.2)
        self.shape([(x - 5, y - h), (x + 5, y - h), (x + 6, y - h - 11), (x - 6, y - h - 11)], GOLD, 1.8, amp=.2, step=8)
        self.shape([(x - 8, y - h - 11), (x + 8, y - h - 11), (x, y - h - 18)], WOOD_DARK, 1.6, amp=.2, step=8)

    def bench(self, x, y, w=34):
        self.line([(x - w / 2 + 4, y - 8), (x - w / 2 + 4, y)], 2, amp=0); self.line([(x + w / 2 - 4, y - 6), (x + w / 2 - 4, y + 2)], 2, amp=0)
        self.shape([(x - w / 2, y - 12), (x + w / 2, y - 10), (x + w / 2, y - 5), (x - w / 2, y - 7)], WOOD, 1.8, amp=.2, step=10)

    def signpost(self, x, y):
        self.line([(x, y), (x, y - 46)], 2.8, amp=.2)
        self.shape([(x - 2, y - 44), (x + 24, y - 44), (x + 30, y - 38), (x + 24, y - 32), (x - 2, y - 32)], PAPER, 1.8, amp=.2, step=10)
        self.shape([(x + 2, y - 28), (x - 24, y - 28), (x - 30, y - 22), (x - 24, y - 16), (x + 2, y - 16)], GOLD, 1.8, amp=.2, step=10)
        self.line([(x + 4, y - 38), (x + 20, y - 38)], 1.1, amp=0); self.line([(x - 20, y - 22), (x - 4, y - 22)], 1.1, amp=0)

    def barrel(self, x, y, k=1.0):
        self.two([(x - 9 * k, y), (x - 11 * k, y - 12 * k), (x - 9 * k, y - 24 * k), (x + 9 * k, y - 24 * k), (x + 11 * k, y - 12 * k), (x + 9 * k, y)], WOOD, 2, 3, 0, amp=.3, step=8)
        for yy in (y - 6 * k, y - 18 * k):
            self.line([(x - 10 * k, yy), (x + 10 * k, yy)], 1.6, amp=.1)

    def crate(self, x, y, goods=(RED, GOLD, ORANGE), k=1.0):
        for i, g in enumerate(goods):
            self.oval(x + (i - (len(goods) - 1) / 2) * 9 * k, y - 20 * k, 5.5 * k, 5.5 * k, g, 1.4, amp=.2)
        self.two([(x - 16 * k, y), (x - 16 * k, y - 17 * k), (x + 16 * k, y - 17 * k), (x + 16 * k, y)], WOOD, 2, 3, 0, amp=.3, step=8)
        self.line([(x - 15 * k, y - 8.5 * k), (x + 15 * k, y - 8.5 * k)], 1.2, amp=.1)

    def laundry(self, x0, y0, x1, y1, n=4):
        self.line([(x0, y0 + 34), (x0, y0)], 2.4, amp=.2); self.line([(x1, y1 + 34), (x1, y1)], 2.4, amp=.2)
        self.line([(x0, y0 + 2), ((x0 + x1) / 2, (y0 + y1) / 2 + 9), (x1, y1 + 2)], 1.3, amp=.3)
        for i in range(n):
            t = (i + .7) / (n + .4)
            fx = x0 + (x1 - x0) * t; fy = y0 + (y1 - y0) * t + 9 * 4 * t * (1 - t) + 2
            col = (WHITE, RED, PAL['sky'], GOLD, WHITE)[i % 5]
            self.shape([(fx - 6, fy), (fx + 6, fy + .5), (fx + 5, fy + 15), (fx - 5, fy + 14)], col, 1.4, amp=.3, step=8)

    def fence(self, x0, y0, x1, y1, posts=6, color=WOOD):
        for d in (15, 7):
            self.line([(x0, y0 - d), (x1, y1 - d)], 3.4, amp=.6); self.line([(x0, y0 - d), (x1, y1 - d)], 1.5, color, amp=.6)
        for i in range(posts):
            t = i / (posts - 1)
            x, y = x0 + (x1 - x0) * t, y0 + (y1 - y0) * t
            self.shape([(x - 3, y), (x - 3, y - 20), (x, y - 23), (x + 3, y - 20), (x + 3, y)], color, 1.7, amp=.2, step=10)

    def garden(self, x, y, w=60, h=30, crop=LEAF):
        self.field(x, y, w, h, WOOD, rows=4, slant=.45, crop=crop)

    def cloud(self, x, y, w=90, fill=WHITE):
        p = self.lumpy(x, y, w / 2, w * .2, 5, .5)
        self.two(p, fill, 2.4, 3, 4, amp=.5, step=9)

    def mountain(self, x, y, w=150, h=170, snow=True):
        """Soft rounded hill-mountain with a snow cap."""
        old = self.rad; self.rad = w * .16
        p = [(x - w * .62, y + 6), (x - w * .3, y - h * .55), (x, y - h), (x + w * .3, y - h * .55), (x + w * .62, y + 6)]
        def cap():
            self.flat([(x - w, y - h * 1.2), (x + w, y - h * 1.2), (x + w, y - h * .62), (x + w * .16, y - h * .7), (x, y - h * .56), (x - w * .16, y - h * .7), (x - w, y - h * .62)], WHITE)
            for i in range(3):
                px, py = x - w * .22 + i * w * .16, y - h * (.34 - i * .06)
                self.line([(px, py), (px + 8, py + 9)], 1.6, TONE3, amp=0)
        self.two(p, TONE2, 2.6, w * .13, 0, inner=cap if snow else None)
        self.rad = old

    def pane(self, wx, wy, ww=20, wh=22, glass=GOLD, shutters=None, box=True):
        if shutters:
            for s in (-1, 1):
                sx = wx + s * (ww / 2 + 5.5)
                self.shape([(sx - 4, wy - wh / 2 - 1), (sx + 4, wy - wh / 2 - 1), (sx + 4, wy + wh / 2 + 1), (sx - 4, wy + wh / 2 + 1)], shutters, 1.5, amp=.2, step=10)
                self.line([(sx - 3, wy), (sx + 3, wy)], .9, amp=0)
        self.shape([(wx - ww / 2 - 2, wy - wh / 2 - 2), (wx + ww / 2 + 2, wy - wh / 2 - 2), (wx + ww / 2 + 2, wy + wh / 2 + 2), (wx - ww / 2 - 2, wy + wh / 2 + 2)], WHITE, 2, amp=.3, step=10)
        self.flat([(wx - ww / 2 + 1, wy - wh / 2 + 1), (wx + ww / 2 - 1, wy - wh / 2 + 1), (wx + ww / 2 - 1, wy + wh / 2 - 1), (wx - ww / 2 + 1, wy + wh / 2 - 1)], glass)
        self.flat([(wx - ww / 2 + 2, wy + wh / 2 - 6), (wx - ww / 2 + 2, wy + wh / 2 - 10), (wx - ww / 2 + 9, wy - wh / 2 + 2), (wx - ww / 2 + 13, wy - wh / 2 + 2)], WHITE, .55)
        self.line([(wx, wy - wh / 2), (wx, wy + wh / 2)], 1.3, amp=.1); self.line([(wx - ww / 2, wy), (wx + ww / 2, wy)], 1.3, amp=.1)
        if box:
            by = wy + wh / 2 + 3
            for i in range(4):
                fx = wx - ww / 2 + 2 + i * (ww - 4) / 3
                self.dotc(fx, by - 2, 2.6, LEAF); self.dotc(fx, by - 4, 1.9, (RED, GOLD, WHITE, ORANGE)[i % 4])
            self.shape([(wx - ww / 2 - 4, by), (wx + ww / 2 + 4, by), (wx + ww / 2 + 3, by + 5), (wx - ww / 2 - 3, by + 5)], WOOD, 1.4, amp=.2, step=10)

    def door(self, x, y, w=24, h=40, fill=WOOD_DARK):
        self.shape([(x - w / 2 - 5, y + 1), (x + w / 2 + 5, y + 1), (x + w / 2 + 8, y + 7), (x - w / 2 - 8, y + 7)], STONE, 1.6, amp=.2, step=10)
        top = [(x + math.cos(math.radians(a)) * w / 2, y - h + w / 2 - math.sin(math.radians(a)) * w / 2) for a in range(0, 181, 30)]
        self.shape([(x + w / 2, y)] + top + [(x - w / 2, y)], fill, 2.2, amp=.3, step=10)
        self.line([(x, y - h + 2), (x, y - 1)], 1.1, amp=.1)
        self.dotc(x + w * .28, y - h * .42, 1.9, GOLD)

    def house(self, x, y, w=120, h=78, depth=56, roof=RED, wall=PAPER, roof_h=58, door=True, windows=2, chimney=False,
              planks=False, shingles=True, side=None, shutters=None, box=True, glass=GOLD, door_fill=WOOD_DARK):
        """Oblique house with shaded walls, a stone footing and a shingled roof.
        x,y is the middle of the front wall's ground line."""
        sk = .36
        side = side or dk(wall)
        fl, fr = (x - w / 2, y), (x + w / 2, y)
        br = (fr[0] + depth, fr[1] - depth * sk)
        tl, tr = (fl[0], fl[1] - h), (fr[0], fr[1] - h)
        btr = (br[0], br[1] - h)
        apex = (x, tl[1] - roof_h)
        bapex = (apex[0] + depth, apex[1] - depth * sk)
        ov = 9
        self.shadow([(fl[0] - 6, y + 4), (fr[0], y + 9), (br[0] + 30, br[1] + 14), (br[0] + 10, br[1] - 4)])
        # side wall
        sidep = [fr, br, btr, tr]
        def side_marks():
            self.flat([tr, btr, (btr[0], btr[1] + 12), (tr[0], tr[1] + 14)], dk(side))
            if planks:
                for i in range(1, 6):
                    self.line([(fr[0], y - h * i / 6), (br[0], br[1] - h * i / 6)], 1.2, dk(side), amp=.3)
            self.flat([fr, br, (br[0], br[1] - 9), (fr[0], fr[1] - 9)], PAL['ice'])
        self.flat(sidep, side); self.inside(sidep, side_marks); self.line(sidep + [fr], 2.6)
        self.line([(fr[0], fr[1] - 9), (br[0], br[1] - 9)], 1.2, amp=.2)
        # front wall
        front = [fl, fr, tr, apex, tl]
        def front_marks():
            self.flat([(tl[0] - 2, tl[1] - 2), apex, (tr[0] + 2, tr[1] - 2), (tr[0] + 2, tr[1] + 11), (apex[0], apex[1] + 16), (tl[0] - 2, tl[1] + 11)], dk(wall))
            if planks:
                for i in range(1, 7):
                    yy = y - (h + roof_h * .5) * i / 7
                    self.line([(fl[0], yy), (fr[0], yy)], 1.3, dk(wall), amp=.4)
            else:
                for i in range(7):
                    bx, by = self.r.uniform(fl[0] + 8, fr[0] - 16), self.r.uniform(y - h, y - 14)
                    self.line([(bx, by), (bx + 9, by)], 1.2, dk(wall), amp=.2)
            self.flat([fl, fr, (fr[0], fr[1] - 9), (fl[0], fl[1] - 9)], STONE)
            for i in range(1, int(w / 16)):
                self.line([(fl[0] + i * 16 + (i % 2) * 3, y - 9), (fl[0] + i * 16 + (i % 2) * 3, y)], 1, amp=0)
        self.flat(front, wall); self.inside(front, front_marks)
        self.line([(fl[0], y - 9), (fr[0], y - 9)], 1.2, amp=.2)
        self.line(front + [fl], 2.6)
        # roof slab with rows of shingles
        roofp = [(apex[0] - 2, apex[1] - 3), (bapex[0], bapex[1] - 3), (btr[0] + ov, btr[1] + ov * .2), (tr[0] + ov, tr[1] + ov * .5)]
        def tiles():
            a0, b0, b1, a1 = roofp
            self.flat([(a1[0], a1[1] - 9), (b1[0], b1[1] - 9), (b1[0] + 4, b1[1] + 4), (a1[0] + 4, a1[1] + 4)], dk(roof))
            if not shingles:
                return
            rows = 5
            for i in range(1, rows):
                t = i / rows
                a = (a0[0] + (a1[0] - a0[0]) * t, a0[1] + (a1[1] - a0[1]) * t); b = (b0[0] + (b1[0] - b0[0]) * t, b0[1] + (b1[1] - b0[1]) * t)
                self.line([a, b], 1.4, dk(roof), amp=.4)
            for i in range(rows):
                t0, t1 = i / rows, (i + 1) / rows
                n = max(3, int(depth / 11))
                for k in range(n):
                    u = (k + (.5 if i % 2 else 0) + .3) / n
                    if u >= 1:
                        continue
                    p0 = [(a0[0] + (a1[0] - a0[0]) * tt) * (1 - u) + (b0[0] + (b1[0] - b0[0]) * tt) * u for tt in (t0, t1)]
                    p1 = [(a0[1] + (a1[1] - a0[1]) * tt) * (1 - u) + (b0[1] + (b1[1] - b0[1]) * tt) * u for tt in (t0, t1)]
                    self.line([(p0[0], p1[0]), (p0[1], p1[1])], 1.1, dk(roof), amp=0)
        self.flat(roofp, roof); self.inside(roofp, tiles); self.line(roofp + [roofp[0]], 2.6)
        self.line([(tl[0] - ov, tl[1] + ov * .5), (apex[0] - 2, apex[1] - 3), (tr[0] + ov, tr[1] + ov * .5)], 3.2)
        if chimney:
            cx, cy = x + depth * .7, apex[1] + roof_h * .2 - depth * .2
            self.two([(cx - 8, cy + 14), (cx - 8, cy - 18), (cx + 8, cy - 18), (cx + 8, cy + 20)], RED, 2.2, 3, 0, amp=.3)
            for yy in (cy - 10, cy - 2, cy + 6):
                self.line([(cx - 7, yy), (cx + 7, yy)], 1, amp=0)
            self.shape([(cx - 10, cy - 18), (cx + 10, cy - 18), (cx + 10, cy - 23), (cx - 10, cy - 23)], STONE, 1.6, amp=.2, step=10)
            self.smoke(cx, cy - 30)
        if door:
            self.door(x, y, 24, h * .56, door_fill)
        for i in range(windows):
            wx = x + (-1 if i == 0 else 1) * w * .31
            self.pane(wx, y - h * .56, 18, 20, glass, shutters, box)
        # round attic window in the gable
        self.oval(x, tl[1] - roof_h * .32, 6.5, 6.5, glass, 1.8, amp=.2)
        self.line([(x - 6, tl[1] - roof_h * .32), (x + 6, tl[1] - roof_h * .32)], 1, amp=0)
        sx, sy = (fr[0] + br[0]) / 2, (fr[1] + br[1]) / 2 - h * .55
        self.shape([(sx - 8, sy - 8), (sx + 8, sy - 14), (sx + 8, sy + 6), (sx - 8, sy + 12)], WHITE, 1.8, amp=.3)
        self.flat([(sx - 6, sy - 6.5), (sx + 6, sy - 11), (sx + 6, sy + 4.5), (sx - 6, sy + 9)], NAVY)
        self.line([(sx, sy - 10), (sx, sy + 8)], 1, WHITE, amp=0)
        return dict(apex=apex, bapex=bapex, fl=fl, fr=fr, br=br, tl=tl, tr=tr, btr=btr)
