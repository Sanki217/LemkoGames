using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Lemko
{
    // Every button answers the pointer: a small swell on hover and a squash on press.
    public sealed class JuicyButton : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerClickHandler
    {
        Selectable control;
        void Awake() { control=GetComponent<Selectable>(); }
        bool Live=>!control || control.IsInteractable();
        public void OnPointerEnter(PointerEventData e) { if(Live) Juice.Punch(transform,.05f,.18f); }
        public void OnPointerDown(PointerEventData e) { if(Live) Juice.Squash(transform,.09f); }
        public void OnPointerClick(PointerEventData e) { if(Live) Juice.Sound("click"); }
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
        }
    }
}
