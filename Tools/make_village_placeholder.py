"""Original editable geometric village illustration; no external assets."""
from pathlib import Path
from PIL import Image, ImageDraw
import random, math
OUT=Path(__file__).resolve().parents[1]/'Assets/Lemko/Sprites/Placeholders/Village'
OUT.mkdir(parents=True,exist_ok=True)
S=2
im=Image.new('RGB',(1920*S,1080*S),'#e9e5cb'); d=ImageDraw.Draw(im)
ink='#465d50'
def poly(p,c,w=3):
 p=[(int(x*S),int(y*S)) for x,y in p];d.polygon(p,fill=c)
 if w:d.line(p+[p[0]],fill=ink,width=w*S,joint='curve')
def line(p,c,w):d.line([(int(x*S),int(y*S)) for x,y in p],fill=c,width=w*S,joint='curve')
def oval(b,c,w=0):d.ellipse(tuple(int(x*S) for x in b),fill=c,outline=ink if w else None,width=w*S)
# Layered foothills and a river around the village.
poly([(0,270),(210,170),(390,230),(690,110),(940,230),(1210,130),(1440,230),(1720,120),(1920,250),(1920,1080),(0,1080)],'#b7c6a0',0)
poly([(0,430),(230,260),(430,320),(780,215),(1080,315),(1350,220),(1630,345),(1920,260),(1920,1080),(0,1080)],'#ccd4ad',0)
river=[(-60,630),(170,570),(230,640),(190,760),(260,890),(490,950),(590,1100)]
line(river,'#8cb8b0',72);line(river,'#bad2c2',5)
# Paths curve through a central gathering lawn, with branches to all eight locations.
centres=[(350,400),(1480,440),(600,290),(1230,270),(420,740),(790,730),(1310,740),(1630,700)]
for x,y in centres:line([(970,555),((970+x)/2, (555+y)/2+35),(x,y+72)],'#adad85',45);line([(970,555),((970+x)/2,(555+y)/2+35),(x,y+72)],'#e5d2a5',35)
oval((825,450,1120,645),'#b3c28e');oval((880,492,1060,602),'#e0cea0')
random.seed(27)
def tree(x,y,r=23):
 line([(x,y),(x,y+45)],'#6c6f4e',7)
 for dx,dy,rr in [(-r*.5,0,r*.8),(r*.55,-2,r*.85),(0,-r*.6,r)]:oval((x+dx-rr,y+dy-rr,x+dx+rr,y+dy+rr),'#8ca679',2)
for x,y in [(100,330),(160,370),(240,255),(820,265),(920,300),(1040,200),(1710,340),(1770,410),(1850,520),(1760,860),(1600,920),(1230,945),(1080,870),(650,910),(85,855),(100,940),(840,405),(1200,520),(1390,590),(700,530)]:tree(x,y,random.randint(18,28))
def building(x,y,wall,roof,w=135,h=95):
 # Consistent oblique projection: front, side, pitched roof.
 oval((x-w*.85,y+25,x+w*.9,y+95),'#acb38a')
 poly([(x-w/2,y-h),(x+w/2,y-h+23),(x+w/2,y+63),(x-w/2,y+40)],wall)
 poly([(x+w/2,y-h+23),(x+w/2+65,y-h-14),(x+w/2+65,y+26),(x+w/2,y+63)],'#b3b295')
 poly([(x-w/2-15,y-h),(x-10,y-h-67),(x+w/2+14,y-h+23)],roof)
 poly([(x-10,y-h-67),(x+55,y-h-103),(x+w/2+79,y-h-14),(x+w/2+14,y-h+23)],roof)
 for t in [.22,.45,.68,.86]:line([(x-10+65*t,y-h-67-36*t),(x+w/2+14+65*t,y-h+23-37*t)],'#665c4d',2)
 poly([(x-9,y+49),(x-9,y-17),(x+21,y-12),(x+21,y+54)],'#526955')
 for dx in [-43,43]:
  poly([(x+dx-11,y-48),(x+dx+11,y-44),(x+dx+11,y-18),(x+dx-11,y-22)],'#f7df9d',2)
  line([(x+dx,y-46),(x+dx,y-20)],ink,2)
 # Stone doorstep.
 poly([(x-17,y+50),(x+28,y+57),(x+42,y+67),(x-4,y+60)],'#b4b3a0',2)
