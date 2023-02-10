using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;

namespace Atoms
{
    public class AnimationCurveFloatProcessor : MonoBehaviour {

        [SerializeField] FloatEventReference input;
        [SerializeField] FloatEvent outputAtom;
        [SerializeField] UnityEvent<float> outputUnity;
        [SerializeField] AnimationCurve processor;

        void OnEnable()
        {
            input.Event.Register(SetValue);
        }

        public void SetValue(float value)
        {
            float newValue = processor.Evaluate(value);
            if (outputAtom) outputAtom.Raise(newValue);
            outputUnity.Invoke(newValue);
        }

        void OnDisable()
        {
            input.Event.Unregister(SetValue);
        }
    }
}