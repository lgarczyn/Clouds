using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayTests
{

  public class FakeTarget : MonoBehaviour, ITarget
  {

    Vector3 lastPosition;

    public Vector3 position => transform.position;

    public Vector3 velocity => _velocity;

    public Vector3 _velocity;

    void Awake()
    {
      lastPosition = transform.position;
      _velocity = Vector3.zero;
    }

    void LateUpdate()
    {
      if (Time.deltaTime == 0f) return;

      _velocity = (transform.position - lastPosition) / (Time.deltaTime);

      lastPosition = transform.position;
    }

    public bool IsVisible(Vector3 position)
    {
      return true;
    }

  }

}
