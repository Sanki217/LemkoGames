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
        SpriteRenderer weatherPreview;
        public VocabularyEntry Selected=>selected;
        protected override void BuildTask()
        {
            selected=null; weatherPreview=null;
            prompt.text=string.IsNullOrWhiteSpace(Current.request) ? Look.Word(Current.word.lemko) : Current.request;
            Token(Current.context ? Current.context : Current.word,new Vector2(-5.5f,.5f),"character",t=>Speak());
            temperature.onValueChanged.RemoveListener(TemperatureChanged);
            temperature.minValue=-30; temperature.maxValue=50; temperature.wholeNumbers=true; temperature.value=10; temperature.interactable=true;
            temperature.onValueChanged.AddListener(TemperatureChanged); TemperatureChanged(temperature.value);
            for(int i=0;i<definition.pool.Count;i++) { var word=definition.pool[i]; Token(word,new Vector2(-1.8f+(i%3)*2.5f,.8f-(i/3)*2),"weather",t=>Choose(word)); }
            var preview=new GameObject("Outing blanket",typeof(SpriteRenderer)); preview.transform.SetParent(taskRoot,false); preview.transform.localPosition=previewPosition;
            blanket=preview.GetComponent<SpriteRenderer>(); blanket.sprite=tokenPrefab.backing.sprite; blanket.drawMode=SpriteDrawMode.Sliced; blanket.size=new Vector2(2.4f,.85f); blanket.color=Look.Sage; blanket.sortingOrder=-1;
            var icon=new GameObject("Outing supplies",typeof(SpriteRenderer)); icon.transform.SetParent(preview.transform,false); supplies=icon.GetComponent<SpriteRenderer>(); supplies.sprite=outingIllustration; supplies.sortingOrder=2;
            if(outingIllustration) icon.transform.localScale=Vector3.one*(.8f/Mathf.Max(outingIllustration.bounds.size.x,outingIllustration.bounds.size.y));
            icon.SetActive(false);
            feedback.text="Pick the weather and set "+Current.minTemperature+" to "+Current.maxTemperature+" °C.";
        }
        protected override bool HasAnswer => selected;
        void TemperatureChanged(float value) {
            if(temperatureLabel) temperatureLabel.text=value.ToString("0")+" °C";
            // The slider reads like a thermometer: cold blue rising to warm brick.
            float t=Mathf.InverseLerp(temperature.minValue,temperature.maxValue,value); var warm=Color.Lerp(Look.Sand,Look.Brick,t);
            if(temperature.fillRect && temperature.fillRect.TryGetComponent<UnityEngine.UI.Graphic>(out var fill)) fill.color=warm;
            if(temperature.handleRect && temperature.handleRect.TryGetComponent<UnityEngine.UI.Graphic>(out var handle)) handle.color=warm;
        }
        public void Choose(VocabularyEntry word) { if(!Ready || !definition.pool.Contains(word)) return; selected=word;
            if(!weatherPreview) { var go=new GameObject("Selected weather preview",typeof(SpriteRenderer)); go.transform.SetParent(taskRoot,false); go.transform.localPosition=new Vector3(-5.5f,2.05f,0); weatherPreview=go.GetComponent<SpriteRenderer>(); weatherPreview.sortingOrder=3; }
            weatherPreview.sprite=word.sprite;
            if(word.sprite) weatherPreview.transform.localScale=Vector3.one*(1.1f/Mathf.Max(word.sprite.bounds.size.x,word.sprite.bounds.size.y));
            PresentationMotion.Pulse(weatherPreview.transform,Session);
            WeatherAmbience.Show(taskRoot,word);
            foreach(var t in tokens) if(t.Key=="weather") { t.backing.color=t.view.Entry==word ? Look.Honey : Look.Paper; if(t.view.Entry==word) Juice.Punch(t.transform,.12f); } }
        public override void Submit() {
            if(!Ready) return;
            var problems=new List<string>();
            if(!selected) problems.Add("Pick a weather picture.");
            else if(selected!=Current.word) problems.Add("That is different weather.");
            if(temperature.value<Current.minTemperature) problems.Add("Too cold.");
            else if(temperature.value>Current.maxTemperature) problems.Add("Too warm.");
            bool correct=problems.Count==0;
            if(Answer(correct,(selected?selected.id:"")+":"+temperature.value.ToString("0"),correct ? "A perfect day for it!" : string.Join(" ",problems)) && correct) {
                blanket.color=Look.GoodSoft; supplies.gameObject.SetActive(outingIllustration); temperature.interactable=false;
                foreach(var token in tokens) if(token.Key=="character") { token.backing.color=Look.GoodSoft; PresentationMotion.Pulse(token.view.worldPicture.transform,Session); }
            }
        }
        public override void Close() { if(temperature) temperature.onValueChanged.RemoveListener(TemperatureChanged); base.Close(); selected=null; blanket=null; supplies=null; }
    }
}
