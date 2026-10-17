"""Original geometric placeholder art; no external assets or language content."""
from pathlib import Path
from PIL import Image, ImageDraw
ROOT = Path(__file__).parent / 'PlaceholderArt'
ROOT.mkdir(exist_ok=True)
S = 3
def canvas(w,h,color=(0,0,0,0)):
    im=Image.new('RGBA',(w*S,h*S),color); return im,ImageDraw.Draw(im)
def box(d,xy,c,r=0):
    xy=tuple(int(v*S) for v in xy)
    if r: d.rounded_rectangle(xy,radius=r*S,fill=c)
    else: d.rectangle(xy,fill=c)
def oval(d,xy,c): d.ellipse(tuple(int(v*S) for v in xy),fill=c)
def poly(d,xy,c): d.polygon([(int(x*S),int(y*S)) for x,y in xy],fill=c)
def save(im,name): im.resize((im.width//S,im.height//S),Image.Resampling.LANCZOS).save(ROOT/(name+'.png'))
for item in ['apple','pear','bread','cheese','egg','milk','water']:
    im,d=canvas(256,256)
    if item=='apple':
        box(d,(122,23,136,76),'#745035',4); oval(d,(133,28,196,59),'#7faa48')
        oval(d,(39,64,144,217),'#e25d46'); oval(d,(108,64,218,217),'#e25d46'); oval(d,(60,84,85,130),'#ffac7c')
    elif item=='pear':
        box(d,(122,20,135,64),'#745035',4); oval(d,(133,25,185,51),'#5e8a3d')
        oval(d,(96,50,158,153),'#b6cf59'); oval(d,(55,103,202,225),'#b6cf59'); oval(d,(74,128,94,171),'#dfee8e')
    elif item=='bread':
        box(d,(27,72,229,205),'#c27e3b',45); box(d,(35,61,221,181),'#ecc078',45)
        for x in [65,111,157]: poly(d,[(x,84),(x+14,79),(x+35,125),(x+19,132)],'#bf8249')
    elif item=='cheese':
        poly(d,[(32,109),(171,48),(228,104),(228,209),(32,209)],'#e4a438'); poly(d,[(32,109),(171,48),(228,104)],'#ffe48a')
        poly(d,[(32,109),(228,104),(228,209),(32,209)],'#f8c955')
        for x,y,r in [(70,146,14),(133,180,19),(189,140,13)]: oval(d,(x-r,y-r,x+r,y+r),'#dca139')
    elif item=='egg':
        oval(d,(59,29,198,229),'#e9d7b4'); oval(d,(71,41,186,213),'#fff1d9'); oval(d,(86,66,107,109),'#ffffff')
    elif item=='milk':
        poly(d,[(66,64),(91,25),(170,25),(193,64)],'#8fc8d7'); box(d,(66,64,193,226),'#f5f1df',6)
        box(d,(66,112,193,176),'#69a8c3'); box(d,(88,27,169,40),'#e1efee')
        oval(d,(108,125,152,164),'#ffffff')
    else:
        box(d,(105,24,152,51),'#5594aa',5); box(d,(100,47,157,84),'#b1e0e9',8)
        box(d,(74,73,183,229),'#85c5d7',24); box(d,(83,115,174,206),'#499cbe',14)
        box(d,(74,139,183,178),'#e1f3ed'); box(d,(91,87,102,128),'#e3f6fa',5)
    save(im,'ph_'+item)

im,d=canvas(1600,900,'#e0d6b4')
box(d,(0,0,1600,100),'#203b3c')
box(d,(25,110,1575,250),'#365b50',24)
box(d,(30,270,915,827),'#4e5b4d',25)
for x in range(50,915,85): box(d,(x,283,x+3,812),'#596451')
box(d,(992,270,1560,830),'#294d46',24)
box(d,(1020,284,1530,298),'#d0ad64',5)
for y in [445,620,795]:
    box(d,(45,y,905,y+18),'#b17b4a',4); box(d,(50,y+18,900,y+29),'#745135',3)
box(d,(0,850,1600,900),'#b6a17b')
save(im,'ph_shop_background')

im,d=canvas(1600,900,'#a9d5d2')
oval(d,(1330,90,1450,210),'#f6dfa0')
oval(d,(-250,260,1100,1150),'#8bb78a'); oval(d,(650,300,1900,1100),'#76a67b')
poly(d,[(750,900),(1040,900),(910,500),(795,500)],'#d4ba88')
colors=['#c78864','#ccb077','#9dba95','#c29b79','#a4babb','#c9a18d','#b3a6b8','#cbbd85']
for i,c in enumerate(colors):
    x=275+(i%4)*350; y=390+(i//4)*210
    box(d,(x-87,y-45,x+87,y+75),c,5); poly(d,[(x-108,y-45),(x,y-120),(x+108,y-45)],'#6c635b')
    box(d,(x-18,y+13,x+18,y+75),'#5b6559',3)
    for dx in [-59,35]: box(d,(x+dx,y-20,x+dx+25,y+8),'#e9dcaa',3)
box(d,(0,0,1600,98),'#203b3c'); box(d,(205,115,1395,255),'#365b50',30)
save(im,'ph_village_background')
print('Created',len(list(ROOT.glob('*.png'))),'original placeholder PNGs.')
