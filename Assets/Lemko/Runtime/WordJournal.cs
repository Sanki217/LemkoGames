using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko
{
    [Serializable] public sealed class JournalRow
    {
        public Button button;
        public Image picture;
        public TMP_Text word, meaning, state;
    }
    public sealed class WordJournal : MonoBehaviour
    {
        public GameSession session;
        public VocabularyDatabase database;
        public GameObject panel;
        public TMP_InputField search;
        public TMP_Text title, pageLabel, detail;
        public Button previous, next, reviewFilter, pronunciation;
        public JournalRow[] rows;
        List<VocabularyEntry> filtered=new List<VocabularyEntry>();
        VocabularyEntry selected;
        int page;
        bool reviewOnly;
        public bool IsOpen=>panel && panel.activeSelf;
        public int VisibleCount=>filtered.Count;
        public VocabularyEntry Selected=>selected;
        void Awake() { search.onValueChanged.AddListener(SearchChanged); }
        public void Open()
        {
            session.tooltip.Hide(); session.audioService.Stop(); page=0; selected=null;
            panel.SetActive(true); panel.transform.SetAsLastSibling(); Refresh();
        }
        public void Close() { if(panel) panel.SetActive(false); if(session) { session.tooltip.Hide(); session.audioService.Stop(); } }
        void SearchChanged(string value) { page=0; Refresh(); }
        public void ToggleReview() { reviewOnly=!reviewOnly; page=0; Refresh(); }
        public void Previous() { page=Math.Max(0,page-1); Refresh(); }
        public void Next() { page++; Refresh(); }
        public void Speak() { if(selected) session.audioService.Play(selected.pronunciation); }
        public void SelectRow(int index) { int n=page*rows.Length+index; if(n>=0 && n<filtered.Count) { selected=filtered[n]; session.audioService.Stop(); ShowDetail(); Highlight(); } }
        bool NeedsReview(VocabularyEntry word)=>session.ReviewPriority(word)>0 || !session.Progress.words.Exists(w=>w.id==word.id);
        public void Refresh()
        {
            if(!IsOpen || session.Progress==null) return;
            string query=(search.text??"").Trim().Normalize();
            bool Match(string s)=>(s??"").Normalize().IndexOf(query,StringComparison.OrdinalIgnoreCase)>=0;
            var encountered=new HashSet<string>(session.Progress.encountered);
            filtered=database.entries.Where(w=>w && !w.IsLegacyPlaceholder && encountered.Contains(w.id) && (!reviewOnly || NeedsReview(w)) &&
                (Match(w.lemko)||Match(w.polish)||Match(w.english))).OrderBy(w=>w.lemko,StringComparer.Ordinal).ToList();
            int pages=Math.Max(1,(filtered.Count+rows.Length-1)/rows.Length); page=Mathf.Clamp(page,0,pages-1);
            title.text="Word journal - "+filtered.Count+" encountered words";
            pageLabel.text=(page+1)+" / "+pages; previous.interactable=page>0; next.interactable=page<pages-1;
            reviewFilter.GetComponentInChildren<TMP_Text>().text=reviewOnly?"Show all encountered":"Show words to practise";
            for(int i=0;i<rows.Length;i++) {
                var row=rows[i]; int index=page*rows.Length+i; row.button.gameObject.SetActive(index<filtered.Count);
                if(index>=filtered.Count) continue;
                var word=filtered[index]; row.word.text=word.lemko; row.meaning.text=word.Translation(session.Progress.english);
                var record=session.Progress.words.Find(w=>w.id==word.id);
                row.state.text=record==null?"Encountered":record.rememberedOnLaterDay?"Recalled on a later day":record.independent>0?"Answered without help":"Practising with support";
                row.picture.sprite=word.sprite; row.picture.enabled=word.sprite;
            }
            if(!filtered.Skip(page*rows.Length).Take(rows.Length).Contains(selected)) { selected=filtered.Skip(page*rows.Length).FirstOrDefault(); session.audioService.Stop(); } ShowDetail(); Highlight();
        }
        void Highlight() {
            for(int i=0;i<rows.Length;i++) {
                int n=page*rows.Length+i; var row=rows[i];
                if(row.button.image) row.button.image.color=n<filtered.Count && filtered[n]==selected ? new Color(.85f,.95f,.9f) : new Color(.95f,.97f,.98f);
            }
        }
        void ShowDetail()
        {
            pronunciation.interactable=selected && selected.pronunciation;
            if(!selected) { detail.text=string.IsNullOrWhiteSpace(search.text)?"No words here yet. Play an activity to encounter vocabulary.":"No matching encountered words. Try another search."; return; }
            var record=session.Progress.words.Find(w=>w.id==selected.id);
            detail.text=selected.lemko+" - "+selected.Translation(session.Progress.english)+"\n"+
                (record==null?"Encountered; no answer history yet.":"Without help: "+record.independent+" | With support: "+record.assisted+" | Mistakes: "+record.mistakes)+
                (selected.pronunciation?"":" | Recording not supplied")+(selected.verified?"":"\nLanguage content awaiting review.");
        }
        void OnDestroy() { if(search) search.onValueChanged.RemoveListener(SearchChanged); }
    }
}
