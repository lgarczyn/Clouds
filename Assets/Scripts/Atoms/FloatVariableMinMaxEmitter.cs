using System;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Atoms
{
  [CreateAssetMenu(fileName = "MinMaxEmitter", menuName = "Atoms", order = 0)]
  public class FloatVariableMinMaxEmitter : ScriptableObject
  {
    [SerializeField] FloatVariable source;
    [SerializeField] float minValue;
    [SerializeField] float maxValue;
    [SerializeField] FloatEvent onReachMin;
    [SerializeField] FloatEvent onLeaveMin;
    [SerializeField] FloatEvent onReachMax;
    [SerializeField] FloatEvent onLeaveMax;

    void OnEnable()
    {
      source.ChangedWithHistory.Register(OnChange);
    }

    void OnDisable()
    {
      source.ChangedWithHistory.Unregister(OnChange);
    }

    void OnChange(FloatPair values)
    {
      if (values.Item1 == values.Item2) return;
      (float newValue, float oldValue) = values; 
      if (onReachMin && newValue <= minValue && oldValue > minValue) {
        onReachMin.Raise(newValue);
      }
      if (onLeaveMin && newValue > minValue && oldValue <= minValue) {
        onLeaveMin.Raise(newValue);
      }
      if (onReachMax && newValue >= maxValue && oldValue < maxValue) {
        onReachMax.Raise(newValue);
      }
      if (onLeaveMax && newValue < maxValue && oldValue >= maxValue) {
        onLeaveMax.Raise(newValue);
      }
    }
  }
}