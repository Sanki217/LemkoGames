using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Lemko
{
    public sealed class WorldShopSmoke : MonoBehaviour
    {
        public GameSession session;
        IEnumerator Start()
        {
            if(!Environment.GetCommandLineArgs().Contains("-lemkoWorldSmoke")) yield break;
            Application.runInBackground=true;
            yield return null;
            session.OpenShop(); yield return null;
            Physics2D.SyncTransforms();
            var views=session.shopController.worldRoot.GetComponentsInChildren<WordView>();
            Require(views.Length==session.shop.products.Count,"spawn count");
            Require(views.All(v=>!v.GetComponentInParent<Canvas>() && v.GetComponent<Collider2D>() && v.worldPicture),"scene objects");
            var camera=Camera.main;
            foreach(var view in views) {
                var data=new PointerEventData(EventSystem.current) { position=camera.WorldToScreenPoint(view.transform.position) };
                var hits=new List<RaycastResult>(); EventSystem.current.RaycastAll(data,hits);
                Require(hits.Count>0 && hits[0].gameObject==view.gameObject,"unblocked product pointer raycast " + view.Entry.id);
            }
            int lives=session.Lives;
            var wrong=views.First(v=>!session.shopController.shoppingList.text.Contains(v.Entry.lemko));
            Click(wrong); Click(wrong); Require(session.Lives==lives-1,"duplicate wrong selection");
            foreach(var view in views) { Click(view); Click(view); }
            Require(session.shopController.Correct==session.shop.taskCount,"completion");
            session.shopController.Complete(); yield return null;
            Require(session.resultsPanel.activeSelf && !session.shopController.worldRoot.gameObject.activeSelf,"results cleanup");
            int count=session.Progress.shopCompletions; session.shopController.Complete(); Require(count==session.Progress.shopCompletions,"duplicate result");
            session.Village(); session.OpenShop(); yield return null;
            Require(session.shopController.worldRoot.GetComponentsInChildren<WordView>().Length==views.Length,"clean reentry");
            while(session.Lives>0) session.Wrong();
            Require(session.exhaustedPanel.activeSelf,"zero life recovery");
            session.StartPractice(); yield return null; session.Wrong(); Require(session.Lives==0,"practice");
            session.FreshAttempt(); yield return null; Require(session.Lives==session.shop.startingLives && !session.Practice,"fresh attempt");
            session.Village(); Require(!session.shopController.worldRoot.gameObject.activeSelf,"village cleanup");
            Debug.Log("LEMKO_WORLD_SMOKE_PASS " + Screen.width + "x" + Screen.height); Application.Quit(0);
        }
        static void Click(WordView view) { ExecuteEvents.Execute(view.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerClickHandler); }
        static void Require(bool condition,string message) { if(!condition) { Debug.LogError("WORLD_SMOKE_FAIL: " + message); Application.Quit(1); throw new Exception(message); } }
    }
}
