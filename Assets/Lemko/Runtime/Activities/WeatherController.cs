using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Lemko
{
    public sealed class WeatherController : LearningActivity
    {
        public Slider temperature;
        public TMP_Text temperatureLabel;
        VocabularyEntry selected;
        public VocabularyEntry Selected=>selected;
        protected override void BuildTask()
        {
            selected=null; prompt.text=string.IsNullOrWhiteSpace(Current.request) ? "I would like "+Current.word.lemko : Current.request;
            Token(Current.context ? Current.context : Current.word,new Vector2(-5.5f,.5f),"character",t=>Speak());
            temperature.minValue=-30; temperature.maxValue=50; temperature.wholeNumbers=true; temperature.value=10;
            temperature.onValueChanged.RemoveAllListeners(); temperature.onValueChanged.AddListener(v=>temperatureLabel.text=v+" °C"); temperatureLabel.text=temperature.value+" °C";
            for(int i=0;i<definition.pool.Count;i++) { var word=definition.pool[i]; Token(word,new Vector2(-1.8f+(i%3)*2.5f,.8f-(i/3)*2),"weather",t=>Choose(word)); }
            feedback.text="Choose weather and adjust temperature, then Submit. Requested range: "+Current.minTemperature+" to "+Current.maxTemperature+" °C.";
        }
        public void Choose(VocabularyEntry word) { if(!Ready || !definition.pool.Contains(word)) return; selected=word; foreach(var t in tokens) if(t.Key=="weather") t.backing.color=t.view.Entry==word ? new Color(.6f,.95f,.6f) : Color.white; }
        public override void Submit() => Answer(selected==Current.word && temperature.value>=Current.minTemperature && temperature.value<=Current.maxTemperature,(selected?selected.id:"")+":"+temperature.value,"Match the requested weather and temperature range ("+Current.minTemperature+" to "+Current.maxTemperature+" °C).");
    }
}
