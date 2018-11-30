using System;
using System.Linq;
using System.Collections.Generic;

using QuickGraph;

using NUnit.Framework;

namespace SINeDUSTRIES.Collections.Test
{
  /// <summary>
  /// <see cref="SINeDUSTRIES.Collections.TreeDirectedRooted{TVertex, TEdge}"/>;
  /// </summary>
  [TestFixture]
  public class TestTreeDirectedRooted
  {
    #region Fields

    private ITreeDirectedRooted<TestVertex, Edge<TestVertex>> _tree;
    private TestVertex _childFoo = new TestVertex("foo");
    private TestVertex _childBar = new TestVertex("bar");
    private TestVertex _childBaz = new TestVertex("baz");

    #endregion

    // setup

    [OneTimeSetUp]
    public void Setup()
    {
      _tree = new TreeDirectedRooted<TestVertex, Edge<TestVertex>>(
        new TestVertex("root"),
        EqualityComparer<TestVertex>.Default, // EqCo, global, reference
        new EqualityComparerVertexLocal() // EqCo, local, values
      );
    }

    [TearDown]
    public void TearDown()
    {
      _tree.Clear();
    }

    // tests

    [Test]
    public void TestRoot()
    {
      Assert.AreEqual(_tree.Root.name, "root");
    }

    [Test]
    public void TestContainsVertex()
    {
      _tree.AddEdge(new Edge<TestVertex>(_tree.Root, _childFoo)); // child

      // using global equality of vertex
      Assert.True(_tree.ContainsVertex(_childFoo));
    }

    /// <summary>
    /// Tests local and global equality;
    /// </summary>
    [Test]
    public void TestEqualityTypes()
    {
      _tree.AddEdge(new Edge<TestVertex>(_tree.Root, _childFoo)); // child

      TestVertex vChildQuery = new TestVertex("foo"); // create query

      // local equality on getting edges
      Assert.True(
        _tree.TryGetEdge(_tree.Root, vChildQuery, out Edge<TestVertex> edge) && // got a child with the query
        edge.Target == _childFoo // the object gotten with the query is 
      );

      Assert.False(_tree.ContainsVertex(vChildQuery)); // global equality on query
    }

    /// <summary>
    /// <see cref="IHierarchyChildManyEnumerable{TElement}"/>;
    /// </summary>
    [Test]
    public void TestChildGetMany()
    {
      // add children
      _tree.AddEdge(new Edge<TestVertex>(_tree.Root, _childFoo));
      _tree.AddEdge(new Edge<TestVertex>(_tree.Root, _childBar)); 
      _tree.AddEdge(new Edge<TestVertex>(_tree.Root, _childBaz));

      Assert.True(_tree.ChildGetManyTry(_tree.Root, out IEnumerable<TestVertex> children)); // get children

      // contains children
      Assert.True(children.Contains(_childFoo));
      Assert.True(children.Contains(_childBar));
      Assert.True(children.Contains(_childBaz));
    }

    /// <summary>
    /// <see cref="UtilHierarchy.DesendantsGetPreOrder{T}(IHierarchyChildManyEnumerable{T}, T)"/>;
    /// </summary>
    [Test]
    public void TestDescendantsGet()
    {
      // setup
      _tree.AddEdge(new Edge<TestVertex>(_tree.Root, _childFoo));
      _tree.AddEdge(new Edge<TestVertex>(_childFoo, _childBar));
      _tree.AddEdge(new Edge<TestVertex>(_childFoo, _childBaz));

      // assert get
      IEnumerable<TestVertex> children = _tree.DesendantsGetPreOrder(_tree.Root); // get children

      // assert contains children
      Assert.True(children.Contains(_childFoo));
      Assert.True(children.Contains(_childBar));
      Assert.True(children.Contains(_childBaz));

      Assert.AreEqual(3, children.Count());

      // assert order
      Assert.AreEqual(children.First(), _childFoo);
      Assert.AreEqual(children.Last(), _childBaz);
    }
  }

  // type definitions

  public class TestVertex
  {
    public String name { get; set; } // auto property

    public TestVertex(String name)
    {
      this.name = name;
    }
  }

  /// <summary>
  /// <see cref="EqualityComparer{T}"/> for <see cref="TestVertex"/> which only teests on <see cref="TestVertex.name"/>;
  /// </summary>
  public class EqualityComparerVertexLocal : EqualityComparer<TestVertex>
  {
    /// <summary>
    /// Implements <see cref="IEqualityComparer{T}.Equals(T, T)"/>;
    /// </summary>
    override public Boolean Equals(TestVertex x, TestVertex y)
    => x.name == y.name;

    /// <summary>
    /// Implements <see cref="IEqualityComparer{T}.GetHashCode(T)"/>;
    /// </summary>
    override public Int32 GetHashCode(TestVertex obj)
    => obj.name.GetHashCode();
  }
}