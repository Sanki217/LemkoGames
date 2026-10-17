using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
namespace Lemko.Editor
{
    [InitializeOnLoad]
    public static class HintPlayChecks
    {
        const string Running="Lemko.HintChecks.Running";
        static double next;
        static HintPointer row,product;
        static WordView view;
        static PointerEventData mouse;
        static ExtendedPointerEventData touch;
        static HintPlayChecks() { EditorApplication.update+=Tick; }
        public static void RunBatch()
        {
            if(!Environment.GetCommandLineArgs().Contains("-lemkoSmoke")) throw new Exception("Use -lemkoSmoke for separate test saves");
            EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath,OpenSceneMode.Single);
            SessionState.SetBool(Running,true); SessionState.SetInt("Lemko.HintStep",0);
            SessionState.SetFloat("Lemko.HintDeadline",(float)EditorApplication.timeSinceStartup+120);
            EditorApplication.EnterPlaymode();
        }
        static void Check(bool condition,string description) { if(!condition) throw new Exception(description); }
        static void Tick()
        {
            if(!SessionState.GetBool(Running,false)) return;
            try {
                if(EditorApplication.timeSinceStartup>SessionState.GetFloat("Lemko.HintDeadline",0)) throw new Exception("Hint checks timed out");
                if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<next) return;
                next=EditorApplication.timeSinceStartup+.7;
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>(); if(!session || session.Progress==null) return;
                int step=SessionState.GetInt("Lemko.HintStep",0);
                if(step==0) session.OpenShop();
                else if(step==1) {
                    var views=session.shopController.worldRoot.GetComponentsInChildren<WordView>();
                    Check(views.Length==8 && views.All(v=>v.worldPicture.sprite && (!v.label || !v.label.gameObject.activeSelf)),"pictures present and names hidden");
                    Check(session.shopController.listRoot.childCount==5,"five individual list rows");
                    Check(session.tooltip.holdSeconds==3,"three second configuration");
                    row=session.shopController.listRoot.GetComponentInChildren<HintPointer>(); view=views[0]; product=view.GetComponent<HintPointer>();
                    Canvas.ForceUpdateCanvases();
                    mouse=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,row.transform.position)};
                    var hits=new List<RaycastResult>(); EventSystem.current.RaycastAll(mouse,hits);
                    Check(hits.Count>0 && hits[0].gameObject==row.gameObject,"list word receives pointer raycasts");
                    row.OnPointerEnter(mouse); Check(!session.tooltip.box.gameObject.activeSelf && session.shopController.HintsUsed==0,"no instant hint");
                    next=EditorApplication.timeSinceStartup+2;
                }
                else if(step==2) {
                    Check(!session.tooltip.box.gameObject.activeSelf && session.shopController.HintsUsed==0,"hint still hidden before three seconds");
                    row.OnPointerExit(mouse); next=EditorApplication.timeSinceStartup+1.5;
                }
                else if(step==3) {
                    Check(!session.tooltip.box.gameObject.activeSelf && session.shopController.HintsUsed==0,"leaving cancels pending hint");
                    row.OnPointerEnter(mouse); next=EditorApplication.timeSinceStartup+3.3;
                }
                else if(step==4) {
                    Check(session.tooltip.IsVisibleFor(row) && session.shopController.HintsUsed==1,"list hint reveals and counts once");
                    Check(session.tooltip.label.text==row.entry.Translation(session.Progress.english),"correct translation");
                    Check(session.shopController.Points==0 && session.Lives==session.shop.startingLives,"hint does not consume points/lives");
                    row.OnPointerEnter(mouse); next=EditorApplication.timeSinceStartup+1;
                }
                else if(step==5) {
                    Check(session.shopController.HintsUsed==1,"steady hover does not repeatedly count");
                    row.OnPointerExit(mouse); row.OnPointerEnter(mouse); next=EditorApplication.timeSinceStartup+3.3;
                }
                else if(step==6) {
                    Check(session.shopController.HintsUsed==2,"second deliberate reveal counts again"); row.OnPointerExit(mouse);
                    touch=new ExtendedPointerEventData(EventSystem.current){pointerType=UIPointerType.Touch,position=new Vector2(Screen.width-1,Screen.height-1)};
                    product.OnPointerDown(touch); next=EditorApplication.timeSinceStartup+3.3;
                }
                else if(step==7) {
                    Check(session.tooltip.IsVisibleFor(product) && session.shopController.HintsUsed==3,"product touch long hold");
                    var box=session.tooltip.box; var parent=(RectTransform)box.parent; var half=box.rect.size*.5f;
                    Check(box.anchoredPosition.x+half.x<=parent.rect.xMax+.01f && box.anchoredPosition.y+half.y<=parent.rect.yMax+.01f,"tooltip clamped to canvas");
                    product.OnPointerUp(touch); int correct=session.shopController.Correct, mistakes=session.shopController.Mistakes;
                    view.OnPointerClick(touch);
                    Check(session.shopController.Correct==correct && session.shopController.Mistakes==mistakes,"long press release does not select product");
                    Check(!session.tooltip.box.gameObject.activeSelf,"touch release hides tooltip");
                    row.OnPointerEnter(mouse); row.gameObject.SetActive(false); next=EditorApplication.timeSinceStartup+3.3;
                }
                else if(step==8) {
                    Check(!session.tooltip.box.gameObject.activeSelf && session.shopController.HintsUsed==3,"disable cancels hint"); row.gameObject.SetActive(true);
                    var ids=session.shopController.listRoot.GetComponentsInChildren<HintPointer>().Select(h=>h.entry.id).ToArray();
                    foreach(var word in session.shopController.worldRoot.GetComponentsInChildren<WordView>().Where(v=>ids.Contains(v.Entry.id))) word.Select();
                    session.shopController.Complete();
                    Check(session.resultsPanel.activeSelf && session.Progress.lastShopHints==3 && session.results.text.Contains("Hints used: 3"),"results record hints");
                    session.OpenShop();
                }
                else if(step==9) {
                    Check(session.shopController.HintsUsed==0,"new round resets hint count");
                    row=session.shopController.listRoot.GetComponentInChildren<HintPointer>(); row.OnPointerEnter(mouse);
                    session.Village(); next=EditorApplication.timeSinceStartup+3.3;
                }
                else {
                    Check(!session.tooltip.box.gameObject.activeSelf,"scene exit clears pending tooltip");
                    Debug.Log("LEMKO_HINT_PLAY_CHECKS_PASS: delayed list/product hints, cancellation, counts, touch suppression, bounds, results, reset, hidden labels.");
                    SessionState.SetBool(Running,false); EditorApplication.Exit(0); return;
                }
                SessionState.SetInt("Lemko.HintStep",step+1);
            } catch(Exception e) { Debug.LogException(e); SessionState.SetBool(Running,false); EditorApplication.Exit(1); }
        }
    }
}
