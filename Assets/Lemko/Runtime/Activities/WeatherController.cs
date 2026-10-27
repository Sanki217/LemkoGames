using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Lemko
{
    public sealed class WeatherController : LearningActivity
    {
        public Slider temperature;
        public TMP_Text temperatureLabel;
        [Header("Outing preview (presentation only)")]
        public Sprite outingIllustration;
        public Vector2 previewPosition=new Vector2(-5.5f,-1.35f);
        SpriteRenderer blanket, supplies;
        VocabularyEntry selected;
        public VocabularyEntry Selected=>selected;
        protected override void BuildTask()
        {
            selected=null;
            prompt.text="Plan the gathering outing:\n"+(string.IsNullOrWhiteSpace(Current.request) ? "I would like "+Current.word.lemko : Current.request);
            Token(Current.context ? Current.context : Current.word,new Vector2(-5.5f,.5f),"character",t=>Speak());
            temperature.onValueChanged.RemoveListener(TemperatureChanged);
            temperature.minValue=-30; temperature.maxValue=50; temperature.wholeNumbers=true; temperature.value=10; temperature.interactable=true;
            temperature.onValueChanged.AddListener(TemperatureChanged); TemperatureChanged(temperature.value);
            for(int i=0;i<definition.pool.Count;i++) { var word=definition.pool[i]; Token(word,new Vector2(-1.8f+(i%3)*2.5f,.8f-(i/3)*2),"weather",t=>Choose(word)); }
            var preview=new GameObject("Outing blanket",typeof(SpriteRenderer)); preview.transform.SetParent(taskRoot,false); preview.transform.localPosition=previewPosition;
            blanket=preview.GetComponent<SpriteRenderer>(); blanket.sprite=tokenPrefab.backing.sprite; blanket.drawMode=SpriteDrawMode.Sliced; blanket.size=new Vector2(2.4f,.85f); blanket.color=new Color(.35f,.4f,.4f); blanket.sortingOrder=-1;
            var icon=new GameObject("Outing supplies",typeof(SpriteRenderer)); icon.transform.SetParent(preview.transform,false); supplies=icon.GetComponent<SpriteRenderer>(); supplies.sprite=outingIllustration; supplies.sortingOrder=2;
            if(outingIllustration) icon.transform.localScale=Vector3.one*(.8f/Mathf.Max(outingIllustration.bounds.size.x,outingIllustration.bounds.size.y));
            icon.SetActive(false);
            feedback.text="Planning puzzle: choose the requested weather and "+Current.minTemperature+" to "+Current.maxTemperature+" °C, then Submit.";
        }
        void TemperatureChanged(float value) { if(temperatureLabel) temperatureLabel.text=value.ToString("0")+" °C"; }
        public void Choose(VocabularyEntry word) { if(!Ready || !definition.pool.Contains(word)) return; selected=word; foreach(var t in tokens) if(t.Key=="weather") t.backing.color=t.view.Entry==word ? new Color(.6f,.95f,.6f) : Color.white; }
        public override void Submit() {
            if(!Ready) return;
            var problems=new List<string>();
            if(!selected) problems.Add("Choose a weather picture.");
            else if(selected!=Current.word) problems.Add("This is not the requested weather.");
            if(temperature.value<Current.minTemperature) problems.Add("Too cold: raise the temperature to at least "+Current.minTemperature+" °C.");
            else if(temperature.value>Current.maxTemperature) problems.Add("Too warm: lower the temperature to at most "+Current.maxTemperature+" °C.");
            bool correct=problems.Count==0;
            if(Answer(correct,(selected?selected.id:"")+":"+temperature.value.ToString("0"),correct ? "Outing plan ready! Weather and temperature match. The supplies are prepared." : string.Join(" ",problems)) && correct) {
                blanket.color=new Color(.55f,.85f,.6f); supplies.gameObject.SetActive(outingIllustration); temperature.interactable=false;
                foreach(var token in tokens) if(token.Key=="character") token.backing.color=new Color(.55f,.85f,.6f);
            }
        }
        public override void Close() { if(temperature) temperature.onValueChanged.RemoveListener(TemperatureChanged); base.Close(); selected=null; blanket=null; supplies=null; }
    }
}
