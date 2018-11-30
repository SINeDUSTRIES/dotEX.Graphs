using System;
using System.Collections;
using System.Collections.Generic;

using QuickGraph;

namespace SINeDUSTRIES.Collections
{
  /// <summary>
  /// Implements <see cref="IPool{TElement, TID}"/> abstractly;
  /// </summary>
  /// <remarks>
  /// + IDs
  ///   - as pools can be populated in any order, IDs are traversed rather than elements, 
  ///   - an <typeparamref name="TID"/> may exist without an <typeparamref name="TElement"/>
  ///   - "Unmapped" <typeparamref name="TID"/> refers to <typeparamref name="TID"/> in the hierarchy which is not mapped to an <typeparamref name="TElement"/>;
  ///   - Inversly, a "mapped" <typeparamref name="TID"/> refers to a <typeparamref name="TID"/> which is mapped to an <typeparamref name="TElement"/>
  ///   - Some methods return unmapped <typeparamref name="TID"/> for the sake of traversal;
  /// </remarks>
  public abstract class APool<TID, TElement> : IPool<TID, TElement>
  {
    #region Public, Methods, Add
    // add

    /// <summary>
    /// Implements <see cref="IAddMap{TKey, TValue}.Add(TKey, TValue)"/>;
    /// </summary>
    virtual public void Add(TID eeAddID, TElement eeAdd)
    => addDo(eeAddID, eeAdd);

    /// <summary>
    /// Implements <see cref="IAdd{TArg0, TArg1}.Add(TArg0, TArg1)"/>;
    /// </summary>
    virtual public void Add(TElement eeAddParent, TElement eeAdd)
    => addDo(eeAddParent, eeAdd);

    // remove

    /// <summary>
    /// Unmap of <typeparamref name="TID"/> ad <typeparamref name="TElement"/>;
    /// Implements <see cref="IRemove{TArg0}"/>;
    /// </summary>
    public Boolean Remove(TElement element)
    {
      if (mapIDAdElement.TryGetValue(element, out TID id)) // got element -> node
      {
        doUnmapAndTruncateUnusedTry(id); // unmap / truncate branch, starting with terminal node

        return true;
      }
      else
      {
        return false;
      }
    }

    /// <summary>
    /// Remove a mapping of <typeparamref name="TID"/> ad <typeparamref name="TElement"/>;
    /// Implements <see cref="IRemoveMap{TKey}"/>;
    /// </summary>
    public Boolean Remove(TID id)
    => doUnmapAndTruncateUnusedTry(id) > -1; // unmap / truncate branch, starting with terminal node

    /// <summary>
    /// Implements <see cref="IClear"/>;
    /// </summary>
    public void Clear()
    {
      // clear data structures
      hierarchyIDs.Clear();
      mapIDAdElement.Clear();

      doMapRoot(); // remap the root
    }

    #endregion

    #region Public, Methods, Get

    // get element

    /// <summary>
    /// <see cref="IGet{TArg0, TResult}.Get(TArg0)"/>;
    /// </summary>
    public TElement Get(TID id)
    => mapIDAdElement[id];

    /// <summary>
    /// <see cref="IGetTry{TArg0, TResult}.TryGet(TArg0, out TResult)"/>;
    /// </summary>
    public Boolean TryGet(TID id, out TElement element)
    => mapIDAdElement.TryGetValue(id, out element);

    // get key

    /// <summary>Implements <see cref="IGetKey{TArg0, TKey}"/></summary>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">If <paramref name="element"/> does not exist in collection;</exception>
    public TID GetKey(TElement element)
    => mapIDAdElement[element]; // element -> node -> id

    /// <summary>
    /// Implements <see cref="IGetKeyTry{TArg0, TKey}.TryGetKey(TArg0, out TKey)"/>;
    /// </summary>
    public Boolean TryGetKey(TElement element, out TID tID)
    => mapIDAdElement.TryGetValue(element, out tID);

    // child get

    /// <summary>
    /// <see cref="IHierarchyChildManyEnumerable{TElement}.ChildGetMany(TElement)"/>;
    /// </summary>
    /// <remarks>Returns all <typeparamref name="TID"/>, regardless of wether or not they are used;</remarks>
    public IEnumerable<TID> ChildGetMany(TID parent)
    => hierarchyIDs.ChildGetMany(parent);

    /// <summary>
    /// <see cref="IHierarchyChildManyEnumerable{TElement}.ChildGetManyTry(TElement, out IEnumerable{TElement})"/>;
    /// </summary>
    /// <remarks>Returns all <typeparamref name="TID"/>, regardless of wether or not they are used;</remarks>
    public Boolean ChildGetManyTry(TID parent, out IEnumerable<TID> children)
    => hierarchyIDs.ChildGetManyTry(parent, out children);

    #endregion

    #region Public, Methods, Checks