for i,(x,y) in sorted(enumerate(centres),key=lambda v:v[1][1]):
 building(x,y,['#e6c694','#dfc89f','#c7b48b','#ece0b8','#cad5b1','#d9d4b3','#dac4a1','#dfd8b3'][i],['#c48765','#856f63','#a16b56','#899987','#879f91','#a78d68','#927e6b','#8b9d8b'][i])
 if i==0:
  poly([(x-110,y+2),(x-35,y+15),(x-35,y+69),(x-110,y+56)],'#b78957')
  for n in range(4):poly([(x-122+n*23,y-35+n*4),(x-99+n*23,y-31+n*4),(x-20+n*0,y+8),(x-43+n*0,y+4)],'#d6a46f' if n%2 else '#f0e1b5',1)
  for n in range(4):oval((x-105+n*17,y+18,x-94+n*17,y+31),'#bc7657')
 if i==2:
  pts=[(x-140,y+45),(x-20,y+70),(x-70,y+115),(x-190,y+90),(x-140,y+45)]
  line(pts,'#8c7956',5)
  for xx,yy in pts[:-1]:line([(xx,yy-20),(xx,yy+8)],'#76694c',5)
  oval((x-144,y+55,x-109,y+78),'#f1e6be');oval((x-122,y+46,x-107,y+63),'#786b52')
 if i==4:
  for j in range(3):poly([(x-115+j*32,y+28),(x-90+j*32,y+34),(x-90+j*32,y+58),(x-115+j*32,y+52)],'#b78d5f',2)
 if i==5:
  for j in range(3):poly([(x-96+j*27,y+30),(x-75+j*27,y+34),(x-75+j*27,y+61),(x-96+j*27,y+57)],'#ecdbab',2)
 if i==7:
  line([(x+124,y+10),(x+124,y-100)],ink,4);poly([(x+124,y-97),(x+171,y-91),(x+124,y-80)],'#d4ac6c',2)
  oval((x-130,y+50,x-80,y+70),'#c6c295')
# Centre: long shared table and bunting; reward cloth is a separate world sprite.
poly([(910,515),(1018,536),(994,567),(886,546)],'#ac8f61')
for x,y in [(910,515),(1018,536),(994,567),(886,546)]:line([(x,y),(x,y+25)],'#796b4c',4)
line([(860,488),(1090,528)],ink,2)
for i in range(9):
 x=875+i*24;y=491+i*4.1;poly([(x,y),(x+15,y+3),(x+7,y+20)],['#c18e65','#829e87','#e5c78d'][i%3],1)
# Footbridge over the stream.
poly([(142,690),(250,722),(236,749),(128,718)],'#bca279',3)
for i in range(7):line([(143+i*15,692+i*4.4),(130+i*15,717+i*4.4)],'#867658',2)
# Sparse meadow marks, kept out of buildings and paths.
for x,y in [(150,480),(720,400),(1110,380),(1550,260),(1800,710),(1120,780),(550,570),(730,880),(1320,880)]:
 for j in range(4):line([(x+j*10,y+5),(x+j*10+3,y-3)],'#8b9c74',2)
im.resize((1920,1080),Image.Resampling.LANCZOS).save(OUT/'sprite_village_map.png')
marker=Image.new('RGBA',(128,64));m=ImageDraw.Draw(marker);m.ellipse((5,8,123,56),outline='#ffe9a2',width=6);marker.save(OUT/'sprite_location_ring.png')
print('Village art prepared')
