"""Sprite kit in the style of the reference illustration: dark maroon hand-drawn outlines of uneven weight,
warm earthy fills sampled from that picture, one darker tone along the lower right of each shape and a few
tone-on-tone marks. Every word picture in the game is drawn with this."""
import math, sys, io
from pathlib import Path
import cairo
import ink
from ink import Canvas, rgb, LWK

PAL = dict(
    grey1='#66747a', grey2='#97a2a6', taupe='#d2ac86', skin='#f2c79c', orange='#e68f4b',
    brick='#d5583a', brown='#8a4b2c', ink='#3a160d', plum='#6e3442', mauve='#b7697a',
    wood='#c07d45', gold='#e0a044', yellow='#f2c85f', cream='#f6e4b0', sand='#e6c58f',
    lime='#cdcb5d', green='#95ad4c', dgreen='#628745', forest='#386137', pine='#4a7a6c',
    sage='#a6c47e', mint='#bfd6a0', pale='#e0ecc6', white='#fbf5d8', silver='#d5d8cf',
    pink='#f2ad9c', rose='#d9806f', maroon='#9a3328', bone='#e9d8ae',
    ice='#d2e2dc', sky='#9dc3cf', blue='#5b87a6', navy='#4a4b70', slate='#39344f', night='#2a2233')
# the shadow colour of each colour is its darker neighbour in the palette
SHADE = dict(white='bone', bone='sand', maroon='plum', silver='ice', ice='sky', sky='blue', blue='navy', navy='slate', slate='night', night='night',
             cream='sand', sand='taupe', taupe='wood', grey2='grey1', grey1='ink', skin='orange', orange='brick', brick='maroon',
             brown='plum', plum='ink', mauve='plum', wood='brown', gold='wood', yellow='gold', lime='green', green='dgreen',
             dgreen='forest', forest='slate', pine='forest', sage='pine', mint='sage', pale='mint', ink='ink', pink='rose', rose='maroon')
# what a cast shadow turns a colour into in pixel art: one gentle step, where SHADE would jump too far
CAST = dict(sage='green', mint='sage', pale='mint', green='dgreen', cream='sand', sand='taupe', taupe='wood', white='bone', bone='sand',
            silver='grey2', ice='silver', pink='rose', skin='taupe', yellow='gold', gold='wood')
LW = 7.0
ink.PALNAMES.update(PAL)
ROOT = Path(__file__).resolve().parents[2]
ACT = ROOT / 'Assets/Lemko/Sprites/Placeholders/Activities'
PROD = ROOT / 'Assets/Lemko/Sprites/Placeholders/Products'
PREVIEW = None            # set to a folder to write there instead of into the project


def col(name):
    return PAL.get(name, name)


