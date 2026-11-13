"""The 44 family pictures: doodle portraits, each with its own look, plus a small relation
chain underneath (red dot = you, circle = woman, square = man, double line = marriage,
a step up or down = a generation)."""
from sk import *

HAIRS = ['brown', 'ink', 'gold', 'brick', 'wood', 'plum']


def person(c, cx, cy, s=1.0, sex='m', age='adult', hair='brown', shirt='blue', extra=()):
    cr = c.c
    cr.save(); cr.translate(cx, cy); cr.scale(s, s)
    old = age in ('elder', 'ancient')
    kid = age in ('child', 'tot')
    if old:
        hair = 'silver' if age == 'elder' else 'white'
    skin = 'cream'
    # long hair behind the head
    if sex == 'f' and not old and 'bun' not in extra and 'short' not in extra:
        if kid:
            for sg in (-1, 1):
                c.ell(sg * 52, 8, 13, 22, hair, rot=sg * .3, w=6.5)
        else:
            c.poly([(-50, -8), (-46, -40), (0, -58), (46, -40), (50, -8), (54, 58), (30, 66), (-30, 66), (-54, 58)], hair, step=12)
    if 'bun' in extra:
        c.ell(0, -58, 17, 15, hair)
    # body
    w = 42 if kid else 64
    c.poly([(-w, 98), (-w + 4, 60), (-w * .42, 42), (w * .42, 42), (w - 4, 60), (w, 98)], shirt, step=12,
           inner=lambda: c.fill([(-13, 40), (13, 40), (0, 62)], skin))
    if 'vest' in extra:
        c.ln([(-w * .42, 44), (-10, 96)], 5.5); c.ln([(w * .42, 44), (10, 96)], 5.5)
    if 'beads' in extra:
        for i in range(5):
            c.dot(-16 + i * 8, 62 + (4 - abs(i - 2) * 3), 4, 'brick')
    c.ln([(-13, 42), (0, 60), (13, 42)], 5)
    # head
    c.ell(0, 0, 44, 46, skin)
    # scarf for grandmothers
    if 'scarf' in extra:
        col_ = 'brick' if age == 'elder' else 'navy'
        c.poly([(-50, 14), (-46, -36), (0, -58), (46, -36), (50, 14), (36, -10), (0, -24), (-36, -10)], col_, step=11)
        c.poly([(-12, 44), (0, 36), (12, 44), (8, 62), (0, 50), (-8, 62)], col_, step=8, w=5.5)
        for x, y in ((-26, -34), (0, -42), (26, -34)):
            c.dot(x, y, 3.2, 'yellow')
        c.fill([(-30, -12), (0, -22), (30, -12), (0, -16)], hair)
    elif sex == 'm' and old:
        for sg in (-1, 1):
            c.lump(sg * 42, -12, 12, 18, hair, bumps=4, w=6)
        if 'hat' in extra:
            c.poly([(-40, -30), (-34, -62), (34, -62), (40, -30)], 'pine', step=10); c.poly([(-58, -26), (58, -26), (56, -36), (-56, -36)], 'pine', step=10, w=6)
        else:
            c.arc(0, -8, 30, 215, 325, 4.5, ry=38)
    elif sex == 'm':
        if 'cap' in extra:
            c.poly([(-46, -14), (-40, -44), (0, -58), (40, -44), (46, -14)], 'pine', step=10); c.poly([(-46, -14), (58, -14), (60, -22), (-44, -24)], 'pine', step=10, w=6)
        elif kid:
            c.poly([(-46, -10), (-42, -40), (-8, -56), (6, -66), (10, -54), (40, -42), (46, -10), (20, -26), (-22, -24)], hair, step=10)
        else:
            c.poly([(-47, -6), (-42, -42), (0, -58), (42, -42), (47, -6), (30, -26), (-4, -30), (-32, -20)], hair, step=10)
    else:
        if 'short' in extra:
            c.poly([(-48, 6), (-44, -40), (0, -58), (44, -40), (48, 6), (32, -22), (0, -30), (-32, -22)], hair, step=10)
        else:
            c.poly([(-46, -6), (-42, -40), (0, -57), (42, -40), (46, -6), (28, -28), (6, -22), (-30, -30)], hair, step=10)
        if 'bow' in extra:
            c.poly([(14, -54), (34, -66), (32, -42)], 'brick', step=8, w=5); c.poly([(14, -54), (-4, -68), (-4, -44)], 'brick', step=8, w=5)
    # face
    ey = 4
    if 'glasses' in extra or (old and 'noglasses' not in extra):
        for sg in (-1, 1):
            c.ell(sg * 17, ey, 12, 11, None, w=4.5)
        c.ln([(-5, ey), (5, ey)], 4)
        c.dot(-17, ey, 3.6); c.dot(17, ey, 3.6)
    else:
        c.dot(-16, ey, 5); c.dot(16, ey, 5)
    c.blush(-27, 18, 7); c.blush(27, 18, 7)
    if 'moustache' in extra:
        c.poly([(0, 16), (-22, 26), (-10, 14), (0, 12), (10, 14), (22, 26)], hair if not old else 'silver', step=8, w=5)
    elif 'beard' in extra:
        c.poly([(-32, 14), (-12, 20), (0, 16), (12, 20), (32, 14), (20, 46), (0, 56), (-20, 46)], hair, step=9, w=5.5)
        c.arc(0, 26, 7, 20, 160, 4)
    else:
        c.smile(0, 20, 16)
    cr.restore()


