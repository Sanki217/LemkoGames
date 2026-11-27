using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Lemko
{
    public sealed class WeatherController : LearningActivity
    {
        public Slider temperature;
        public TMP_Text temperatureLabel;
        // Retained for existing scene and setup references.
        [HideInInspector] public Sprite outingIllustration;
        [HideInInspector] public Vector2 previewPosition=new Vector2(-5.5f,-1.35f);
        VocabularyEntry selected;
        bool temperatureAnimating;
        readonly List<VocabularyEntry> options=new List<VocabularyEntry>();
        public VocabularyEntry Selected=>selected;
        public int ObservedTemperature {get;private set;}
        public bool TemperatureStage {get;private set;}
        protected override VocabularyEntry AnswerWord=>TemperatureStage ? Current.context : Current.word;

        protected override void BuildTask()
        {
            selected=null; TemperatureStage=false;
            prompt.text="What is the weather?";
            prompt.GetComponent<HintPointer>().Bind(null,Session.tooltip);
            speak.interactable=false;
            // Use plausible temperatures from the condition's configured range.
            int first=Mathf.CeilToInt(Current.minTemperature/5f);
            int last=Mathf.FloorToInt(Current.maxTemperature/5f);
            ObservedTemperature=first<=last ? Random.Range(first,last+1)*5 : Current.minTemperature;
            WeatherAmbience.Show(taskRoot,Current.word);
            var weatherCard=new GameObject("Weather observation card",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            weatherCard.transform.SetParent(taskRoot,false);
            weatherCard.transform.localPosition=new Vector3(-5,.2f,0);
            Pixels.Card(weatherCard,new Vector2(2.8f,2.4f),Look.Panel);
            weatherCard.color=Look.Paper; weatherCard.sortingOrder=1;
            var observation=new GameObject("Observed weather",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            observation.transform.SetParent(taskRoot,false);
            observation.transform.localPosition=new Vector3(-5,.2f,0);
            observation.sprite=Current.word.sprite;
            observation.sortingOrder=2;
            Pixels.Native(observation.transform);

            temperature.onValueChanged.RemoveListener(TemperatureChanged);
            temperature.minValue=-30; temperature.maxValue=50;
            temperature.wholeNumbers=true; temperature.interactable=false;
            temperature.transition=Selectable.Transition.None;
            temperature.gameObject.SetActive(false);
            temperatureLabel.gameObject.SetActive(false);
            ShowOptions();
        }
        static int WeatherGroup(VocabularyEntry word)
        {
            if(word.id=="weather_wind") return 2;
            if(word.id=="weather_sunny" || word.id=="weather_rainbow") return 1;
            return 0;
        }
        void ShowOptions()
        {
            foreach(var token in tokens) { token.gameObject.SetActive(false); Destroy(token.gameObject); }
            tokens.Clear(); selected=null;
            options.Clear();
            var candidates=TemperatureStage ? definition.tasks.Select(t=>t.context).Where(w=>w).Distinct().ToList() : definition.pool.Distinct().ToList();
            Shuffle(candidates);
            options.Add(AnswerWord);
            foreach(var word in candidates) {
                if(options.Contains(word)) continue;
                if(!TemperatureStage && options.Any(w=>WeatherGroup(w)==WeatherGroup(word))) continue;
                options.Add(word);
                if(options.Count==3) break;
            }
            Shuffle(options);
            for(int i=0;i<options.Count;i++) {
                var word=options[i];
                float x=options.Count>3 ? .9f+(i%2)*3.8f : 2.8f;
                float y=1.3f-(options.Count>3 ? i/2 : i)*1.15f;
                var token=Token(word,new Vector2(x,y),"weather",t=>Choose(word),word.lemko,false);
                var size=new Vector2(options.Count>3 ? 3.6f : 5f,.9f);
                Pixels.Card(token.backing,size);
                token.GetComponent<BoxCollider2D>().size=size;
                token.caption.rectTransform.sizeDelta=size-new Vector2(.25f,.12f);
                token.caption.enableAutoSizing=true;
                token.caption.fontSizeMin=2;
                token.caption.fontSizeMax=5;
            }
        }
        protected override bool HasAnswer=>selected && !temperatureAnimating;

        IEnumerator ShowTemperature()
        {
            temperatureAnimating=true;
            float start=temperature.value;
            if(!GameSession.ReducedMotion) {
                for(float elapsed=0;elapsed<.7f;elapsed+=Time.unscaledDeltaTime) {
                    float progress=Mathf.SmoothStep(0,1,elapsed/.7f);
                    temperature.SetValueWithoutNotify(Mathf.Lerp(start,ObservedTemperature,progress));
                    TemperatureChanged(temperature.value);
                    yield return null;
                }
            }
            temperature.SetValueWithoutNotify(ObservedTemperature);
            TemperatureChanged(ObservedTemperature);
            temperatureAnimating=false;
        }
        void TemperatureChanged(float value)
        {
            if(temperatureLabel) temperatureLabel.text=value.ToString("0")+" °C";
        }
        public void Choose(VocabularyEntry word)
        {
            if(!Ready || temperatureAnimating || !options.Contains(word)) return;
            selected=word;
            foreach(var token in tokens) if(token.Key=="weather")
                token.backing.color=token.view.Entry==word ? Look.Honey : Look.Paper;
        }
        public override void Submit()
        {
            if(!Ready || temperatureAnimating) return;
            if(!selected) return;
            bool correct=selected==AnswerWord;
            if(!TemperatureStage && correct) {
                Session.RecordAnswer(Current.word,true);
                Session.PresentFeedback(true);
                TemperatureStage=true;
                prompt.text="What is the temperature?";
                temperature.gameObject.SetActive(true);
                temperatureLabel.gameObject.SetActive(true);
                ShowOptions();
                StartCoroutine(ShowTemperature());
                return;
            }
            Answer(correct,(TemperatureStage ? "temperature:" : "weather:")+selected.id,"");
        }
        public override void Close()
        {
            if(temperature) temperature.onValueChanged.RemoveListener(TemperatureChanged);
            base.Close(); selected=null; TemperatureStage=false; temperatureAnimating=false; options.Clear();
        }
    }
}
