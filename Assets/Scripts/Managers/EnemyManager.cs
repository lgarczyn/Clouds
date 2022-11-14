using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : Manager<EnemyManager>
{
  public Transform enemyParent
  {
    get
    {
      return transform;
    }
  }
}
