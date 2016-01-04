using UnityEngine;
using System;

namespace DDD
{
    [Serializable]
    public class MusicSectionDefinition
    {
        public enum TransitionType
        {
            Through,
            Loop,
            End
        }

        [SerializeField]
        private string name;

        [SerializeField]
        private int bpm = 120;

        [SerializeField]
        private int barsInSection = 8;

        [SerializeField]
        private int beatsInBar = 4;

        [SerializeField]
        private TransitionType transition;

        public string Name
        {
            get { return name; }
        }

        public int BPM
        {
            get { return bpm; }
        }

        public int BarsInSection
        {
            get { return barsInSection; }
        }

        public int BeatsInSection
        {
            get { return barsInSection * beatsInBar; }
        }

        public int BeatsInBar
        {
            get { return beatsInBar; }
        }

        public float SecondsInSection
        {
            get { return (float)BeatsInSection / (float)BPM * 60.0f; }
        }

        public TransitionType Transition
        {
            get { return transition; }
        }
    }
}