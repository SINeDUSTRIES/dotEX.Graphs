using System;
using System.Collections.Generic;

namespace SINeDUSTRIES.Collections
{
  /// <summary>
  /// Utility class for hierarchies;
  /// <see cref="IHierarchyChildManyEnumerable{TElement}"/>;
  /// <see cref="IHierarchyChildManyKey{TElement, TKeyParent, TKeyChild}"/>;
  /// </summary>
  static public class UtilHierarchy
  {
    /// <summary>
    /// Traverse recurse a <paramref name="thisHierarchy"/> post-order and invoke visitation functions; 
    /// </summary>
    /// <param name="thisHierarchy">Hierarchy to visit;</param>
    /// <param name="eeVist">Element being visited;</param>
    /// <param name="onVisit">Invoked on <paramref name="eeVist"/>;</param>
    /// <param name="onReturn">Invoked on <paramref name="eeVist"/> when returning from a recursion on children;</param>
    /// <remarks>
    /// Pre-order, visits parents first;
    /// "top-down" invokation of <paramref name="onVisit"/>;
    /// </remarks>
    static public void VisitPreOrder<TVertex>
    (
      this IHierarchyChildManyEnumerable<TVertex> thisHierarchy,
      TVertex eeVist,
      Action<TVertex> onVisit,
      Action<TVertex> onReturn = null
    )
    {
      onVisit(eeVist); // vist the vistee

      // recurse on any children
      if (thisHierarchy.ChildGetManyTry(eeVist, out IEnumerable<TVertex> children)) // if has children
      {
        foreach (TVertex aChild in children) // for all edge: vertex -> some child
        {
          thisHierarchy.VisitPreOrder(aChild, onVisit, onReturn); // visit child of eeVisit
          onReturn?.Invoke(eeVist); // return from child
        }
      }
    }

    /// <summary>
    /// Traverse recurse a <paramref name="thisHierarchy"/> post-order and invoke visitation functions; 
    /// </summary>
    /// <param name="thisHierarchy">Hierarchy to visit;</param>
    /// <param name="eeVist">Element being visited;</param>
    /// <param name="onVisit">Invoked on <paramref name="eeVist"/>;</param>
    /// <param name="onReturn">Invoked on <paramref name="eeVist"/> when returning from a recursion on children;</param>
    /// <remarks>
    /// Post order, visits descendants first;
    /// "bottom-up" invokation of <paramref name="onVisit"/>;
    /// </remarks>
    static public void VisitPostOrder<TVertex>
    (
      this IHierarchyChildManyEnumerable<TVertex> thisHierarchy,
      TVertex eeVist,
      Action<TVertex> onVisit,
      Action<TVertex> onReturn = null
    )
    {
      // recurse on any children
      if (thisHierarchy.ChildGetManyTry(eeVist, out IEnumerable<TVertex> children)) // if has children
      {
        foreach (TVertex aChild in children) // for all edge: vertex -> some child
        {
          thisHierarchy.VisitPostOrder(aChild, onVisit, onReturn); // visit child of eeVisit
          onReturn?.Invoke(eeVist); // return from child
        }
      }

      onVisit(eeVist); // vist the vistee
    }

    /// <summary>
    /// Get all descendants of <paramref name="parent"/>;
    /// Pre-order traversal;
    /// </summary>
    /// <remarks>
    /// Does not include <paramref name="parent"/>;
    /// </remarks>
    static public IEnumerable<T> DesendantsGetPreOrder<T>(this IHierarchyChildManyEnumerable<T> thisHierarchy, T parent)
    {
      ICollection<T> descendants = new LinkedList<T>(); // new linked list

      thisHierarchy.VisitPreOrder(parent, (aNode) => descendants.Add(aNode));  // onVisit, add vertex to list

      descendants.Remove(parent); // remove root

      return descendants;
    }
  }
}
