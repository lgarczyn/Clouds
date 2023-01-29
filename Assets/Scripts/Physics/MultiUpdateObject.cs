using UnityEngine;

public abstract class MultiUpdateObject : MonoBehaviour
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

  bool resetMultiUpdateBaseCalled = false;
  bool beforeUpdatesBaseCalled = false;
  bool afterUpdatesBaseCalled = false;

  protected void OnEnable()
  {
    ResetMultiUpdate(Time.time);
    if (!resetMultiUpdateBaseCalled) Debug.LogError("Override did not call base.BeforeUpdates()");

    AfterEnable();
  }

  protected virtual void ResetMultiUpdate(float time)
  {
    timeOfLastUpdate = currentTime = prevFrameTime = nextFrameTime = time;
    timeToNextUpdate = 0;
    rofMultiplier = 1f;
    resetMultiUpdateBaseCalled = true;
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

    // Allow child classes to setup their multi update sequence
    BeforeUpdates();

    // Verify there was a base.BeforeUpdates call
    if (!beforeUpdatesBaseCalled) Debug.LogError("Override did not call base.BeforeUpdates()", this);
    beforeUpdatesBaseCalled = false;

    CallMultiUpdates();
    // Allow child classes to clean their multi update sequence
    AfterUpdates();

    // Verify there was a base.AfterUpdates call
    if (!afterUpdatesBaseCalled) Debug.LogError("Override did not call base.AfterUpdates()", this);
    afterUpdatesBaseCalled = false;
  }

  protected abstract Wait MultiUpdate(float deltaTime);

  protected void SetRofMultiplier(float multiplier)
  {
    rofMultiplier = multiplier;
  }

  protected virtual void BeforeUpdates()
  {
    beforeUpdatesBaseCalled = true;
  }
  protected virtual void AfterUpdates()
  {
    afterUpdatesBaseCalled = true;
  }

  protected virtual void AfterEnable() { }
}
