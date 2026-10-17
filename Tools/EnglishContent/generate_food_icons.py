"""Original geometric placeholders. Existing supplied artwork is never edited."""
import json, math, random
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
root=Path(__file__).parent
entries=json.loads((root/'entries.json').read_text(encoding='utf8'))['entries']
dest=root/'GeneratedSprites'; dest.mkdir(exist_ok=True)
S=3
def rect(b,c,r=0):
    b=tuple(int(v*S) for v in b)
    if r: d.rounded_rectangle(b,r*S,fill=c)
    else: d.rectangle(b,fill=c)
def ellipse(b,c): d.ellipse(tuple(int(v*S) for v in b),fill=c)
def line(pts,c,w=5): d.line([(int(x*S),int(y*S)) for x,y in pts],fill=c,width=w*S,joint='curve')
def poly(pts,c): d.polygon([(int(x*S),int(y*S)) for x,y in pts],fill=c)
def leaf(x,y,c='#6ea44e',angle=0):
    poly([(x,y+30),(x-17,y+9),(x-13,y-13),(x,y-30),(x+15,y-13),(x+18,y+9)],c)
    line([(x,y-23),(x,y+22)],'#416e38',2)
def leaves(x=128,y=50):
    for dx,dy in [(-24,0),(0,-17),(24,0)]: leaf(x+dx,y+dy)
def berry(x,y,r,c):
    ellipse((x-r,y-r,x+r,y+r),c); ellipse((x-r*.45,y-r*.6,x-r*.05,y-r*.2),'#ffffff77')
def bottle(liquid,glass='#adcbbd'):
    rect((106,25,150,55),'#5b6a58',5); rect((110,53,146,86),glass,7)
    poly([(110,75),(146,75),(178,110),(178,219),(78,219),(78,110)],glass)
    rect((88,120,168,208),liquid,12); rect((98,99,107,135),'#e2eeee',3)
def jar(base,dots=None):
    rect((77,53,179,221),base,17); rect((72,37,184,63),'#928979',7)
    if dots:
        random.seed(9)
        for _ in range(50):
            x=random.randint(87,165); y=random.randint(80,204); ellipse((x,y,x+8,y+5),dots)
def cake(color,top):
    poly([(43,103),(174,56),(221,104),(221,220),(43,220)],color)
    rect((44,144,220,158),'#f6dfb2'); rect((44,183,220,193),'#f6dfb2')
    poly([(43,103),(174,56),(221,104)],top)
def fish():
    ellipse((38,81,183,174),'#75a9ba'); poly([(166,126),(225,77),(225,178)],'#5e8da5')
    poly([(96,85),(128,49),(153,92)],'#4e839d'); ellipse((60,104,76,120),'#ffffff'); ellipse((64,108,72,116),'#253b44')
    line([(89,99),(95,126),(89,152)],'#426f81',4)
def grains(c):
    jar('#d7c2a3',c)
    line([(196,202),(201,75)],'#b99c58',4)
    for y in range(85,180,18): ellipse((183,y,201,y+12),'#d6ba6e'); ellipse((201,y+7,219,y+19),'#d6ba6e')

