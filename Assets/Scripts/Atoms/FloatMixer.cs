using System;
using System.Collections.Generic;
using System.Linq;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;

namespace Atoms
{
    [Serializable]
    public enum MixingType
    {
        Average,
        Multiply,
        Add,
        Minimum,
        Maximum,
    }
    public class FloatMixer : MonoBehaviour
    {
        // TODO: change to some kind of managed event list
        [SerializeField] MixingType mixing;
        [SerializeField] List<FloatReference> inputs;
        [SerializeField] UnityEvent<float> unityOutput;
        [SerializeField] UnityEvent<Vector3> scaleOutput;
        [SerializeField] FloatEvent atomOutput;

        float _lastOutput = float.NaN;

        void Raise()
        {
            IEnumerable<float> it = inputs
                .Select(i => (float)i);

            float output = mixing switch
            {
                MixingType.Average => it.Average(),
                MixingType.Multiply => it.Aggregate((a, b) => a * b),
                MixingType.Add => it.Aggregate((a, b) => a + b),
                MixingType.Minimum => it.Min(),
                MixingType.Maximum => it.Max(),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (_lastOutput.Equals(output)) return;
            _lastOutput = output;
            
            if (atomOutput) atomOutput.Raise(output);
            unityOutput.Invoke(output);
            scaleOutput.Invoke(output * Vector3.one);
        }

        void Update() => Raise();

        void LateUpdate() => Raise();
        void FixedUpdate() => Raise();
        void Start() => Raise();
    }
}