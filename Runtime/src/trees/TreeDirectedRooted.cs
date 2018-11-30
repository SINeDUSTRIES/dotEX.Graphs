using System;
using System.Collections.Generic;
using System.Linq;

using QuickGraph;

namespace SINeDUSTRIES.Collections
{
  /// <summary>
  /// Tree that is directed and rooted;
  /// </summary>
  /// <remarks>
  /// "Just a tree";
  /// Behaves the way one would expect;
  /// + Equality, Vertex:
  ///   + global equality
  ///     - uniqe across all <typeparamref name="TVertex"/>
  ///   + local equality
  ///     - uniqe across child <typeparamref name="TVertex"/>
  ///   - For equality globally, <see cref="eqCompVertexGlobal"/> is used;
  ///   - For equality localally, <see cref="eqCompVertexLocal"/> is used;
  ///   - generally parents are evaluated globally, while children are evaluated locally
  /// + Equality, Edge
  ///   - edges are not compared, but their <typeparamref name="TVertex"/> are;
  ///     - ie,<see cref="IEdge{TVertex}.Source"/> and <see cref="IEdge{TVertex}.Target"/>;
  ///     - <see cref="IEdge{TVertex}.Source"/> uses global, while <see cref="IEdge{TVertex}.Target"/> uses local;
  /// + Binary tree?
  ///   - enforcable by wrapping this in another <see cref="ITreeDirectedRooted{TVertex, TEdge}"/>;
  ///   - <see cref="SortedSet{T}"/>; https://stackoverflow.com/questions/3262947/is-there-a-built-in-binary-search-tree-in-net-4-0
  ///   - <see cref="SortedDictionary{TKey, TValue}"/>; // https://msdn.microsoft.com/en-us/library/f7fta44c(v=vs.110).aspx
  /// </remarks>
  /// <notes>
  /// + Direction
  ///   - There are no edges from child ad parent;
  ///   - While there is a cache which maps child to its parent, TREES ARE NOT BI-DIRECTIONAL;
  ///   - One instance of a <see cref="IEdge{TVertex}"/> per pair of parent and child
  /// </notes>
  public class TreeDirectedRooted<TVertex, TEdge> :
    ITreeDirectedRooted<TVertex, TEdge>

