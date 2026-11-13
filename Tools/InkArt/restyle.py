"""Redraw existing colour pictures as ink-on-paper doodles with the shared limited palette.

Each picture is enlarged, its outlines are turned into wobbly ink, its fills become white or
warm grey by brightness, and strongly warm colours become the gold or red accent.
Usage: python3 restyle.py <source dir of original PNGs> <target dir>
"""
import sys, zlib
from pathlib import Path
import numpy as np
from PIL import Image
from scipy import ndimage as ndi

INK, WHITE, T1, T2, T3 = [np.array([int(h[i:i + 2], 16) for i in (1, 3, 5)], float) for h in
                          ('#2A2420', '#FBF7EE', '#E0D5C0', '#C8BAA1', '#9B8D79')]
GOLD = np.array([0xF0, 0xB4, 0x29], float); RED = np.array([0xD8, 0x55, 0x3A], float)
SIZE = 512


def smooth(x, a, b):
    t = np.clip((x - a) / (b - a), 0, 1)
    return t * t * (3 - 2 * t)


def restyle(im, seed=0, edges=False):
    im = im.convert('RGBA').resize((SIZE, SIZE), Image.Resampling.BICUBIC)
    a = np.asarray(im, float) / 255
    rgb, alpha = a[..., :3], a[..., 3]
    # hand wobble: a gentle low-frequency displacement so ruler-straight edges look drawn
    rng = np.random.default_rng(seed)
    yy, xx = np.mgrid[0:SIZE, 0:SIZE].astype(float)
    def field():
        return ndi.zoom(rng.normal(0, 1, (10, 10)), SIZE / 10, order=3)[:SIZE, :SIZE] * 2.2
    dx, dy = field(), field()
    warp = lambda ch: ndi.map_coordinates(ch, [yy + dy, xx + dx], order=1, mode='constant')
    rgb = np.dstack([warp(rgb[..., i] * alpha) for i in range(3)])
    alpha = warp(alpha)
    rgb = rgb / np.maximum(alpha, 1e-4)[..., None]
    alpha = smooth(ndi.gaussian_filter(alpha, 1.0), .35, .65)          # crisp, smooth silhouette
    rgb = ndi.gaussian_filter(rgb, (1.1, 1.1, 0))

    mx, mn = rgb.max(-1), rgb.min(-1)
    lum = rgb @ np.array([.299, .587, .114])
    sat = (mx - mn) / np.maximum(mx, 1e-4)
    r, g, b = rgb[..., 0], rgb[..., 1], rgb[..., 2]
    hue = np.degrees(np.arctan2(np.sqrt(3) * (g - b), 2 * r - g - b)) % 360

    # ink: the dark outline colour, slightly thickened
    ink = 1 - smooth(lum, .26, .40)
    ink = np.maximum(ink, smooth(ndi.gaussian_filter(ink, 1.6), .30, .55))
    # fills: brightness picks white or one of three warm greys
    k1, k2, k3 = smooth(lum, .40, .46), smooth(lum, .58, .64), smooth(lum, .76, .82)
    fill = T3 + (T2 - T3) * k1[..., None] + (T1 - T2) * k2[..., None] + (WHITE - T1) * k3[..., None]
    # accents: only clearly warm, vivid colours keep a colour
    vivid = smooth(sat, .44, .50) * smooth(mx, .58, .66)
    warm_gold = vivid * smooth(hue, 12, 16) * (1 - smooth(hue, 64, 70))
    warm_red = vivid * np.maximum(1 - smooth(hue, 12, 16), smooth(hue, 340, 344))
    fill = fill + (GOLD - fill) * warm_gold[..., None]
    fill = fill + (RED - fill) * warm_red[..., None]
    # a clean ink rim around the whole cut-out, so pictures drawn without outlines get one
    soft = ndi.gaussian_filter(alpha, 2.0)
    ink = np.maximum(ink, (1 - smooth(soft, .80, .93)) * (alpha > .5))
    if edges:
        # pictures that had no inner outlines: draw ink where two flat colours meet
        g = sum(ndi.gaussian_gradient_magnitude(rgb[..., i], 1.2) for i in range(3))
        ink = np.maximum(ink, smooth(ndi.gaussian_filter(smooth(g, .10, .16), 1.0), .35, .6) * (alpha > .5))
    out = fill + (INK - fill) * ink[..., None]
    res = np.dstack([out, alpha[..., None] * 255])
    return Image.fromarray(np.clip(res, 0, 255).astype(np.uint8), 'RGBA')


if __name__ == '__main__':
    src, dst = Path(sys.argv[1]), Path(sys.argv[2]); dst.mkdir(parents=True, exist_ok=True)
    for f in sorted(src.glob('*.png')):
        restyle(Image.open(f), zlib.crc32(f.name.encode()), '--edges' in sys.argv).save(dst / f.name)
