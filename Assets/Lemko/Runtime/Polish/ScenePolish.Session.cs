using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko
{
    // The shape of a sitting: results card, village lanterns and signs, title screen and finale.
    public sealed partial class ScenePolish
    {
        static string Pretty(string activity)=>activity=="FamilyTree" ? "Family Tree" : activity;
        const int LanternCount=SessionStats.Places;

        // ---------- results card ----------
        RoundResult pendingResult; RectTransform resultCard;
        void RoundFinished(RoundResult result) { if(built) ShowResult(result); else pendingResult=result; }
        void ShowResult(RoundResult result)
        {
            var panel=session.resultsPanel; if(!panel || !panel.activeInHierarchy) return;
            // The original text blocks keep their content for other tools, but the card below is what the player sees.
            if(session.results) session.results.color=Color.clear;
            if(session.wordRecap) session.wordRecap.color=Color.clear;
            if(resultCard) Destroy(resultCard.gameObject);
            resultCard=Box(panel.transform,"Result card",new Vector2(110,40),new Vector2(780,400));
            Label(resultCard,"Title",result.complete ? Pretty(result.activity)+" complete!" : "Good try!",40,Look.Ink,new Vector2(0,170),new Vector2(760,56));
            for(int i=0;i<3;i++) {
                bool earned=i<result.stars;
                var shape=Look.StarOf(i==1 ? 44 : 36);   // big stars drawn at their own size, the middle one a little bigger
                var star=Picture(resultCard,"Star",shape,earned ? Look.Honey : Look.Empty,new Vector2((i-1)*92,92),Pixels.UiSize(shape));
                if(earned) { Juice.Pop(star.transform,.35f+i*.3f); int index=i; StartCoroutine(StarLands(star,.5f+i*.3f,index)); }
            }
            var points=Label(resultCard,"Points","0 points",34,Look.Ink,new Vector2(0,-4),new Vector2(760,46));
            StartCoroutine(CountUp(points,result.points));
            string stats=result.correct+" correct   ·   "+result.mistakes+(result.mistakes==1 ? " mistake" : " mistakes")+"   ·   "+result.hints+(result.hints==1 ? " hint" : " hints")
                ;
            Label(resultCard,"Stats",stats,18,Look.Muted,new Vector2(0,-42),new Vector2(770,26),false);
            string note=(result.contribution??"").Split('\n')[0];
            if(note.Length>0) Label(resultCard,"Contribution",note,19,Look.Good,new Vector2(0,-74),new Vector2(770,28));
            if(result.revisit.Count>0) {
                Label(resultCard,"Revisit label","Practise next:",18,Look.Ink,new Vector2(-300,-118),new Vector2(160,30));
                for(int i=0;i<result.revisit.Count;i++) {
                    var word=result.revisit[i]; Button chip=null;
                    chip=PushButton(resultCard,word.lemko,new Vector2(-110+i*200,-118),new Vector2(186,44),Look.Peach,()=> {
                        session.audioService.Play(word.pronunciation);
                        session.tooltip.RevealNow(this,word,Juice.UiToScreen((RectTransform)chip.transform)); StopCoroutine(nameof(HideHint)); StartCoroutine(nameof(HideHint));
                    },20);
                    var label=chip.GetComponentInChildren<TMP_Text>(); label.enableAutoSizing=true; label.fontSizeMin=12; label.fontSizeMax=20;
                    Juice.Pop(chip.transform,1.3f+i*.1f);
                }
            }
            // Existing buttons: the suggested next place becomes the obvious next step.
            if(session.nextPlaceButton && session.nextPlaceButton.gameObject.activeSelf) Juice.Breathe(session.nextPlaceButton.transform,.05f,1.2f);
            if(result.complete) { Juice.Confetti(); Juice.Sound("fanfare"); }
        }
        IEnumerator StarLands(Image star,float delay,int index)
        {
            yield return new WaitForSecondsRealtime(delay);
            if(!star) yield break;
            Juice.Sparkle(Juice.UiToScreen(star.rectTransform),10); Juice.Sound("star",1+index*.12f);
        }
        IEnumerator CountUp(TMP_Text label,int target)
        {
            if(!GameSession.ReducedMotion) for(float t=0;t<1;t+=Time.unscaledDeltaTime/.9f) { if(!label) yield break; label.text=Mathf.RoundToInt(target*(1-(1-t)*(1-t)))+" points"; yield return null; }
            if(label) { label.text=target+" points"; Juice.Punch(label.transform,.12f,.25f); }
        }

        // ---------- village ----------
        readonly Dictionary<VillageHotspot,SpriteRenderer[]> houseStars=new Dictionary<VillageHotspot,SpriteRenderer[]>();
        TMP_Text journalLabel; int shownWords=-1; VillageHotspot bobbing;
        // Where each place stands on the painted village map (world units; the map fills the 16 x 9 view): the middle
        // and size of the building, which is what the player clicks, and the patch of bare ground in front of it,
        // which carries the name tag, the stars and the highlight. The finished gift stands beside the tag.
        static readonly Dictionary<string,(Vector2 at,Vector2 size,Vector2 tag)> Places=new Dictionary<string,(Vector2,Vector2,Vector2)> {
            {"Shop",(new Vector2(-4.94f,1.82f),new Vector2(2.2f,1.5f),new Vector2(-4.84f,.91f))},
            {"FamilyTree",(new Vector2(-1.4f,2.9f),new Vector2(2.8f,2.3f),new Vector2(-1.01f,1.49f))},
            {"Weather",(new Vector2(4.63f,2.8f),new Vector2(1.6f,1.7f),new Vector2(4.54f,1.8f))},
            {"Memory",(new Vector2(.04f,.08f),new Vector2(3.6f,1.8f),new Vector2(.04f,-1.14f))},
            {"Farm",(new Vector2(5.35f,.15f),new Vector2(3.1f,1.6f),new Vector2(5.49f,-1.03f))},
            {"Alphabet",(new Vector2(-5.37f,-.62f),new Vector2(1.9f,1.6f),new Vector2(-5.13f,-1.76f))},
            {"Numbers",(new Vector2(-2.93f,-2.58f),new Vector2(3f,1.3f),new Vector2(-3.12f,-3.66f))},
            {"House",(new Vector2(2.75f,-2.34f),new Vector2(3.6f,1.6f),new Vector2(2.67f,-3.44f))},
        };
        void PlaceVillage()
        {
            var gifts=new Dictionary<string,Transform>();
            foreach(var r in FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                if(r.name.StartsWith("Contribution ")) gifts[r.name.Substring(13)]=r.transform;
            foreach(var spot in session.storyView.worldLocations ?? new VillageHotspot[0]) {
                if(!spot || !Places.TryGetValue(spot.activityId,out var place)) continue;
                var t=spot.transform; t.position=new Vector3(place.at.x,place.at.y,t.position.z); t.localScale=Vector3.one; t.localRotation=Quaternion.identity;
                if(spot.TryGetComponent<BoxCollider2D>(out var box)) { box.offset=Vector2.zero; box.size=place.size; }
                var tag=place.tag-place.at;
                if(spot.label) spot.label.transform.localPosition=new Vector3(tag.x,tag.y,spot.label.transform.localPosition.z);
                if(spot.highlight) {
                    // A soft ring on the ground around the name tag.
                    spot.highlight.sprite=Look.Ring; var one=Look.Ring.bounds.size;
                    spot.highlight.transform.localPosition=new Vector3(tag.x,tag.y-.12f,spot.highlight.transform.localPosition.z);
                    spot.highlight.transform.localScale=new Vector3(2.7f/one.x,1.05f/one.y,1); spot.highlight.transform.localRotation=Quaternion.identity;
                }
                if(gifts.TryGetValue(spot.activityId,out var gift)) { gift.position=new Vector3(place.tag.x+1.3f,place.tag.y+.15f,gift.position.z); gift.localScale=Vector3.one; }
            }
        }
        void BuildVillage()
        {
            PlaceVillage();
            foreach(var spot in session.storyView.worldLocations ?? new VillageHotspot[0]) {
                if(!spot || !spot.label) continue;
                // One small name tag under each building, and up to three stars earned there this session.
                var label=spot.label; label.color=Look.Ink; label.fontSize*=.82f; label.textWrappingMode=TextWrappingModes.NoWrap; label.alignment=TextAlignmentOptions.Center;
                float scale=label.transform.localScale.x; var wanted=label.GetPreferredValues(Lang.T(label.text))*scale;
                var sign=new GameObject("Name tag",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); sign.transform.SetParent(label.transform.parent,false);
                sign.transform.localPosition=Pixels.Snap(label.transform.localPosition+new Vector3(0,0,.01f));
                Pixels.Card(sign,new Vector2(wanted.x+.5f,Mathf.Max(.5f,wanted.y+.2f)),Look.Chip); sign.color=Look.Paper; sign.sortingOrder=-1;
                var stars=new SpriteRenderer[3]; int earned=SessionStats.StarsFor(spot.activityId);
                for(int i=0;i<3;i++) {
                    var star=new GameObject("House star",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); star.transform.SetParent(spot.transform,false);
                    star.transform.localPosition=Pixels.Snap(label.transform.localPosition+new Vector3((i-1)*.375f,-.45f,0));   // 14 pixel stars, a pixel apart
                    star.sprite=Look.Star; star.color=Look.Honey; star.sortingOrder=5; star.enabled=i<earned; stars[i]=star;
                }
                houseStars[spot]=stars;
            }
            // The map speaks for itself: the long texts go, and the two village buttons sit in the bottom corners.
            var view=session.storyView;
            foreach(var text in new[]{view.title,view.introduction,view.status,session.modeDescription,session.villageStatus}) if(text) text.gameObject.SetActive(false);
            foreach(var button in root.GetComponentsInChildren<Button>(true)) if(button.name=="Village") button.gameObject.SetActive(false);
            if(session.modeButton) Corner((RectTransform)session.modeButton.transform,new Vector2(516,-318));
            if(session.villagePanel) {
                var journal=session.villagePanel.transform.Find("Word journal");
                if(journal) { journalLabel=journal.GetComponentInChildren<TMP_Text>(true); Corner((RectTransform)journal,new Vector2(-516,-318)); }
            }
            int lit=Mathf.Min(LanternCount,session.Progress.story.contributions.Count);
            if(lit>SessionStats.LanternsShown) StartCoroutine(LightLanterns(SessionStats.LanternsShown,lit));
            SessionStats.LanternsShown=lit;
            if(!SessionStats.VisitStarted) ShowTitle();
            else if(SessionStats.GoalReached(session.Progress) && !SessionStats.FinaleShown) StartCoroutine(FinaleSoon());
            else if(SessionStats.FinaleShown) Festival();
        }
        static void Corner(RectTransform rect,Vector2 position)
        {
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f); rect.sizeDelta=new Vector2(216,52);
            rect.position=rect.GetComponentInParent<Canvas>().rootCanvas.transform.TransformPoint(position);
            var label=rect.GetComponentInChildren<TMP_Text>(true); if(label) { label.enableAutoSizing=true; label.fontSizeMin=12; label.fontSizeMax=20; label.textWrappingMode=TextWrappingModes.NoWrap; }
        }
        IEnumerator LightLanterns(int from,int to)
        {
            yield return new WaitForSecondsRealtime(.7f);
            for(int i=from;i<to && i<pips.Count;i++) {
                if(!pips[i]) yield break;
                Juice.Punch(pips[i].transform,1.1f,.5f); Juice.Sparkle(Juice.UiToScreen(pips[i].rectTransform),10); Juice.Sound("star",1+i*.06f);
                yield return new WaitForSecondsRealtime(.3f);
            }
        }
        void RefreshVillage()
        {
            int words=session.Progress.encountered.Count;
            if(journalLabel && words!=shownWords) { shownWords=words; journalLabel.text="Journal · "+words+(words==1 ? " word" : " words"); }
            // The suggested next place bobs so the eye finds it.
            VillageHotspot next=null; foreach(var spot in houseStars.Keys) if(spot && spot.Recommended) next=spot;
            if(next!=bobbing) {
                if(bobbing && bobbing.highlight) Juice.Calm(bobbing.highlight.transform);
                bobbing=next; if(bobbing && bobbing.highlight) { Juice.Bob(bobbing.highlight.transform,.06f,1.4f); Juice.Breathe(bobbing.highlight.transform,.06f,1.4f); }
            }
        }
        void Festival()
        {
            var subtitle=session.villagePanel ? session.villagePanel.transform.Find("Subtitle") : null;
            if(subtitle && subtitle.TryGetComponent<TMP_Text>(out var text)) text.text="Festival time! Replay any place and beat your best.";
        }

        // ---------- title screen ----------
        RectTransform Overlay(string name)
        {
            var overlay=Picture(root,name,Look.Square,Look.Alpha(Look.Ink,.6f),Vector2.zero,Vector2.zero); overlay.raycastTarget=true;
            var rect=overlay.rectTransform; rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=rect.offsetMax=Vector2.zero; rect.SetAsLastSibling();
            var content=Box(rect,"Content",Vector2.zero,new Vector2(1280,720));
            Picture(content,"Card",Look.Panel,Look.Peach,Vector2.zero,new Vector2(1180,660),true);
            return content;
        }
        void ShowTitle()
        {
            var content=Overlay("Title screen");
            var title=Label(content,"Title","Lemko Village",76,Look.Ink,new Vector2(0,190),new Vector2(1100,100)); Juice.Pop(title.transform,.15f);
            Label(content,"Subtitle","Learn Lemko words while the village gets ready for its gathering.",24,Look.Muted,new Vector2(0,118),new Vector2(1000,40),false);
            var row=Box(content,"Places",new Vector2(0,20),new Vector2(900,90));
            var contributions=session.story ? session.story.contributions : null; int shown=0;
            if(contributions!=null) foreach(var c in contributions) if(c!=null && shown<LanternCount) {
                var position=new Vector2((shown-3.5f)*104,0);
                var icon=c.illustration ? Picture(row,"Place",c.illustration,Color.white,position,Pixels.UiSize(c.illustration))
                    : Picture(row,"Place",Look.Button,Look.Paper,position,new Vector2(76,80),true);
                if(!c.illustration) Label(icon.transform,"Letter","А",44,Look.Ink,Vector2.zero,new Vector2(70,74));
                Juice.Pop(icon.transform,.3f+shown*.06f); shown++;
            }
            Choice(content,"Learning challenge","8 places · limited hearts",new Vector2(-190,-130),Look.Honey,true);
            Choice(content,"Relaxed experience","8 places · unlimited tries",new Vector2(190,-130),Look.Paper,false);
            // The language can be chosen before anything else; the same switch is in the menu.
            Button language=null;
            language=PushButton(content,LanguageName(),new Vector2(430,-270),new Vector2(250,48),Look.Paper,()=> { session.ToggleLanguage(); language.GetComponentInChildren<TMP_Text>().text=LanguageName(); },18);
        }
        string LanguageName()=>session.Progress.english ? "Language: English" : "Language: Polish";
        void Choice(RectTransform content,string name,string detail,Vector2 position,Color color,bool challenge)
        {
            var button=PushButton(content,name,position,new Vector2(340,110),color,()=> {
                session.SetChallengeMode(challenge);
                SessionStats.VisitStarted=true; SessionStats.StartedAt=Time.realtimeSinceStartup; Juice.Sound("fanfare");
                Destroy(content.parent.gameObject); RefreshHud(true);
            },30);
            button.GetComponentInChildren<TMP_Text>().rectTransform.anchoredPosition=new Vector2(0,18);
            Label(button.transform,"Detail",detail,17,Look.Ink,new Vector2(0,-24),new Vector2(320,26),false);
            Juice.Pop(button.transform,.55f);
        }

        // ---------- finale ----------
        IEnumerator FinaleSoon() { yield return new WaitForSecondsRealtime(1.6f); if(session && !SessionStats.FinaleShown) ShowFinale(); }
        void ShowFinale()
        {
            SessionStats.FinaleShown=true; if(session.journal) session.journal.Close(); if(session.menu) session.menu.Close();
            var content=Overlay("Finale");
            var title=Label(content,"Title","The gathering is ready!",58,Look.Ink,new Vector2(0,250),new Vector2(1100,80)); Juice.Pop(title.transform,.1f);
            // Guests arrive, then everything the player prepared lands on the table.
            var database=session.journal ? session.journal.database : null; int guest=0;
            if(database) foreach(var word in database.entries) {
                if(!word || !word.sprite || word.tags==null || !word.tags.Contains("family") || guest>=7) continue;
                var face=Picture(content,"Guest",word.sprite,Color.white,new Vector2((guest-3)*118,150),Pixels.UiSize(word.sprite));
                Juice.Pop(face.transform,.4f+guest*.1f); Juice.Hop(face.transform,18,1.6f+guest*.12f); guest++;
            }
            Picture(content,"Table",Look.Button,Look.Sand,new Vector2(0,30),new Vector2(980,70),true);
            int gift=0; var done=session.Progress.story.contributions;
            if(session.story) foreach(var id in done) {
                var c=session.story.Find(id); if(c==null || !c.illustration) continue;
                var item=Picture(content,"Contribution",c.illustration,Color.white,new Vector2(Pixels.SnapUi((gift-(done.Count-1)*.5f)*112),78),Pixels.UiSize(c.illustration));
                Juice.Pop(item.transform,1.1f+gift*.14f); gift++;
            }
            int minutes=Mathf.Max(1,Mathf.RoundToInt((Time.realtimeSinceStartup-SessionStats.StartedAt)/60f));
            int places=done.Count, words=session.Progress.encountered.Count;
            Summary(content,-360,SessionStats.TotalStars+" / "+places*3,"stars");
            Summary(content,-120,words.ToString(),words==1 ? "word met" : "words met");
            Summary(content,120,SessionStats.BestStreak.ToString(),"best streak");
            Summary(content,360,minutes.ToString(),minutes==1 ? "minute" : "minutes");
            PushButton(content,"Keep playing",new Vector2(-170,-270),new Vector2(300,70),Look.Paper,()=> { Destroy(content.parent.gameObject); Festival(); },24);
            PushButton(content,"New game",new Vector2(170,-270),new Vector2(300,70),Look.Honey,()=>session.NewSession(),24);
            StartCoroutine(Celebrate());
        }
        void Summary(RectTransform content,float x,string value,string caption)
        {
            var card=Picture(content,"Summary",Look.Button,Look.Paper,new Vector2(x,-120),new Vector2(210,130),true);
            Label(card.transform,"Value",value,44,Look.Ink,new Vector2(0,18),new Vector2(200,56)); Label(card.transform,"Caption",caption,19,Look.Muted,new Vector2(0,-30),new Vector2(200,28),false);
            Juice.Pop(card.transform,2f+(x+360)/240f*.15f);
        }
        IEnumerator Celebrate()
        {
            Juice.Sound("fanfare");
            for(int i=0;i<4;i++) { Juice.Confetti(); yield return new WaitForSecondsRealtime(.9f); }
        }
    }
}
