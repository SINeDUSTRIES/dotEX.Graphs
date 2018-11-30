using System.Collections.Generic;

namespace SINeDUSTRIES.Collections
{
  /// <summary>
  /// Hierarchial collection of <typeparamref name="TID"/> which map to an <typeparamref name="TElement"/>s;
  /// ie, a <see cref="IDictionary{TKey, TValue}"/> whose keys are organized as a <see cref="IHierarchyChildManyKey{TElement, TKeyParent, TKeyChild}"/>;
  /// </summary>
  /// <notes>
  /// There is no point in a <see cref="IPool{TElement, TID}"/> that is not <typeparamref name="TID"/>, cause that would just be a tree!;
  /// No type parameter for <see cref="QuickGraph.IEdge{TVertex}"/>, as that is an implementation detail;
  /// + Why no <see cref="IHierarchyChildManyKey{TElement, TKeyParent, TKeyChild}"/>
  ///   - as a hierarchy of <typeparamref name="TID"/> where the child <typeparamref name="TID"/> includes the parent, it would reptitive
  ///   - ie: ChildGet("//parent", "//parent/child");
  /// </notes>
  public interface IPool<TID, TElement> :

    // collection

    IAdd<TElement, TElement>,
    IAddMap<TID, TElement>,

    IRemove<TElement>,
    IRemoveMap<TID>,

    IClear,

    // get

    IGet<TID, TElement>,
    IGetTry<TID, TElement>,

    IGetKey<TElement, TID>,
    IGetKeyTry<TElement, TID>,

    // checks

    IContains<TElement>,
    IContainsKey<TID>,

    ICount,

    // enum

    IEnumerable<KeyValuePair<TID, TElement>>,

    //IHierarchyChildManyKey<TElement, TID, TID>,
    IHierarchyChildManyEnumerable<TID>
  {
    /// <summary>
    /// All <typeparamref name="TID"/>;
    /// Used an unsued;
    /// </summary>
    IEnumerable<TID> IDs { get; }
  }
}