STEP = 34


def chain(c, path, sex):
    """path: list of (step, sex of the person reached). step is u, d, s or m."""
    pts = [(0, 0)]
    for st, _ in path:
        x, y = pts[-1]
        pts.append((x + STEP, y + (-15 if st == 'u' else 15 if st == 'd' else 0)))
    xs = [p[0] for p in pts]; ys = [p[1] for p in pts]
    ox = 128 - (min(xs) + max(xs)) / 2; oy = 230 - (min(ys) + max(ys)) / 2
    pts = [(x + ox, y + oy) for x, y in pts]
    for (st, _), a, b in zip(path, pts, pts[1:]):
        if st == 'm':
            c.ln([(a[0], a[1] - 4), (b[0], b[1] - 4)], 3.6, amp=.2); c.ln([(a[0], a[1] + 4), (b[0], b[1] + 4)], 3.6, amp=.2)
        elif st == 's':
            c.ln([a, (a[0] + 6, a[1] - 13), (b[0] - 6, b[1] - 13), b], 4.5, amp=.2)
        else:
            c.ln([a, ((a[0] + b[0]) / 2, a[1]), ((a[0] + b[0]) / 2, b[1]), b], 4.5, amp=.2)
    for i, (x, y) in enumerate(pts):
        if i == 0:
            c.ell(x, y, 8, 8, 'brick', w=4.5, shade=False)
            continue
        s = path[i - 1][1]
        r = 9.5 if i == len(pts) - 1 else 6.5
        if s == 'f':
            c.ell(x, y, r, r, 'pale' if i == len(pts) - 1 else 'white', w=4.5, shade=False)
        elif s == 'm':
            c.rr(x - r, y - r, 2 * r, 2 * r, 2, 'sky' if i == len(pts) - 1 else 'white', w=4.5, shade=False, step=8, amp=.3)
        else:
            c.dot(x, y, 4.5)


# name: (sex, age, hair, shirt, extras, relation path)
P = lambda s: [(t[0], t[1]) for t in s.split()]
PEOPLE = {
    'you': ('m', 'adult', 'brown', 'brick', (), ''),
    'mother': ('f', 'adult', 'brown', 'sage', ('bun', 'beads'), 'uf'),
    'father': ('m', 'adult', 'ink', 'navy', ('moustache',), 'um'),
    'son': ('m', 'child', 'brown', 'lime', (), 'dm'),
    'daughter': ('f', 'child', 'brown', 'orange', ('bow',), 'df'),
    'brother': ('m', 'adult', 'wood', 'pine', (), 'sm'),
    'sister': ('f', 'adult', 'wood', 'mauve', (), 'sf'),
    'husband': ('m', 'adult', 'ink', 'blue', ('beard',), 'mm'),
    'wife': ('f', 'adult', 'gold', 'brick', ('beads',), 'mf'),
    'grandfather': ('m', 'elder', '', 'pine', ('moustache',), 'u? um'),
    'grandmother': ('f', 'elder', '', 'mauve', ('scarf',), 'u? uf'),
    'great_grandfather': ('m', 'ancient', '', 'grey1', ('beard', 'hat'), 'u? u? um'),
    'great_grandmother': ('f', 'ancient', '', 'plum', ('scarf',), 'u? u? uf'),
    'grandson': ('m', 'tot', 'gold', 'sky', (), 'd? dm'),
    'granddaughter': ('f', 'tot', 'gold', 'yellow', ('bow',), 'd? df'),
    'aunt': ('f', 'adult', 'brick', 'sage', ('short', 'glasses'), 'u? sf'),
    'paternal_uncle': ('m', 'adult', 'ink', 'wood', ('moustache', 'cap'), 'um sm'),
    'maternal_uncle': ('m', 'adult', 'brown', 'green', ('beard',), 'uf sm'),
    'paternal_uncles_wife': ('f', 'adult', 'plum', 'gold', ('bun',), 'um sm mf'),
    'maternal_uncles_wife': ('f', 'adult', 'gold', 'navy', ('short', 'beads'), 'uf sm mf'),
    'male_cousin': ('m', 'adult', 'brick', 'lime', ('glasses',), 'u? s? dm'),
    'female_cousin': ('f', 'adult', 'ink', 'orange', (), 'u? s? df'),
    'brothers_son': ('m', 'child', 'wood', 'navy', (), 'sm dm'),
    'brothers_daughter': ('f', 'child', 'wood', 'mint', (), 'sm df'),
    'brothers_wife': ('f', 'adult', 'ink', 'yellow', ('bun',), 'sm mf'),
    'sisters_son': ('m', 'child', 'ink', 'gold', ('cap',), 'sf dm'),
    'sisters_daughter': ('f', 'child', 'brick', 'blue', ('bow',), 'sf df'),
    'son_in_law': ('m', 'adult', 'gold', 'slate', ('glasses',), 'df mm'),
    'daughter_in_law': ('f', 'adult', 'brick', 'pale', (), 'dm mf'),
    'husbands_father': ('m', 'elder', '', 'navy', ('moustache', 'hat'), 'mm um'),
    'husbands_mother': ('f', 'elder', '', 'sage', ('scarf', 'noglasses'), 'mm uf'),
    'husbands_brother': ('m', 'adult', 'ink', 'sand', ('moustache',), 'mm sm'),
    'husbands_sister': ('f', 'adult', 'ink', 'lime', ('short',), 'mm sf'),
    'wifes_father': ('m', 'elder', '', 'brown', ('beard',), 'mf um'),
    'wifes_mother': ('f', 'elder', '', 'gold', ('bun', 'beads'), 'mf uf'),
    'wifes_brother': ('m', 'adult', 'gold', 'dgreen', ('cap',), 'mf sm'),
    'wifes_sister': ('f', 'adult', 'gold', 'sky', ('bow',), 'mf sf'),
}
NAMES = list(PEOPLE) + ['parents', 'grandparents', 'children', 'grandchildren', 'child', 'family', 'tree']


