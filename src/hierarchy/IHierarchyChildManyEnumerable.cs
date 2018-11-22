using System;
using System.Collections.Generic;

namespace SINeDUSTRIES.Collections
{
  /// <summary>
  /// Hierarchy of parent(s)-to-childs, where the children can be enumerated;
  /// </summary>
  /// <typeparam name="TElement">Type of parent/ child;</typeparam>
  public interface IHierarchyChildManyEnumerable<TElement>
  {
    /// <summary>
    /// Get the children of <paramref name="parent"/>;
    /// </summary>
    IEnumerable<TElement> ChildGetMany(TElement parent);

    /// <summary>
    /// Try to get the children of <paramref name="parent"/>;
    /// </summary>
    Boolean ChildGetManyTry(TElement parent, out IEnumerable<TElement> elements);
  }
}
