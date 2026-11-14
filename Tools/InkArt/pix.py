"""Pixel art back end for the drawing kit.

Every picture in the game sits on one grid: 640 x 360 pixels for the 16 x 9 world, 40 pixels per world unit.
A picture is drawn twice. The first pass draws the vector shapes as before, only to learn how big the drawing is,
and writes every drawing call down. The second pass replays the calls on the pixel grid:

  * filled shapes are sampled 5 x 5 times per pixel without smoothing and every pixel takes the colour that covers
    most of it, so shapes come out as clean flat clusters;
  * the outline of a shape is not stroked: it is the row of that shape's own pixels that touch whatever lies below
    it, so every outline is exactly one pixel wide and hugs its fill;
  * loose lines (mouths, whiskers, planks) are walked pixel by pixel with no doubled corners;
  * every pixel is snapped to the palette. A shadow becomes the darker palette neighbour of the colour under it.
"""
import math
import cairo
import numpy as np
from PIL import Image

PPU = 40                 # pixels per world unit, the same for every picture in the game
S = 5                    # samples per pixel edge
QUERIES = {'text_extents', 'get_matrix', 'copy_path', 'copy_path_flat', 'in_clip', 'in_fill', 'user_to_device',
           'device_to_user', 'get_current_point', 'font_extents', 'get_line_width', 'path_extents', 'fill_extents',
           'stroke_extents', 'clip_extents', 'has_current_point'}
SKIP = {'set_antialias', 'set_operator', 'set_tolerance'}


# Lettering too small to draw with shapes is set in two tiny hand-made fonts (rows of pixels, 'X' is ink).
FONT7 = {
    'А': '.XXX. X...X X...X XXXXX X...X X...X X...X', 'а': '..... ..... .XXX. ....X .XXXX X...X .XXXX',
    'Б': 'XXXXX X.... X.... XXXX. X...X X...X XXXX.', 'б': '..XXX .X... XXXX. X...X X...X X...X .XXX.',
    'В': 'XXXX. X...X X...X XXXX. X...X X...X XXXX.', 'в': '..... ..... XXXX. X...X XXXX. X...X XXXX.',
    '0': '.XXX. X...X X...X X...X X...X X...X .XXX.', '1': '..X.. .XX.. ..X.. ..X.. ..X.. ..X.. .XXX.',
    '2': '.XXX. X...X ....X ...X. ..X.. .X... XXXXX', '3': 'XXXX. ....X ....X .XXX. ....X ....X XXXX.',
    '4': '...X. ..XX. .X.X. X..X. XXXXX ...X. ...X.', '5': 'XXXXX X.... XXXX. ....X ....X X...X .XXX.',
    '6': '.XXX. X.... X.... XXXX. X...X X...X .XXX.', '7': 'XXXXX ....X ...X. ..X.. ..X.. ..X.. ..X..',
    '8': '.XXX. X...X X...X .XXX. X...X X...X .XXX.', '9': '.XXX. X...X X...X .XXXX ....X ....X .XXX.',
    'M': 'X...X XX.XX X.X.X X.X.X X...X X...X X...X', 'S': '.XXXX X.... X.... .XXX. ....X ....X XXXX.',
    'A': '.XXX. X...X X...X XXXXX X...X X...X X...X', '%': 'XX..X XX..X ...X. ..X.. .X... X..XX X..XX',
    '?': '.XXX. X...X ....X ...X. ..X.. ..... ..X..', '+': '..... ..X.. ..X.. XXXXX ..X.. ..X.. .....',
    '−': '..... ..... ..... XXXXX ..... ..... .....', ' ': '... ... ... ... ... ... ...',
}
FONT5 = {
    '0': 'XXX X.X X.X X.X XXX', '1': '.X. XX. .X. .X. XXX', '2': 'XXX ..X XXX X.. XXX', '3': 'XXX ..X XXX ..X XXX',
    '4': 'X.X X.X XXX ..X ..X', '5': 'XXX X.. XXX ..X XXX', '6': 'XXX X.. XXX X.X XXX', '7': 'XXX ..X ..X ..X ..X',
    '8': 'XXX X.X XXX X.X XXX', '9': 'XXX X.X XXX ..X XXX', ' ': '.. .. .. .. ..',
}