    where TEdge : IEdge<TVertex>
  {
    #region Public, Methods, Add
    // in a tree, a vertice must be added with its edge

    // vertices and edge

    /// <summary>
    /// <see cref="IMutableEdgeListGraph{TVertex, TEdge}.AddEdge(TEdge)"/>;
    /// </summary>
    public Boolean AddEdge(TEdge aAdd)
    => insertVerticesAndEdgeTry(aAdd);

    /// <summary>
    /// <see cref="IMutableEdgeListGraph{TVertex, TEdge}.AddEdgeRange(IEnumerable{TEdge})"/>;
    /// </summary>
    public Int32 AddEdgeRange(IEnumerable<TEdge> aAdd)
    => aAdd.Sum((aEdge) => AddEdge(aEdge) ? 1 : 0);

    #endregion

    #region Public, Methods, Remove

    // edges

    /// <summary>
    /// Disconnects and truncates the sub-tree;
    /// post-order ("bottom-up") deletion;
    /// Implements <see cref="IMutableEdgeListGraph{TVertex, TEdge}.RemoveEdge(TEdge)"/>;
    /// </summary>
    /// <remarks>
    /// Ensures connectedness;
    /// </remarks>
    public Boolean RemoveEdge(TEdge eeRemove)
    {
      if (ContainsVertex(eeRemove.Source)) // contains parent
      {
        this.VisitPostOrder(eeRemove.Target, (aDescendant) => delete(aDescendant)); // delete, child and below, bottom-up; ie, truncate subtree
        return true;
      }
      else // no contains
      {
        return false;
      }
    }

    /// <summary>
    /// <see cref="IMutableEdgeListGraph{TVertex, TEdge}.RemoveEdgeIf(EdgePredicate{TVertex, TEdge})"/>;
    /// </summary>
    public Int32 RemoveEdgeIf(EdgePredicate<TVertex, TEdge> predicate)
    => Edges.Shadow().Sum((aEdge) => RemoveEdge(aEdge) ? 1 : 0);

    /// <summary>
    /// <see cref="IMutableIncidenceGraph{TVertex, TEdge}.RemoveOutEdgeIf(TVertex, EdgePredicate{TVertex, TEdge})"/>
    /// </summary>
    public Int32 RemoveOutEdgeIf(TVertex v, EdgePredicate<TVertex, TEdge> predicate)
    => OutEdges(v).Shadow().Sum((aEdge) => RemoveEdge(aEdge) ? 1 : 0);

    /// <summary>
    /// <see cref="IMutableIncidenceGraph{TVertex, TEdge}.ClearOutEdges(TVertex)"/>;
    /// </summary>
    public void ClearOutEdges(TVertex removee)
    {
      foreach (TEdge edge in OutEdges(removee).Shadow())
      {
        RemoveEdge(edge);
      }
    }

    /// <summary>
    /// <see cref="IMutableGraph{TVertex, TEdge}.Clear"/>;
    /// Does not affect <see cref="ITreeDirectedRooted{TVertex, TEdge}.Root"/>;
    /// </summary>
    public void Clear()
    {
      // clear collections
      _edges_parentAdChildren.Clear();
      _edges_cahce_childAdParent.Clear();
      _vertexAdDepth.Clear();

      insertRoot(_root); // readd the root to collections

      _cleared?.Invoke(this, null); // invoke event
    }

    /// <summary>
    /// <see cref="IMutableIncidenceGraph{TVertex, TEdge}.TrimEdgeExcess"/>;
    /// </summary>
    public void TrimEdgeExcess()
    {
      throw new NotImplementedException("Yet"); // TODO: implement this
    }

    #endregion // remove

    #region Public, Methods, Get

    // IImplicitGrahp

    /// <summary>
    /// <see cref="IImplicitGraph{TVertex, TEdge}.OutEdge(TVertex, Int32)"/>
    /// </summary>
    public TEdge OutEdge(TVertex parent, Int32 index)
    =>
      _edges_parentAdChildren.TryGetValue(parent, out IDictionary<TVertex, TEdge> outEdges) ? // if there are out edges...
        outEdges.Values.ElementAt(index) : // return edge at that index
        default(TEdge); // else, return default

    /// <summary>
    /// <see cref="IImplicitGraph{TVertex, TEdge}.OutEdges(TVertex)"/>;
    /// </summary>
    public IEnumerable<TEdge> OutEdges(TVertex parent)
    =>
      _edges_parentAdChildren.TryGetValue(parent, out IDictionary<TVertex, TEdge> outEdges) ? // if there are out edges...
        outEdges.Values : // return edges
        default(IEnumerable<TEdge>); // else return default

    // IIncidenceGraph

    /// <summary>
    /// <see cref="IIncidenceGraph{TVertex, TEdge}.TryGetEdge(TVertex, TVertex, out TEdge)"/>
    /// </summary>
    /// <param name="parentKey">Global equality;</param>
    /// <param name="childKey">Local equality;</param>
    /// <param name="edge"><typeparamref name="TEdge"/> connecting <paramref name="parentKey"/> to <paramref name="childKey"/>;</param>
    public Boolean TryGetEdge(TVertex parentKey, TVertex childKey, out TEdge edge)
    =>
      _edges_parentAdChildren.TryGetValue(parentKey, out IDictionary<TVertex, TEdge> outEdges) ? // got out edges (to children)
        outEdges.TryGetValue(childKey, out edge) : // got edge to specific child
        default(TEdge).Out(out edge, false);

    /// <summary>
    /// <see cref=" IIncidenceGraph{TVertex, TEdge}.TryGetEdges(TVertex, TVertex, out IEnumerable{TEdge})"/>
    /// </summary>
    /// <param name="edges">Contains the one edge, or @default <see cref="IEnumerable{T}"/>;</param>
    /// <param name="parentKey">Global equality;</param>
    /// <param name="childKey">Local equality;</param>
    public Boolean TryGetEdges(TVertex parentKey, TVertex childKey, out IEnumerable<TEdge> edges)
    =>
      TryGetEdge(parentKey, childKey, out TEdge theEdge) ? // got the one edge
        (new TEdge[] { theEdge }).Out(out edges, true) : // enumerable of just the one edge
        default(IEnumerable<TEdge>).Out(out edges, false);

    /// <summary>
    /// <see cref="IImplicitGraph{TVertex, TEdge}.TryGetOutEdges(TVertex, out IEnumerable{TEdge})"/>;
    /// </summary>
    /// <param name="parentKey">Global equality;</param>
    /// <param name="edges"><typeparamref name="TEdge"/> out from <paramref name="parentKey"/>;</param>
    public Boolean TryGetOutEdges(TVertex parentKey, out IEnumerable<TEdge> edges)
    =>
      _edges_parentAdChildren.TryGetValue(parentKey, out IDictionary<TVertex, TEdge> outEdges) ? // if there are out edges...
        outEdges.Values.Out(out edges, true) :
        default(IEnumerable<TEdge>).Out(out edges, false);

    #endregion

    #region Pubclic, Methods, Get, Tree

    // parent

    /// <summary>
    /// Given a child <typeparamref name="TVertex"/>, get the parent <typeparamref name="TVertex"/> and the edge connecting the child to the parent;
    /// Implements <see cref="ITreeDirectedRooted{TVertex, TEdge}.ParentAndEdgeInGet(TVertex)"/>;
    /// </summary>
    /// <param name="child">Global equality;</param>
    /// <exception cref="KeyNotFoundException"><paramref name="child"/> does not have parent; ie, not in tree, or root;</exception>
    public Tuple<TVertex, TEdge> ParentAndEdgeInGet(TVertex child)
    => _edges_cahce_childAdParent[child];

    /// <summary>
    /// Given a child <typeparamref name="TVertex"/>, get the parent <typeparamref name="TVertex"/> and the edge connecting the parent to <paramref name="childKey"/>;
    /// Implements <see cref="ITreeDirectedRooted{TVertex, TEdge}.ParentAndEdgeInGetTry(TVertex, out Tuple{TVertex, TEdge})"/>;
    /// </summary>
    /// <param name="childKey">Global equality;</param>
    /// <param name="parentAndEdge">Tuple of parent of <paramref name="childKey"/>, and the edge connecting them;</param>
    public Boolean ParentAndEdgeInGetTry(TVertex childKey, out Tuple<TVertex, TEdge> parentAndEdge)
    => _edges_cahce_childAdParent.TryGetValue(childKey, out parentAndEdge); // if there is a mapping

    // hierarchy

    /// <summary>
    /// Implements <see cref="IHierarchyChildManyKey{TElement, TKeyParent, TKeyChild}.ChildGet(TKeyParent, TKeyChild)"/>
    /// </summary>
    public TVertex ChildGet(TVertex parent, TVertex keyChild)
    => _edges_parentAdChildren[parent][keyChild].Target; // oh the 

    /// <summary>
    /// Implements <see cref="IHierarchyChildManyKey{TElement, TKeyParent, TKeyChild}.ChildGetTry(TKeyParent, TKeyChild, out TElement)"/>
    /// </summary>
    public Boolean ChildGetManyTry(TVertex parent, out IEnumerable<TVertex> elements)
    =>
      _edges_parentAdChildren.TryGetValue(parent, out IDictionary<TVertex, TEdge> map) ? // got parent
        map.Keys.Out(out elements, true) : // parent :> (child :> edge)
        default(IEnumerable<TVertex>).Out(out elements, false);

    /// <summary>
    /// Implements <see cref="IHierarchyChildManyEnumerable{TElement}.ChildGetMany(TElement)"/>;
    /// </summary>
    public IEnumerable<TVertex> ChildGetMany(TVertex parent)
    =>
      _edges_parentAdChildren.TryGetValue(parent, out IDictionary<TVertex, TEdge> parentAdChild) ?
        (from aEdgeParentAdChild in parentAdChild.Values select aEdgeParentAdChild.Target) :
        default(IEnumerable<TVertex>);

    /// <summary>
    /// Implements <see cref="IHierarchyChildManyKey{TElement, TKeyParent, TKeyChild}.ChildGetTry(TKeyParent, TKeyChild, out TElement)"/>;
    /// </summary>
    public Boolean ChildGetTry(TVertex parent, TVertex keyChild, out TVertex childActual)
    =>
      TryGetEdge(parent, keyChild, out TEdge edge) ? // got the edge
        edge.Target.Out(out childActual, true) : // child actual is the target of the edge, naturally
        default(TVertex).Out(out childActual, false);

    #endregion

    #region Public, Methods, Checks

    /// <summary>
    /// <see cref="IIncidenceGraph{TVertex, TEdge}.ContainsEdge(TVertex, TVertex)"/>
    /// </summary>
    public Boolean ContainsEdge(TVertex parent, TVertex childLocal)
    =>
        _edges_parentAdChildren.TryGetValue(parent, out IDictionary<TVertex, TEdge> outEdges) // contains the source
        && outEdges.ContainsKey(childLocal); // contained e[source -> target]

    /// <summary>
    /// FOverload for <see cref="ContainsEdge(TVertex, TVertex)"/>;
    /// <see cref="IEdgeSet{TVertex, TEdge}.ContainsEdge(TEdge)"/>
    /// </summary>
    public Boolean ContainsEdge(TEdge parentAdChild)
    => ContainsEdge(parentAdChild.Source, parentAdChild.Target);

    /// <summary>
    /// <see cref="IImplicitGraph{TVertex, TEdge}.IsOutEdgesEmpty(TVertex)"/>;
    /// </summary>
    public Boolean IsOutEdgesEmpty(TVertex parent)
    =>
      !_edges_parentAdChildren.TryGetValue(parent, out IDictionary<TVertex, TEdge> outEdges) // return true if did not get out edges collection OR
      || outEdges.Count <= 0; // got out edges collections, and there are none

    /// <summary>
    /// <see cref="IImplicitGraph{TVertex, TEdge}.OutDegree(TVertex)"/>
    /// </summary>
    public Int32 OutDegree(TVertex parent)
     =>
       _edges_parentAdChildren.TryGetValue(parent, out IDictionary<TVertex, TEdge> outEdges) ?  // if vertex exists
       outEdges.Count : // true, use the amount 
       -1; // else

    /// <summary>
    /// <see cref="IImplicitVertexSet{TVertex}.ContainsVertex(TVertex)"/>;
    /// </summary>
    public Boolean ContainsVertex(TVertex parent)
    => _edges_parentAdChildren.ContainsKey(parent);

    #endregion

    #region Public, Propeties, Graph

    /// <summary>
    /// <see cref="IGraph{TVertex, TEdge}.IsDirected"/>
    /// </summary>
    public Boolean IsDirected // all trees are directed;
    => true;

    /// <summary>
    /// <see cref="IGraph{TVertex, TEdge}.AllowParallelEdges"/>;
    /// </summary>
    public Boolean AllowParallelEdges
    => false; // directed rooted trees do not allow;

    #endregion

    #region Public, Properties, Vertex Set

    /// <summary>
    /// <see cref="IVertexSet{TVertex}.IsVerticesEmpty"/>;
    /// </summary>
    public Boolean IsVerticesEmpty
    => VertexCount <= 0;

    /// <summary>
    /// <see cref="IVertexSet{TVertex}.VertexCount"/>;
    /// </summary>
    public Int32 VertexCount
    => _edges_parentAdChildren.Count;

    /// <summary>
    /// <see cref="IVertexSet{TVertex}.Vertices"/>;
    /// </summary>
    public IEnumerable<TVertex> Vertices
    => _edges_parentAdChildren.Keys;

    #endregion

    #region Public, Properties, Edge Set

    /// <summary>
    /// <see cref="IEdgeSet{TVertex, TEdge}.IsEdgesEmpty"/>;
    /// </summary>
    public Int32 EdgeCount
    => VertexCount - 1; // in trees, there are always one less edges than nodes

    /// <summary>
    /// <see cref="IEdgeSet{TVertex, TEdge}.IsEdgesEmpty"/>;
    /// </summary>
    public Boolean IsEdgesEmpty
    => EdgeCount <= 0;

    /// <summary>
    /// <see cref="IEdgeSet{TVertex, TEdge}.Edges"/>;
    /// </summary>
    public IEnumerable<TEdge> Edges
    {
      get
      {
        foreach (IEnumerable<TEdge> someEdges in this._edges_parentAdChildren.Values)
        {
          foreach (TEdge aEdge in someEdges)
          {
            yield return aEdge;
          }
        }
      }
    }

    #endregion

    #region Public, Properties, Events

    /// <summary>
    /// <see cref="IMutableGraph{TVertex, TEdge}.Cleared"/>;
    /// </summary>
    public event EventHandler Cleared
    {
      add
      {
        _cleared += value;
      }

      remove
      {
        _cleared -= value;
      }
    }

    ///// <summary>
    ///// Implements <see cref="IMutableVertexSet{TVertex}.VertexAdded"/>;
    ///// </summary>
    //public event VertexAction<TVertex> VertexAdded
    //{
    //  add
    //  {
    //    _vertexAdded += value;
    //  }

    //  remove
    //  {
    //    _vertexAdded -= value;
    //  }
    //}

    ///// <summary>
    ///// Implements <see cref="IMutableVertexSet{TVertex}.VertexRemoved"/>;
    ///// </summary>
    //public event VertexAction<TVertex> VertexRemoved
    //{
    //  add
    //  {
    //    _vertexRemoved += value;
    //  }

    //  remove
    //  {
    //    _vertexRemoved -= value;
    //  }
    //}

    /// <summary>
    /// Implements <see cref="IMutableEdgeListGraph{TVertex, TEdge}.EdgeAdded"/>;
    /// </summary>
    public event EdgeAction<TVertex, TEdge> EdgeAdded
    {
      add
      {
        _edgeAdded += value;
      }

      remove
      {
        _edgeAdded -= value;
      }
    }

    /// <summary>
    /// Implements <see cref="IMutableEdgeListGraph{TVertex, TEdge}.EdgeRemoved"/>;
    /// </summary>
    public event EdgeAction<TVertex, TEdge> EdgeRemoved
    {
      add
      {
        _edgeRemoved += value;
      }

      remove
      {
        _edgeRemoved -= value;
      }
    }

    #endregion

    #region Public, Properties, Tree

    /// <summary>
    /// <see cref="_root"/>;
    /// </summary>
    public TVertex Root
    => _root;

    #endregion

    #region Protected, Properties, Tree

    /// <summary>
    /// <see cref="IEqualityComparer{TVertex}"/> for comparing <typeparamref name="TVertex"/> globally;
    /// </summary>
    protected IEqualityComparer<TVertex> eqCompVertexGlobal
    => _eqCompGlobalVertex;

    /// <summary>
    /// <see cref="IEqualityComparer{TVertex}"/> for comparing <typeparamref name="TVertex"/> locally;
    /// </summary>
    protected IEqualityComparer<TVertex> eqCompVertexLocal
    => _eqCompLocalVertex;

    #endregion

    #region Private, Methods, Insert

    /// <summary>
    /// <see cref="Func{TResult}"/> which creates the object which holds <typeparamref name="TEdge"/> for a <typeparamref name="TVertex"/>;
    /// </summary>
    protected Func<IDictionary<TVertex, TEdge>> ctorOutEdgeObject
    => () => new Dictionary<TVertex, TEdge>(eqCompVertexLocal);

    /// <summary>
    /// Initialize <typeparamref name="TVertex"/> so it can have out edges;
    /// Uses <see cref="ctorOutEdgeObject"/>;
    /// </summary>
    private void initializeOutEdges(TVertex vertex/*, Func<IDictionary<TVertex, TEdge>> funcDictionaryCtor*/)
    => _edges_parentAdChildren.Add(vertex, ctorOutEdgeObject()); // add new out edge object

    /// <summary>
    /// Add a child to a <paramref name="parent"/> that is known to exist;
    /// Adds both <typeparamref name="TVertex"/> and <typeparamref name="TEdge"/>;
    /// </summary>
    /// <notes>
    /// Prime overload;
    /// Only method that should be doing any adding;
    /// </notes>
    private void insertVerticesAndEdge(TVertex child, TEdge edgeParentAdChild, TVertex parent, Int32 depthChild)
    {
      _edges_parentAdChildren[parent].Add(child, edgeParentAdChild); // edge add: parent -> (child -> edge)
      _edges_cahce_childAdParent.Add(child, new Tuple<TVertex, TEdge>(parent, edgeParentAdChild)); // cache edge

      _vertexAdDepth.Add(child, depthChild); // add child -> depth;

      initializeOutEdges(child); // initialize edges for the child

      // invoke events
      //_vertexAdded?.Invoke(child);
      _edgeAdded?.Invoke(edgeParentAdChild);
    }

    // overload
    private void insertVerticesAndEdge(TEdge parentAdChild)
    =>
      insertVerticesAndEdge
      (
        parentAdChild.Target,
        parentAdChild,
        parentAdChild.Source,
        _vertexAdDepth[parentAdChild.Source] + 1 // depth is parent depth + 1 
      );

    /// <summary>
    /// Add root;
    /// </summary>
    private void insertRoot(TVertex root)
    {
      _root = root;

      initializeOutEdges(root); // initialize the out edges fo the root

      // no edges

      _vertexAdDepth.Add(root, 0);
      //_vertexAdded?.Invoke(root);
    }

    // overload
    private Boolean insertVerticesAndEdge(TVertex eeInsertVertex_child, TEdge eeInsertEdge_parentAdChild, TVertex parent)
    {
      if // if
      (
        ContainsVertex(parent) && // contains parent
        !ContainsVertex(eeInsertVertex_child) // but does not contains the child
      )
      {
        insertVerticesAndEdge(eeInsertVertex_child, eeInsertEdge_parentAdChild, parent, _vertexAdDepth[parent] + 1); // add mapping

        return true;
      }
      else // else does not contain parent OR contains the child
      {
        return false;
      }
    }

    // vertices and edge

    private Boolean insertVerticesAndEdgeTry(TEdge parentAdChild)
    =>
      insertVerticesAndEdge
      (
        parentAdChild.Target,
        parentAdChild,
        parentAdChild.Source
      );


    #endregion

    #region Private, Methods, Delete 
    // delete: "remove from fields"

    // delete in
    //  delete a vertex, and the edge into it 

    /// <summary>
    /// Delete <paramref name="child"/> and <paramref name="edge"/> from all fields;
    /// Prime overload;
    /// </summary>
    /// <param name="child">Child to remove;</param>
    /// <param name="edge">Actual edge to remove;</param>
    /// <param name="parentKey">Parent of <paramref name="child"/>; NOT DELETED; Used as key the edge;</param>
    /// <notes>
    /// Prime overload;
    /// Does not ensure connectedness;
    /// </notes>
    private void deleteIn(TVertex child, TEdge edge, TVertex parentKey)
    {
      _edges_parentAdChildren[parentKey].Remove(child); // delete e: parent -> child //~ since there is one edge, removes the child too!
      _edges_cahce_childAdParent.Remove(child); // delete e: parent -> child

      _edgeRemoved?.Invoke(edge); // event
    }

    /// <summary>
    /// Delete <paramref name="someChild"/> and all <typeparamref name="TEdge"/> that go into <paramref name="someChild"/>;
    /// Foverload;
    /// </summary>
    private void deleteIn(TVertex someChild, Tuple<TVertex, TEdge> parentAndEdge)
    => deleteIn(someChild, parentAndEdge.Item2, parentAndEdge.Item1); // remove parent, child, and parent -> child

    /// <summary>
    /// Delete <paramref name="someChild"/> and all <typeparamref name="TEdge"/> that go into <paramref name="someChild"/>;
    /// Foverload;
    /// </summary>
    private void deleteIn(TVertex someChild)
    => deleteIn(someChild, _edges_cahce_childAdParent[someChild]); // remove parent, child, and parent -> child

    /// <summary>
    /// Delete an <typeparamref name="TEdge"/> and the child (ie, <see cref="IEdge{TVertex}.Target"/>);
    /// </summary>
    private void deleteIn(TEdge someeParentAdChild)
    => deleteIn(someeParentAdChild.Target, someeParentAdChild, someeParentAdChild.Source);

    /// <summary>
    /// Try to <see cref="deleteIn(TVertex, TEdge, TVertex)"/>;
    /// </summary>
    private Boolean deleteInTry(TVertex someChild, TVertex someParentKey)
    {
      if (TryGetEdge(someParentKey, someChild, out TEdge parentAdChild)) // if got edge parent -> child
      {
        deleteIn(someChild, parentAdChild, someParentKey); // remove parent, child, parent -> child

        return true;
      }
      else // no get edge
      {
        return false;
      }
    }

    // delete 

    /// <summary>
    /// <see cref="deleteIn(TEdge)"/> on all <typeparamref name="TEdge"/> that <paramref name="someParent"/> is in;
    /// ie, delete the children of <paramref name="someParent"/>;
    /// </summary>
    private void deleteOut(TVertex someParent)
    {
      // for all the edges out of the parent, delete the 

      foreach (KeyValuePair<TVertex, TEdge> aEdge in _edges_parentAdChildren[someParent].Shadow()) // for all e: someParent -> aEdge
      {
        deleteIn(aEdge.Value.Target, aEdge.Value, aEdge.Value.Source); // remove child and e: someParent -> child; 
      }
    }

    /// <summary>
    /// Removes the <typeparamref name="TVertex"/> and all involved <typeparamref name="TEdge"/>;
    /// </summary>
    /// <remarks>
    /// Does not ensure connectedness;
    /// </remarks>
    /// <notes>Prime overload;</notes>
    private void delete(TVertex someVertex)
    {
      // TODO: don't allow root!

      deleteOut(someVertex); // all e: someVertex -> to child
      deleteIn(someVertex); // e: parent -> someVertex

      _edges_parentAdChildren.Remove(someVertex); // delete vertex

      _vertexAdDepth.Remove(someVertex); // remove depth;

      //_vertexRemoved?.Invoke(someVertex); // event
    }

    #endregion

    #region Private, Fields

    // graph

    /// <summary>
    /// The root;
    /// </summary>
    private TVertex _root;

    /// <summary>
    /// All "out edges" from a parent to its children;
    /// - key0 (global): vertex, parent;
    /// + key1 (local): child
    ///   + value1: edge 
    ///     - Source: vertex, parent; key0
    ///     - Target: vertex, child; key1
    /// </summary>
    /// <notes>
    /// What's great about this is that 2 different comparers are used;
    /// actual objects are different from queries and never reveal themselves during gets and TryGet; 
    /// BUT value1, the <typeparamref name="TEdge"/>, contains the actual <typeparamref name="TVertex"/> instances, at the end;
    /// One could think of value1 as a <see cref="Tuple{T1, T2}"/> to get from two queries;
    /// </notes>
    private IDictionary<TVertex, IDictionary<TVertex, TEdge>> _edges_parentAdChildren; // edges (children) as wanted

    /// <summary>
    /// Cached mappings of a child to its parent;
    /// key (global): child 
    /// value:
    /// * Item1: parent
    /// * Item2: edge from @Item1 to @key;
    /// </summary>
    /// <notes>
    /// Objects in this object are the same as the ones in <see cref="_edges_parentAdChildren"/>, just arranged differently;
    /// </notes>
    private IDictionary<TVertex, Tuple<TVertex, TEdge>> _edges_cahce_childAdParent; // only one in edge (parent)

    /// <summary>
    /// Key (global): the vertex;
    /// Value: level;
    /// </summary>
    private IDictionary<TVertex, Int32> _vertexAdDepth; // levels independant of vertex type

    // equality comparers

    /// <summary>
    /// <see cref="IEqualityComparer{TVertex}"/> for comparing <typeparamref name="TVertex"/> globally;
    /// </summary>
    protected IEqualityComparer<TVertex> _eqCompGlobalVertex;

    /// <summary>
    /// <see cref="IEqualityComparer{TVertex}"/> for comparing <typeparamref name="TVertex"/> locally;
    /// </summary>
    protected IEqualityComparer<TVertex> _eqCompLocalVertex;

    // events

    /// <summary>
    /// <see cref="IMutableGraph{TVertex, TEdge}.Cleared"/>;
    /// </summary>
    protected event EventHandler _cleared;

    ///// <summary>
    ///// Event invoked when <
    ///// </summary>
    //protected event VertexAction<TVertex> _vertexAdded;
    ///// <summary>
    ///// 
    ///// </summary>
    //protected event VertexAction<TVertex> _vertexRemoved;

    /// <summary>
    /// Event invoked when edge is added;
    /// </summary>
    protected event EdgeAction<TVertex, TEdge> _edgeAdded;
    /// <summary>
    /// Event invoked when edge is removed;
    /// </summary>
    protected event EdgeAction<TVertex, TEdge> _edgeRemoved;

    ///// <summary>
    ///// <see cref="IEqualityComparer{TEdge}"/> for comparing <see cref="TEdge"/> globally;
    ///// </summary>
    //protected IEqualityComparer<TEdge> _eqCompGlobalEdge;

    ///// <summary>
    /////<see cref="IEqualityComparer{TEdge}"/> for comparing <see cref="TEdge"/> locally;
    ///// </summary>
    //protected IEqualityComparer<TEdge> _eqCompLocalEdge;

    #endregion

    #region Lifecycle

    /// <summary>
    /// Prime;
    /// </summary>
    public TreeDirectedRooted
    (
      TVertex root,
      IEqualityComparer<TVertex> ecGlobal,
      IEqualityComparer<TVertex> ecLocal
    )
    {
      // initialize fields
      this._edges_parentAdChildren = new Dictionary<TVertex, IDictionary<TVertex, TEdge>>(ecGlobal); // comparer is used for keys of the outter dictionary
      this._edges_cahce_childAdParent = new Dictionary<TVertex, Tuple<TVertex, TEdge>>(ecGlobal);
      this._vertexAdDepth = new Dictionary<TVertex, Int32>(ecGlobal);

      // set fields
      this._eqCompGlobalVertex = ecGlobal;
      this._eqCompLocalVertex = ecLocal;

      insertRoot(root);
    }

    /// <summary>
    /// Uses default <see cref="EqualityComparer{T}"/> for vertex;
    /// </summary>
    public TreeDirectedRooted
    (
      TVertex root
    )
    :
    this
    (
      root,
      EqualityComparer<TVertex>.Default, // global
      EqualityComparer<TVertex>.Default // local
    )
    {

    }


    #endregion
  }
}

