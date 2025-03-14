﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityAtoms;
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
    Maximum
  }

  public delegate void MixFloat(float value); 

  public class FloatMixer : MonoBehaviour
  {
    [SerializeField] MixingType mixing = MixingType.Multiply;
    [SerializeField] List<FloatReference> inputs;
    public event MixFloat onMixFloat;

    public bool TryGetLastOutput(out float lastOutput)
    {
      lastOutput = _lastOutput;
      return !float.IsNaN(_lastOutput);
    }

    float _lastOutput = float.NaN;

    List<FloatEvent> _listeners;

    void OnEnable()
    {
      AddListeners();
      Raise();
    }

    void OnDisable()
    {
      RemoveListeners();
    }

    void OnValidate()
    {
      if (inputs.Any(r => r == null || r.IsUnassigned)) Debug.LogWarning("Unassigned input on FloatMixer", this);
      if (!Application.isPlaying) return;
      RemoveListeners();
      AddListeners();
      Raise();
    }

    void AddListeners()
    {
      _listeners = inputs
        .Where(r => r.Usage != AtomReferenceUsage.VALUE)
        .Where(r => r.Usage != AtomReferenceUsage.CONSTANT)
        .Select(r => r.GetEvent<FloatEvent>())
        .ToList();
      _listeners.ForEach(e => e.Register(Raise));
    }

    void RemoveListeners()
    {
      _listeners?.ForEach(e => e.Unregister(Raise));
      _listeners = null;
    }

    void Raise()
    {
      IEnumerable<float> it = inputs
        .Where(r => !r.IsUnassigned)
        .Select(i => i.Value);

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

      onMixFloat?.Invoke(output);
    }
  }
}