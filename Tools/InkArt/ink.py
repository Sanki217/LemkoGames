"""Hand-drawn ink-on-paper drawing kit. Original art, no external assets.

Everything is drawn with wobbly ink outlines, white or warm grey fills and one or
two warm accents, so every picture in the game shares one limited palette.
"""
import math, random
import cairo

# The game's 31-colour palette; these are the ones the scenery uses.
INK = '#4b2a20'
WHITE = '#fffaf0'
PAPER = '#ffedc4'      # indoor walls
TONE1 = '#f3d28f'      # floors, paths, side walls
TONE2 = '#d8bfa0'      # rock, stone
TONE3 = '#a39386'      # rock in shade
GOLD = '#ffd44d'
RED = '#e4472f'
SKY = '#d3f3e2'
GRASS = '#86cc7a'
GRASS_DARK = '#76bd3c'
GRASS_LIGHT = '#a5e3c0'
SHADOW = '#3d8f6a'     # ground shadows, drawn half transparent
PINE = '#3d8f6a'
PINE_DARK = '#2a6e45'
LEAF = '#76bd3c'
LEAF_DARK = '#3f9438'
WOOD = '#c98343'
WOOD_DARK = '#8f4a2a'
WATER = '#8cc6ee'
WATER_DARK = '#4d93dc'
STONE = '#e8dfd2'
NAVY = '#3861b0'
ORANGE = '#f78b2d'
LWK = 1.75             # every outline is this much thicker than the first, thin version


def rgb(h):
    h = h.lstrip('#')
    return tuple(int(h[i:i + 2], 16) / 255 for i in (0, 2, 4))


