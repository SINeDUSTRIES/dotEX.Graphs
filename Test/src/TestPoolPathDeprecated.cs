using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

//using SINeDUSTRIES.RegularExpressions;

namespace SINeDUSTRIES.Collections.Test
{
  /// <summary>
  /// <see cref="SINeDUSTRIES.Collections.TreeDirectedRooted{TVertex, TEdge}"/>;
  /// </summary>
  [TestFixture]
  public class TestPoolPathDeprecated
  {
    private IPool<String, StringWrapper> _pool;
    private StringWrapper _foo;
    private StringWrapper _bar;
    private StringWrapper _baz;

    [OneTimeSetUp]
    public void Setup()
    {
      _pool = new PoolTest
        (
          new StringWrapper("root"),
          "/",
          '/',
          EqualityComparer<String>.Default,
          EqualityComparer<String>.Default // with local equality comparer,
        );

      _foo = new StringWrapper("foo");
      _bar = new StringWrapper("bar");
      _baz = new StringWrapper("baz");
    }

    [TearDown]
    public void TearDown()
    {
      _pool.Clear();
    }

    [Test]
    public void TestAddKeyGet()
    {
      _pool.Add("//foo", _foo);
      _pool.Add("//foo/bar", _bar);

      Assert.AreEqual(_pool.Get("//foo/bar"), _bar);
    }

    /// <summary>
    /// Test for adding a <see cref="INodeMutable{TElement}"/> where the parent <see cref="INodeMutable{TElement}"/> does not yet exist;
    /// </summary>
    /// <remarks>
    /// implicit parents are an implementation detail that are not publicly exposed; 
    /// setting a breakpoint here is required;
    /// </remarks>
    [Test]
    public void TestAddKeyImplicit()
    {
      _pool.Add("//foo/bar", _bar); // assume add parent implicit "//foo"

      Assert.False(_pool.TryGet("//foo", out StringWrapper parentImplicit)); // assert did not get parent implicit;
      Assert.AreEqual(parentImplicit, default(StringWrapper)); // assert parent implicit is default
    }

    /// <summary>
    /// Tests removal;
    /// </summary>
    [Test]
    public void TestRemove()
    {
      _pool.Add("//foo", _foo); // assume add parent
      _pool.Add("//foo/bar", _bar); // assume add child

      Assert.True(_pool.Remove(_bar)); // assert child removed "//foo/bar"
      Assert.False(_pool.TryGet("//foo/bar", out StringWrapper element)); // assert child removed "//foo/bar"
    }

    /// <summary>
    /// Tests removal of empty branches;
    /// </summary>
    /// <remarks>
    /// implicit parents are an implementation detail that are not publicly exposed; 
    /// setting a breakpoint here is required;
    /// </remarks>
    [Test]
    public void TestRemoveImplicit()
    {
      _pool.Add("//foo/bar", _bar); // assume add parent implcitly "//foo"

      // assertions
      Assert.True(_pool.Remove(_bar)); // assert remove child  "//foo/bar""
      Assert.False(_pool.TryGet("//foo", out StringWrapper element)); // assert removed parent implicitly
    }

    /// <summary>
    /// <see cref="UtilHierarchy.DesendantsGetPreOrder{T}(IHierarchyChildManyEnumerable{T}, T)"/>;
    /// </summary>
    [Test]
    public void TestDescendantsGet()
    {
      // setup
      _pool.Add("//foo", _foo);
      _pool.Add("//foo/bar", _bar);
      _pool.Add("//foo/baz", _baz);
      IEnumerable<String> children = _pool.DesendantsGetPreOrder("/"); // get children

      // assertions
      Assert.True(children.Contains("//foo"));
      Assert.True(children.Contains("//foo/bar"));
      Assert.True(children.Contains("//foo/baz"));
      Assert.AreEqual(3, children.Count());
      Assert.AreEqual(children.First(), "//foo");
      Assert.AreEqual(children.Last(), "//foo/baz");
    }

    //[Test]
    //public void TestRegexGet()
    //{
    //  // setup
    //  _pool.Add("//foo", _foo);
    //  _pool.Add("//foo/bar", _bar);
    //  _pool.Add("//foo/baz", _baz);
    //  IEnumerable<String> matches = _pool.IDs.Matches("//foo/.*");

    //  // assertions
    //  Assert.AreEqual(2, matches.Count());
    //  Assert.AreEqual(matches.First(), "//foo/bar");
    //  Assert.AreEqual(matches.Last(), "//foo/baz");
    //}
  }
}