/*
Equality problem

  key0_Vertex -> (key1_VertexTarget -> edgeOut) 
	
	key0 should be more discriminate then key1

  Need to allow traversal of out edges using query objects
    * eg, something to the effect of

      * Get(params string[] path)
				foreach String pathPart in path
          new VertexQuery(aPathPart)...
	
	Requirements for EqualityComparer:
		1. prevent duplicates for key0
		2. query objects must be equal to out edge objects


  * eg, EqualityComparer looks at reference
    * satisfies 1, as reference objects are never equal
      * unless you are adding a vertex again(and that's a BAD thing)
		* does not satisfy 2, as query objects are never equal!
		
	* eg, EqualityComparer looks at fields
		* satisfies 2
		* does not satisfy 1
			* collides A LOT if equality is evaluated ONLY on commonly duplicated fields
			* eg, just the name field
				* mario/actions/walk.actorstate
        * alpha/actions/walk.actorstate

  Solution
		2 EqualityComparers, one for each
    Out Edges are stored in a seperate dictionary
    once find the edge, can then

  Common Implementation
    key0 EqCo must be the ACTUAL object
      eg, "ContainsVertex" should only return true when it's the actual vertex!
    key1 EqCo only looks at field
      eg, "ContainsEdge(TVertex source, TVertex target) should return true when 
        souce is the actual instance
        target is a field


  Well how do I start traversing via out edges (name + Type)
    Start from an empty root!

*/
