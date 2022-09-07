using System.Collections.Generic;

public static class IEnumerableHelper
{
  public static void Do<T>(this IEnumerable<T> list, System.Action<T> action)
  {
    foreach (T item in list)
    {
      action(item);
    }
  }
}