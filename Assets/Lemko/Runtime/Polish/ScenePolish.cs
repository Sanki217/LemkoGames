using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace Lemko
{
    // Presentation layer for one scene: recolours the interface to match the illustrations, draws the icon HUD,
    // and reacts to game events with motion. It reads game state but never changes answers or scores.
    public sealed partial class ScenePolish : MonoBehaviour
    {
        GameSession session; RectTransform root; TMP_FontAsset font; bool built;
        LearningActivity Activity=>session.activity;
        ShopController Shop=>session.activity ? null : session.shopController;
        bool InVillage=>session.storyView;

        void Awake() { session=GetComponent<GameSession>(); }
        void OnEnable() { GameEvents.TokenSpawned+=Spawned; GameEvents.TaskStarted+=TaskStarted; GameEvents.Answered+=Answered; GameEvents.RoundFinished+=RoundFinished; }
        void OnDisable() { GameEvents.TokenSpawned-=Spawned; GameEvents.TaskStarted-=TaskStarted; GameEvents.Answered-=Answered; GameEvents.RoundFinished-=RoundFinished; }
        IEnumerator Start()
        {
            // One frame later, so the session has finished opening its first screen (hidden by the scene cover).
            yield return null;
            if(!session || !session.hud || session.Progress==null) yield break;
            root=(RectTransform)session.hud.canvas.rootCanvas.transform; font=session.hud.font;
            Theme(); BuildHud(); BuildFlow();
            if(InVillage) BuildVillage();
            foreach(var button in root.GetComponentsInChildren<Button>(true)) if(!button.GetComponent<JuicyButton>()) button.gameObject.AddComponent<JuicyButton>();
            built=true;
            if(pendingResult!=null) { var r=pendingResult; pendingResult=null; ShowResult(r); }
        }

        // ---------- small builders ----------
        RectTransform Box(Transform parent,string name,Vector2 position,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform)); go.transform.SetParent(parent,false);
            var rect=(RectTransform)go.transform; rect.anchoredPosition=position; rect.sizeDelta=size; return rect;
        }
        Image Picture(Transform parent,string name,Sprite sprite,Color color,Vector2 position,Vector2 size,bool sliced=false)
        {
            var image=Box(parent,name,position,size).gameObject.AddComponent<Image>(); image.sprite=sprite; image.color=color; image.raycastTarget=false;
            if(sliced) image.type=Image.Type.Sliced; return image;
        }
        TextMeshProUGUI Label(Transform parent,string name,string text,float size,Color color,Vector2 position,Vector2 box,bool bold=true)
        {
            var label=Box(parent,name,position,box).gameObject.AddComponent<TextMeshProUGUI>(); label.font=font; label.text=text; label.fontSize=size; label.color=color;
            label.alignment=TextAlignmentOptions.Center; label.fontStyle=bold ? FontStyles.Bold : FontStyles.Normal; label.raycastTarget=false; return label;
        }
        Button PushButton(Transform parent,string text,Vector2 position,Vector2 size,Color color,UnityAction clicked,float textSize=22)
        {
            var image=Picture(parent,text,Look.Button,color,position,size,true); image.raycastTarget=true;
            var button=image.gameObject.AddComponent<Button>(); button.targetGraphic=image; Tint(button); button.onClick.AddListener(clicked);
            Label(image.transform,"Label",text,textSize,Look.Ink,new Vector2(0,2),size-new Vector2(16,8));
            image.gameObject.AddComponent<JuicyButton>(); return button;
        }
        static void Tint(Selectable control)
        {
            var colors=control.colors; colors.normalColor=Color.white; colors.highlightedColor=new Color(.95f,.95f,.95f); colors.pressedColor=new Color(.84f,.84f,.84f);
            colors.selectedColor=Color.white; colors.disabledColor=new Color(.82f,.82f,.82f,.55f); colors.colorMultiplier=1; control.colors=colors;
        }

        // ---------- theme ----------
        static bool Purple(Color c)=>c.b>c.r+.15f && c.b>c.g+.15f;
        static bool Light(Color c)=>Mathf.Min(c.r,Mathf.Min(c.g,c.b))>.8f;
        void Theme()
        {
            if(Camera.main) Camera.main.backgroundColor=InVillage ? Look.Mint : Look.Peach;   // matches the top edge of each backdrop
            foreach(var image in root.GetComponentsInChildren<Image>(true)) {
                string n=image.name; var c=image.color;
                if(n=="Clean top bar") { image.color=Look.Pine; AddAwning(image.rectTransform); continue; }
                if(n=="Illustration" || n=="Picture" || n=="Handle" || c.a<.05f) continue;
                if(n=="Modal input blocker") { image.color=Look.Alpha(Look.Ink,.55f); continue; }
                if(n=="Click or tap anywhere") { image.color=Look.Mint; continue; }
                var control=image.GetComponent<Selectable>();
                if(control is Button) {
                    bool primary=Purple(c) && c.r<.7f;
                    image.sprite=Look.Button; image.type=Image.Type.Sliced; image.color=primary ? Look.Honey : Purple(c) ? Look.Sage : Look.Paper; Tint(control);
                } else if(control is Slider) { image.sprite=Look.Pill; image.type=Image.Type.Sliced; image.color=Look.Sand; }
                else if(control) { image.sprite=Look.Button; image.type=Image.Type.Sliced; image.color=Color.white; }
                else if(Light(c)) { image.sprite=Look.Panel; image.type=Image.Type.Sliced; image.color=Look.Paper; }
            }
            // World props that carry their own colour (the shop's shelf boards) become ink lines.
            foreach(var board in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)) if(board.name.StartsWith("Shelf")) board.color=Look.Ink;
            foreach(var text in root.GetComponentsInChildren<TMP_Text>(true)) {
                var c=text.color;
                text.color=Purple(c) ? Look.Brick : (c.r>.3f && c.r<.5f && c.g>.38f && c.g<.55f) ? Look.Muted : Look.Ink;
            }
            // The text HUD stays in the scene (other code writes to it) but the icon HUD replaces it on screen.
            session.hud.color=Color.clear;
            if(session.tooltip.progressRing) { session.tooltip.progressRing.color=Look.Honey; session.tooltip.progressRing.trackColor=Look.Alpha(Look.Ink,.55f); }
            if(session.sessionProgress && session.sessionProgress.transform.parent!=root) session.sessionProgress.transform.parent.gameObject.SetActive(false);
            if(session.exhaustedPanel) {
                Rename(session.exhaustedPanel.transform,"Message","Out of hearts!\nTake a breath, then carry on.");
                Rename(session.exhaustedPanel.transform,"Fresh attempt","Restart with full hearts");
                Rename(session.exhaustedPanel.transform,"Unlimited practice","Keep going in Learning mode");
            }
        }
        static void Rename(Transform parent,string child,string text)
        {
            var target=parent.Find(child); if(!target) return;
            var label=target.GetComponentInChildren<TMP_Text>(true); if(label) label.text=text;
        }
        void AddAwning(RectTransform bar)
        {
            // The scalloped awning from the Shop backdrop runs under the top bar on every screen.
            var strip=Picture(bar.parent,"Awning trim",Look.Awning,Color.white,bar.anchoredPosition+new Vector2(0,-bar.sizeDelta.y*.5f-11),new Vector2(1296,24));
            strip.type=Image.Type.Tiled; strip.pixelsPerUnitMultiplier=1.16f; strip.rectTransform.SetSiblingIndex(bar.GetSiblingIndex()+1);
        }

        // ---------- icon HUD ----------
        RectTransform hudRow; readonly List<Image> hearts=new List<Image>(), pips=new List<Image>();
        Image heartsPill, pipsPill, scorePill, streakPill, bonusRing, goldenPill; TextMeshProUGUI scoreText, streakText;
        int shownLives=-1, shownPips=-1, shownFilled=-1, shownStreak=-1; float shownScore; int targetScore;
        void BuildHud()
        {
            hudRow=Box(root,"Icon HUD",new Vector2(0,320),new Vector2(1280,60)); hudRow.SetSiblingIndex(session.hud.transform.GetSiblingIndex()+1);
            heartsPill=Picture(hudRow,"Hearts",Look.Pill,Look.Paper,Vector2.zero,new Vector2(160,40),true);
            for(int i=0;i<Mathf.Min(8,session.shop ? session.shop.startingLives : 5);i++) hearts.Add(Picture(heartsPill.transform,"Heart",Look.Heart,Look.Brick,Vector2.zero,new Vector2(24,24)));
            pipsPill=Picture(hudRow,"Progress",Look.Pill,Look.Paper,Vector2.zero,new Vector2(160,40),true);
            for(int i=0;i<12;i++) pips.Add(Picture(pipsPill.transform,"Pip",Look.Dot,Look.Empty,Vector2.zero,new Vector2(18,18)));
            scorePill=Picture(hudRow,"Score",Look.Pill,Look.Paper,Vector2.zero,new Vector2(140,40),true);
            Picture(scorePill.transform,"Star",Look.Star,Look.Honey,new Vector2(-46,0),new Vector2(28,28));
            scoreText=Label(scorePill.transform,"Points","0",22,Look.Ink,new Vector2(14,1),new Vector2(96,34));
            bonusRing=Picture(scorePill.transform,"Quick bonus",Look.Dot,Look.Honey,new Vector2(88,0),new Vector2(26,26));
            bonusRing.type=Image.Type.Filled; bonusRing.fillMethod=Image.FillMethod.Radial360; bonusRing.fillOrigin=(int)Image.Origin360.Top; bonusRing.fillClockwise=false;
            streakPill=Picture(hudRow,"Streak",Look.Pill,Look.Honey,Vector2.zero,new Vector2(74,40),true);
            streakText=Label(streakPill.transform,"Multiplier","x2",22,Look.Ink,new Vector2(0,1),new Vector2(70,34));
            goldenPill=Picture(hudRow,"Golden word",Look.Pill,Look.Honey,Vector2.zero,new Vector2(170,40),true);
            Label(goldenPill.transform,"Label","Golden word x2",17,Look.Ink,new Vector2(0,1),new Vector2(160,34));
            RefreshHud(true);
        }
        void RefreshHud(bool force=false)
        {
            if(!hudRow) return;
            bool playing=Activity ? Activity.Running : Shop && Shop.HasRound;
            int lives=session.Practice ? -1 : session.Lives;
            int total=0, filled=0, points=0, multiplier=1;
            if(Activity && Activity.Running) {
                if(Activity is MemoryController memory) { total=Activity.definition.pairs; filled=memory.PairsFound; if(memory.AssociationActive || memory.AssociationPending) filled=total; }
                else { total=Activity.SessionTaskCount; filled=Activity.Correct; }
                points=Activity.Points; multiplier=Activity.Multiplier;
            } else if(Shop && Shop.HasRound) { total=Shop.TargetCount; filled=Mathf.Min(total,Shop.BasketCount); points=Shop.Points; multiplier=Shop.NextMultiplier; }
            else if(InVillage) { total=LanternCount; filled=Mathf.Min(total,session.Progress.story.contributions.Count); points=SessionStats.TotalStars; }
            total=Mathf.Min(total,pips.Count);
            if(force || lives!=shownLives) {
                if(!force && lives>=0 && lives<shownLives && lives<hearts.Count) { Juice.Shake(heartsPill.transform,8); Juice.Sound("heart"); }
                shownLives=lives; heartsPill.gameObject.SetActive(lives>=0);
                for(int i=0;i<hearts.Count;i++) { hearts[i].color=i<lives ? Look.Brick : Look.Empty; hearts[i].rectTransform.anchoredPosition=new Vector2((i-(hearts.Count-1)*.5f)*27,0); }
                heartsPill.rectTransform.sizeDelta=new Vector2(hearts.Count*27+24,40);
            }
            if(force || total!=shownPips || filled!=shownFilled) {
                bool lantern=InVillage;
                for(int i=0;i<pips.Count;i++) {
                    pips[i].gameObject.SetActive(i<total); if(i>=total) continue;
                    pips[i].sprite=lantern ? Look.Lantern : Look.Dot; pips[i].rectTransform.sizeDelta=lantern ? new Vector2(18,24) : new Vector2(18,18);
                    pips[i].color=i<filled ? (lantern ? Look.Honey : Look.Good) : Look.Empty; pips[i].rectTransform.anchoredPosition=new Vector2((i-(total-1)*.5f)*24,0);
                    if(!force && i<filled && i>=shownFilled) Juice.Punch(pips[i].transform,.6f,.35f);
                }
                shownPips=total; shownFilled=filled; pipsPill.gameObject.SetActive(total>0); pipsPill.rectTransform.sizeDelta=new Vector2(total*24+26,40);
            }
            targetScore=points; if(force || !playing) shownScore=points;
            scorePill.gameObject.SetActive(playing || InVillage);
            bool streak=playing && multiplier>=2;
            if(force || multiplier!=shownStreak) {
                if(!force && streak && multiplier>shownStreak) Juice.Punch(streakPill.transform,.35f,.35f);
                shownStreak=multiplier; streakText.text="x"+multiplier; streakPill.gameObject.SetActive(streak);
            }
            float bonus=Activity ? Activity.QuickBonus : 0; bonusRing.gameObject.SetActive(bonus>0); bonusRing.fillAmount=bonus;
            bool golden=Activity && Activity.GoldenTask && !Activity.Locked; if(golden!=goldenPill.gameObject.activeSelf) { goldenPill.gameObject.SetActive(golden); if(golden) Juice.Pop(goldenPill.transform); }
            // Lay the visible pills out from the left edge.
            float x=-622;
            foreach(var pill in new[]{heartsPill,pipsPill,scorePill,streakPill,goldenPill}) {
                if(!pill.gameObject.activeSelf) continue;
                float w=pill.rectTransform.sizeDelta.x+(pill==scorePill && bonusRing.gameObject.activeSelf ? 30 : 0);
                pill.rectTransform.anchoredPosition=new Vector2(x+pill.rectTransform.sizeDelta.x*.5f,0); x+=w+10;
            }
        }
        void LateUpdate()
        {
            if(!built || !session || session.Progress==null) return;
            RefreshHud();
            if(!Mathf.Approximately(shownScore,targetScore)) shownScore=GameSession.ReducedMotion ? targetScore : Mathf.MoveTowards(shownScore,targetScore,Mathf.Max(60,Mathf.Abs(targetScore-shownScore)*6)*Time.unscaledDeltaTime);
            string score=Mathf.RoundToInt(shownScore).ToString(); if(scoreText.text!=score) scoreText.text=score;
            RefreshFlow();
            if(InVillage) RefreshVillage();
        }

        // ---------- task flow ----------
        TMP_Text headline; Image headlineCard; Button hintButton; bool submitLive;
        void BuildFlow()
        {
            headline=Activity ? Activity.prompt : Shop ? Shop.feedback : null;
            if(headline) {
                var rect=headline.rectTransform;
                headlineCard=Picture(rect.parent,"Prompt card",Look.Button,Look.Paper,rect.anchoredPosition,rect.sizeDelta,true);
                headlineCard.rectTransform.SetSiblingIndex(rect.GetSiblingIndex());
            }
            if(Activity && !(Activity is MemoryController) && Activity.speak) {
                var speak=(RectTransform)Activity.speak.transform;
                hintButton=PushButton(speak.parent,"Hint",speak.anchoredPosition+new Vector2(speak.sizeDelta.x*.5f+75,0),new Vector2(130,speak.sizeDelta.y),Look.Sage,ShowHint,20);
                var label=hintButton.GetComponentInChildren<TMP_Text>(); label.rectTransform.anchoredPosition=new Vector2(14,2);
                Picture(hintButton.transform,"Bulb",Look.Bulb,Look.Honey,new Vector2(-38,1),new Vector2(28,28));
            }
        }
        void ShowHint()
        {
            if(!Activity || !Activity.prompt) return;
            session.HintNow(this,Juice.UiToScreen(Activity.prompt.rectTransform)-new Vector2(0,Screen.height*.2f));
            StopCoroutine(nameof(HideHint)); StartCoroutine(nameof(HideHint));
        }
        IEnumerator HideHint() { yield return new WaitForSecondsRealtime(2.8f); if(session && session.tooltip) session.tooltip.Hide(this); }
        void RefreshFlow()
        {
            if(headlineCard) {
                bool show=headline.gameObject.activeInHierarchy && !string.IsNullOrEmpty(headline.text);
                if(headlineCard.gameObject.activeSelf!=show) headlineCard.gameObject.SetActive(show);
                if(show) {
                    var box=headline.rectTransform.sizeDelta;
                    headlineCard.rectTransform.sizeDelta=new Vector2(Mathf.Clamp(headline.preferredWidth+64,220,box.x+40),Mathf.Clamp(headline.preferredHeight+20,54,130));
                    headlineCard.rectTransform.anchoredPosition=headline.rectTransform.anchoredPosition;
                }
            }
            if(hintButton) { bool show=Activity.Running && !Activity.Locked && Activity.Current!=null; if(hintButton.gameObject.activeSelf!=show) hintButton.gameObject.SetActive(show); }
            // The Check button breathes gently once there is something to check.
            var submit=Activity ? Activity.submit : Shop ? Shop.finishButton : null;
            bool live=submit && submit.gameObject.activeInHierarchy && submit.interactable && (Activity || Shop.BasketCount>0);
            if(live!=submitLive && submit) { submitLive=live; if(live) Juice.Breathe(submit.transform,.05f,1.2f); else Juice.Calm(submit.transform); }
        }

        // ---------- reactions ----------
        int spawnFrame=-1, spawnIndex;
        void Spawned(ActivityToken token)
        {
            if(!token) return;
            if(Time.frameCount!=spawnFrame) { spawnFrame=Time.frameCount; spawnIndex=0; }
            Juice.Pop(token.transform,Mathf.Min(spawnIndex++,14)*.03f);
        }
        void TaskStarted() { if(built && headline) Juice.Punch(headline.transform,.05f,.25f); }
        void Answered(bool correct,int awarded,int streak)
        {
            if(!built) return;
            var at=Juice.PointerScreen;
            if(!correct) { if(headlineCard) Juice.Shake(headlineCard.transform,9); if(headline) Juice.Shake(headline.transform,9); Juice.Sound("wrong"); return; }
            Juice.Sparkle(at,14); Juice.Sound("correct",1+Mathf.Min(streak,8)*.06f);
            if(awarded>0 && scorePill.gameObject.activeInHierarchy)
                Juice.FloatText("+"+awarded,at,Look.Honey,font,Juice.UiToScreen(scorePill.rectTransform),()=> { if(scorePill) Juice.Punch(scorePill.transform,.22f,.25f); });
            if(streak==5) Juice.FloatText("On a roll!",new Vector2(Screen.width*.5f,Screen.height*.6f),Look.Paper,font);
        }
    }
}
