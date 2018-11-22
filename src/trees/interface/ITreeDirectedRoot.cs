using System;
using System.Collections.Generic;
using QuickGraph;

namespace SINeDUSTRIES.Collections
{
  /// <summary>
  /// Tree which is directed and has a root;
  /// </summary>
  /// <implementation>
  /// + <see cref="IMutableVertexSet{TVertex}.RemoveVertex(TVertex)"/> and <see cref="IMutableEdgeListGraph{TVertex, TEdge}.RemoveEdge(TEdge)"/> should truncate
  /// </implementation>
  /// <notes>
  /// + Why not <see cref="IHierarchy{TVertex, TEdge}"/>?
  ///   - implements <see cref="IMutableVertexSet{TVertex}"/>, which doesn't make sense for trees, which are connected;
  ///     - ie, where would <see cref="IMutableVertexSet{TVertex}.AddVertex(TVertex)"/> go?
  ///   - some other methods in <see cref="IHierarchy{TVertex, TEdge}"/> are repititive
  /// 
  /// + Why truncate?
  ///   + to remain a tree, deletion of a vertex has two outcomes
  ///     - truncate
  ///     - reconnect
  ///     
  ///   + L's Substitution Principle
  ///     + Implementation-specific strategies of "Remove" violates L's substitution principle
  ///       - ergo, all <see cref="ITreeDirectedRooted{TVertex, TEdge}"/> must use the same strategy
  ///         
  ///   + Why not "just" delete?
  ///     - "just" removing an vertex/ edge does not ensure connectedness;
  ///     - a public method should not break the object
  ///     
  ///   + Meta SOLID
  ///     - based on the assumption that the developer used SOLID OOP, and truncation makes the least assumptions on how the object behaves
  ///       - ie, the developer assumes that users assume the developer chose the least ambiguous implementation
  ///       - eg, As a developer, "I assume that users will assume that I will implement this as unambiguously"
  ///       - eg, As a user, "If I assume that the developer (assumed that I would assume he) implemented this unambiguously, what is the implementation that is the least ambiguous?"
  ///   - Not convinced? There is already code that relies on truncating
  ///     ~ Don't break your code, dummy!
  /// </notes>
  public interface ITreeDirectedRooted<TVertex, TEdge> :
    // only vertexes which add members were used

    IGraph<TVertex, TEdge>,
    IVertexSet<TVertex>,
    IImplicitVertexSet<TVertex>,
    IEdgeSet<TVertex, TEdge>,
    IEdgeListGraph<TVertex, TEdge>, // IGraph, IEdgeSet, IVertexSet, IImplicitVertexSet

    IImplicitGraph<TVertex, TEdge>, // IGraph, IImplicitVertexSet;

    IIncidenceGraph<TVertex, TEdge>, // IGraph, IImplicitVertexSet; indicence :: "out edges"

    IEdgeListAndIncidenceGraph<TVertex, TEdge>, // IEdgeListGraph, IGraph, IEdgeSet, IVertexSet, IImplicitVertexSet, IIncidenceGraph, IImplicitGraph; many interfaces

    IMutableGraph<TVertex, TEdge>, // IGraph
    IMutableIncidenceGraph<TVertex, TEdge>, // IMutableGraph, IIncidenceGraph
    IMutableEdgeListGraph<TVertex, TEdge>, // IMutableGraph, IEdgeListGraph, IGraph, IEdgeSet, IVertexSet, IImplicitVertexSet

    IHierarchyChildManyEnumerable<TVertex>, // get edge :> child
    IHierarchyChildManyKey<TVertex, TVertex, TVertex> // get edge :> child

    where TEdge : IEdge<TVertex>
  {
    // TODO: Methods: , 
    // IHierarchyUpOne.ParentGet; allows for utility: traverseUp, isAncesrtorOf, isRoot, isparentof, (breadth first), isDescendantOf, get anscestors
    // getDepth, getNodesAtDepth, getPeers

    /// <summary>
    /// root of the tree;
    /// </summary>
    TVertex Root { get; }

    /// <summary>
    /// Given a child <typeparamref name="TVertex"/>, get the parent <typeparamref name="TVertex"/> and the edge connecting the child to the parent;
    /// </summary>
    /// <param name="child">Global equality;</param>
    /// <exception cref="KeyNotFoundException"><paramref name="child"/> does not have parent; ie, not in tree, or root;</exception>
    Tuple<TVertex, TEdge> ParentAndEdgeInGet(TVertex child);

    /// <summary>
    /// Given a child <typeparamref name="TVertex"/>, get the parent <typeparamref name="TVertex"/> and the edge connecting the parent to <paramref name="child"/>;
    /// </summary>
    /// <param name="child">Global equality;</param>
    /// <param name="parentAndEdge">Tuple of parent of <paramref name="child"/>, and the <typeparamref name="TEdge"/> that connects them;</param>
    Boolean ParentAndEdgeInGetTry(TVertex child, out Tuple<TVertex, TEdge> parentAndEdge);
  }
}
