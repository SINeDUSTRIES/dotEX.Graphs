using System;
using System.Collections.Generic;

namespace SINeDUSTRIES.Collections
{
  /// <summary>
  /// Static class which contains algorithms;
  /// </summary>
  static public class UtilGraphDirected
  {
    /// <summary>
    /// Sort topologically using Khan's Algorithm;
    /// </summary>
    /// <typeparam name="TNode">The Type of the node in the graph;</typeparam>
    /// <param name="directedGraph">A directed acyclic graph.</param>
    /// <returns>Nodes in topological order.</returns>
    static public List<TNode> SortTopological<TNode>(this GraphDirected<TNode> directedGraph)
    => UtilAlgorithms.SortKahns(directedGraph.Nodes, directedGraph.Edges);
  }
}