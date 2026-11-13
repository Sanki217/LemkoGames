from pathlib import Path
from PIL import Image,ImageDraw
S=Path(__file__).resolve().parents[1]
art=S/'Assets/Lemko/Sprites/UI';art.mkdir(parents=True,exist_ok=True)
for name in ['card','button']:
 im=Image.new('RGBA',(192,192));d=ImageDraw.Draw(im)
 d.rounded_rectangle((2,8,190,190),radius=48,fill='#cbd4dd')
 d.rounded_rectangle((2,2,190,180 if name=='button' else 187),radius=48,fill='white')
 im.resize((64,64),Image.Resampling.LANCZOS).save(art/('ui_'+name+'.png'))
# Original layout artwork. Product sprites remain separate editable world objects.
im=Image.new('RGB',(1920,1080),'#f4f7f8');d=ImageDraw.Draw(im)
def card(xy,fill='white',rad=28):
 x0,y0,x1,y1=xy;d.rounded_rectangle((x0,y0+5,x1,y1+5),rad,fill='#dee5e8');d.rounded_rectangle(xy,rad,fill=fill)
card((45,280,1110,770));card((45,805,1110,1005),'#e9f7ef');card((1160,280,1875,1005))
for row in range(2):
 for col in range(4):card((80+col*255,315+row*220,305+col*255,510+row*220),'#f3f6fa',22)
path=S/'Assets/Lemko/Sprites/Placeholders/Backgrounds/sprite_shop_clean.png';path.parent.mkdir(parents=True,exist_ok=True);im.save(path)
print('Prepared hint accounting, cleaner copy and original UI sprites')
