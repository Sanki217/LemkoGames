using UnityEngine;
namespace Lemko
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class PronunciationService : MonoBehaviour
    {
        AudioSource source;
        AudioSource Source => source ? source : (source = GetComponent<AudioSource>());
        public void SetMuted(bool muted) { Source.mute = muted; if (muted) Stop(); }
        public void Play(AudioClip clip) { Stop(); if (clip) { Source.clip = clip; Source.Play(); } }
        public void Stop() => Source.Stop();
        void OnDisable() => Stop();
    }
}
