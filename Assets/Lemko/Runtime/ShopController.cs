using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko
{
    public sealed class ShopController : MonoBehaviour
    {
        public ShopDefinition definition;
        public WordView wordPrefab;
        public RectTransform productRoot;
        [Header("Scene products (optional; overrides UI product root)")]
        public Transform worldRoot;
        public Transform[] productSlots;
        public TMP_Text shoppingList, feedback;
        public Button finishButton;
        [Header("Interactive shopping list")]
        public RectTransform listRoot;
        public TMP_Text listRowPrefab;
        readonly List<TMP_Text> listRows=new List<TMP_Text>();
        public int HintsUsed { get; private set; }
        public void RecordHint() { if(round!=null) { HintsUsed++; session.RefreshHud(); } }
        // Where the products lie in the shop drawn in the backdrop (x, y, tilt in degrees): three on the display table,
        // two on the wall shelves, one in each crate and one on the rack. Slots 9 to 12 are spare places on the shelves.
        static readonly Vector3[] Stands={ new Vector3(-6.75f,1.45f,8),new Vector3(-5.15f,1.2f,-7),new Vector3(-3.75f,1.65f,5),new Vector3(-1.9f,1.75f,-4),new Vector3(.75f,.1f,6),
            new Vector3(-2.6f,-1.7f,-11),new Vector3(-.55f,-1.75f,9),new Vector3(3.15f,1.45f,-5),new Vector3(.6f,1.75f,4),new Vector3(-1.7f,.1f,-6),new Vector3(3.15f,-.35f,7),new Vector3(3.15f,-2.1f,-4) };
        // The list lies in the bottom left corner, the basket stands in the bottom right one.
        void Arrange()
        {
            for(int i=0;i<productSlots.Length && i<Stands.Length;i++) if(productSlots[i]) {
                productSlots[i].localPosition=new Vector3(Stands[i].x,Stands[i].y,0); productSlots[i].localRotation=Quaternion.Euler(0,0,Stands[i].z);
            }
            basketStart=new Vector3(4.15f,-2.72f,0); basketSpacing=.5f; basketItemScale=.45f;
            Place(shoppingList ? shoppingList.rectTransform : null,new Vector2(-5.6f,-.5f),new Vector2(300,40),.82f);
            Place(listRoot,new Vector2(-5.6f,-2.75f),new Vector2(350,420),.72f);
        }
        static void Place(RectTransform rect,Vector2 world,Vector2 size,float scale)
        {
            if(!rect) return;
            var canvas=rect.GetComponentInParent<Canvas>(); if(!canvas) return;
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f); rect.sizeDelta=size; rect.localScale=Vector3.one*scale;
            rect.position=canvas.rootCanvas.transform.TransformPoint(world*80f);   // the 1280 x 720 canvas shows 16 x 9 world units
        }
        readonly List<WordView> views = new List<WordView>();
        List<VocabularyEntry> targets;
        ShopBasket round;
        [Header("Editable basket presentation")]
        public Transform basketRoot;
        public Vector3 basketStart=new Vector3(-6.55f,-2.65f,0);
        public float basketSpacing=.94f;
        public float basketItemScale=.48f;
        public TMP_Text basketStatus;
        readonly List<WordView> basketViews=new List<WordView>();
        public int BasketCount=>round?.Count??0;
        public int TargetCount=>targets?.Count??definition.taskCount;
        GameSession session;
        bool active;
        ShopScore score = new ShopScore();
        public bool HasRound => round!=null;
        public bool RoundComplete => round!=null && round.Complete;
        public int Points => score.Points;
        public int NextMultiplier => score.NextMultiplier;
        public int Correct => round!=null && round.Complete ? targets.Count : 0;
        public int Mistakes => round?.Mistakes ?? 0;
        public string ProgressLabel => active ? "Shopping list: " + Correct + "/" + targets.Count : "Village learning";
        public void Begin(GameSession owner)
        {
            Close(); session = owner; session.BeginLearningSession(); score = new ShopScore(); HintsUsed=0;
            var error = definition.Validate();
            if (error != null) { feedback.text = error; return; }
            if (worldRoot && (productSlots == null || productSlots.Length < Mathf.Min(definition.displayedProductCount,definition.products.Count))) {
                feedback.text = "Add one scene product slot for every configured product."; return;
            }
            if(worldRoot) { worldRoot.gameObject.SetActive(true); Arrange(); }
            var products = new List<VocabularyEntry>(definition.products);
            Shuffle(products);
            products=PracticeSelection.Order(products,w=>w,session,definition.taskCount);
            products = products.Take(definition.displayedProductCount).ToList();
            targets = products.Take(definition.taskCount).ToList();
            round = new ShopBasket(targets.Select(w => w.id), products.Select(w => w.id));
            Shuffle(products); active = true;
            foreach (var word in products)
            {
                var parent = worldRoot ? productSlots[views.Count] : productRoot;
                var view = Instantiate(wordPrefab, parent); views.Add(view);
                view.Bind(word, session.tooltip, session.audioService, Select);
                session.Encounter(word);
            }
            if(listRoot && listRowPrefab) foreach(var word in targets) {
                var row=Instantiate(listRowPrefab,listRoot); row.gameObject.SetActive(true);
                row.GetComponent<HintPointer>().Bind(word,session.tooltip);
                foreach(var control in row.GetComponentsInChildren<HoldTranslationHint>()) control.Bind(word,session.tooltip);
                listRows.Add(row);
            }
            feedback.text = "Find everything on the list.";
            if(finishButton) finishButton.gameObject.SetActive(false); GameEvents.Started(); UpdateBasket(); UpdateList();
        }
        static void Shuffle<T>(List<T> items)
        {
            for (int i = items.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (items[i], items[j]) = (items[j], items[i]); }
        }
        // One click decides: a product from the list flies into the basket and is crossed off, anything else is a mistake.
        void Select(WordView view)
        {
            if(!active || !session.CanAnswer || round==null) return;
            var result=round.Pick(view.Entry.id);
            if(result==SelectionResult.Ignored) return;
            session.tooltip.Hide();
            if(result==SelectionResult.Incorrect) {
                session.RecordAnswer(view.Entry,false); score.Apply(result); Juice.Shake(view.transform,.12f);
                feedback.text="That is not on the list."+(round.Mistakes>=2 ? " Look for: "+string.Join(", ",targets.Where(w=>round.Missing.Contains(w.id)).Select(w=>w.lemko)) : "");
                session.PresentFeedback(false); GameEvents.Answer(false,0,0); session.Wrong(); session.RefreshHud(); return;
            }
            session.RecordAnswer(view.Entry,true); int awarded=score.Apply(result);
            Vector3 from=view.transform.position; UpdateBasket(); UpdateList(); session.RefreshHud(); Juice.Sound("pickup");
            var copy=basketViews.Find(v=>v.Entry==view.Entry); if(copy) PresentationMotion.Travel(copy.transform,from,session);
            int row=targets.IndexOf(view.Entry); if(row>=0 && row<listRows.Count) Juice.Punch(listRows[row].transform,.18f);
            SessionStats.BestStreak=Mathf.Max(SessionStats.BestStreak,score.Streak); GameEvents.Answer(true,awarded,score.Streak);
            if(!round.Complete) { feedback.text=round.Missing.Length==1 ? "One more thing." : round.Missing.Length+" more things."; return; }
            active=false; feedback.text="Everything on the list!"; session.PresentFeedback(true); StartCoroutine(FinishSoon());
        }
        void UpdateBasket()
        {
            foreach(var view in basketViews) if(view) { view.gameObject.SetActive(false); Destroy(view.gameObject); }
            basketViews.Clear();
            foreach(var view in views) {
                bool selected=round.Contains(view.Entry.id); view.gameObject.SetActive(!selected);   // a picked product leaves its shelf
                if(!selected || !basketRoot) continue;
                var copy=Instantiate(wordPrefab,basketRoot); copy.Bind(view.Entry,session.tooltip,session.audioService,_=>{});
                copy.transform.localPosition=basketStart+Vector3.right*basketViews.Count*basketSpacing;
                copy.transform.localScale=Vector3.one*basketItemScale; basketViews.Add(copy);
            }
            if(basketStatus) basketStatus.text="";   // the basket in the picture shows what is in it
        }
        // Kept for older callers: there is no basket check any more, every click is judged at once.
        public void SubmitBasket() { }
        void UpdateList()
        {
            if(listRoot && listRowPrefab) {
                shoppingList.text=definition.shoppingListTitle;
                for(int i=0;i<targets.Count;i++) { listRows[i].text=targets[i].lemko; bool got=round.Contains(targets[i].id); listRows[i].color=got ? Look.Good : Look.Ink; listRows[i].fontStyle=got ? FontStyles.Strikethrough : FontStyles.Normal; }
            } else shoppingList.text=definition.shoppingListTitle+"\n\n"+string.Join("\n",targets.Select(w=>w.lemko));
        }
        // Results open by themselves after a short celebration.
        System.Collections.IEnumerator FinishSoon() {
            yield return new WaitForSecondsRealtime(1.7f);
            while(session && session.menu && session.menu.IsOpen) yield return null;
            if(session && round!=null && round.Complete) session.Finish(true);
        }
        public void Complete() { if(round!=null && session && round.Complete) session.Finish(true); }
        public void Close()
        {
            if(session) session.StopFeedback();
            StopAllCoroutines(); active = false;
            round = null;
            targets = null;
            foreach (var view in views) if (view) { view.gameObject.SetActive(false); Destroy(view.gameObject); }
            views.Clear();
            foreach(var view in basketViews) if(view) { view.gameObject.SetActive(false); Destroy(view.gameObject); } basketViews.Clear();
            foreach(var row in listRows) if(row) { row.gameObject.SetActive(false); Destroy(row.gameObject); } listRows.Clear();
            if(worldRoot) worldRoot.gameObject.SetActive(false);
            if (session) { session.tooltip.Hide(); session.audioService.Stop(); }
        }
        void OnDisable() => Close();
    }
}