class Sp(Canvas):
    def __init__(self, seed=1, size=256):
        super().__init__(size, size, 3, None, seed)
        self.size = size
        self.rad = 9
        self.wob = 1.1         # how far the hand-drawn lines stray, in canvas pixels
        self.seg = 20

    # ---- hand-drawn line: hand() and pen() come from Canvas ---------------------------
    def line(self, pts, w=2.6, color='#3a160d', amp=1.3, step=16):
        p = self.hand(pts, False, min(self.wob, amp) * (.8 if len(pts) < 4 else .5)) if len(pts) > 1 else pts
        self.pen(p, w * LWK, color, False)

    # ---- outlined shapes -----------------------------------------------------------
    def poly(self, pts, fill='white', w=LW, shade=True, amp=1.4, step=14, inner=None, sx=4, sy=5, color='ink', shine=None, raw=False):
        """Shape in the reference style: flat fill, a darker tone along the lower right, two light dashes on big dark
        shapes, and a hand-drawn outline."""
        p = list(pts) if raw else self.hand(pts)
        cr = self.c
        xs = [q[0] for q in p]; ys = [q[1] for q in p]
        bw, bh = max(xs) - min(xs), max(ys) - min(ys)
        big = min(bw, bh)
        k = 1.5 if big > 90 else 1.0
        self._path(p, True)
        if fill:
            dark = SHADE.get(fill, fill) if shade is True else shade
            cr.set_source_rgb(*rgb(col(dark if dark else fill))); cr.fill_preserve()
            cr.save(); cr.clip()
            if dark:
                self._path([(x - sx * k, y - sy * k) for x, y in p], True); cr.set_source_rgb(*rgb(col(fill))); cr.fill()
            if not ink.PIX and (shine if shine is not None else (big > 56 and fill not in ('white', 'cream', 'bone', 'ink', 'pale', 'skin'))):
                L = max(6, min(13, big * .11))
                cr.set_source_rgb(*mix(col(fill), '#fff6da', .5)); cr.set_line_width(3.2 + big * .012)
                for fx, fy in ((.31, .19), (.19, .34)):
                    x, y = min(xs) + bw * fx, min(ys) + bh * fy
                    cr.move_to(x - L * .5, y + L * .36); cr.line_to(x + L * .5, y - L * .36); cr.stroke()
            if inner:
                inner()
            cr.restore()
        else:
            cr.new_path()
        if w:
            (self.edge if fill else self.pen)(p, w, color)
        return p

    def blobp(self, pts, fill='white', rounds=3, **k):
        """Soft blob through a rough control polygon (corners are cut until it is round)."""
        return self.poly(chaikin(pts, rounds), fill, **k)

    def merged(self, shapes):
        """Several shapes fused into one silhouette with a single outline around all of them.
        shapes: list of (points, fill, keyword arguments for poly)."""
        hs = [self.hand(p) for p, f, k in shapes]
        if ink.PIX:
            for h, (p, f, k) in zip(hs, shapes):
                self.poly(h, f, w=0, raw=True, **k)
            for i, h in enumerate(hs):
                self._path(h, True, add=i > 0)
            self.c.outline(col('ink'))
            return
        for h in hs:
            self.pen(h, LW * 2)
        for h, (p, f, k) in zip(hs, shapes):
            self.poly(h, f, w=0, raw=True, **k)

    def ell(self, cx, cy, rx, ry, fill='white', rot=0, n=44, **k):
        ca, sa = math.cos(rot), math.sin(rot)
        pts = []
        for i in range(n):
            a = 6.2832 * i / n
            x, y = math.cos(a) * rx, math.sin(a) * ry
            pts.append((cx + x * ca - y * sa, cy + x * sa + y * ca))
        k.setdefault('step', 9); k.setdefault('amp', 1.0)
        return self.poly(pts, fill, **k)

    def rr(self, x, y, ww, hh, r=14, fill='white', **k):
        """Rounded rectangle from its top-left corner."""
        r = min(r, ww / 2, hh / 2)
        pts = []
        for cx, cy, a0 in ((x + ww - r, y + r, -90), (x + ww - r, y + hh - r, 0), (x + r, y + hh - r, 90), (x + r, y + r, 180)):
            for i in range(5):
                a = math.radians(a0 + i * 22.5)
                pts.append((cx + math.cos(a) * r, cy + math.sin(a) * r))
        return self.poly(pts, fill, **k)

    def lump(self, cx, cy, rx, ry, fill='white', bumps=7, depth=.18, **k):
        """Cloud-like closed outline."""
        pts = []
        ph = self.r.uniform(0, 6.28)
        n = bumps * 8
        for i in range(n):
            a = 6.2832 * i / n
            kk = 1 + depth * abs(math.sin(a * bumps / 2 + ph)) - depth * .5
            pts.append((cx + math.cos(a) * rx * kk, cy + math.sin(a) * ry * kk))
        k.setdefault('step', 8); k.setdefault('amp', .8)
        return self.poly(pts, fill, **k)

    # ---- lines and marks -------------------------------------------------------------
    def ln(self, pts, w=LW, color='ink', amp=1.2, step=14):
        self.line(pts, w, col(color), amp, step)

    def arc(self, cx, cy, r, a0, a1, w=LW, color='ink', ry=None, n=14):
        ry = r if ry is None else ry
        pts = [(cx + math.cos(math.radians(a0 + (a1 - a0) * i / n)) * r, cy + math.sin(math.radians(a0 + (a1 - a0) * i / n)) * ry) for i in range(n + 1)]
        self.line(pts, w, col(color), .6, 8)

    def dot(self, x, y, r=5, color='ink'):
        cr = self.c; cr.set_source_rgb(*rgb(col(color))); cr.arc(x, y, r, 0, 6.3); cr.fill()

    def fill(self, pts, color):
        self.flat(pts, col(color))

    def fe(self, cx, cy, rx, ry, color):
        cr = self.c; cr.save(); cr.translate(cx, cy); cr.scale(rx, ry); cr.arc(0, 0, 1, 0, 6.3); cr.restore()
        cr.set_source_rgb(*rgb(col(color))); cr.fill()

    def hi(self, pts, w=4.5, color='white'):
        """Small highlight stroke."""
        self.line(pts, w, col(color), .4, 8)

    def eye(self, x, y, r=5.5):
        self.dot(x, y, r)

    def bigeye(self, x, y, rx=10, ry=13, shut=False):
        """The plain dark oval eye of the characters (the reference has no highlight dots)."""
        if shut:
            self.arc(x, y + 2, rx, 200, 340, 5, ry=ry * .7); return
        if ink.PIX:
            self.c.block(x, y, rx, ry, col('ink')); return
        self.fe(x, y, rx, ry, 'ink')

    def mouth(self, x, y, w=9, lw=4):
        """Tiny w-shaped smile."""
        self.arc(x - w / 2, y, w / 2, 20, 170, lw, ry=w * .5); self.arc(x + w / 2, y, w / 2, 10, 160, lw, ry=w * .5)

    def smile(self, x, y, w=14, w2=4.5):
        self.arc(x, y, w / 2, 20, 160, w2, ry=w * .32)

    def blush(self, x, y, r=7, color='rose'):
        """Cheek: a soft oval with three little hatch strokes, as in the reference."""
        self.fe(x, y, r, r * .68, color)
        if ink.PIX:
            return
        cr = self.c; cr.set_source_rgb(*rgb(col(SHADE.get(color, 'maroon')))); cr.set_line_width(max(1.6, r * .22))
        for i in (-1, 0, 1):
            cr.move_to(x + i * r * .5 - r * .14, y + r * .32); cr.line_to(x + i * r * .5 + r * .14, y - r * .32); cr.stroke()

    def text(self, s, x, y, size=60, color='ink', font='DejaVu Sans', bold=True, center=True):
        cr = self.c
        cr.select_font_face(font, cairo.FONT_SLANT_NORMAL, cairo.FONT_WEIGHT_BOLD if bold else cairo.FONT_WEIGHT_NORMAL)
        cr.set_font_size(size)
        ext = cr.text_extents(s)
        cr.set_source_rgb(*rgb(col(color)))
        cr.move_to(x - ext.x_bearing - ext.width / 2 if center else x, y); cr.show_text(s)

    def ground(self, y=214, x0=40, x1=216, fill='sage'):
        """Small patch of ground under scenery."""
        return self.poly([(x0, y), (x0 + 26, y - 14), ((x0 + x1) / 2, y - 20), (x1 - 22, y - 12), (x1, y), (x1 - 14, y + 16), (x0 + 14, y + 16)], fill, step=12)

    def px(self, box=46, frame=48, **k):
        """The drawing as pixel art: scaled so that it fills a box x box square in the middle of a frame x frame picture.
        Word pictures are 48 pixels: 1.2 world units at the game's 40 pixels per unit."""
        x0, y0, x1, y1 = self.c.bounds()
        s = min(box / (x1 - x0), box / (y1 - y0))
        w, h = (x1 - x0) * s, (y1 - y0) * s
        m = cairo.Matrix(s, 0, 0, s, round((frame - w) / 2) - x0 * s, round((frame - h) / 2) - y0 * s)
        return self.pixels(frame, frame, m, **k)

    def out(self, folder, name, fit=.94):
        """Crop to the drawing, scale it to fill the frame, and save the picture (512 px smooth, or 48 px pixel art)."""
        from PIL import Image
        if ink.PIX:
            target = Path(PREVIEW) if PREVIEW else folder
            res = self.px(**getattr(self, 'pixopts', {})).image()
            res.save(target / ('sprite_' + name + '.png'), optimize=True)
            return res
        buf = io.BytesIO(); self.surface.write_to_png(buf); buf.seek(0)
        im = Image.open(buf).convert('RGBA')
        box = im.getchannel('A').point(lambda v: 255 if v > 8 else 0).getbbox()
        im = im.crop(box)
        k = fit * 512 / max(im.size)
        im = im.resize((max(1, round(im.width * k)), max(1, round(im.height * k))), Image.Resampling.LANCZOS)
        res = Image.new('RGBA', (512, 512), (0, 0, 0, 0))
        res.paste(im, ((512 - im.width) // 2, (512 - im.height) // 2))
        target = Path(PREVIEW) if PREVIEW else folder
        res.save(target / ('sprite_' + name + '.png'), optimize=True)
        return res


def mix(a, b, t):
    ra, rb = rgb(a), rgb(b)
    return tuple(ra[i] + (rb[i] - ra[i]) * t for i in range(3))


def chaikin(pts, rounds=3):
    p = list(pts)
    for _ in range(rounds):
        q = []
        for i in range(len(p)):
            a, b = p[i], p[(i + 1) % len(p)]
            q.append((a[0] * .75 + b[0] * .25, a[1] * .75 + b[1] * .25)); q.append((a[0] * .25 + b[0] * .75, a[1] * .25 + b[1] * .75))
        p = q
    return p


def star_pts(cx, cy, r, r2=None, n=5, rot=-90):
    r2 = r2 or r * .45
    return [(cx + math.cos(math.radians(rot + i * 180 / n)) * (r if i % 2 == 0 else r2),
             cy + math.sin(math.radians(rot + i * 180 / n)) * (r if i % 2 == 0 else r2)) for i in range(2 * n)]


def sheet(names, folder, path, cols=8, cell=150, bg='#f9f0cc'):
    """Contact sheet for looking at a group of pictures."""
    from PIL import Image
    target = Path(PREVIEW) if PREVIEW else folder
    rows = (len(names) + cols - 1) // cols
    sh = Image.new('RGB', (cols * cell, rows * cell), bg)
    for k, n in enumerate(names):
        im = Image.open(target / ('sprite_' + n + '.png')).convert('RGBA')
        if ink.PIX:
            z = max(1, (cell - 10) // im.width); im = im.resize((im.width * z, im.height * z), Image.Resampling.NEAREST)
        else:
            im = im.resize((cell - 10, cell - 10), Image.Resampling.LANCZOS)
        sh.paste(im, ((k % cols) * cell + (cell - im.width) // 2, (k // cols) * cell + (cell - im.height) // 2), im)
    sh.save(path)