def lettering(text, em):
    """Pixels of a short text in the tiny font that suits a font size of em pixels: (points, width, height), or None
    when the size is big enough to draw the real letters (or too small to draw anything: then points is empty)."""
    cap = em * .73
    if cap >= 9.5:
        return None
    if cap < 3.6:
        return [], 0, 0
    font = FONT5 if cap < 5 and all(ch in FONT5 for ch in text) else FONT7
    if any(ch not in font for ch in text):
        return None
    pts = []; x = 0; rows = 0
    for ch in text:
        g = font[ch].split(); rows = len(g)
        for j, row in enumerate(g):
            pts += [(x + i, j) for i, v in enumerate(row) if v == 'X']
        x += len(g[0]) + 1
    return pts, x - 1, rows


def pack(h):
    h = h.lstrip('#')
    return int(h, 16)


class Recorder:
    """Stands in for a cairo context: draws the vector picture (its bounds are needed) and writes each call down."""

    def __init__(self, w, h):
        self.surface = cairo.ImageSurface(cairo.FORMAT_ARGB32, int(math.ceil(w)), int(math.ceil(h)))
        self._c = cairo.Context(self.surface)
        self.ops = []

    def __getattr__(self, name):
        fn = getattr(self._c, name)
        if name in QUERIES:
            return fn

        def call(*a):
            self.ops.append((name, a))
            return fn(*a)
        return call

    def block(self, x, y, rx, ry, color):
        """A small solid mark (an eye) that must survive at any size: it is drawn as a block of whole pixels."""
        self.ops.append(('@block', (x, y, rx, ry, color)))
        c = self._c; c.save(); c.translate(x, y); c.scale(rx, ry); c.arc(0, 0, 1, 0, 6.3); c.restore()
        c.set_source_rgb(0, 0, 0); c.fill()

    def shade(self):
        """The current path is a cast shadow: whatever lies under it turns one step darker."""
        self.ops.append(('@shadow', ()))
        self._c.new_path()

    def outline(self, color):
        """The current path is the edge of a filled shape: give it a one pixel outline."""
        self.ops.append(('@outline', (color,)))
        self._c.new_path()

    def bounds(self):
        """Box around everything drawn so far, in logical units, from the vector pass."""
        a = np.frombuffer(self.surface.get_data(), np.uint8).reshape(self.surface.get_height(), self.surface.get_width(), 4)[..., 3]
        ys, xs = np.nonzero(a > 8)
        if len(xs) == 0:
            return 0, 0, self.surface.get_width(), self.surface.get_height()
        return xs.min(), ys.min(), xs.max() + 1, ys.max() + 1


def _chain(points):
    """Pixels along a polyline, eight-connected and without doubled corners."""
    px = []
    for (x0, y0), (x1, y1) in zip(points, points[1:]):
        n = int(max(abs(x1 - x0), abs(y1 - y0)) * 2) + 1
        for i in range(n + 1):
            t = i / n
            q = (int(math.floor(x0 + (x1 - x0) * t)), int(math.floor(y0 + (y1 - y0) * t)))
            if not px or px[-1] != q:
                px.append(q)
    if len(points) == 1:
        px.append((int(math.floor(points[0][0])), int(math.floor(points[0][1]))))
    out = []
    i = 0
    while i < len(px):
        if out and i < len(px) - 1:
            a, b, c = out[-1], px[i], px[i + 1]
            if (a[0] == b[0] or a[1] == b[1]) and (c[0] == b[0] or c[1] == b[1]) and a[0] != c[0] and a[1] != c[1]:
                i += 1
                continue
        out.append(px[i])
        i += 1
    return out


