using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using System.Linq;
using Text = TMPro.TextMeshProUGUI;

[System.Serializable]
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
}

[System.Serializable]
public enum WarningLevel
{
  Notice,
  Warning,
  Danger,
}

[System.Serializable]
public struct WarningInfo
{
  public WarningLevel level;
  public float timeout;
  public string message;
}

[System.Serializable]
public class WarningLevelColorMap : SerializableDictionaryBase<WarningLevel, Color> { };

[System.Serializable]
public class WarningInfoMap : SerializableDictionaryBase<WarningType, WarningInfo> { };

[RequireComponent(typeof(Text))]
public class WarningManager : Manager<WarningManager>
{
  [BoundedCurve]
  [SerializeField] AnimationCurve alphaOverLifetime;

  [SerializeField] WarningLevelColorMap warningColors;

  [SerializeField] WarningInfoMap warningInfo;

  Dictionary<WarningType, float> timers = new Dictionary<WarningType, float>();

  public void SendWarning(WarningType type)
  {
    WarningInfo info;

    if (!warningInfo.TryGetValue(type, out info))
    {
      Debug.LogWarning("No info for warning type: " + type);
    }

    timers[type] = Mathf.Max(timers.GetValueOrDefault(type), info.timeout);
  }

  void LateUpdate()
  {
    foreach (WarningType w in timers.Keys.ToArray())
    {
      timers[w] = Mathf.Max(timers[w] - Time.deltaTime, 0f);
    }

    string message = timers
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

    GetComponent<Text>().text = message;
  }
}