def make():
    for i, (name, (sex, age, hair, shirt, extra, path)) in enumerate(PEOPLE.items()):
        c = Sp(100 + i)
        kid = age in ('child', 'tot')
        person(c, 128, 88 if not kid else 96, 1.0 if age != 'tot' else .86, sex, age, hair, shirt, extra)
        chain(c, P(path), sex)
        c.out(ACT, 'family_' + name)
    def pair(name, a, b, seed, s=.78):
        c = Sp(seed); person(c, 84, 120, s, *a); person(c, 176, 132, s, *b); c.out(ACT, 'family_' + name)
    pair('parents', ('f', 'adult', 'brown', 'sage', ('bun', 'beads')), ('m', 'adult', 'ink', 'navy', ('moustache',)), 150)
    pair('grandparents', ('f', 'elder', '', 'mauve', ('scarf',)), ('m', 'elder', '', 'pine', ('moustache',)), 151)
    pair('children', ('f', 'child', 'brown', 'orange', ('bow',)), ('m', 'child', 'brown', 'lime', ()), 152)
    pair('grandchildren', ('f', 'tot', 'gold', 'yellow', ('bow',)), ('m', 'tot', 'gold', 'sky', ()), 153, .7)
    c = Sp(154); person(c, 128, 110, 1.0, 'm', 'tot', 'wood', 'yellow', ()); c.out(ACT, 'family_child', .7)
    c = Sp(155)
    person(c, 70, 96, .68, 'f', 'adult', 'brown', 'sage', ('bun', 'beads')); person(c, 186, 96, .68, 'm', 'adult', 'ink', 'navy', ('moustache',))
    person(c, 128, 168, .62, 'f', 'child', 'brown', 'orange', ('bow',)); c.out(ACT, 'family_family')
    # family tree: a tree whose crown holds the family's faces
    c = Sp(156)
    c.poly([(112, 236), (118, 150), (138, 150), (144, 236)], 'wood', step=12)
    c.ln([(128, 160), (84, 120)], 9, 'ink'); c.ln([(128, 160), (172, 120)], 9, 'ink')
    c.lump(128, 104, 108, 84, 'green', bumps=9, depth=.16)
    for x, y, colr in ((128, 56, 'silver'), (80, 104, 'brown'), (176, 104, 'ink'), (128, 142, 'gold')):
        c.ell(x, y, 23, 24, 'cream', w=5.5)
        c.poly([(x - 24, y - 4), (x - 20, y - 22), (x, y - 30), (x + 20, y - 22), (x + 24, y - 4), (x + 12, y - 14), (x - 12, y - 14)], colr, step=8, w=5)
        c.dot(x - 8, y + 3, 3); c.dot(x + 8, y + 3, 3); c.arc(x, y + 10, 6, 20, 160, 3.5, ry=4)
    c.ln([(128, 82), (128, 96), (80, 96)], 4.5, 'cream', amp=.3); c.ln([(128, 96), (176, 96)], 4.5, 'cream', amp=.3)
    c.out(ACT, 'family_tree')


if __name__ == '__main__':
    import sk
    if len(sys.argv) > 1:
        sk.PREVIEW = sys.argv[1]
    make()
    if len(sys.argv) > 1:
        sheet(['family_' + n for n in NAMES], ACT, sys.argv[1] + '/_family.png', cols=9, cell=170)
