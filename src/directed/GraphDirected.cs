using System;
using System.Collections.Generic;

namespace SINeDUSTRIES.Collections
{
  /// <summary>
  /// A simple graph;
  /// </summary>
  /// <typeparam name="TNode">Type of a Node in this graph;</typeparam>
  public class GraphDirected<TNode>
  {
    #region Public, Methods

    /// <summary>
    /// Add an edge to the graph;
    /// <paramref name="node1"/> to <paramref name="node2"/>;
    /// </summary>
    /// <param name="node1">First node;</param>
    /// <param name="node2">Second node;</param>
    /// <param name="forward">True: Edge is from <paramref name="node1"/> to <paramref name="node2"/>; True: Edge is from <paramref name="node2"/> to <paramref name="node1"/>;</param>
    /// <param name="allowSelfLoop">Allow an edge both of whose endpoints are the same vertex?</param>
    virtual public void EdgeAdd(TNode node1, TNode node2, Boolean forward = true, Boolean allowSelfLoop = false)
    {
      if
      (
        allowSelfLoop // if allowLoop OR
        || !node1.Equals(node2) // NOT allow same and not the same
      )
      {
        nodeAdd(node1);
        nodeAdd(node2);

        if (forward)
        {
          _edges.Add(new Tuple<TNode, TNode>(node1, node2));
          //global::UnityEngine.Debug.LogFormat("Adding Edge {0}->{1}", node1, node2);
        }
        else // backwards
        {
          _edges.Add(new Tuple<TNode, TNode>(node2, node1));
          //global::UnityEngine.Debug.LogFormat("Adding Edge {0}->{1}", node2, node1);
        }
      }
    }

    #endregion

    #region Public, Properties

    /// <see cref="_nodes"/>
    public HashSet<TNode> Nodes 
    => this._nodes;

    /// <see cref="_edges"/>
    public HashSet<Tuple<TNode, TNode>> Edges
    => this._edges;

    #endregion

    #region Private, Methods

    /// <summary>
    /// Add a node to the graph;
    /// </summary>
    /// <param name="node">Node to add;</param>
    private void nodeAdd(TNode node) 
    => _nodes.Add(node);

    /// <summary>
    /// Add an edge to the graph;
    /// </summary>
    /// <param name="node1">Node, start;</param>
    /// <param name="node2">Node, finish;</param>
    private void edgeAdd(TNode node1, TNode node2) 
    => _edges.Add(new Tuple<TNode, TNode>(node1, node2));

    #endregion

    #region Private, Fields

    /// <summary>
    /// Set of nodes in the graph;
    /// </summary>
    private HashSet<TNode> _nodes = new HashSet<TNode>();

    /// <summary>
    /// Set of edges in the graph;
    /// </summary>
    private HashSet<Tuple<TNode, TNode>> _edges = new HashSet<Tuple<TNode, TNode>>();

    #endregion
  }
}