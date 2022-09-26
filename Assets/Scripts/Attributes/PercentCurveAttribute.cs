using UnityEngine;
using System.Collections;

public class PercentCurveAttribute : BoundedCurveAttribute
{
  public PercentCurveAttribute(int height = 1) : base(new Rect(0, 0, 100, 1), height)
  {

  }
}

