using System;
using System.Collections.Generic;
using System.Linq;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using Text = TMPro.TextMeshProUGUI;

[Serializable]
public enum WarningType
{
  EnemyLock,
  LowShield,
  BrokenShield,
  LowHealth,
  LowBoost,
  RegeneratingShield,
  FullShield,
  EnemySpawn,
  LowLaser
}

[Serializable]
public enum WarningLevel
{
  Notice,
  Warning,
  Danger
}

[Serializable]
public struct WarningInfo
{
  public WarningLevel level;
  public float timeout;
  public string message;
}

[Serializable]
public class WarningLevelColorMap : SerializableDictionaryBase<WarningLevel, Color> { }

[Serializable]
public class WarningInfoMap : SerializableDictionaryBase<WarningType, WarningInfo> { }

public class WarningManager : Manager<WarningManager>
{
  [BoundedCurve] [SerializeField] AnimationCurve alphaOverLifetime;

  [SerializeField] WarningLevelColorMap warningColors;

  [SerializeField] WarningInfoMap warningInfo;

  [SerializeField] [RequiredComponent] Text reqText;

  readonly Dictionary<WarningType, float> _timers = new();

  void LateUpdate()
  {
    foreach (WarningType w in _timers.Keys.ToArray()) _timers[w] = Mathf.Max(_timers[w] - Time.deltaTime, 0f);

    string message = _timers
      .Where(kv => kv.Value > 0f)
      .OrderBy(kv => kv.Value)
      .Select(kv =>
      {
        WarningType type = kv.Key;
        WarningInfo info = warningInfo[type];
        float lifetimeRemaining = kv.Value / info.timeout;
        Color color = warningColors[info.level];
        color.a *= alphaOverLifetime.Evaluate(1 - lifetimeRemaining);
        string colorText = ColorUtility.ToHtmlStringRGBA(color);
        return $"<color=#{colorText}>{info.message}</color>";
      }).Aggregate("", (i, j) => i + "\n" + j);

    reqText.text = message;
  }

  public void SendWarning(WarningType type)
  {
    if (warningInfo.TryGetValue(type, out WarningInfo info))
      _timers[type] = Mathf.Max(_timers.GetValueOrDefault(type), info.timeout);
    else
      Debug.LogWarning("No info for warning type: " + type);
  }
}