def _mode(values, mask):
    """Most common value along the last axis among the masked samples (0 where nothing is masked)."""
    n = values.shape[-1]
    best = np.zeros(values.shape[:-1], np.int32)
    win = np.zeros(values.shape[:-1], values.dtype)
    for k in range(n):
        vk = values[..., k]
        cnt = ((values == vk[..., None]) & mask).sum(-1)
        cnt = np.where(mask[..., k], cnt, 0)
        better = cnt > best
        best = np.where(better, cnt, best)
        win = np.where(better, vk, win)
    return win, best


class Pixels:
    """Result of a replay: colours, region ids and helpers to finish and save the picture."""

    def __init__(self, rgb, solid):
        self.rgb = rgb          # H x W packed 0xRRGGBB
        self.solid = solid      # H x W bool

    def image(self, scale=1):
        h, w = self.rgb.shape
        out = np.zeros((h, w, 4), np.uint8)
        out[..., 0] = (self.rgb >> 16) & 255
        out[..., 1] = (self.rgb >> 8) & 255
        out[..., 2] = self.rgb & 255
        out[..., 3] = np.where(self.solid, 255, 0)
        im = Image.fromarray(out, 'RGBA')
        if scale != 1:
            im = im.resize((w * scale, h * scale), Image.Resampling.NEAREST)
        return im


