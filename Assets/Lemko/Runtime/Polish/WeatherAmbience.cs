using UnityEngine;
namespace Lemko
{
    // Presentation only: the sky answers the weather picture the player picks.
    public sealed class WeatherAmbience : MonoBehaviour
    {
        enum Mood { Clear, Sun, Rain, Snow, Dark }
        SpriteRenderer tint; Mood mood; Color target; float nextDrop;
        public static void Show(Transform taskRoot,VocabularyEntry word)
        {
            if(!taskRoot || !word) return;
            var ambience=taskRoot.GetComponentInChildren<WeatherAmbience>();
            if(!ambience) {
                var go=new GameObject("Weather ambience",typeof(SpriteRenderer)); go.transform.SetParent(taskRoot,false); go.transform.localPosition=new Vector3(0,-.6f,0);
                ambience=go.AddComponent<WeatherAmbience>(); ambience.tint=go.GetComponent<SpriteRenderer>(); ambience.tint.sprite=Look.Square; ambience.tint.sortingOrder=-9;
                var size=Look.Square.bounds.size; go.transform.localScale=new Vector3(16.2f/size.x,7.8f/size.y,1); ambience.tint.color=Color.clear;
            }
            ambience.Set(Read(word.id));
        }
        static Mood Read(string id)
        {
            id=(id??"").ToLowerInvariant();
            if(id.Contains("snow") || id.Contains("frost") || id.Contains("blizzard") || id.Contains("ice") || id.Contains("winter")) return Mood.Snow;
            if(id.Contains("rain") || id.Contains("storm") || id.Contains("thunder") || id.Contains("lightning") || id.Contains("moisture")) return Mood.Rain;
            if(id.Contains("night") || id.Contains("dark") || id.Contains("moon") || id.Contains("stars")) return Mood.Dark;
            if(id.Contains("sun") || id.Contains("hot") || id.Contains("heat") || id.Contains("warm") || id.Contains("summer") || id.Contains("bright")) return Mood.Sun;
            return Mood.Clear;
        }
        void Set(Mood value)
        {
            mood=value;
            target=value==Mood.Sun ? Look.Alpha(Look.Honey,.22f) : value==Mood.Rain ? new Color(.36f,.47f,.62f,.3f) : value==Mood.Snow ? new Color(1,1,1,.38f) : value==Mood.Dark ? new Color(.16f,.18f,.3f,.38f) : Color.clear;
        }
        void Update()
        {
            tint.color=GameSession.ReducedMotion ? target : Color.Lerp(tint.color,target,1-Mathf.Exp(-5*Time.unscaledDeltaTime));
            if((mood!=Mood.Rain && mood!=Mood.Snow) || Time.unscaledTime<nextDrop) return;
            nextDrop=Time.unscaledTime+(mood==Mood.Rain ? .035f : .09f);
            Juice.Fall(mood==Mood.Snow);
        }
    }
}
