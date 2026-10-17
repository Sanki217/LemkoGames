using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Lemko
{
    // Opt-in standalone regression runner. Normal players never execute this path.
    public sealed class MilestoneSmoke : MonoBehaviour
    {
        public GameSession session;
        bool smoke;
        float deadline;
        string output;
        Camera captureCamera;
        RenderTexture captureTarget;
        IEnumerator Capture(string name)
        {
            Canvas.ForceUpdateCanvases();
            yield return new WaitForEndOfFrame();
            // Hidden windows need an explicit render. Captures are diagnostic only;
            // cached UI geometry may be incomplete on some graphics drivers.
            RenderPipeline.SubmitRenderRequest(captureCamera, new UniversalRenderPipeline.SingleCameraRequest { destination = captureTarget });
            var previousActive = RenderTexture.active;
            var texture = new Texture2D(captureTarget.width,captureTarget.height,TextureFormat.RGB24,false);
            try
            {
                RenderTexture.active = captureTarget;
                texture.ReadPixels(new Rect(0,0,captureTarget.width,captureTarget.height),0,0); texture.Apply();
                File.WriteAllBytes(Path.Combine(output,name + ".png"),texture.EncodeToPNG());
            }
            finally { RenderTexture.active = previousActive; Destroy(texture); }
        }
        void OnDestroy()
        {
            if (captureTarget) { if(captureCamera) captureCamera.targetTexture = null; captureTarget.Release(); Destroy(captureTarget); }
        }
        void Update() { if (smoke && Time.realtimeSinceStartup > deadline) { Debug.LogError("Smoke test timed out"); Application.Quit(1); } }
        IEnumerator Start()
        {
            if (!Environment.GetCommandLineArgs().Contains("-lemkoSmoke")) yield break;
            smoke = true; deadline = Time.realtimeSinceStartup + 45;
            Application.runInBackground = true;
            output = Path.Combine("SmokeResults", Screen.width + "x" + Screen.height);
            Directory.CreateDirectory(output);
            // Keep one camera/target throughout the offscreen test so UI culling stays stable.
            captureCamera = FindFirstObjectByType<Camera>();
            captureTarget = new RenderTexture(Screen.width,Screen.height,24); captureTarget.Create();
            captureCamera.targetTexture = captureTarget;
            var canvas = session.villagePanel.GetComponentInParent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = captureCamera; canvas.planeDistance = 1;
            yield return null;
            session.OpenShop(); yield return null;
            if (session.shopController.productRoot.childCount != session.shop.products.Count) throw new Exception("Product count mismatch");
            yield return new WaitForEndOfFrame();
            yield return Capture("shop");
            int initialLives = session.Lives;
            var wrong = session.shopController.productRoot.GetComponentsInChildren<WordView>().First(v => !session.shopController.shoppingList.text.Contains(v.Entry.lemko));
            wrong.selectButton.onClick.Invoke(); wrong.selectButton.onClick.Invoke();
            if (session.Lives != initialLives - 1) throw new Exception("Duplicate wrong click changed lives");
            foreach (var view in session.shopController.productRoot.GetComponentsInChildren<WordView>())
            { view.selectButton.onClick.Invoke(); view.selectButton.onClick.Invoke(); }
            if (session.shopController.Correct != session.shop.taskCount || session.shopController.Mistakes > session.shop.products.Count - session.shop.taskCount)
                throw new Exception("Shop cannot complete or duplicates affect state");
            session.shopController.Complete(); yield return null;
            if (!session.resultsPanel.activeSelf) throw new Exception("Results not shown");
            yield return new WaitForEndOfFrame();
            yield return Capture("results");
            int completions = session.Progress.shopCompletions;
            session.shopController.Complete();
            if (session.Progress.shopCompletions != completions) throw new Exception("Duplicate result recorded");
            int lives = session.Lives;
            session.Village(); session.OpenShop(); yield return null;
            if (session.Lives != lives) throw new Exception("Navigation reset lives");
            while(session.Lives > 0) session.Wrong();
            if (!session.exhaustedPanel.activeSelf) throw new Exception("Zero-life recovery missing");
            lives = session.Lives;
            session.StartPractice(); yield return null;
            foreach (var view in session.shopController.productRoot.GetComponentsInChildren<WordView>()) view.selectButton.onClick.Invoke();
            if (session.Lives != lives) throw new Exception("Practice consumed lives");
            session.Village(); yield return null;
            session.FreshAttempt(); yield return null;
            if (session.Lives != session.shop.startingLives || session.Practice) throw new Exception("Fresh attempt did not reset session");
            session.Village(); yield return null;
            yield return new WaitForEndOfFrame();
            yield return Capture("village");
            File.WriteAllText(Path.Combine(output,"result.txt"), "PASS: standalone Shop completion, duplicate correct/wrong callbacks, duplicate results, navigation lives, zero-life recovery, practice and fresh attempt. Starting lives: " + initialLives + "; resolution: " + Screen.width + "x" + Screen.height);
            Application.Quit(0);
        }
    }
}
