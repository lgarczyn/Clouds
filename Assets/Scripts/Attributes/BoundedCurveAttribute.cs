using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundedCurveAttribute : PropertyAttribute
{
  public Rect bounds;
  public int height;

  public BoundedCurveAttribute(Rect bounds, int height = 1)
  {
    this.bounds = bounds;
    this.height = height;
  }

  public BoundedCurveAttribute(int height = 1)
  {
    this.bounds = new Rect(0, 0, 1, 1);
    this.height = height;
  }
}
