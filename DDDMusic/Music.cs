﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DDD
{
    [RequireComponent(typeof(AudioSource))]
    public class Music : MonoBehaviour
    {
        [SerializeField]
        private List<MusicSectionDefinition> sectionDefinitions = new List<MusicSectionDefinition>();

        private List<MusicSection> sections = new List<MusicSection>();

        public delegate void UpdateHandler(int sectionIndex,int barIndex,int beatIndex);

        public event UpdateHandler OnUpdateBeat;

        private int lastPlayedSectionIndex = -1;
        private int lastPlayedBarIndex = -1;
        private int lastPlayedBeatIndex = -1;

        private static readonly float TransitionTime = 1.0f;

        public bool IsPlaying { get { return sections.Any(e => e.IsPlaying); } }

        public AudioDataLoadState LoadState { get; private set; }

        public float Time
        {
            get
            {
                var playingSection = sections.Find(x => x.IsPlaying);
                if (playingSection == null)
                {
                    return 0.0f;
                }

                return playingSection.Time;
            }
        }

        void Awake()
        {
            LoadState = AudioDataLoadState.Loading;
        }

        IEnumerator Start()
        {
            var source = GetComponent<AudioSource>();

            while (source.clip.loadState == AudioDataLoadState.Loading)
            {
                yield return new WaitForSeconds(1.0F);
            }

            if (source.clip.loadState == AudioDataLoadState.Failed)
            {
                LoadState = AudioDataLoadState.Failed;
                yield break;
            }

            var offsetTime = 0.0f;
            sections = sectionDefinitions.ConvertAll(e =>
            {
                var section = new GameObject(e.Name + "_Section", typeof(MusicSection)).GetComponent<MusicSection>();
                section.SetSource(source, offsetTime, e);

                offsetTime += e.SecondsInSection;
                return section;
            });

            var playOnAwake = source.playOnAwake;

            source.playOnAwake = false;
            source.enabled = false;

            if (playOnAwake)
            {
                Play();
            }

            LoadState = AudioDataLoadState.Loaded;
        }

        void Update()
        {
            var currentSectionIndex = sections.FindIndex(e => e.IsPlaying);
            if (currentSectionIndex == -1)
            {
                return;
            }

            var currentSection = sections [currentSectionIndex];

            var currentBarIndex = currentSection.CurrentBar;
            var currentBeatIndex = currentSection.CurrentBeat;

            if (lastPlayedSectionIndex == currentSectionIndex && lastPlayedBarIndex == currentBarIndex && lastPlayedBeatIndex == currentBeatIndex)
            {
                // Do not count the same beat twice.
                return;
            }

            if (currentSection.Progress == 1.0f)
            {
                // Do not count the end of the section.
                // It should be counted as the first beat of the next section.
                return;
            }

            lastPlayedSectionIndex = currentSectionIndex;
            lastPlayedBarIndex = currentBarIndex;
            lastPlayedBeatIndex = currentBeatIndex;

            var handlers = OnUpdateBeat;
            if (handlers != null)
            {
                handlers(currentSectionIndex, currentBarIndex, currentBeatIndex);
            }

            var remainingTime = currentSection.RemainingTime;
            if (remainingTime < TransitionTime)
            {
                if (currentSection.ShouldLoop || currentSection.ShouldEnd || currentSectionIndex == sections.Count - 1)
                {
                    return;
                }

                var nextSection = sections [currentSectionIndex + 1];
                nextSection.PlayDelayed(remainingTime);
            }
        }

        public void Play()
        {
            if (lastPlayedSectionIndex == -1)
            {
                sections [0].Play();
                return;
            }

            sections [lastPlayedSectionIndex].Play();
        }

        public void Skip()
        {
            var currentSectionIndex = sections.FindIndex(e => e.IsPlaying);
            if (currentSectionIndex == -1)
            {
                return;
            }

            var currentSection = sections [currentSectionIndex];

            if (currentSection.ShouldEnd || currentSectionIndex == sections.Count - 1)
            {
                return;
            }

            var nextSection = sections [currentSectionIndex + 1];
            nextSection.Play();
            currentSection.Stop();
        }

        public void Stop()
        {
            sections.ForEach(e => e.Stop());

            lastPlayedSectionIndex = -1;
            lastPlayedBarIndex = -1;
            lastPlayedBeatIndex = -1;
        }
    }
}
