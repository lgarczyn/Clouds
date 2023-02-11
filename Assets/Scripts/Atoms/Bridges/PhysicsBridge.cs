using UnityEngine;

namespace Atoms.Bridges
{
  [CreateAssetMenu(fileName = "Physics", menuName = "Atoms/Bridges/Physics", order = 0)]
  public class PhysicsBridge : ScriptableObject
  {
    public void SetGravity(float gravity)
    {
      Physics.gravity = Vector3.down * gravity;
    }
  }
}