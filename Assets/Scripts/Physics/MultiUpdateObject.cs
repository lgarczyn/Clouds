using UnityEngine;

public class MultiUpdateObject : MonoBehaviour
{

  protected float prevFrameTime;
  protected float currentTime;
  protected float nextFrameTime;
  protected float timeToNextUpdate;
  protected float timeOfLastUpdate;
  protected float rofMultiplier;

  protected float frameRatio
  {
    get
    {
      return (currentTime - prevFrameTime) / (nextFrameTime - prevFrameTime);
    }
  }

  protected float deltaTime
  {
    get
    {
      return currentTime - timeOfLastUpdate;
    }
  }

  protected float timeToEndOfFrame
  {
    get
    {
      return nextFrameTime - currentTime;
    }
  }

  protected void OnEnable()
  {
    ResetMultiUpdate(Time.time);
    AfterEnable();
  }

  protected virtual void ResetMultiUpdate(float time)
  {
    timeOfLastUpdate = currentTime = prevFrameTime = nextFrameTime = time;
    timeToNextUpdate = 0;
    rofMultiplier = 1f;
  }

  [System.Serializable]
  protected struct Wait
  {
    public enum WaitType
    {
      Time,
      Frame,
      Ever
    }

    public WaitType waitingType;
    public float waitingTime;

    private Wait(WaitType type, float forDelay = 0)
    {
      waitingType = type;
      waitingTime = forDelay;
    }

    public static Wait ForNothing()
    {
      return new Wait(WaitType.Time, 0f);
    }

    public static Wait For(float delay)
    {
      return new Wait(WaitType.Time, delay);
    }

    public static Wait ForFrame()
    {
      return new Wait(WaitType.Frame);
    }

    public static Wait ForEver()
    {
      return new Wait(WaitType.Ever);
    }

  }

  void CallMultiUpdates()
  {
    float remainingTime = nextFrameTime - prevFrameTime;
    remainingTime *= rofMultiplier;

    int safety = 0;
    if (remainingTime == 0)
      return;

    while (timeToNextUpdate <= remainingTime)
    {
      remainingTime -= timeToNextUpdate;

      currentTime += timeToNextUpdate / rofMultiplier;
      timeOfLastUpdate = currentTime;

      Wait wait = MultiUpdate(timeToNextUpdate);

      switch (wait.waitingType)
      {
        case Wait.WaitType.Frame:
          timeToNextUpdate = 0;
          return;
        case Wait.WaitType.Ever:
          timeToNextUpdate = float.MaxValue;
          this.enabled = false;
          return;
        case Wait.WaitType.Time:
          timeToNextUpdate = wait.waitingTime;
          break;
      }

      safety++;
      if (safety > 10000)
      {
        enabled = false;
        Debug.LogError("Too many frames. Endless loop probable.");
        break;
      }
    }
    timeToNextUpdate -= remainingTime;
  }

  void FixedUpdate()
  {
    prevFrameTime = nextFrameTime;
    nextFrameTime = Time.time;
    currentTime = prevFrameTime;

    BeforeUpdates();
    CallMultiUpdates();
    AfterUpdates();
  }

  protected virtual Wait MultiUpdate(float deltaTime)
  {
    return Wait.ForFrame();
  }

  protected void SetRofMultiplier(float multiplier)
  {
    rofMultiplier = multiplier;
  }

  protected virtual void BeforeUpdates() { }
  protected virtual void AfterUpdates() { }

  protected virtual void AfterEnable() { }
}
