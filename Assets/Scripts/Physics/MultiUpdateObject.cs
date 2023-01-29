using UnityEngine;

public abstract class MultiUpdateObject : MonoBehaviour
{

  protected double prevFrameTime;
  protected double currentTime;
  protected double nextFrameTime;
  protected double timeToNextUpdate;
  protected double timeOfLastUpdate;
  protected double rofMultiplier;

  protected double frameRatio
  {
    get
    {
      return (currentTime - prevFrameTime) / (nextFrameTime - prevFrameTime);
    }
  }

  protected double deltaTime
  {
    get
    {
      return currentTime - timeOfLastUpdate;
    }
  }

  protected double timeToEndOfFrame
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
    ResetMultiUpdate(Time.timeAsDouble);
    if (!resetMultiUpdateBaseCalled) Debug.LogError("Override did not call base.BeforeUpdates()");

    AfterEnable();
  }

  protected virtual void ResetMultiUpdate(double time)
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
    public double waitingTime;

    private Wait(WaitType type, double forDelay = 0)
    {
      waitingType = type;
      waitingTime = forDelay;
    }

    public static Wait ForNothing()
    {
      return new Wait(WaitType.Time, 0f);
    }

    public static Wait For(double delay)
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
    double remainingTime = nextFrameTime - prevFrameTime;
    remainingTime *= rofMultiplier;

    int safety = 0;
    if (remainingTime == 0)
      return;

    while (timeToNextUpdate <= remainingTime)
    {
      remainingTime -= timeToNextUpdate;

      currentTime += timeToNextUpdate / rofMultiplier;
      double deltaTime = currentTime - timeOfLastUpdate;
      timeOfLastUpdate = currentTime;

      Wait wait = MultiUpdate(deltaTime);

      switch (wait.waitingType)
      {
        case Wait.WaitType.Frame:
          timeToNextUpdate = 0;
          return;
        case Wait.WaitType.Ever:
          timeToNextUpdate = double.MaxValue;
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
    nextFrameTime = Time.timeAsDouble;
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

  protected abstract Wait MultiUpdate(double deltaTime);

  protected void SetRofMultiplier(double multiplier)
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