    /// <summary>
    /// <see cref="IContains{TElement}"/>;
    /// </summary>
    public Boolean Contains(TElement element)
    => mapIDAdElement.ContainsKey(element);

    /// <summary>
    /// Contains an <typeparamref name="TID"/> mapped to an <typeparamref name="TElement"/>?
    /// Implements <see cref="IContains{TElement}"/>;
    /// </summary>
    /// <notes>
    /// <typeparamref name="TID"/> for the <typeparamref name="TElement"/>;
    /// </notes>
    public Boolean ContainsKey(TID id)
    => mapIDAdElement.ContainsKey(id);

    #endregion

    #region Public, Methods, Enumerable

    // enumerators

    /// <summary>
    /// Mappings of <typeparamref name="TID"/> to <typeparamref name="TElement"/>;
    /// </summary>
    public IEnumerator<KeyValuePair<TID, TElement>> GetEnumerator()
    => mapIDAdElement.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    => GetEnumerator();

    #endregion

    #region Public, Properties

    /// <summary>Number of <typeparamref name="TElement"/>;</summary>
    public Int32 Count
    => mapIDAdElement.Count;

    /// <summary>
    /// Implements <see cref="IPool{TID, TElement}.IDs"/>;
    /// </summary>
    public IEnumerable<TID> IDs
    => hierarchyIDs.Vertices;

    #endregion

    #region Protected, Methods

    /// <summary>
    /// Get <typeparamref name="TID"/> from parent to a child;
    /// aka, the "name";
    /// </summary>
    abstract protected TID idRelativeChildGet(TElement child);

    /// <summary>
    /// Create <typeparamref name="TID"/> of a child;
    /// </summary>
    /// <remarks>Inverse of <see cref="idGlobalParentGet(TID)"/>; ie, creates a child id with a parent;</remarks>
    abstract protected TID idGlobalChildGet(TID idRelativeChild, TID idGlobalParent);

    /// <summary>
    /// Get fully qualified <typeparamref name="TID"/> of a parent <typeparamref name="TElement"/>, given the <typeparamref name="TID"/> of its child;
    /// </summary>
    /// <remarks>Inverse of <see cref="idGlobalChildGet(TID, TID)"/>; ie, creates a parent id with child</remarks>
    /// <example>
    /// <paramref name="idGlobalChild"/>: "/foo/bar";
    /// returns: /foo
    /// </example>
    abstract protected TID idGlobalParentGet(TID idGlobalChild);

    #endregion

    #region Private, Methods, Delete

    // delete node

    /// <summary>
    /// Unmap <see cref="INodeMutable{TElement}"/>;
    /// </summary>
    /// <remarks>
    /// Does not check if <see cref="INodeMutable{TElement}.Clear"/>;
    /// </remarks>
    protected Boolean unmapDo(TID idDelete)
    {
      return mapIDAdElement.Remove(idDelete); // delete id <=> node
    }

    /// <summary>
    /// <see cref="unmapDo(TID)"/> and truncates the branch;
    /// ie, Recurses upwards and <see cref="Remove(TElement)"/> until <typeparamref name="TID"/> used;
    /// </summary>
    private Int32 doUnmapAndTruncateUnusedTry(TID idDelete, Int32 countTruncated = -1)
    {
      unmapDo(idDelete); // unmap ID to element

      // remove ID from hierarchy
      if // if
      (
        hierarchyIDs.IsOutEdgesEmpty(idDelete) // deleted id is terminal
        && hierarchyIDs.ParentAndEdgeInGetTry(idDelete, out Tuple<TID, Edge<TID>> parentAndEdgeIn) // got parent; must get parent before truncating!
      )
      {
        hierarchyIDs.RemoveEdge(parentAndEdgeIn.Item2); // truncate terminal edge id parent to id deleted
        countTruncated++; // truncated, so increment

        // recurse if should
        if (!mapIDAdElement.ContainsKey(parentAndEdgeIn.Item1)) // if parent ID unused; ie, if should recurse
        {
          countTruncated += doUnmapAndTruncateUnusedTry(parentAndEdgeIn.Item1, countTruncated); // unmap/ truncate parent; recurse upwards
        }
      }

      return countTruncated;
    }

    #endregion

    #region Private, Methods, Insert

    /// <summary>
    /// Map the <see cref="_idRoot"/> ad <see cref="hierarchyIDs"/> <see cref="ITreeDirectedRooted{TVertex, TEdge}.Root"/>;
    /// </summary>
    private void doMapRoot()
    {
      this.mapIDAdElement.Add(this._idRoot, this._root);
    }

