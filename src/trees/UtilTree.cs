using System;
using System.Collections.Generic;

using QuickGraph;

namespace SINeDUSTRIES.Collections
{
  /// <summary>
  /// Utility class for a <see cref="TreeDirectedRooted{TVertex, TEdge}"/>;
  /// </summary>
  static public class UtilTree
  {
    #region Removal

    /// <summary>
    /// Remove a <typeparamref name="TVertex"/> if <paramref name="predicate"/> is satisfied;
    /// </summary>
    /// <thoughts>
    /// Is this stupid?
    /// </thoughts>
    static public Boolean RemoveVertexIf<TVertex, TVertexSet>(this TVertexSet thisMutableVertexSet, TVertex eeRemove, Func<TVertex, Boolean> predicate)
      where TVertexSet : IMutableVertexSet<TVertex>
    {
      if (predicate(eeRemove)) // satisfied
      {
        thisMutableVertexSet.RemoveVertex(eeRemove); // remove
        return true;
      }
      else // no satisfaction
      {
        return false;
      }
    }

    #endregion

    #region Misc

    /// <summary>
    /// Try to queries of a next possible <typeparamref name="TVertex"/>; 
    /// Get the children visited;
    /// </summary>
    /// <param name="thisTree"><see cref="ITreeDirectedRooted{TVertex, TEdge}"/> to traverse;</param>
    /// <param name="childKeys">Objects which have local equality to children in the tree;</param>
    /// <param name="found">All the children found;</param>
    /// <returns>All queries were succesfull? :: All children found?;</returns>
    static public Boolean TraverseVerticesTry<TVertex, TEdge>
    (
      this ITreeDirectedRooted<TVertex, TEdge> thisTree,
      IEnumerable<TVertex> childKeys,
      out IEnumerable<TVertex> found
    )
      where TEdge : IEdge<TVertex>
    {
      List<TVertex> foundList = new List<TVertex>(); // found
      found = foundList; // set out as reference to found

      TVertex itParent = thisTree.Root; // iteration variable of the parent; starts at the root (ie, ultimate parent)

      foreach (TVertex aChildKey in childKeys) // for all the queries
      {
        if (thisTree.ChildGetTry(itParent, aChildKey, out itParent)) // got nex vertex with query
        {
          foundList.Add(itParent); // add the child that was found
        }
        else // not find next vertex
        {
          return false;
        }
      } // got through all the queries

      return true;
    }

    #endregion
  }
}
