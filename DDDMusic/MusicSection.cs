using UnityEngine;

namespace DDD
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicSection : MonoBehaviour
    {
        private AudioSource sectionSource;

        private MusicSectionDefinition sectionDefinition;

        public bool IsPlaying
        {
            get { return sectionSource.isPlaying; }
        }

        public float Progress
        {
            get { return (float)sectionSource.timeSamples / (float)sectionSource.clip.samples; }
        }

        public int CurrentBar
        {
            get { return Mathf.FloorToInt(sectionDefinition.BarsInSection * Progress); }
        }

        public int CurrentBeat
        {
            get { return Mathf.FloorToInt(sectionDefinition.BeatsInSection * Progress) % sectionDefinition.BeatsInBar; }
        }

        public float RemainingTime
        {
            get { return (float)(sectionSource.clip.samples - sectionSource.timeSamples) / (float)sectionSource.clip.frequency; }
        }

        public bool ShouldLoop
        {
            get { return sectionDefinition.Transition == MusicSectionDefinition.TransitionType.Loop; }
        }

        public bool ShouldEnd
        {
            get { return sectionDefinition.Transition == MusicSectionDefinition.TransitionType.End; }
        }

        void Awake()
        {
            sectionSource = GetComponent<AudioSource>();
        }

        public void SetSource(AudioSource wholeSource, float offsetTime, MusicSectionDefinition definition)
        {
            var samples = Mathf.FloorToInt(wholeSource.clip.frequency * definition.SecondsInSection);
            var offsetSamples = Mathf.FloorToInt(wholeSource.clip.frequency * offsetTime);

            var sectionClip = AudioClip.Create(definition.Name + "_Clip", samples, wholeSource.clip.channels, wholeSource.clip.frequency, false);
            var sectionData = new float[samples * wholeSource.clip.channels];
            wholeSource.clip.GetData(sectionData, offsetSamples);
            sectionClip.SetData(sectionData, 0);

            sectionSource.clip = sectionClip;
            sectionSource.playOnAwake = false;
            sectionSource.loop = (definition.Transition == MusicSectionDefinition.TransitionType.Loop);
            sectionSource.volume = wholeSource.volume;

            this.transform.SetParent(wholeSource.transform);
            this.sectionDefinition = definition;
        }

        public void Play()
        {
            sectionSource.Play();
        }

        public void PlayDelayed(float delayTime)
        {
            sectionSource.PlayDelayed(delayTime);
        }

        public void Stop()
        {
            sectionSource.Stop();
        }
    }
}