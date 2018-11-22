using System;

namespace SINeDUSTRIES.Collections
{
  /// <summary>
  /// Hierarchy of parent(s)-to-childs, where the children can be accessed with a key;
  /// </summary>
  /// <typeparam name="TElement">Type of parent/ child;</typeparam>
  /// <typeparam name="TKeyParent">Type of key used to access parent;</typeparam>
  /// <typeparam name="TKeyChild">Type of key used to acccess children;</typeparam>
  public interface IHierarchyChildManyKey<TElement, TKeyParent, TKeyChild>
  {
    /// <summary>
    /// Get a specific child of <paramref name="parent"/>;
    /// </summary>
    /// <param name="parent"><typeparamref name="TElement"/> whose child to get;</param>
    /// <param name="keyChild">Object used to access child;</param>
    /// <returns>Child gotten;</returns>
    TElement ChildGet(TKeyParent parent, TKeyChild keyChild);

    /// <summary>
    /// Try to get a specific child of <paramref name="parent"/>;
    /// </summary>
    /// <param name="parent"><typeparamref name="TElement"/> whose child to get;</param>
    /// <param name="keyChild">Object used to access child;</param>
    /// <param name="childActual">Child that was got, or default value;</param>
    /// <returns>Got child?</returns>
    Boolean ChildGetTry(TKeyParent parent, TKeyChild keyChild, out TElement childActual);
  }
}
