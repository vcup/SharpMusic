namespace SharpMusic.Core.Descriptor;

public interface IDescriptor
{
    public Guid Guid { get; }

    public IList<string> Names { get; set; }

    public string Description { get; set; }
}