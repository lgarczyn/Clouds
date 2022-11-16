using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayTests
{
  public class FakeTargetManager : Manager<ITargetManager>, ITargetManager
  {
    [SerializeField][RequiredComponent] FakeTarget reqFakeTarget;

    public ITarget GetTarget() => reqFakeTarget;
  }

}
