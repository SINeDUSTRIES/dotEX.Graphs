using System;
using System.Collections.Generic;

using QuickGraph;

namespace SINeDUSTRIES.Collections
{
  /// <summary>
  /// <see cref="IEqualityComparer{T}"/> for <see cref="IEdge{TVertex}"/> via a <see cref="IEqualityComparer{TVertex}"/> for the vertex;
  /// ie, if <see cref="IEqualityComparer{TVertex}.Equals(TVertex, TVertex)"/> for <see cref="IEdge{TVertex}.Source"/> and <see cref="IEdge{TVertex}.Target"/> for two <see cref="IEdge{TVertex}"/>, they are equal;
  /// </summary>
  public class EqualityComparerEdgeViaVertices<TVertex, TEdge> : EqualityComparer<TEdge>
    where TEdge : IEdge<TVertex>
  {
    #region Public, Methods

    /// <summary>
    /// Check equality via <see cref="_equalityComparerVertex"/>;
    /// Implements <see cref="IEqualityComparer{T}.Equals(T, T)"/>;
    /// </summary>
    override public Boolean Equals(TEdge x, TEdge y)
    =>
      _equalityComparerVertex.Equals(x.Source, y.Source) &&
      _equalityComparerVertex.Equals(x.Target, y.Target);

    /// <summary>
    /// via <see cref="UtilHash.CreateFNV(object[])"/> on <see cref="IEdge{TVertex}.Source"/> and <see cref="IEdge{TVertex}.Target"/>;
    /// </summary>
    override public Int32 GetHashCode(TEdge obj)
    =>
      UtilHash.CreateFNV
      (
        _equalityComparerVertex.GetHashCode(obj.Source),
        _equalityComparerVertex.GetHashCode(obj.Target)
      );

    #endregion

    #region Private, Fields

    /// <summary>
    /// Used for the <see cref="IEdge{TVertex}.Source"/> and <see cref="IEdge{TVertex}.Target"/>;
    /// </summary>
    private IEqualityComparer<TVertex> _equalityComparerVertex;

    #endregion

    #region Lifecycle

    /// <summary>
    /// Constructor;
    /// </summary>
    /// <param name="equalityComparerVertex">Used to compare <typeparamref name="TVertex"/>;</param>
    public EqualityComparerEdgeViaVertices(IEqualityComparer<TVertex> equalityComparerVertex)
    {
      this._equalityComparerVertex = equalityComparerVertex;
    }

    #endregion
  }
}
