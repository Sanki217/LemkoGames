using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Lemko
{
    // Every button answers the pointer: it rises a little under it, squashes when pressed and springs back.
    public sealed class JuicyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
    {
        Selectable control;
        void Awake() { control=GetComponent<Selectable>(); }
        bool Live=>!control || control.IsInteractable();
        public void OnPointerEnter(PointerEventData e) { if(Live) Juice.Hover(transform,true); }
        public void OnPointerExit(PointerEventData e) { Juice.Hover(transform,false); }
        public void OnPointerDown(PointerEventData e) { if(Live && e.button==PointerEventData.InputButton.Left) Juice.Squash(transform,.16f); }
        void OnDisable() { Juice.Hover(transform,false); }
        public void OnPointerClick(PointerEventData e) { if(Live && e.button==PointerEventData.InputButton.Left) Juice.Sound("click"); }
    }
    public static class PolishBootstrap
    {
        static bool hooked;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset() { hooked=false; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Begin()
        {
            if(!hooked) { hooked=true; UnityEngine.SceneManagement.SceneManager.sceneLoaded+=(scene,mode)=>Attach(); }
            Attach(); Juice.Boot();
        }
        // The polish layer installs itself beside every GameSession, so no scene needs editing.
        static void Attach()
        {
            foreach(var session in Object.FindObjectsByType<GameSession>(FindObjectsSortMode.None))
                if(session.hud && session.tooltip && !session.GetComponent<ScenePolish>()) session.gameObject.AddComponent<ScenePolish>();
            foreach(var session in Object.FindObjectsByType<GameSession>(FindObjectsSortMode.None))
                if(!session.GetComponent<LangApply>()) session.gameObject.AddComponent<LangApply>();
        }
    }
}
