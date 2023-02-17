using JetBrains.Annotations;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;

namespace Atoms
{
    public class SmoothDampFloatVariable: MonoBehaviour
    {
        [SerializeField] FloatEventReference input;
        [SerializeField] [CanBeNull] FloatEvent atomOutput;
        [SerializeField] UnityEvent<float> unityOutput;

        [SerializeField] float smoothTime;
        [SerializeField] float maxSpeed;

        float _target;
        float _value;
        float _speed;
        double _timeOfLastUpdate;

        void OnEnable()
        {
            input.Event.Register(SetTarget);
        }

        void OnDisable()
        {
            input.Event.Unregister(SetTarget);
        }

        public void SetTarget(float target)
        {
            _target = target;
            UpdateValue();
        }

        void Raise()
        {
            if (atomOutput) atomOutput.Raise(_value);
            unityOutput.Invoke(_value);
        }

        void UpdateValue()
        {
            if (_timeOfLastUpdate == 0)
            {
                _timeOfLastUpdate = Time.timeAsDouble;
                _value = _target;
                Raise();
                return;
            }

            // Compare between float is bad, but smoothdamp ends with perfect equality
            if (_value.Equals(_target))
            {
                // _value is supposed to be already sent
                return;
            }

            float deltaTime = (float)(Time.timeAsDouble - _timeOfLastUpdate); 
            _timeOfLastUpdate = Time.timeAsDouble;
            _value = Mathf.SmoothDamp(_value, _target, ref _speed, smoothTime, maxSpeed, deltaTime);
            Raise();
        }

        void Start()
        {
            UpdateValue();
        }

        void Update()
        {
            UpdateValue();
        }
    }
}