"""Sprite kit: thick wobbly ink outlines, flat fills from the game's 31-colour palette,
one soft shadow per shape. Every word picture in the game is drawn with this."""
import math, sys, io
from pathlib import Path
import cairo
from ink import Canvas, rgb

PAL = dict(
    grey1='#636663', grey2='#87857c', taupe='#bcad9f', skin='#f2b888', orange='#eb9661',
    brick='#b55945', brown='#734c44', ink='#3d3333', plum='#593e47', mauve='#7a5859',
    wood='#a57855', gold='#de9f47', yellow='#fdd179', cream='#fee1b8', sand='#d4c692',
    lime='#a6b04f', green='#819447', dgreen='#44702d', forest='#2f4d2f', pine='#546756',
    sage='#89a477', mint='#a4c5af', pale='#cae6d9', white='#f1f6f0', silver='#d5d6db',
    ice='#bbc3d0', sky='#96a9c1', blue='#6c81a1', navy='#405273', slate='#303843', night='#14233a')
# the shadow colour of each colour is its darker neighbour in the palette
SHADE = dict(white='silver', silver='ice', ice='sky', sky='blue', blue='navy', navy='slate', slate='night', night='night',
             cream='sand', sand='taupe', taupe='grey2', grey2='grey1', grey1='ink', skin='orange', orange='brick', brick='brown',
             brown='plum', plum='ink', mauve='plum', wood='brown', gold='wood', yellow='gold', lime='green', green='dgreen',
             dgreen='forest', forest='slate', pine='forest', sage='pine', mint='sage', pale='mint', ink='ink')
LW = 7.5
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

    # ---- outlined shapes -----------------------------------------------------------
    def poly(self, pts, fill='white', w=LW, shade=True, amp=1.4, step=14, inner=None, sx=5, sy=6, color='ink'):
        p = self.wobble(pts, amp, step, closed=True)
        cr = self.c
        self._path(p, True)
        if fill:
            dark = SHADE.get(fill, fill) if shade is True else shade
            cr.set_source_rgb(*rgb(col(dark if dark else fill))); cr.fill_preserve()
            cr.save(); cr.clip()
            if dark:
                self._path([(x - sx, y - sy) for x, y in p], True); cr.set_source_rgb(*rgb(col(fill))); cr.fill()
            if inner:
                inner()
            cr.restore()
        else:
            cr.new_path()
        if w:
            self._path(p, True); cr.set_source_rgb(*rgb(col(color))); cr.set_line_width(w); cr.stroke()
        return p

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
        self.dot(x, y, r); self.dot(x - r * .3, y - r * .35, r * .32, 'white')

    def smile(self, x, y, w=14, w2=4.5):
        self.arc(x, y, w / 2, 20, 160, w2, ry=w * .32)

    def blush(self, x, y, r=7, color='orange'):
        self.fe(x, y, r, r * .7, color)

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

    def out(self, folder, name, fit=.94):
        """Crop to the drawing, scale it to fill the frame, and save a 512 px picture."""
        from PIL import Image
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


def star_pts(cx, cy, r, r2=None, n=5, rot=-90):
    r2 = r2 or r * .45
    return [(cx + math.cos(math.radians(rot + i * 180 / n)) * (r if i % 2 == 0 else r2),
             cy + math.sin(math.radians(rot + i * 180 / n)) * (r if i % 2 == 0 else r2)) for i in range(2 * n)]


def sheet(names, folder, path, cols=8, cell=150, bg='#fee1b8'):
    """Contact sheet for looking at a group of pictures."""
    from PIL import Image
    target = Path(PREVIEW) if PREVIEW else folder
    rows = (len(names) + cols - 1) // cols
    sh = Image.new('RGB', (cols * cell, rows * cell), bg)
    for k, n in enumerate(names):
        im = Image.open(target / ('sprite_' + n + '.png')).convert('RGBA').resize((cell - 10, cell - 10), Image.Resampling.LANCZOS)
        sh.paste(im, ((k % cols) * cell + 5, (k // cols) * cell + 5), im)
    sh.save(path)
