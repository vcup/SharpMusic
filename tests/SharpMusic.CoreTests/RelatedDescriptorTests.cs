using SharpMusic.Core.Descriptor;
using SharpMusic.Core.Utils;

namespace SharpMusic.CoreTests;

public class RelatedDescriptorTests
{
    [Test]
    public void Add_AddA2B_BContainsA()
    {
        // arrange
        var a = new Descriptor();
        var b = new Descriptor();

        // act
        b.RelatedDescriptors.Add(a);

        // assert
        Assert.That(a.RelatedDescriptors, Does.Contain(b));
        Assert.That(b.RelatedDescriptors, Does.Contain(a));
    }

    [Test]
    public void Remove_RemoveAInB_ADoesNotContainB()
    {
        // arrange
        var a = new Descriptor();
        var b = new Descriptor();
        a.RelatedDescriptors.AddWithoutNotify(b);
        b.RelatedDescriptors.AddWithoutNotify(a);

        // act
        b.RelatedDescriptors.Remove(a);

        // assert
        Assert.That(a.RelatedDescriptors, Does.Not.Contain(b));
        Assert.That(b.RelatedDescriptors, Does.Not.Contain(a));
    }

    [Test]
    public void ClearItems_ClearInA_BDoesNotContainA()
    {
        // arrange
        var a = new Descriptor();
        var b = new Descriptor();
        a.RelatedDescriptors.AddWithoutNotify(b);
        b.RelatedDescriptors.AddWithoutNotify(a);

        // act
        b.RelatedDescriptors.Clear();

        // assert
        Assert.That(a.RelatedDescriptors, Does.Not.Contain(b));
        Assert.That(b.RelatedDescriptors, Does.Not.Contain(a));
    }

    [Test]
    public void Replace_ReplaceAInBWithC_BContainCAndNotContainA()
    {
        // arrange
        var a = new Descriptor();
        var b = new Descriptor();
        var c = new Descriptor();
        a.RelatedDescriptors.AddWithoutNotify(b);
        b.RelatedDescriptors.AddWithoutNotify(a);

        // act
        b.RelatedDescriptors[0] = c;

        // assert
        Assert.That(a.RelatedDescriptors, Does.Not.Contain(b));
        Assert.That(b.RelatedDescriptors, Does.Not.Contain(a).And.Contain(c));
        Assert.That(c.RelatedDescriptors, Does.Contain(b));
    }

    [Test]
    public void Replace_ReplaceAInBWithItSelf_NothingChange()
    {
        // arrange
        var a = new Descriptor();
        var b = new Descriptor();
        a.RelatedDescriptors.AddWithoutNotify(b);
        b.RelatedDescriptors.AddWithoutNotify(a);

        // act
        b.RelatedDescriptors[0] = a;

        // assert
        Assert.That(a.RelatedDescriptors, Does.Contain(b));
        Assert.That(b.RelatedDescriptors, Does.Contain(a));
    }

    class Descriptor : IDescriptor
    {
        public Descriptor()
        {
            RelatedDescriptors = new RelatedDescriptors<Descriptor, Descriptor>(i => i.RelatedDescriptors, this);
        }

        public Guid Guid { get; } = Guid.NewGuid();

        public IList<string> Names
        {
            get => null!;
            set { }
        }

        public string Description
        {
            get => string.Empty;
            set { }
        }

        public RelatedDescriptors<Descriptor, Descriptor> RelatedDescriptors { get; }
    }
}