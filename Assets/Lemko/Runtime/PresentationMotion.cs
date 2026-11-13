using UnityEngine;
namespace Lemko {
    // Kept for existing callers; the motion itself now lives in the shared juice kit.
    public static class PresentationMotion {
        public static void Pulse(Transform target,GameSession session) { if(target && session) Juice.Punch(target,.12f,.28f); }
        public static void Travel(Transform target,Vector3 from,GameSession session) { if(target && session) Juice.TravelFrom(target,from); }
    }
}
