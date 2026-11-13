using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Lemko.Editor
{
    [InitializeOnLoad] public static class BasketJournalChecks
    {
        const string Running="Lemko.BasketJournalChecks";
        static double next;
        static BasketJournalChecks() { EditorApplication.update+=Tick; }
        static void Check(bool condition,string message) { if(!condition) throw new Exception(message); }
        public static void Data()
        {
            var basket=new ShopBasket(new[]{"a","b"},new[]{"a","b","c"});
            Check(basket.Submit()==SelectionResult.Ignored && !basket.Toggle("unknown"),"empty and unknown");
            basket.Toggle("c"); Check(basket.Submit()==SelectionResult.Incorrect,"wrong basket");
            Check(basket.Submit()==SelectionResult.Ignored && basket.Mistakes==1,"duplicate guard");
            basket.Toggle("c"); basket.Toggle("a"); Check(basket.Submit()==SelectionResult.Incorrect,"new rejected basket");
            basket.Toggle("a"); basket.Toggle("c"); Check(basket.Submit()==SelectionResult.Ignored,"revisiting rejected set");
            basket.Toggle("c"); basket.Toggle("a"); basket.Toggle("b");
            Check(basket.Submit()==SelectionResult.Correct && basket.Complete,"exact matching");
            Check(!basket.Toggle("a") && basket.Submit()==SelectionResult.Ignored && basket.Mistakes==2,"completed immutable");
            bool invalid=false; try { new ShopBasket(new[]{"x"},new[]{"a"}); } catch(ArgumentException) { invalid=true; }
            Check(invalid,"unsolvable data blocked"); Debug.Log("LEMKO_BASKET_DATA_PASS");
        }
        public static void RunBatch()
        {
            if(!Environment.GetCommandLineArgs().Contains("-lemkoSmoke")) throw new Exception("Use isolated smoke save.");
            Data(); EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath);
            SessionState.SetBool(Running,true); SessionState.SetInt(Running+"Step",0);
            SessionState.SetFloat(Running+"Deadline",(float)EditorApplication.timeSinceStartup+100); EditorApplication.EnterPlaymode();
        }
        static void Capture(string name) {
            if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            PrototypeVisualChecks.Capture(name,1280,720); PrototypeVisualChecks.Capture(name,960,540);
        }
        static void Tick()
        {
            if(!SessionState.GetBool(Running,false)) return;
            try {
                if(EditorApplication.timeSinceStartup>SessionState.GetFloat(Running+"Deadline",0)) throw new Exception("Timeout");
                if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<next) return;
                next=EditorApplication.timeSinceStartup+1;
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>(); if(!session || session.Progress==null) return;
                int step=SessionState.GetInt(Running+"Step",0);
                var journal=session.journal;
                if(step==0) {
                    session.Progress.encountered.Clear(); session.Progress.words.Clear();
                    journal.Open(); Check(journal.VisibleCount==0 && !journal.pronunciation.interactable,"empty journal");
                } else if(step==1) {
                    Capture("Journal-empty");
                    foreach(var word in journal.database.entries.Take(15)) session.Encounter(word);
                    journal.Refresh(); Check(journal.VisibleCount==15 && journal.next.interactable,"encountered only, pagination");
                    journal.Next(); Check(journal.previous.interactable,"next page"); journal.Previous();
                    var firstWord=journal.database.entries.First(); journal.search.text=firstWord.lemko;
                    Check(journal.VisibleCount>0 && journal.Selected,"Unicode search");
                    journal.search.text="NO_MATCH_987"; Check(journal.VisibleCount==0,"search excludes");
                    journal.search.text=""; journal.ToggleReview(); Check(journal.VisibleCount==15,"unanswered review pool"); journal.ToggleReview();
                    session.ToggleLanguage(); Check(journal.detail.text.Contains(journal.Selected.Translation(session.Progress.english)),"language refresh");
                    Check(session.Progress.words.Count==0,"journal gives no recall credit");
                } else if(step==2) {
                    Capture("Journal-populated"); session.Village(); Check(!journal.IsOpen,"village closes journal");
                    journal.Open(); journal.Close(); journal.Open(); Check(journal.VisibleCount==15,"safe reopen"); session.OpenShop();
                } else if(step==3) {
                    var shop=session.shopController; var products=shop.productSlots.Select(s=>s.GetComponentInChildren<WordView>()).Where(v=>v).ToArray();
                    var ids=shop.listRoot.GetComponentsInChildren<HintPointer>().Select(h=>h.entry.id).ToArray();
                    int lives=session.Lives; products[0].Select();
                    Check(shop.BasketCount==1 && shop.basketRoot.GetComponentsInChildren<WordView>().Length==1,"basket world copy");
                    shop.basketRoot.GetComponentInChildren<WordView>().Select();
                    Check(shop.BasketCount==0 && session.Lives==lives && shop.Points==0,"remove copy free");
                    foreach(var item in products.Where(v=>ids.Contains(v.Entry.id))) item.Select();
                    Check(shop.BasketCount==5 && shop.Points==0 && shop.listRoot.GetComponentsInChildren<TMPro.TMP_Text>().All(t=>!t.text.Contains("[OK]")),"no answer leak before submit");
                } else if(step==4) {
                    Capture("Shop-editable-basket"); session.shopController.SubmitBasket();
                    Check(session.shopController.Points==1500 && session.IndependentAnswers==5,"independent basket");
                    session.shopController.Complete(); session.Village();
                } else if(step==5) {
                    journal.Open(); Check(journal.VisibleCount>=15 && session.Progress.words.Count==5,"journal learning history");
                } else {
                    Capture("Journal-after-shop"); Debug.Log("LEMKO_BASKET_JOURNAL_PLAY_PASS: free basket edits, world copies, exact answers, journal search/paging/filter/translation, no credit for reading, navigation and save reload.");
                    SessionState.SetBool(Running,false); EditorApplication.Exit(0); return;
                }
                SessionState.SetInt(Running+"Step",step+1);
            } catch(Exception e) { Debug.LogException(e); SessionState.SetBool(Running,false); EditorApplication.Exit(1); }
        }
    }
}