    /// <summary>
    /// Insert a <typeparamref name="TElement"/> into @this;
    /// </summary>
    /// <param name="eeAddID"><typeparamref name="TID"/> of <typeparamref name="TElement"/>;</param>
    /// <param name="eeAddNode"><typeparamref name="TElement"/> added;</param>
    /// <param name="eeAddIDParent"><typeparamref name="TID"/> of parent of <paramref name="eeAddNode"/>; Used for <see cref="ITreeDirectedRooted{TVertex, TEdge}"/>;</param>
    private void addDo(TID eeAddID, TElement eeAddNode, TID eeAddIDParent)
    {
      hierarchyIDs.AddEdge(new Edge<TID>(eeAddIDParent, eeAddID)); // insert edge: parent -> child
      mapIDAdElement.Add(eeAddID, eeAddNode); // insert: id <-> node
    }

    /// <summary>
    /// Insert a <typeparamref name="TElement"/> into @this;
    /// <see cref="idGlobalParentGetAndEnsure(TID)"/>;
    /// </summary>
    private void addDo(TID eeAddID, TElement eeAddElement)
    => addDo(eeAddID, eeAddElement, idGlobalParentGetAndEnsure(eeAddID));

    /// <summary>
    /// Add <typeparamref name="TID"/> and <typeparamref name="TElement"/>;
    /// </summary>
    /// <notes>
    /// For adding with <typeparamref name="TElement"/>, do or do not, there is no try;
    /// ie, if the parent does not exist, then exceptions are thrown;
    /// </notes>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException"><paramref name="eeAddParent"/> is not in @this;</exception>
    /// <param name="eeAddParent">Parent <typeparamref name="TElement"/> of <paramref name="eeAddElement"/>;</param>
    /// <param name="eeAddElement"><typeparamref name="TElement"/> to add;</param>
    protected void addDo(TElement eeAddParent, TElement eeAddElement)
    {
      TID parentID = mapIDAdElement[eeAddParent]; // get node parent -> id parent;

      TID eeAddID = idGlobalChildGet(idRelativeChildGet(eeAddElement), parentID); // gets node parent -> id child

      addDo(eeAddID, eeAddElement, parentID); // map
    }

    /// <summary>
    /// Ensures <see cref="hierarchyIDs"/> contains all parent <typeparamref name="TID"/> of <paramref name="idGlobalChild"/>;
    /// ie, adds all parent <typeparamref name="TID"/> for <paramref name="idGlobalChild"/>;
    /// Does not add <typeparamref name="TElement"/>;
    /// </summary>
    /// <remarks>
    /// Recursives upwards while check/ add;
    /// </remarks>
    private TID idGlobalParentGetAndEnsure(TID idGlobalChild)
    {
      TID idParent = idGlobalParentGet(idGlobalChild); // get id parent from id child

      // recurse up, adding ids to tree
      if (!hierarchyIDs.ContainsVertex(idParent)) // if not contain parent; id parent -> node parent
      {
        // else new map; new add

        TID idParentParent = idGlobalParentGetAndEnsure(idParent); // get/ map parent of parent; recursing upwards

        hierarchyIDs.AddEdge(new Edge<TID>(idParentParent, idParent)); // insert edge: parent -> child
      }

      return idParent; // return parent node got or inserted
    }

    #endregion

    #region Private, Fields

    /// <summary>
    /// ID of the root;
    /// </summary>
    private readonly TID _idRoot;

    /// <summary>
    /// Root element;
    /// </summary>
    private readonly TElement _root;

    /// <summary>
    /// <see cref="TreeDirectedRooted{TVertex, TEdge}"/> which contains the elements;
    /// </summary>
    private TreeDirectedRooted<TID, Edge<TID>> hierarchyIDs;

    /// <summary>
    /// ID and elements;
    /// key-value: path/ id;
    /// value-key:  with path/ id of @key-value;
    /// </summary>
    private DictionaryMirrorer<
      TID, // key-value
      TElement, // value-key 
      IDictionary<TID, TElement>, // type forward
      IDictionary<TElement, TID> // type reverse
    >
    mapIDAdElement = new DictionaryMirrorer<TID, TElement, IDictionary<TID, TElement>, IDictionary<TElement, TID>>();

    #endregion

    #region Lifecycle

    /// <summary>
    /// Constructor;
    /// </summary>
    /// <param name="idRoot"><typeparamref name="TID"/> of <see cref="_root"/>;</param>
    /// <param name="root"><see cref="_root"/>;</param>
    /// <param name="eqCompGlobal"><see cref="EqualityComparer{T}"/> for determining equality across all nodes;</param>
    /// <param name="eqCompLocal"><see cref="EqualityComparer{T}"/> for determining equality across child nodes;</param>
    public APool
    (
      TElement root,
      TID idRoot,
      IEqualityComparer<TID> eqCompGlobal,
      IEqualityComparer<TID> eqCompLocal
    )
    {
      this._idRoot = idRoot;
      this._root = root;

      hierarchyIDs = new TreeDirectedRooted<TID, Edge<TID>>(idRoot, eqCompGlobal, eqCompLocal);

      doMapRoot(); // insert the root
    }

    #endregion
  }
}