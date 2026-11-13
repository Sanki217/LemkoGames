"""Number tickets: hand-drawn digits on paper tickets (110 pictures)."""
from sk import *

DIGITS = {
    '0': ([(.5, .02), (.22, .15), (.14, .5), (.22, .86), (.5, 1), (.78, .86), (.86, .5), (.78, .15), (.5, .02)], True),
    '1': ([(.22, .28), (.56, .02), (.56, 1)], False),
    '2': ([(.15, .26), (.3, .07), (.58, .02), (.82, .18), (.8, .4), (.5, .66), (.14, 1), (.9, 1)], True),
    '3': ([(.15, .1), (.55, .02), (.82, .2), (.72, .42), (.42, .5), (.75, .58), (.86, .8), (.6, .99), (.14, .9)], True),
    '4': ([(.7, 1), (.7, .02), (.1, .68), (.94, .68)], False),
    '5': ([(.84, .03), (.26, .03), (.18, .46), (.5, .4), (.8, .52), (.86, .78), (.6, .99), (.14, .9)], True),
    '6': ([(.76, .05), (.42, .2), (.2, .52), (.2, .84), (.46, 1), (.76, .9), (.83, .68), (.6, .5), (.24, .62)], True),
    '7': ([(.1, .04), (.9, .04), (.4, 1)], False),
    '8': ([(.5, .48), (.22, .35), (.25, .12), (.5, .02), (.75, .12), (.78, .35), (.5, .48), (.2, .65), (.2, .88), (.5, 1), (.8, .88), (.8, .65), (.5, .48)], True),
    '9': ([(.78, .38), (.5, .52), (.22, .4), (.2, .18), (.46, .02), (.74, .12), (.8, .4), (.76, .75), (.5, .98), (.2, .9)], True),
}


def smooth(pts, rounds=2):
    for _ in range(rounds):
        out = [pts[0]]
        for a, b in zip(pts, pts[1:]):
            out += [(a[0] * .75 + b[0] * .25, a[1] * .75 + b[1] * .25), (a[0] * .25 + b[0] * .75, a[1] * .25 + b[1] * .75)]
        out.append(pts[-1]); pts = out
    return pts


def ticket(c, text, rows=None):
    rows = rows or [text]
    n = max(len(r) for r in rows)
    dw, dh, gap, rgap = 62, 104, 16, 26
    w = n * dw + (n - 1) * gap
    h = len(rows) * dh + (len(rows) - 1) * rgap
    x0, y0 = 60, 70
    colr = 'gold' if len(text) >= 4 else 'pale' if n == 3 else 'cream' if n == 1 else 'yellow' if text[0] == '1' else 'skin' if text[1] == '0' else 'white'
    px, py = 34, 30
    L, R, T, B = x0 - px, x0 + w + px, y0 - py, y0 + h + py
    cut = 16
    pts = [(L + cut, T), (R - cut, T), (R, T + cut), (R, (T + B) / 2 - 14), (R - 12, (T + B) / 2), (R, (T + B) / 2 + 14), (R, B - cut), (R - cut, B),
           (L + cut, B), (L, B - cut), (L, (T + B) / 2 + 14), (L + 12, (T + B) / 2), (L, (T + B) / 2 - 14), (L, T + cut)]
    cr = c.c
    cr.save(); cr.translate((L + R) / 2, (T + B) / 2); cr.rotate(-.04); cr.translate(-(L + R) / 2, -(T + B) / 2)
    c.poly(pts, colr, step=14, sx=6, sy=7)
    c.ln([(L + 22, T + 12), (R - 22, T + 12)], 3.5, SHADE[colr], amp=.3); c.ln([(L + 22, B - 12), (R - 22, B - 12)], 3.5, SHADE[colr], amp=.3)
    for j, row in enumerate(rows):
        off = (n - len(row)) * (dw + gap)          # right-align shorter rows
        for i, ch in enumerate(row):
            pts, curved = DIGITS[ch]
            p = [(x0 + off + i * (dw + gap) + x * dw, y0 + j * (dh + rgap) + y * dh) for x, y in pts]
            if curved:
                p = smooth(p)
            c.line(p, 13, col('ink'), .9, 10)
    cr.restore()


NAMES = [str(i) for i in range(1, 100)] + ['100', '200', '300', '400', '500', '600', '700', '800', '900', '1000', '1000000']


def make():
    for i, t in enumerate(NAMES):
        # the canvas is wider for long numbers; pictures are scaled to fit afterwards
        rows = ['1000', '000'] if t == '1000000' else None
        c = Sp(900 + i, size=max(256, 120 + min(len(t), 4) * 78, 420 if rows else 0)); ticket(c, t, rows); c.out(ACT, 'number_' + t)


if __name__ == '__main__':
    import sk
    if len(sys.argv) > 1:
        sk.PREVIEW = sys.argv[1]
    make()
    if len(sys.argv) > 1:
        sheet(['number_' + n for n in ['1', '2', '3', '4', '5', '6', '7', '8', '9', '10', '11', '17', '20', '34', '56', '78', '90', '99', '100', '500', '1000', '1000000']], ACT, sys.argv[1] + '/_numbers.png', cols=8, cell=170)