def render(ops, W, H, matrix, palette, shade, ink='#3a160d', thin_ink=3.6, thin=2.2, edge='ink', open_edges=True, cast=None,
           keep_small=True):
    """Replay recorded drawing calls on a W x H pixel grid.

    matrix   cairo.Matrix from logical units to pixels
    palette  dict name -> '#rrggbb'; shade: dict name -> name of the darker neighbour
    cast     dict name -> name of the colour a cast shadow turns it into (defaults to shade)
    thin_ink, thin   strokes narrower than this many pixels (ink / other colours) become one pixel lines
    edge     'ink' for dark outlines everywhere, 'tone' for outlines in a darker tone of the shape inside the picture
    open_edges   True when the picture has a transparent surround (sprites); False for full backdrops
    """
    surf = [cairo.ImageSurface(cairo.FORMAT_ARGB32, W * S, H * S) for _ in range(3)]
    cc, ic, zc = ctx = [cairo.Context(s) for s in surf]
    fo = cairo.FontOptions(); fo.set_antialias(cairo.ANTIALIAS_NONE)
    for c in ctx:
        c.set_antialias(cairo.ANTIALIAS_NONE); c.set_font_options(fo); c.scale(S, S); c.transform(matrix)
    n = 0                      # id of the latest shape; ids grow in painting order
    outlined = {}              # id -> packed outline colour
    lines = []                 # (seq, packed colour, [(x, y)...])
    source = [0, 0, 0, 1.0]
    inkp = pack(ink)
    font_size = 10.0

    BAYER = (0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5)
    screens = {}

    def screen(level):
        """A 4 x 4 ordered dither pattern, one cell per picture pixel, for see-through fills."""
        if level not in screens:
            m = cairo.ImageSurface(cairo.FORMAT_A8, 4, 4)
            data = m.get_data(); stride = m.get_stride()
            for j in range(4):
                for i in range(4):
                    data[j * stride + i] = 255 if BAYER[j * 4 + i] < level else 0
            m.mark_dirty()
            pat = cairo.SurfacePattern(m); pat.set_extend(cairo.EXTEND_REPEAT); pat.set_filter(cairo.FILTER_NEAREST)
            screens[level] = (pat, m)
        return screens[level][0]

    MARK = (1.0, 0.0, 1.0, .5)      # cast shadows are painted in a colour no picture uses and resolved per pixel

    def idc(c, k):
        c.set_source_rgb(((k >> 16) & 255) / 255, ((k >> 8) & 255) / 255, (k & 255) / 255)

    def flat_paths():
        """Current path as polylines in pixel coordinates."""
        out = []; cur = []
        for kind, pts in cc.copy_path_flat():
            if kind == cairo.PATH_MOVE_TO:
                if cur:
                    out.append(cur)
                cur = [pts]
            elif kind == cairo.PATH_LINE_TO:
                cur.append(pts)
            elif kind == cairo.PATH_CLOSE_PATH and cur:
                cur.append(cur[0])
        if cur:
            out.append(cur)
        res = []
        for poly in out:
            dev = [cc.user_to_device(*p) for p in poly]
            res.append([(x / S, y / S) for x, y in dev])
        return res

    for name, a in ops:
        if name in SKIP:
            continue
        if name == 'set_source_rgb':
            source = [a[0], a[1], a[2], 1.0]; cc.set_source_rgb(*a); continue
        if name == 'set_source_rgba':
            source = list(a); cc.set_source_rgba(*a); continue
        if name == 'set_font_size':
            font_size = a[0]
        if name == 'show_text':
            m = cc.get_matrix()
            tiny = lettering(a[0], font_size * math.sqrt(abs(m.xx * m.yy - m.xy * m.yx)) / S)
            if tiny is not None:
                # small lettering: whole pixels from the tiny font, centred where the real text would have been
                pts, tw, th = tiny
                x, y = cc.get_current_point(); ext = cc.text_extents(a[0])
                cx, by = cc.user_to_device(x + ext.x_bearing + ext.width / 2, y)
                x0 = int(round(cx / S - tw / 2)); y0 = int(round(by / S)) - th
                colour = (int(round(source[0] * 255)) << 16) | (int(round(source[1] * 255)) << 8) | int(round(source[2] * 255))
                pts = [(x0 + i, y0 + j) for i, j in pts if 0 <= x0 + i < W and 0 <= y0 + j < H]
                if pts:
                    lines.append((n, colour, pts))
                for c in ctx:
                    c.move_to(x + ext.x_advance, y)
                continue
        if name == '@block':
            x, y, rx, ry, colour = a
            m = cc.get_matrix(); k = math.sqrt(abs(m.xx * m.yy - m.xy * m.yx)) / S
            px_, py_ = cc.user_to_device(x, y)
            w = max(1, int(1.1 * rx * k + .5)); h = max(2, int(1.25 * ry * k + .5))
            x0 = int(round(px_ / S - w / 2)); y0 = int(round(py_ / S - h / 2))
            pts = [(i, j) for j in range(y0, y0 + h) for i in range(x0, x0 + w) if 0 <= i < W and 0 <= j < H]
            if pts:
                lines.append((n, pack(colour), pts))
            continue
        if name in ('fill', 'fill_preserve', 'paint', 'show_text'):
            opaque = source[3] >= .999
            colour = (int(round(source[0] * 255)) << 16) | (int(round(source[1] * 255)) << 8) | int(round(source[2] * 255))
            if not opaque and name in ('fill', 'fill_preserve'):
                # a see-through fill becomes a dither of solid pixels
                level = min((2, 4, 8, 12, 16), key=lambda v: abs(v / 16 - source[3]))
                pat = screen(level)
                pm = cairo.Matrix(*cc.get_matrix()).multiply(cairo.Matrix(1 / S, 0, 0, 1 / S, 0, 0)); pat.set_matrix(pm)
                cc.save(); cc.clip_preserve(); cc.set_source_rgb(*source[:3]); cc.mask(pat); cc.restore()
                cc.set_source_rgba(*source)
                if name == 'fill':
                    for c in ctx:
                        c.new_path()
                continue
            getattr(cc, name)(*a)
            if opaque:
                n += 1
                for c in (ic, zc):
                    idc(c, n); getattr(c, name)(*a)
            else:
                for c in (ic, zc):
                    if name == 'fill':
                        c.new_path()
                    elif name == 'show_text':
                        c.move_to(*cc.get_current_point())
            continue
        if name == '@shadow':
            cc.set_source_rgba(*MARK); cc.fill(); cc.set_source_rgba(*source); ic.new_path(); zc.new_path()
            continue
        if name == '@outline':
            n += 1
            idc(ic, n); ic.fill(); cc.new_path(); zc.new_path()
            outlined[n] = pack(a[0])
            continue
        if name in ('stroke', 'stroke_preserve'):
            m = cc.get_matrix()
            wpx = cc.get_line_width() * math.sqrt(abs(m.xx * m.yy - m.xy * m.yx)) / S
            colour = (int(round(source[0] * 255)) << 16) | (int(round(source[1] * 255)) << 8) | int(round(source[2] * 255))
            limit = thin_ink if colour == inkp else thin
            if wpx < limit and source[3] >= .5:
                for poly in flat_paths():
                    pts = [q for q in _chain(poly) if 0 <= q[0] < W and 0 <= q[1] < H and cc.in_clip(*cc.device_to_user((q[0] + .5) * S, (q[1] + .5) * S))]
                    if pts:
                        lines.append((n, colour, pts))
                if name == 'stroke':
                    for c in ctx:
                        c.new_path()
            else:
                getattr(cc, name)()
                if source[3] >= .999:
                    n += 1
                    for c in (ic, zc):
                        idc(c, n); getattr(c, name)()
                elif name == 'stroke':
                    ic.new_path(); zc.new_path()
            continue
        for c in ctx:
            getattr(c, name)(*a)

    def grid(s):
        b = np.frombuffer(s.get_data(), np.uint8).reshape(H * S, W * S, 4).astype(np.int32)
        v = b[..., 0] | (b[..., 1] << 8) | (b[..., 2] << 16)
        return v.reshape(H, S, W, S).transpose(0, 2, 1, 3).reshape(H, W, S * S), b[..., 3].reshape(H, S, W, S).transpose(0, 2, 1, 3).reshape(H, W, S * S)

    col, alpha = grid(surf[0])
    ids, _ = grid(surf[1])
    seq, _ = grid(surf[2])
    covered = seq > 0
    rgb = np.zeros((H, W), np.int32); wid = np.zeros((H, W), np.int32); top = np.zeros((H, W), np.int32); solid = np.zeros((H, W), bool)
    for y0 in range(0, H, 24):
        sl = slice(y0, y0 + 24)
        cv = covered[sl]
        sol = cv.sum(-1) * 2 >= S * S
        w_, _ = _mode(ids[sl], cv)
        mine = cv & (ids[sl] == w_[..., None])
        c_, _ = _mode(col[sl], mine)
        t_ = np.where(mine & (col[sl] == c_[..., None]), seq[sl], 0).max(-1)
        rgb[sl] = c_; wid[sl] = np.where(sol, w_, 0); top[sl] = np.where(sol, t_, 0); solid[sl] = sol

    # small things that no pixel kept (an eye, a berry, a door knob) get one pixel back
    if keep_small:
        flat_ids = ids.reshape(H, W, S * S)
        present = np.unique(wid)
        alive = np.unique(np.where(covered, flat_ids, 0))
        lost = np.setdiff1d(alive, present)
        if len(lost):
            yy, xx = np.mgrid[0:H, 0:W]
            for k in lost:
                if k == 0:
                    continue
                m = (flat_ids == k) & covered
                cnt = m.sum(-1)
                tot = cnt.sum()
                if tot < S * S * .3:
                    continue
                cells = cnt > 0
                ys, xs = np.nonzero(cells)
                if ys.max() - ys.min() > 2 or xs.max() - xs.min() > 2:
                    continue
                j = np.argmax(cnt[ys, xs]); y, x = ys[j], xs[j]
                vals = col[y, x][m[y, x]]
                u, c = np.unique(vals, return_counts=True)
                rgb[y, x] = u[np.argmax(c)]; solid[y, x] = True; top[y, x] = max(top[y, x], int(seq[y, x][m[y, x]].max()))
                if wid[y, x] == 0:
                    wid[y, x] = k

    # snap to the palette; blends with the shadow colour become the darker neighbour of what lies beneath
    names = list(palette); pal = np.array([pack(palette[k]) for k in names], np.int32)
    pr, pg, pb = (pal >> 16) & 255, (pal >> 8) & 255, pal & 255
    index = {int(p): i for i, p in enumerate(pal)}
    cast = dict(shade, **(cast or {})); table = {}
    for i, k in enumerate(names):
        once = names.index(cast.get(k, k)); twice = names.index(cast.get(names[once], names[once]))
        base = (int(pr[i]), int(pg[i]), int(pb[i]))
        mix = tuple(int(round(v * .5 + m * 255 * .5)) for v, m in zip(base, MARK))
        table[(mix[0] << 16) | (mix[1] << 8) | mix[2]] = int(pal[once])
        mix2 = tuple(int(round(v * .5 + m * 255 * .5)) for v, m in zip(mix, MARK))
        table[(mix2[0] << 16) | (mix2[1] << 8) | mix2[2]] = int(pal[twice])

    def snap(v):
        v = int(v)
        if v in index:
            return v
        if v in table:
            return table[v]
        near = None
        for dv in table:      # cairo rounds blends a little differently
            if abs(((dv >> 16) & 255) - ((v >> 16) & 255)) <= 2 and abs(((dv >> 8) & 255) - ((v >> 8) & 255)) <= 2 and abs((dv & 255) - (v & 255)) <= 2:
                near = table[dv]; break
        if near is None:
            r, g, b = (v >> 16) & 255, (v >> 8) & 255, v & 255
            near = int(pal[np.argmin((pr - r) ** 2 * 2 + (pg - g) ** 2 * 4 + (pb - b) ** 2 * 3)])
        table[v] = near
        return near
    for v in np.unique(rgb[solid]):
        s = snap(v)
        if s != v:
            rgb[(rgb == v) & solid] = s

    # outlines: a shape's own pixels that touch something painted earlier (or nothing at all)
    dark = {int(pal[i]): int(pal[names.index(shade.get(k, k))]) for i, k in enumerate(names)}
    if outlined:
        flag = np.zeros(n + 2, bool); ocol = np.zeros(n + 2, np.int32)
        for k, c in outlined.items():
            flag[k] = True; ocol[k] = c
        pad = np.pad(wid, 1, mode='constant' if open_edges else 'edge')
        mine = flag[wid]
        lower = np.zeros((H, W), bool); outer = np.zeros((H, W), bool)
        for dy, dx in ((0, 1), (2, 1), (1, 0), (1, 2)):
            nb = pad[dy:dy + H, dx:dx + W]
            lower |= nb < wid
            outer |= nb == 0
        e = mine & lower & solid
        if edge == 'tone':
            inner = e & ~outer
            twice = np.vectorize(lambda v: dark.get(dark.get(int(v), int(v)), int(v)))
            rgb[inner] = twice(rgb[inner]) if inner.any() else rgb[inner]
            rgb[e & outer] = ocol[wid][e & outer]
        else:
            rgb[e] = ocol[wid][e]

    # loose lines, in painting order; a line under a later shape stays hidden
    for q, colour, pts in lines:
        c = snap(colour)
        for x, y in pts:
            if top[y, x] <= q:
                rgb[y, x] = c; solid[y, x] = True
    return Pixels(rgb, solid)


def remap(px, table):
    """Swap colours by a dict of packed values."""
    out = px.rgb.copy()
    for a, b in table.items():
        out[px.rgb == a] = b
    return Pixels(out, px.solid)
