using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayTests {

  [RequireComponent(typeof(FakeTarget))]
  public class FakeTargetManager : Manager<ITargetManager>, ITargetManager
  {
    public ITarget GetTarget()
    {
      return GetComponent<FakeTarget>();
    }
  }

}
