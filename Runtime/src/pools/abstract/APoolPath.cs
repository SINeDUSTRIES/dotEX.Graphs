using System;
using System.Collections.Generic;

namespace SINeDUSTRIES.Collections
{
  /// <summary>
  /// + implementation of <see cref="APool{TElement, TID}"/> using path semantics;
  /// keys are stored as <see cref="System.String"/>;
  /// </summary>
  /// <remarks>
  /// - all paths start with the id of the root
  ///   - ie, there is no "special" prefix for paths
  ///   - eg, $(rootID)/child
  /// </remarks>
  /// <example>
  /// + notable case, where root ID is empty string, ""
  ///   - child IDs begin with seperator;
  ///   + eg, seperator: '/'
  ///     - root ID: ""
  ///     - child ID: /child
  /// + notable case, where root ID is the seperator
  ///   - child IDs begin with DOUBLE seperators;
  ///   - first seperator is the ID of the root, second seperator is a delimiter
  ///   + eg, seperator: /
  ///     - root ID: /
  ///     - child ID: //child
  /// </example>
  /// <notes>
  /// + Why not a set of extension methods?
  ///   - interface extension methods should not assume any specific implementation
  ///     - Path-based ID is an implementation detail
  /// </notes>
  public abstract class APoolPath<TElement> : APool<String, TElement>
  {
    #region Public, Properties

    /// <summary>
    /// <see cref="Char"/> which seperates the ID of a parent from its child;
    /// </summary>
    /// <example>
    /// '\' in a file system;
    /// </example>
    public Char Seperator { get; }

    #endregion

    #region Protected, Methods

    /// <summary>
    /// Implements <see cref="APool{TID, TElement}.idGlobalChildGet(TID, TID)"/>;
    /// </summary>
    override protected String idGlobalChildGet(String idChildRelative, String idParent)
    => StringsUtils.Combine(idParent, idChildRelative, this.Seperator);

    /// <summary>
    /// Implements <see cref="APool{TID, TElement}.idGlobalParentGet(TID)"/>;
    /// </summary>
    override protected String idGlobalParentGet(String idGlobalChild)
    => idGlobalChild.PathUp(this.Seperator);

    #endregion

    #region Lifecycle

    /// <summary>
    /// Constructor;
    /// </summary>
    /// <param name="rootID">ID of root node;;</param>
    /// <param name="rootElement">root node;</param>
    /// <param name="seperator">Seperator </param>
    /// <param name="eqCompGlobal"><see cref="EqualityComparer{T}"/> for determining equality across all nodes;</param>
    /// <param name="eqCompLocal"><see cref="EqualityComparer{T}"/> for determining equality across child nodes;</param>
    public APoolPath
    (
      TElement rootElement,
      String rootID,
      Char seperator,
      IEqualityComparer<String> eqCompGlobal,
      IEqualityComparer<String> eqCompLocal
    ) :
    base
    (
      rootElement,
      rootID,
      eqCompGlobal,
      eqCompLocal
    )
    {
      this.Seperator = seperator;
    }

    #endregion
  }
}
