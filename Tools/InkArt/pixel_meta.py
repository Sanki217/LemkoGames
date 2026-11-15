"""Import settings for the pixel art: every picture at 40 pixels per unit, point filtered, uncompressed, no mipmaps,
drawn on a full rectangle. Run it after adding or redrawing pictures; it only touches the .meta files of pictures the
drawing kit makes, and it writes a .meta (with a stable id) for a new picture or folder that has none yet.
Usage: python3 pixel_meta.py
"""
import hashlib, re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
A = ROOT / 'Assets/Lemko'
BORDER = {'ui_card.png': 6, 'ui_button.png': 6}          # nine-slice borders, in pixels


def pictures():
    for folder in ('Sprites/Placeholders/Activities', 'Sprites/Placeholders/Products', 'Sprites/Products', 'Sprites/Placeholders/Village'):
        yield from sorted((A / folder).glob('*.png'))
    yield from sorted((A / 'Sprites/Placeholders/Backgrounds').glob('scene_*.png'))
    yield A / 'Sprites/Placeholders/Backgrounds/sprite_shop_clean.png'
    yield from sorted((A / 'Sprites/Placeholders/Rooms').glob('sprite_empty_room_*.png'))
    yield from (A / 'Sprites/UI' / n for n in BORDER)
    yield from sorted((A / 'Resources').rglob('*.png'))


def guid(path):
    return hashlib.md5(('lemko:' + path.relative_to(ROOT).as_posix()).encode()).hexdigest()


def setting(text, key, value):
    text, n = re.subn(r'(?m)^(\s*' + re.escape(key) + r':).*$', lambda m: m.group(1) + ' ' + value, text)
    assert n, key
    return text


FOLDER = 'fileFormatVersion: 2\nguid: %s\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'


def main():
    template = (A / 'Resources/Rooms/furniture_bar.png.meta').read_text()
    changed = 0
    for folder in sorted(p for p in (A / 'Resources').rglob('*') if p.is_dir()):
        meta = folder.with_name(folder.name + '.meta')
        if not meta.exists():
            meta.write_text(FOLDER % guid(folder)); changed += 1
    for png in pictures():
        meta = png.with_name(png.name + '.meta')
        text = meta.read_text() if meta.exists() else setting(template, 'guid', guid(png))
        new = setting(text, 'spritePixelsToUnits', '40')
        new = setting(new, 'filterMode', '0')
        new = setting(new, 'enableMipMap', '0')
        new = setting(new, 'spriteMeshType', '0')
        new = setting(new, 'textureCompression', '0')
        b = BORDER.get(png.name, 0)
        new = setting(new, 'spriteBorder', '{x: %d, y: %d, z: %d, w: %d}' % (b, b, b, b))
        if new != text or not meta.exists():
            meta.write_text(new); changed += 1
    print(changed, 'meta files written')


if __name__ == '__main__':
    main()
