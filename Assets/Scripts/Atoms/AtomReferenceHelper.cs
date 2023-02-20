using UnityAtoms;
using UnityEngine;

namespace Atoms
{
  static class AtomReferenceHelper
  {

    static TE GetFakeEvent<T, TE>(T replayValue)
      where TE : AtomEvent<T>
    {
      TE output = ScriptableObject.CreateInstance<TE>();
      // Add replay value to buffer, so that new listeners get one event
      output.Raise(replayValue);
      return output;
    }
    
    public static TE1 GetOrCreateEvent<T, TP, TC, TV, TE1, TE2, TF, TVi>(this AtomReference<T, TP, TC, TV, TE1, TE2, TF, TVi> reference)
      where TP : struct, IPair<T>
      where TC : AtomBaseVariable<T>
      where TV : AtomVariable<T, TP, TE1, TE2, TF>
      where TE1 : AtomEvent<T>
      where TE2 : AtomEvent<TP>
      where TF : AtomFunction<T, T>
      where TVi : AtomVariableInstancer<TV, TP, T, TE1, TE2, TF>
    {
      switch (reference.Usage)
      {
        case AtomReferenceUsage.VALUE :
        case AtomReferenceUsage.CONSTANT : return GetFakeEvent<T, TE1>(reference.Value);
        default: return reference.GetEvent<TE1>();
      }
    }
  }
}