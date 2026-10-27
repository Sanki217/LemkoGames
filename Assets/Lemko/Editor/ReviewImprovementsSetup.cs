using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko.Editor
{
    public static class ReviewImprovementsSetup
    {
        const string Art="Assets/Lemko/Sprites/Placeholders/Village/";
        static TMP_FontAsset Font=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset");
        [MenuItem("Lemko/Setup/Apply reviewed interaction and village improvements")]
        public static void Apply() {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach(var path in Directory.GetFiles(Art,"*.png")) {
                var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
                importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=120; importer.alphaIsTransparency=true; importer.mipmapEnabled=false;
                importer.textureCompression=TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            }
            foreach(var path in new[]{SeparateScenesSetup.VillagePath}.Concat(SeparateScenesSetup.Activities.Select(n=>SeparateScenesSetup.Folder+"/"+n+".unity"))) {
                var scene=EditorSceneManager.OpenScene(path); var s=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if(!s) throw new Exception("Session missing in "+path);
                if(!s.tooltip.waiting) {
                    var wait=Rect(s.tooltip.box.parent,"Hint wait indicator",new Vector2(120,74),Vector2.zero);
                    var cg=wait.gameObject.AddComponent<CanvasGroup>(); cg.blocksRaycasts=false; cg.interactable=false;
                    var ring=Rect(wait,"Three second progress",new Vector2(36,36),new Vector2(0,12)).gameObject.AddComponent<HintProgressRing>();
                    ring.color=new Color(1,.84f,.4f); ring.raycastTarget=false;
                    var cue=Text(wait,"Hold cue",new Vector2(120,24),new Vector2(0,-19),16); cue.text="Hold for hint";
                    cue.outlineWidth=.25f; cue.outlineColor=new Color32(20,40,35,255);
                    s.tooltip.waiting=wait; s.tooltip.progressRing=ring; wait.gameObject.SetActive(false);
                }
                if(s.activity is HouseController house && !house.requestedRoomLabel) {
                    house.prompt.rectTransform.sizeDelta=new Vector2(550,55); house.prompt.rectTransform.anchoredPosition=new Vector2(-300,225);
                    house.requestedRoomLabel=Text(house.prompt.transform.parent,"Requested room hint",new Vector2(550,55),new Vector2(300,225),25);
                    house.requestedRoomLabel.raycastTarget=true; house.requestedRoomLabel.gameObject.AddComponent<HintPointer>();
                    // A padded graphic gives the entire room heading a dependable mouse/touch target.
                    var hit=Rect(house.roomLabel.transform,"Room hint hit area",house.roomLabel.rectTransform.sizeDelta,Vector2.zero).gameObject.AddComponent<Image>();
                    hit.color=Color.clear; hit.raycastTarget=true; house.roomLabel.raycastTarget=true;
                    house.roomLabel.transform.SetAsLastSibling();
                    house.furnitureScale=1; house.placementArea=new Rect(-1.6f,-1.85f,8,3.6f);
                }
                if(scene.name=="Village") MakeVillage(s);
                foreach(var root in scene.GetRootGameObjects()) foreach(var t in root.GetComponentsInChildren<Transform>(true))
                    if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0) throw new Exception("Missing script: "+path+" / "+t.name);
                if(!s.tooltip.waiting || !s.tooltip.progressRing) throw new Exception("Hint indicator not wired: "+path);
                if(s.activity is HouseController h && (!h.requestedRoomLabel || !h.roomLabel.GetComponent<HintPointer>())) throw new Exception("House hint wiring incomplete");
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath);
            Debug.Log("LEMKO_REVIEW_IMPROVEMENTS_WIRING_PASS: nine scenes; hint indicators, House hints, eight world map destinations. No player builds or simulations.");
        }
        static void MakeVillage(GameSession s) {
            var view=s.storyView;
            if(view.worldLocations!=null && view.worldLocations.Length==8) return; // Preserve later hand edits.
            foreach(var b in view.locations) if(b) b.gameObject.SetActive(false);
            var panel=s.villagePanel.GetComponent<Image>(); if(panel) { panel.color=Color.clear; panel.raycastTarget=false; }
            view.title.rectTransform.anchoredPosition=new Vector2(0,285); view.title.fontSize=29; view.title.color=new Color(.12f,.24f,.2f);
            view.introduction.rectTransform.anchoredPosition=new Vector2(0,251); view.introduction.rectTransform.sizeDelta=new Vector2(1100,30); view.introduction.fontSize=17; view.introduction.color=view.title.color;
            view.status.rectTransform.anchoredPosition=new Vector2(-260,-257); view.status.rectTransform.sizeDelta=new Vector2(680,58); view.status.fontSize=16; view.status.color=view.title.color;
            s.modeButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(463,-237);
            s.modeDescription.rectTransform.anchoredPosition=new Vector2(420,-275); s.modeDescription.rectTransform.sizeDelta=new Vector2(400,35); s.modeDescription.fontSize=15;
            view.goldChoice.GetComponent<RectTransform>().anchoredPosition=new Vector2(-140,-205);
            view.blueChoice.GetComponent<RectTransform>().anchoredPosition=new Vector2(140,-205);
            var old=GameObject.Find("Placeholder Village Background");
            if(old) old.SetActive(false);
            var map=new GameObject("Illustrated village map",typeof(SpriteRenderer));
            var renderer=map.GetComponent<SpriteRenderer>(); renderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"sprite_village_map.png"); renderer.sortingOrder=-20;
            map.transform.position=new Vector3(0,-.7f,0); map.transform.localScale=new Vector3(1,.8f,1);
            var places=new GameObject("Village destinations - move colliders and labels here").transform;
            Vector2[] pixels={new Vector2(350,400),new Vector2(1480,440),new Vector2(600,290),new Vector2(1230,270),new Vector2(420,740),new Vector2(790,730),new Vector2(1310,740),new Vector2(1630,700)};
            view.worldLocations=new VillageHotspot[8];
            for(int i=0;i<8;i++) {
                string id=VillageStoryProgress.Activities[i]; var p=pixels[i];
                var go=new GameObject(id+" destination",typeof(BoxCollider2D),typeof(VillageHotspot)); go.transform.SetParent(places,false);
                go.transform.position=new Vector3(p.x/120-8,(4.5f-p.y/120)*.8f-.7f,0);
                var collider=go.GetComponent<BoxCollider2D>(); collider.size=new Vector2(2.1f,1.85f); collider.offset=new Vector2(.2f,.4f);
                var hotspot=go.GetComponent<VillageHotspot>(); hotspot.session=s; hotspot.activityId=id; view.worldLocations[i]=hotspot;
                var label=new GameObject("Location name",typeof(TextMeshPro)).GetComponent<TextMeshPro>(); label.transform.SetParent(go.transform,false); label.transform.localPosition=new Vector3(.2f,-.78f,0);
                label.font=Font; label.fontSize=2.5f; label.alignment=TextAlignmentOptions.Center; label.rectTransform.sizeDelta=new Vector2(2.8f,.65f); label.color=new Color(.1f,.23f,.18f); label.GetComponent<MeshRenderer>().sortingOrder=10; label.text=id; hotspot.label=label;
                var ring=new GameObject("Ready and hover highlight",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); ring.transform.SetParent(go.transform,false); ring.transform.localPosition=new Vector3(.2f,-.3f,0); ring.transform.localScale=new Vector3(2,1.3f,1); ring.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"sprite_location_ring.png"); ring.sortingOrder=4; ring.enabled=false; hotspot.highlight=ring;
                if(i<view.contributions.Length && view.contributions[i]) { var r=view.contributions[i]; r.transform.position=go.transform.position+new Vector3(-.72f,-.15f,0); r.transform.localScale*=.7f; }
            }
            if(view.tablecloth) { view.tablecloth.transform.position=new Vector3(-.08f,-.7f,0); view.tablecloth.transform.localScale=new Vector3(1.05f,.25f,1); }
            if(view.worldLocations.Any(h=>!h || !h.label || !h.highlight || !h.GetComponent<Collider2D>())) throw new Exception("Village destinations incomplete");
        }
        static RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 pos) {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent,false); rect.sizeDelta=size; rect.anchoredPosition=pos; return rect;
        }
        static TMP_Text Text(Transform parent,string name,Vector2 size,Vector2 pos,int fontSize) {
            var text=Rect(parent,name,size,pos).gameObject.AddComponent<TextMeshProUGUI>(); text.font=Font; text.fontSize=fontSize; text.color=Color.white; text.alignment=TextAlignmentOptions.Center; text.raycastTarget=false; return text;
        }
    }
}
