using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko.Editor {
    public static class CleanInterfaceSetup {
        static readonly Color Ink=new Color(.14f,.2f,.24f),Muted=new Color(.38f,.44f,.49f),Purple=new Color(.43f,.33f,.84f),Paper=new Color(.956f,.969f,.973f),Mint=new Color(.88f,.96f,.92f);
        static Sprite card,button;
        static TMP_FontAsset Font=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset");
        [MenuItem("Lemko/Setup/Apply clean interface")]
        public static void Apply() {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach(string path in new[]{"Assets/Lemko/Sprites/UI/ui_card.png","Assets/Lemko/Sprites/UI/ui_button.png","Assets/Lemko/Sprites/Placeholders/Backgrounds/sprite_shop_clean.png"}) {
                var i=(TextureImporter)AssetImporter.GetAtPath(path); i.textureType=TextureImporterType.Sprite; i.spriteImportMode=SpriteImportMode.Single;
                i.spritePixelsPerUnit=path.Contains("sprite_shop") ? 120 : 64; i.spriteBorder=path.Contains("sprite_shop") ? Vector4.zero : Vector4.one*18;
                i.alphaIsTransparency=true; i.mipmapEnabled=false; i.textureCompression=TextureImporterCompression.Uncompressed; i.SaveAndReimport();
            }
            card=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Lemko/Sprites/UI/ui_card.png"); button=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Lemko/Sprites/UI/ui_button.png");
            StylePrefabs();
            foreach(var path in new[]{SeparateScenesSetup.VillagePath}.Concat(SeparateScenesSetup.Activities.Select(n=>SeparateScenesSetup.Folder+"/"+n+".unity"))) {
                var scene=EditorSceneManager.OpenScene(path); var s=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                var canvas=s.hud.GetComponentInParent<Canvas>();
                foreach(var t in canvas.GetComponentsInChildren<TextMeshProUGUI>(true)) { t.color=Ink; t.fontStyle=FontStyles.Normal; }
                foreach(var b in canvas.GetComponentsInChildren<Button>(true)) StyleButton(b);
                foreach(var input in canvas.GetComponentsInChildren<TMP_InputField>(true)) if(input.targetGraphic is Image im) { im.sprite=card; im.type=Image.Type.Sliced; im.color=Paper; }
                // A simple consistent top bar. Settings remain available in the shared menu.
                var top=canvas.transform.Find("Clean top bar") as RectTransform;
                if(!top) { top=Rect(canvas.transform,"Clean top bar",new Vector2(1280,88),new Vector2(0,316)); top.gameObject.AddComponent<Image>().color=Color.white; top.GetComponent<Image>().raycastTarget=false; top.SetAsFirstSibling(); }
                s.muteButton.gameObject.SetActive(false); s.languageButton.gameObject.SetActive(false);
                s.hud.rectTransform.sizeDelta=new Vector2(790,60); s.hud.rectTransform.anchoredPosition=new Vector2(-210,320); s.hud.alignment=TextAlignmentOptions.MidlineLeft; s.hud.fontSize=21;
                foreach(var b in canvas.GetComponentsInChildren<Button>(true)) {
                    if(b.transform.parent!=canvas.transform) continue;
                    var text=b.GetComponentInChildren<TMP_Text>(); if(!text) continue;
                    if(b.name=="Menu") { b.GetComponent<RectTransform>().anchoredPosition=new Vector2(360,320); b.GetComponent<RectTransform>().sizeDelta=new Vector2(145,46); }
                    if(b.name=="Village") { b.GetComponent<RectTransform>().anchoredPosition=new Vector2(530,320); b.GetComponent<RectTransform>().sizeDelta=new Vector2(155,46); }
                }
                if(!s.sessionProgress) {
                    var rail=Rect(canvas.transform,"Session progress",new Vector2(1210,8),new Vector2(0,274)); var bg=rail.gameObject.AddComponent<Image>(); bg.sprite=card; bg.type=Image.Type.Sliced; bg.color=new Color(.88f,.9f,.93f); bg.raycastTarget=false;
                    var fill=Rect(rail,"Fill",rail.sizeDelta,Vector2.zero).gameObject.AddComponent<Image>(); fill.sprite=card; fill.type=Image.Type.Filled; fill.fillMethod=Image.FillMethod.Horizontal; fill.fillAmount=0; fill.color=Purple; fill.raycastTarget=false; s.sessionProgress=fill;
                }
                if(s.storyRequest) s.storyRequest.gameObject.SetActive(false);
                if(s.answerFeedback) { s.answerFeedback.successColor=new Color(.08f,.48f,.31f); s.answerFeedback.correctionColor=new Color(.67f,.32f,.08f); }
                var tip=s.tooltip.box.GetComponent<Image>(); if(tip) { tip.sprite=card; tip.type=Image.Type.Sliced; tip.color=Color.white; }
                s.tooltip.label.color=Ink;
                if(s.tooltip.waiting) { var cue=s.tooltip.waiting.GetComponentInChildren<TMP_Text>(true); if(cue) { cue.color=Ink; cue.outlineWidth=0; } s.tooltip.progressRing.color=Purple; s.tooltip.progressRing.trackColor=new Color(.8f,.84f,.88f); }
                foreach(var panel in new[]{s.resultsPanel,s.exhaustedPanel}) if(panel) { var image=panel.GetComponent<Image>(); if(image) { image.sprite=card; image.type=Image.Type.Sliced; image.color=Color.white; } }
                if(s.activity) {
                    var bg=s.activity.worldRoot.Find("Editable background"); if(bg) bg.GetComponent<SpriteRenderer>().color=Paper;
                    var panel=s.shopPanel.GetComponent<Image>(); if(panel) { panel.color=Color.clear; panel.raycastTarget=false; }
                    s.activity.prompt.fontSize=28; s.activity.feedback.fontSize=18;
                    s.activity.submit.GetComponentInChildren<TMP_Text>().text="Check answer";
                    if(s.activity.hintControl) { s.activity.hintControl.GetComponentInChildren<TMP_Text>().text="Hold for help"; }
                }
                if(scene.name=="Shop") StyleShop(s);
                if(s.journal) {
                    var image=s.journal.panel.GetComponent<Image>(); image.sprite=card; image.type=Image.Type.Sliced; image.color=Color.white;
                    s.journal.detail.color=Muted;
                }
                foreach(var root in scene.GetRootGameObjects()) foreach(var t in root.GetComponentsInChildren<Transform>(true)) if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0) throw new Exception("Missing script: "+t.name);
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath);
            Debug.Log("LEMKO_CLEAN_UI_SETUP_PASS: nine scenes, shared menu/word prefabs, list hints removed, progress bars and clean palette. No build.");
        }
        static void StyleShop(GameSession s) {
            var shop=s.shopController;
            var bg=shop.worldRoot.Find("Placeholder Shop Background");
            if(bg) { var sr=bg.GetComponent<SpriteRenderer>(); sr.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Lemko/Sprites/Placeholders/Backgrounds/sprite_shop_clean.png"); sr.color=Color.white; bg.localScale=Vector3.one; bg.localPosition=Vector3.zero; }
            for(int i=0;i<shop.productSlots.Length;i++) shop.productSlots[i].localPosition=new Vector3(-6.4f+(i%4)*2.125f,1.067f-(i/4)*1.833f,0);
            shop.basketStart=new Vector3(-6.5f,-3.1f,0); shop.basketSpacing=.94f; shop.basketItemScale=.5f;
            shop.feedback.rectTransform.anchoredPosition=new Vector2(0,260); shop.feedback.rectTransform.sizeDelta=new Vector2(1120,55); shop.feedback.fontSize=28; shop.feedback.text="Choose the products on your list.";
            shop.shoppingList.rectTransform.anchoredPosition=new Vector2(390,185); shop.shoppingList.rectTransform.sizeDelta=new Vector2(430,55); shop.shoppingList.fontSize=25;
            shop.listRoot.anchorMin=shop.listRoot.anchorMax=new Vector2(.5f,.5f); shop.listRoot.pivot=new Vector2(.5f,1); shop.listRoot.anchoredPosition=new Vector2(390,130); shop.listRoot.sizeDelta=new Vector2(415,310);
            var layout=shop.listRoot.GetComponent<VerticalLayoutGroup>(); if(layout) { layout.spacing=10; layout.childControlWidth=true; layout.childControlHeight=true; layout.childForceExpandWidth=true; layout.childForceExpandHeight=false; }
            shop.finishButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(390,-220); shop.finishButton.GetComponent<RectTransform>().sizeDelta=new Vector2(380,56);
            shop.finishButton.image.color=Purple; var action=shop.finishButton.GetComponentInChildren<TMP_Text>(); action.color=Color.white; action.text="Check basket";
            shop.basketStatus.rectTransform.anchoredPosition=new Vector2(-270,-165); shop.basketStatus.rectTransform.sizeDelta=new Vector2(675,40); shop.basketStatus.fontSize=18; shop.basketStatus.color=Muted; shop.basketStatus.alignment=TextAlignmentOptions.MidlineLeft; shop.basketStatus.margin=new Vector4(16,0,0,0);
            var panel=s.shopPanel.GetComponent<Image>(); if(panel) { panel.color=Color.clear; panel.raycastTarget=false; }
        }
        static void StylePrefabs() {
            foreach(var path in new[]{"Assets/Lemko/Prefabs/UI/GameMenu.prefab","Assets/Lemko/Prefabs/Words/ShoppingListRow.prefab","Assets/Lemko/Prefabs/Words/ActivityToken.prefab","Assets/Lemko/Prefabs/Words/WorldWord.prefab"}) {
                var root=PrefabUtility.LoadPrefabContents(path);
                try {
                    if(path.Contains("ShoppingListRow")) {
                        foreach(var hint in root.GetComponentsInChildren<HoldTranslationHint>(true)) if(hint.gameObject!=root) UnityEngine.Object.DestroyImmediate(hint.gameObject);
                        var text=root.GetComponent<TMP_Text>(); text.fontSize=27; text.color=Ink; text.alignment=TextAlignmentOptions.MidlineLeft; text.margin=new Vector4(16,0,12,0); text.raycastTarget=true;
                        var layout=root.GetComponent<LayoutElement>(); if(!layout) layout=root.AddComponent<LayoutElement>(); layout.preferredHeight=52; layout.minHeight=52;
                    }
                    if(path.Contains("GameMenu")) {
                        foreach(var image in root.GetComponentsInChildren<Image>(true)) if(!image.GetComponent<Button>()) {
                            image.sprite=card; image.type=Image.Type.Sliced;
                            image.color=image.name=="Modal input blocker" ? new Color(.1f,.15f,.2f,.55f) : image.name.StartsWith("Slot ") ? Paper : Color.white;
                        }
                        foreach(var text in root.GetComponentsInChildren<TMP_Text>(true)) text.color=Ink;
                        foreach(var b in root.GetComponentsInChildren<Button>(true)) StyleButton(b);
                    }
                    var token=root.GetComponent<ActivityToken>(); if(token) { token.backing.sprite=card; token.backing.color=Color.white; }
                    var word=root.GetComponent<WordView>();
                    if(word && !word.selectionMarker) {
                        var marker=new GameObject("Selection highlight",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); marker.transform.SetParent(root.transform,false); marker.sprite=card; marker.drawMode=SpriteDrawMode.Sliced; marker.size=new Vector2(1.8f,1.6f); marker.color=Mint; marker.sortingOrder=0; marker.enabled=false; word.selectionMarker=marker;
                    }
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                } finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }
        static void StyleButton(Button b) {
            if(!b.image) return; bool primary=b.name.Contains("Submit") || b.name=="Next" || b.name.Contains("Check") || b.name=="Confirm" || b.name=="Load";
            b.image.sprite=button; b.image.type=Image.Type.Sliced; b.image.color=primary ? Purple : Paper;
            var colors=b.colors; colors.normalColor=Color.white; colors.highlightedColor=new Color(.94f,.95f,1); colors.pressedColor=new Color(.82f,.85f,.94f); colors.selectedColor=colors.highlightedColor; colors.disabledColor=new Color(.8f,.82f,.85f,.6f); colors.fadeDuration=.1f; b.colors=colors;
            var t=b.GetComponentInChildren<TMP_Text>(true); if(t) { t.color=primary ? Color.white : Ink; t.fontSize=Mathf.Min(t.fontSize,22); t.enableAutoSizing=true; t.fontSizeMin=14; t.fontSizeMax=22; t.fontStyle=FontStyles.Bold; }
        }
        static RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 pos) { var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false); r.sizeDelta=size; r.anchoredPosition=pos; return r; }
        public static void CheckHints() {
            var ledger=new SessionHintUsage();
            if(!ledger.Register("food_carrot") || ledger.Register("food_carrot") || !ledger.Register("food_cheese") || ledger.Register(null)) throw new Exception("Unique word hint accounting failed");
            ledger.Reset(); if(!ledger.Register("food_carrot")) throw new Exception("New-session hint reset failed");
            Debug.Log("LEMKO_UNIQUE_HINT_DATA_PASS: first hint, repeated ID, distinct word, missing ID and new session.");
        }
    }
}
