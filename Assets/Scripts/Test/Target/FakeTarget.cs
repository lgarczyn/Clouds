using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayTests
{

  public class FakeTarget : MonoBehaviour, ITarget
  {

    Vector3 lastPosition;

    public Vector3 Position => transform.position;

    public Vector3 Velocity => _velocity;

    Vector3 _velocity;

    void Awake()
    {
      lastPosition = transform.position;
      _velocity = Vector3.zero;
    }

    void LateUpdate()
    {
      if (Time.deltaTime == 0f) return;

      Vector3 newPosition = transform.position;
      _velocity = (newPosition - lastPosition) / (Time.deltaTime);

      lastPosition = newPosition;
    }

    public bool IsVisible(Vector3 seekerPosition)
    {
      return true;
    }

  }

}
