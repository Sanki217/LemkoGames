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
            if(worldRoot) worldRoot.gameObject.SetActive(true);
            var products = new List<VocabularyEntry>(definition.products);
            Shuffle(products);
            var review=products.Where(w=>session.ReviewPriority(w)>0).OrderByDescending(session.ReviewPriority).Take(2).ToList();
            products=review.Concat(products.Where(w=>!review.Contains(w))).ToList();
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
            ProgressStore.Save(session.Progress);
            if(listRoot && listRowPrefab) foreach(var word in targets) {
                var row=Instantiate(listRowPrefab,listRoot); row.gameObject.SetActive(true);
                row.GetComponent<HintPointer>().Bind(word,session.tooltip);
                foreach(var control in row.GetComponentsInChildren<HoldTranslationHint>()) control.Bind(word,session.tooltip);
                listRows.Add(row);
            }
            feedback.text = "Collect the supplies for our shared table. Click pictures to add or remove; then check your basket.";
            finishButton.gameObject.SetActive(true); finishButton.GetComponentInChildren<TMP_Text>().text="Check basket"; UpdateBasket(); UpdateList();
        }
        static void Shuffle<T>(List<T> items)
        {
            for (int i = items.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (items[i], items[j]) = (items[j], items[i]); }
        }
        void Select(WordView view)
        {
            if(!active || !session.CanAnswer || !round.Toggle(view.Entry.id)) return;
            session.tooltip.Hide(); UpdateBasket(); session.RefreshHud();
        }
        void UpdateBasket()
        {
            foreach(var view in basketViews) if(view) { view.gameObject.SetActive(false); Destroy(view.gameObject); }
            basketViews.Clear();
            foreach(var view in views) {
                bool selected=round.Contains(view.Entry.id); view.SetSelected(selected);
                if(!selected || !basketRoot) continue;
                var copy=Instantiate(wordPrefab,basketRoot); copy.Bind(view.Entry,session.tooltip,session.audioService,Select);
                copy.transform.localPosition=basketStart+Vector3.right*basketViews.Count*basketSpacing;
                copy.transform.localScale=Vector3.one*basketItemScale; basketViews.Add(copy);
            }
            if(basketStatus) basketStatus.text="Your basket: "+round.Count+" items - click an item to remove";
        }
        public void SubmitBasket()
        {
            if(!active || !session.CanAnswer) return;
            var result=round.Submit();
            if(result==SelectionResult.Ignored) { feedback.text=round.Count==0 ? "Add products before checking your basket." : "You already checked this basket. Change the products and try again."; return; }
            if(result==SelectionResult.Incorrect) {
                foreach(var word in targets.Where(w=>round.Missing.Contains(w.id))) session.RecordAnswer(word,false);
                foreach(var view in views.Where(w=>round.Extra.Contains(w.Entry.id))) session.RecordAnswer(view.Entry,false);
                score.Apply(result);
                feedback.text="Check the list: "+round.Missing.Length+" missing, "+round.Extra.Length+" extra. Edit your basket and try again.";
                session.PresentFeedback(false); session.Wrong(); return;
            }
            int awarded=0;
            foreach(var word in targets) { session.RecordAnswer(word,true); awarded+=score.Apply(SelectionResult.Correct); }
            active=false; UpdateList(); session.RefreshHud();
            foreach(var view in views) if(round.Contains(view.Entry.id)) view.Mark(true);
            finishButton.GetComponentInChildren<TMP_Text>().text="View results";
            feedback.text="Supplies ready for the shared table! +"+awarded+" points. View your results."; session.PresentFeedback(true);
        }
        void UpdateList()
        {
            if(listRoot && listRowPrefab) {
                shoppingList.text=definition.shoppingListTitle;
                for(int i=0;i<targets.Count;i++) listRows[i].text=(round.Complete ? "[OK] " : "[  ] ")+targets[i].lemko;
            } else shoppingList.text=definition.shoppingListTitle+"\n\n"+string.Join("\n",targets.Select(w=>(round.Complete ? "[OK] " : "[  ] ")+w.lemko));
        }
        public void Complete() { if(round==null || !session) return; if(round.Complete) session.Finish(true); else SubmitBasket(); }
        public void Close()
        {
            if(session) session.StopFeedback();
            active = false;
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