for entry in entries:
    if entry['oldSpritePath']: continue
    name=entry['slug']; im=Image.new('RGBA',(256*S,256*S)); d=ImageDraw.Draw(im)
    if name=='butter':
        poly([(29,155),(75,85),(214,85),(229,181),(66,212)],'#e6e3d7'); rect((58,82,205,174),'#efd273',13); poly([(58,82),(91,56),(220,56),(205,82)],'#fff0a5')
    elif name in ('meat','ham'):
        ellipse((33,52,225,216),'#b85451' if name=='meat' else '#d98784'); ellipse((45,65,211,204),'#e2a29b'); ellipse((67,79,198,189),'#c96d68')
        ellipse((126,117,169,160),'#f5e9d1'); line([(71,106),(106,136),(81,171)],'#efc1b0',7)
    elif name=='sausage':
        line([(68,63),(42,103),(52,157),(92,193),(152,199),(196,173),(207,124)],'#985747',40)
        line([(68,63),(49,104),(59,151),(99,186),(153,190),(190,165),(198,125)],'#ce8867',25)
        for x,y in [(64,132),(103,184),(165,187)]: line([(x-9,y-9),(x+9,y+9)],'#f1be92',3)
    elif name=='cream':
        poly([(54,91),(202,91),(184,220),(72,220)],'#e9eadf'); rect((62,130,194,172),'#88abc7'); ellipse((51,69,205,109),'#d7ddd4')
        ellipse((73,53,184,99),'#fff5df'); ellipse((99,33,164,88),'#fff5df')
    elif name=='flour':
        poly([(65,42),(184,42),(196,218),(53,218)],'#d6ba86'); rect((61,101,188,191),'#f2e7c9',8)
        line([(125,178),(125,115)],'#bd9651',4)
        for y in range(118,169,13): ellipse((109,y,125,y+10),'#c9a360'); ellipse((126,y,142,y+10),'#c9a360')
    elif name=='sugar':
        jar('#e7e3d7');
        for x,y in [(91,91),(131,109),(95,151),(137,174)]: rect((x,y,x+31,y+29),'#ffffff',3)
    elif name in ('salt','black_pepper'):
        rect((82,72,174,225),'#d3d5cd',18); rect((92,96,164,214),'#fff1dc' if name=='salt' else '#69564c',9)
        ellipse((81,38,175,95),'#a4ada8')
        for x,y in [(111,54),(137,54),(123,71)]: ellipse((x,y,x+7,y+7),'#53625e')
        if name=='black_pepper':
            for x,y in [(107,120),(139,150),(111,181)]: ellipse((x,y,x+11,y+11),'#3a3330')
    elif name in ('oil','vinegar','vodka'):
        bottle({'oil':'#d9c76e','vinegar':'#864b46','vodka':'#e3eeee'}[name])
        if name=='oil': leaf(127,161,'#79944b')
        elif name=='vinegar':
            for x,y in [(117,150),(139,150),(128,169)]: berry(x,y,12,'#8e6b96')
        else:
            line([(127,144),(127,185)],'#88bccc',3); line([(108,154),(146,175)],'#88bccc',3); line([(108,175),(146,154)],'#88bccc',3)
    elif name in ('tea','coffee'):
        ellipse((163,91,225,168),'#d1ad7e'); ellipse((176,104,213,154),(0,0,0,0))
        rect((46,84,185,208),'#b7d0b1' if name=='tea' else '#d5b28d',22); ellipse((45,71,186,105),'#866d43' if name=='tea' else '#664739')
        line([(93,59),(87,44),(100,25)],'#d8e2d3',5)
        if name=='tea': leaf(115,152)
        else: ellipse((97,135,137,179),'#7c5841'); line([(123,140),(112,157),(116,172)],'#e3ba86',3)
    elif name=='beer':
        ellipse((155,86,223,190),'#e5ca78'); ellipse((170,101,208,176),(0,0,0,0))
        rect((49,81,180,220),'#d5ac54',12)
        for x in [69,101,133]: rect((x,108,x+18,204),'#efd285',5)
        for x,y in [(68,78),(96,61),(130,67),(161,80)]: ellipse((x-29,y-23,x+29,y+23),'#fff4d2')
    elif name=='pasta':
        for i in range(7): line([(56+i*20,49),(49+i*20,74),(61+i*20,100),(52+i*20,124),(61+i*20,157)],'#e6bf68',9)
        poly([(29,156),(226,156),(191,222),(65,222)],'#7da7aa'); ellipse((29,143,226,174),'#e8cc80')
    elif name in ('fish','canned_herring'):
        if name=='canned_herring': rect((21,59,238,199),'#c4cfca',25); rect((32,72,226,187),'#f0dca7',20)
        fish()
    elif name in ('raspberries','blackberries'):
        color='#c95f6a' if name=='raspberries' else '#565064'
        for dx in [-50,43]:
            for x,y in [(105,84),(129,81),(92,111),(117,110),(142,111),(104,136),(130,137),(117,159)]: berry(x+dx*.6,y+30,15,color)
            leaf(118+dx*.6,67,'#6f9b50')
    elif name in ('blueberries','grapes','currant','gooseberry'):
        color={'blueberries':'#557aab','grapes':'#9074aa','currant':'#c55d59','gooseberry':'#abc566'}[name]
        for x,y in [(93,91),(140,82),(174,115),(71,135),(120,131),(153,157),(105,182)]: berry(x,y,23,color)
        leaf(133,48)
        if name=='gooseberry':
            for x,y in [(93,91),(120,131),(153,157)]: line([(x,y-19),(x+6,y),(x,y+19)],'#819a50',2)
    elif name=='strawberries':
        poly([(45,104),(76,71),(119,93),(137,129),(91,219)],'#d26857'); poly([(124,77),(158,57),(206,77),(218,119),(164,198)],'#df725a')
        leaves(95,77); leaf(170,56)
        for x,y in [(69,123),(92,148),(96,108),(164,98),(190,112),(170,146),(87,183)]: ellipse((x,y,x+5,y+8),'#f5dba0')
    elif name=='plums':
        ellipse((40,89,123,216),'#796d9a'); ellipse((126,75,217,203),'#8e7ba7'); leaf(131,65); line([(83,112),(91,184)],'#ab94bc',3)
    elif name=='lemon':
        poly([(33,128),(72,69),(175,63),(223,124),(181,190),(78,196)],'#e9d46d'); ellipse((52,69,203,193),'#f3dc75'); ellipse((83,91,108,116),'#fff0a7')
    elif name in ('sour_cherries','sweet_cherries'):
        line([(75,159),(111,54),(162,54),(182,162)],'#6c8550',6); leaf(147,56)
        berry(75,171,41,'#de6b5e' if name=='sour_cherries' else '#aa4e59'); berry(178,176,41,'#de6b5e' if name=='sour_cherries' else '#aa4e59')
    elif name=='potatoes':
        for x,y in [(78,91),(161,109),(100,171)]:
            ellipse((x-47,y-34,x+47,y+37),'#b99b74')
            for dx,dy in [(-20,0),(14,-8),(5,19)]: ellipse((x+dx,y+dy,x+dx+6,y+dy+4),'#8b7359')
    elif name in ('beetroots','turnip','radish','parsley_root','horseradish'):
        leaves(125,49)
        if name in ('parsley_root','horseradish'): poly([(72,89),(181,89),(142,192),(116,234)],'#e1d5ae'); line([(89,126),(161,133)],'#b6a986',3)
        else:
            c={'beetroots':'#9f596d','turnip':'#e7d7b3','radish':'#d56a79'}[name]
            ellipse((61,80,196,204),c); poly([(110,184),(148,184),(131,234)],c)
            if name=='turnip': ellipse((65,80,192,134),'#b78fa5')
    elif name in ('beans','fava_bean'):
        for x,y in [(76,102),(166,111),(115,186)]:
            ellipse((x-35,y-28,x+35,y+28),'#b68272' if name=='beans' else '#a9bf7a'); ellipse((x-8,y-10,x+18,y+4),'#f0dcb7')
    elif name=='peas':
        poly([(25,169),(69,84),(157,63),(231,99),(181,186),(96,210)],'#5d9a60')
        for x,y in [(70,155),(113,132),(158,111),(198,103)]: berry(x,y,24,'#b0cd71')
    elif name in ('cabbage','lettuce'):
        for x,y,r in [(89,116,60),(163,113,61),(125,167,65)]: berry(x,y,r,'#91b878' if name=='cabbage' else '#74a867')
        for i in range(4):
            d.arc(tuple(v*S for v in (65+i*10,74+i*10,195-i*10,220-i*10)),40,310,fill='#c6d69c',width=4*S)
    elif name=='bell_pepper':
        for x in [95,131,165]: ellipse((x-37,77,x+37,219),'#d76a50')
        line([(127,90),(132,43),(156,36)],'#639354',13)
    elif name=='cucumbers':
        for x,y in [(72,52),(136,78)]:
            rect((x,y,x+54,y+152),'#6b9d61',25)
            for dy in range(20,140,25): ellipse((x+16,y+dy,x+23,y+dy+5),'#b0c786')
    elif name=='garlic':
        poly([(112,26),(141,26),(152,104),(95,104)],'#e2dec9')
        for x,y in [(86,146),(165,146),(127,150)]: ellipse((x-38,y-63,x+38,y+65),'#f3e6cd'); line([(x,y-46),(x-9,y+33)],'#d8c5af',3)
    elif name=='onion':
        ellipse((52,82,210,224),'#c69e6b'); poly([(82,105),(119,27),(145,27),(173,105)],'#b6a16c')
        for x in [93,128,163]: line([(128,73),(x,139),(128,211)],'#e7c797',3)
    elif name=='pumpkin':
        for x,r in [(78,41),(178,41),(107,45),(150,45),(129,37)]: ellipse((x-r,83,x+r,219),'#db944b')
        line([(128,91),(126,43),(146,36)],'#64844a',16)
    elif name=='tomato':
        ellipse((41,67,219,219),'#d86b53'); poly([(128,44),(143,83),(185,81),(151,103),(161,130),(128,110),(93,129),(105,98),(69,80),(113,82)],'#658c4c')
    elif name in ('mint','parsley','rosemary','marjoram','bay_leaf'):
        if name=='bay_leaf': leaf(126,126,'#879f66')
        else:
            for dx in [-43,0,43]:
                line([(128,226),(128+dx,62)],'#587e48',5)
                for j in range(3): leaf(128+dx,76+j*42,'#76a663' if name!='rosemary' else '#6d927a')
    elif name in ('chive','chives'):
        for x in range(60,199,20): line([(115+(x-120)*.5,221),(x,39)],'#69a363',9 if name=='chive' else 4)
        if name=='chive': ellipse((97,191,142,231),'#e3e2bc')
    elif name=='buns':
        for x,y in [(77,101),(172,108),(127,184)]:
            ellipse((x-46,y-33,x+46,y+33),'#d6ab6c'); line([(x-16,y-14),(x+18,y+9)],'#f3d29c',7)
    elif name=='croissant':
        poly([(28,152),(44,94),(81,61),(127,52),(175,65),(214,104),(229,164),(191,151),(171,123),(142,113),(108,114),(80,130),(63,162)],'#d9a35d')
        for x,y in [(77,76),(117,58),(159,67),(191,94)]: line([(x,y),(x+12,y+48)],'#f2cc84',5)
    elif name in ('buckwheat_groats','barley_groats','lentil'):
        grains({'buckwheat_groats':'#8e785b','barley_groats':'#e6d4aa','lentil':'#c58b5e'}[name])
    elif name in ('walnuts','hazelnuts'):
        for x,y in [(82,110),(165,113),(122,190)]:
            ellipse((x-37,y-32,x+37,y+32),'#a27e54' if name=='walnuts' else '#b48a60')
            if name=='walnuts': line([(x-23,y+6),(x-12,y-17),(x,y+15),(x+13,y-16),(x+25,y+9)],'#d4b380',4)
            else: ellipse((x-26,y-30,x+26,y-10),'#debd83')
    elif name in ('mushrooms','porcini_mushrooms'):
        for x,y,scale in [(84,129,.8),(166,89,1.0)]:
            rect((x-18*scale,y,x+18*scale,y+92*scale),'#e3d7b9',12)
            ellipse((x-56*scale,y-40*scale,x+56*scale,y+27*scale),'#aa8a66' if name=='mushrooms' else '#906747')
            rect((x-55*scale,y+8*scale,x+55*scale,y+25*scale),'#ccb99a',5)
    elif name=='corn':
        ellipse((90,35,168,220),'#ebc46a')
        for x in [105,124,143]:
            for y in range(58,203,19): rect((x,y,x+13,y+13),'#ffe091',4)
        poly([(33,86),(82,108),(129,228),(68,192)],'#6f9f56'); poly([(216,76),(172,101),(129,228),(190,189)],'#87b367')
    elif name=='yeast':
        poly([(50,78),(155,37),(211,79),(211,197),(103,226),(50,184)],'#caba91'); poly([(50,78),(155,37),(211,79),(102,119)],'#e4d6b2'); poly([(102,119),(211,79),(211,197),(103,226)],'#d3c49f')
    elif name=='aspic':
        rect((40,82,217,211),'#b79a67',35); ellipse((40,62,217,121),'#d9be83')
        for x,y in [(78,106),(150,118),(112,170),(181,168)]: ellipse((x,y,x+21,y+15),'#d38752'); leaf(133,89,'#7c9a63')
    elif name in ('apple_pie','poppy_seed_cake'):
        cake('#c7a274' if name=='apple_pie' else '#655853','#e4bf80')
        for x,y in [(100,83),(147,77),(177,97)]: ellipse((x,y,x+13,y+7),'#c48052' if name=='apple_pie' else '#564f4d')
    elif name=='challah':
        for i in range(5):
            ellipse((36+i*33,78+(i%2)*25,104+i*33,162+(i%2)*25),'#d5a05b')
            line([(58+i*33,98+(i%2)*25),(70+i*33,111+(i%2)*25)],'#f5d794',5)
    else: raise ValueError('No icon drawing for '+name)
    im.resize((256,256),Image.Resampling.LANCZOS).save(dest/(entry['spriteId']+'.png'))

files=sorted(dest.glob('*.png')); sheet=Image.new('RGB',(8*150,math.ceil(len(files)/8)*170),'#e9e8df')
sd=ImageDraw.Draw(sheet)
for i,p in enumerate(files):
    icon=Image.open(p).resize((128,128)); x=(i%8)*150+11; y=(i//8)*170
    sheet.paste(icon,(x,y),icon); sd.text((x,y+132),p.stem.removeprefix('sprite_')[:23],fill='#263b38')
sheet.save(root/'food-contact-sheet.png'); print('Generated',len(files),'new icons; existing 8 sprites preserved.')