class Canvas:
    def __init__(self, w, h, scale=2, bg=PAPER, seed=7, ysquash=1.0):
        """w,h are logical pixels. ysquash stretches the drawing vertically for sprites
        that the scene shows squashed (the village map is shown at 0.8 height)."""
        self.w, self.h, self.s = w, h, scale
        self.surface = cairo.ImageSurface(cairo.FORMAT_ARGB32, int(w * scale), int(h * scale * ysquash))
        self.c = cairo.Context(self.surface)
        self.c.scale(scale, scale * ysquash)
        self.c.set_line_cap(cairo.LINE_CAP_ROUND)
        self.c.set_line_join(cairo.LINE_JOIN_ROUND)
        self.r = random.Random(seed)
        if bg:
            self.c.set_source_rgb(*rgb(bg)); self.c.paint()

    # --- hand-drawn line helpers -------------------------------------------------
    rad = 2.5       # every corner is rounded by this much: the style has no sharp points

    def wobble(self, pts, amp=1.3, step=16, closed=False):
        """Outlines used to be shaky ink lines; the style is now smooth, so the points pass through unchanged."""
        return list(pts)

    def _path(self, pts, closed=False, rad=None):
        """Polyline with every corner rounded."""
        c = self.c
        r = self.rad if rad is None else rad
        pts = [p for i, p in enumerate(pts) if i == 0 or math.hypot(p[0] - pts[i - 1][0], p[1] - pts[i - 1][1]) > .01]
        if closed and len(pts) > 1 and math.hypot(pts[0][0] - pts[-1][0], pts[0][1] - pts[-1][1]) < .01:
            pts = pts[:-1]
        c.new_path()
        n = len(pts)
        if n < 3 or r <= 0:
            c.move_to(*pts[0])
            for p in pts[1:]:
                c.line_to(*p)
            if closed:
                c.close_path()
            return
        def corner(i, first):
            P = pts[i]; A = pts[(i - 1) % n]; B = pts[(i + 1) % n]
            da = math.hypot(A[0] - P[0], A[1] - P[1]); db = math.hypot(B[0] - P[0], B[1] - P[1])
            d = min(r, da / 2, db / 2)
            p1 = (P[0] + (A[0] - P[0]) / da * d, P[1] + (A[1] - P[1]) / da * d)
            p2 = (P[0] + (B[0] - P[0]) / db * d, P[1] + (B[1] - P[1]) / db * d)
            (c.move_to if first else c.line_to)(*p1)
            c.curve_to(p1[0] + (P[0] - p1[0]) * .66, p1[1] + (P[1] - p1[1]) * .66, p2[0] + (P[0] - p2[0]) * .66, p2[1] + (P[1] - p2[1]) * .66, *p2)
        if closed:
            for i in range(n):
                corner(i, i == 0)
            c.close_path()
        else:
            c.move_to(*pts[0])
            for i in range(1, n - 1):
                corner(i, False)
            c.line_to(*pts[-1])

    def line(self, pts, w=2.6, color=INK, amp=1.3, step=16):
        self._path(self.wobble(pts, amp, step))
        self.c.set_source_rgb(*rgb(color)); self.c.set_line_width(w * LWK); self.c.stroke()

    def shape(self, pts, fill=WHITE, w=2.6, color=INK, amp=1.3, step=16):
        p = self.wobble(pts, amp, step, closed=True)
        self._path(p, True)
        if fill:
            self.c.set_source_rgb(*rgb(fill)); self.c.fill_preserve()
        if w:
            self.c.set_source_rgb(*rgb(color)); self.c.set_line_width(w * LWK); self.c.stroke()
        else:
            self.c.new_path()
        return p

    def flat(self, pts, fill, alpha=1.0):
        """Unoutlined fill (shadows, ground patches)."""
        self._path(pts, True); self.c.set_source_rgba(*rgb(fill), alpha); self.c.fill()

    def shadow(self, pts):
        self.flat(pts, SHADOW, .45)

    def sh_oval(self, cx, cy, rx, ry):
        self.flat([(cx + math.cos(6.2832 * i / 30) * rx, cy + math.sin(6.2832 * i / 30) * ry) for i in range(30)], SHADOW, .45)

    def blob(self, cx, cy, rx, ry, fill=WHITE, w=2.6, bumps=9, lump=.16, amp=.8, color=INK):
        """A lumpy closed outline: tree crowns, bushes, clouds, stones."""
        pts = []
        ph = self.r.uniform(0, 6.28)
        n = bumps * 6
        for i in range(n):
            a = 6.2832 * i / n
            k = 1 + lump * abs(math.sin(a * bumps / 2 + ph)) - lump * .5
            pts.append((cx + math.cos(a) * rx * k, cy + math.sin(a) * ry * k))
        return self.shape(pts, fill, w, color, amp, step=9)

    def oval(self, cx, cy, rx, ry, fill=WHITE, w=2.6, color=INK, amp=.7):
        pts = [(cx + math.cos(6.2832 * i / 40) * rx, cy + math.sin(6.2832 * i / 40) * ry) for i in range(40)]
        return self.shape(pts, fill, w, color, amp, step=8)

    def hatch(self, x0, y0, x1, y1, gap=7, w=1.6, color=INK, slant=.6):
        """Short diagonal shading strokes inside a box."""
        x = x0
        while x < x1:
            h = (y1 - y0) * self.r.uniform(.6, 1)
            self.line([(x, y1), (x + h * slant, y1 - h)], w, color, amp=.5, step=10)
            x += gap * self.r.uniform(.8, 1.3)

    # --- landscape pieces ---------------------------------------------------------
    def grass(self, x, y, k=1.0):
        for dx in (-5, 0, 5):
            self.line([(x + dx * k, y), (x + dx * k * 1.5, y - 6 * k)], 1.6, PINE, amp=.3, step=5)

    def tick(self, x, y, k=1.0, color=INK):
        self.line([(x - 6 * k, y - 3 * k), (x - 2 * k, y), (x + 1 * k, y - 4 * k), (x + 5 * k, y)], 1.8, color, amp=.2, step=5)

    def pine(self, x, y, h=58, shade=PINE_DARK):
        """x,y is the foot of the trunk."""
        w = h * .56
        self.shadow([(x - w * .5, y + 2), (x + w * .9, y + 6), (x + w * .5, y - 2)])
        self.shape([(x - 3, y), (x - 3, y - h * .2), (x + 3, y - h * .2), (x + 3, y)], WOOD_DARK, 2, amp=.2)
        tiers = 3
        for i in range(tiers):
            base = y - h * (.12 + .24 * i)
            top = base - h * (.42 if i < tiers - 1 else .5)
            ww = w * (1 - .22 * i)
            p = [(x - ww / 2, base), (x, top), (x + ww / 2, base)]
            self.shape(p, PINE, 0)
            self.flat([(x + ww * .06, base), (x + ww * .02, top + (base - top) * .25), (x + ww / 2 - 2, base)], shade)
            self.line(p + [p[0]], 2.4, amp=.7, step=10)
        return self

    def tree(self, x, y, r=24, fill=LEAF):
        self.oval(x + r * .35, y + 3, r * .9, r * .28, SHADOW, 0) if False else self.shadow([(x - r * .5, y + 2), (x + r * .3, y + r * .3), (x + r * 1.2, y + 3), (x + r * .3, y - r * .22)])
        self.shape([(x - 4, y), (x - 3, y - r * 1.1), (x + 3, y - r * 1.1), (x + 4, y)], WOOD, 2.2, amp=.3, step=8)
        self.blob(x, y - r * 1.75, r, r * .92, fill, 2.4, bumps=7, lump=.2)
        self.line([(x - r * .35, y - r * 1.6), (x - r * .1, y - r * 1.45)], 1.6, LEAF_DARK, amp=.2, step=6)
        self.line([(x + r * .15, y - r * 2.0), (x + r * .4, y - r * 1.9)], 1.6, LEAF_DARK, amp=.2, step=6)

    def bush(self, x, y, r=16):
        self.blob(x, y - r * .6, r, r * .7, LEAF, 2.2, bumps=5, lump=.22)

    def mountain(self, x, y, w=150, h=170, snow=True):
        """x,y is the middle of the base."""
        peak = (x + w * .04, y - h)
        left, right = (x - w / 2, y), (x + w / 2, y)
        self.shape([left, peak, right], TONE2, 0)
        self.flat([peak, right, (x + w * .12, y)], TONE3)
        if snow:
            sy = y - h * .45
            zig = [(x - w * .28, sy + 8), (x - w * .14, sy - 12), (x - w * .04, sy + 10), (x + w * .06, sy - 8),
                   (x + w * .15, sy + 12), (x + w * .26, sy - 2)]
            self.flat([(x - w * .28, sy + 8), peak, (x + w * .26, sy - 2)] + zig[::-1][1:-1], WHITE)
            self.flat([peak, (x + w * .26, sy - 2), (x + w * .15, sy + 12), (x + w * .09, sy - 4)], STONE)
            self.line(zig, 1.6, amp=.6, step=10)
        self.line([left, peak, right], 2.8, amp=1.6, step=20)

    def cloud(self, x, y, w=90, fill=WHITE):
        self.blob(x, y, w / 2, w * .2, fill, 2.4, bumps=5, lump=.5)

    def stones(self, x, y, n=3):
        for i in range(n):
            self.blob(x + i * 13 + self.r.uniform(-3, 3), y + self.r.uniform(-3, 3), 7, 5, STONE, 1.6, bumps=4, lump=.15)

    def fence(self, x0, y0, x1, y1, posts=6):
        for i in range(posts):
            t = i / (posts - 1)
            x, y = x0 + (x1 - x0) * t, y0 + (y1 - y0) * t
            self.shape([(x - 3, y), (x - 3, y - 20), (x + 3, y - 20), (x + 3, y)], WOOD, 1.8, amp=.3)
        for d in (14, 6):
            self.line([(x0, y0 - d), (x1, y1 - d)], 3.2, amp=.8); self.line([(x0, y0 - d), (x1, y1 - d)], 1.2, WOOD, amp=.8)

    # --- buildings ------------------------------------------------------------------
    def house(self, x, y, w=120, h=78, depth=56, roof=RED, wall=PAPER, roof_h=58, door=True, windows=2,
              chimney=False, planks=False, shingles=True, side=None):
        """Oblique little house. x,y is the middle of the front wall's ground line.
        Returns handy anchor points."""
        sk = .36                       # how much the side wall rises as it goes back
        fl, fr = (x - w / 2, y), (x + w / 2, y)
        br = (fr[0] + depth, fr[1] - depth * sk)
        tl, tr = (fl[0], fl[1] - h), (fr[0], fr[1] - h)
        btr = (br[0], br[1] - h)
        apex = (x, tl[1] - roof_h)
        bapex = (apex[0] + depth, apex[1] - depth * sk)
        ov = 9
        # ground shadow
        self.shadow([(fl[0] - 6, y + 4), (fr[0], y + 9), (br[0] + 26, br[1] + 12), (br[0] + 10, br[1] - 4)])
        # side wall, front wall
        self.shape([fr, br, btr, tr], side or TONE1, 2.6)
        self.shape([fl, fr, tr, apex, tl], wall, 2.6)
        if planks:
            for i in range(1, 5):
                yy = y - h * i / 5
                self.line([(fl[0] + 3, yy), (fr[0] - 3, yy)], 1.4, amp=.5)
        # roof slab (right slope seen from the side)
        self.shape([(apex[0] - 2, apex[1] - 3), (bapex[0], bapex[1] - 3), (btr[0] + ov, btr[1] + ov * .2),
                    (tr[0] + ov, tr[1] + ov * .5)], roof, 2.6)
        if shingles:
            for t in (.3, .55, .8):
                a = (apex[0] + (tr[0] + ov - apex[0]) * t, apex[1] + (tr[1] - apex[1]) * t)
                b = (bapex[0] + (btr[0] + ov - bapex[0]) * t, bapex[1] + (btr[1] - bapex[1]) * t)
                self.line([a, b], 1.5, amp=.6)
        # front gable edge with a little overhang
        self.line([(tl[0] - ov, tl[1] + ov * .5), (apex[0] - 2, apex[1] - 3), (tr[0] + ov, tr[1] + ov * .5)], 3.0)
        if chimney:
            cx, cy = x + depth * .7, apex[1] + roof_h * .2 - depth * .2
            self.shape([(cx - 8, cy + 14), (cx - 8, cy - 18), (cx + 8, cy - 18), (cx + 8, cy + 20)], TONE2, 2.4, amp=.4)
            self.smoke(cx, cy - 24)
        if door:
            self.shape([(x - 12, y), (x - 12, y - h * .52), (x + 12, y - h * .52), (x + 12, y)], WOOD_DARK, 2.2, amp=.4)
            self.c.set_source_rgb(*rgb(GOLD)); self.c.arc(x + 6, y - h * .26, 1.8, 0, 6.3); self.c.fill()
        for i in range(windows):
            wx = x + (-1 if i == 0 else 1) * w * .31
            wy = y - h * .56
            self.shape([(wx - 10, wy - 11), (wx + 10, wy - 11), (wx + 10, wy + 11), (wx - 10, wy + 11)], GOLD, 2.2, amp=.4)
            self.line([(wx, wy - 11), (wx, wy + 11)], 1.5, amp=.2); self.line([(wx - 10, wy), (wx + 10, wy)], 1.5, amp=.2)
        # side window
        sx, sy = (fr[0] + br[0]) / 2, (fr[1] + br[1]) / 2 - h * .55
        self.shape([(sx - 8, sy - 8), (sx + 8, sy - 14), (sx + 8, sy + 6), (sx - 8, sy + 12)], NAVY, 2, amp=.3)
        return dict(apex=apex, bapex=bapex, fl=fl, fr=fr, br=br, tl=tl, tr=tr)

    def smoke(self, x, y):
        for i, r in enumerate((6, 8, 10)):
            self.blob(x + i * 7, y - i * 15, r, r * .85, WHITE, 2, bumps=4, lump=.25)

    def flag(self, x, y, h=44, color=GOLD, right=True):
        self.line([(x, y), (x, y - h)], 2.6, amp=.3)
        d = 1 if right else -1
        self.shape([(x, y - h), (x + d * 26, y - h + 7), (x, y - h + 16)], color, 2.2, amp=.5)

    def save(self, path, size=None):
        import io
        from PIL import Image
        buf = io.BytesIO(); self.surface.write_to_png(buf); buf.seek(0)
        im = Image.open(buf).convert('RGBA')
        if size:
            im = im.resize(size, Image.Resampling.LANCZOS)
        im.convert('RGBA' if im.getextrema()[3][0] < 255 else 'RGB').save(path)
        return im
