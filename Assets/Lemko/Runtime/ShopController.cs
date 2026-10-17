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
        readonly List<WordView> views = new List<WordView>();
        List<VocabularyEntry> targets;
        ShopRound round;
        GameSession session;
        bool active;
        public int Correct => round?.Correct ?? 0;
        public int Mistakes => round?.Mistakes ?? 0;
        public string ProgressLabel => active ? "Shopping list: " + Correct + "/" + round.Total : "Village learning";
        public void Begin(GameSession owner)
        {
            Close(); session = owner;
            var error = definition.Validate();
            if (error != null) { feedback.text = error; return; }
            if (worldRoot && (productSlots == null || productSlots.Length < definition.products.Count)) {
                feedback.text = "Add one scene product slot for every configured product."; return;
            }
            if(worldRoot) worldRoot.gameObject.SetActive(true);
            var products = new List<VocabularyEntry>(definition.products);
            Shuffle(products);
            targets = products.Take(definition.taskCount).ToList();
            round = new ShopRound(targets.Select(w => w.id), products.Select(w => w.id));
            Shuffle(products); active = true;
            foreach (var word in products)
            {
                var parent = worldRoot ? productSlots[views.Count] : productRoot;
                var view = Instantiate(wordPrefab, parent); views.Add(view);
                view.Bind(word, session.tooltip, session.audioService, Select);
                session.Encounter(word);
            }
            ProgressStore.Save(session.Progress);
            feedback.text = "Choose each product on the list. Hover for translation; speaker plays available audio.\nDEVELOPMENT CONTENT - not verified Lemko.";
            finishButton.gameObject.SetActive(false); UpdateList();
        }
        static void Shuffle<T>(List<T> items)
        {
            for (int i = items.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (items[i], items[j]) = (items[j], items[i]); }
        }
        void Select(WordView view)
        {
            if (!active || !session.CanAnswer) return;
            var result = round.Select(view.Entry.id);
            if (result == SelectionResult.Ignored) return;
            view.Mark(result == SelectionResult.Correct);
            if (result == SelectionResult.Incorrect)
            {
                feedback.text = "This product is not on your list. Choose another product.";
                session.Wrong();
                if (!active) return;
            }
            else feedback.text = "Correct! Product checked off.";
            UpdateList(); session.RefreshHud();
            if (round.Complete) { active = false; finishButton.gameObject.SetActive(true); feedback.text = "Everything collected! View your results."; }
        }
        void UpdateList() => shoppingList.text = "SHOPPING LIST\n\n" + string.Join("\n", targets.Select(w => (round.IsComplete(w.id) ? "[OK] " : "[  ] ") + w.lemko));
        public void Complete() { if (round != null && round.Complete && session) session.Finish(true); }
        public void Close()
        {
            active = false;
            round = null;
            targets = null;
            foreach (var view in views) if (view) { view.gameObject.SetActive(false); Destroy(view.gameObject); }
            views.Clear();
            if(worldRoot) worldRoot.gameObject.SetActive(false);
            if (session) { session.tooltip.Hide(); session.audioService.Stop(); }
        }
        void OnDisable() => Close();
    }
}
