using System.Collections;

namespace SharpMusic.Core.Descriptor;

public class Playlist : IDescriptor, IList<Music>, IReadOnlyList<Music>
{
    private readonly List<Music> _musics;

    internal Playlist(Guid guid)
    {
        Guid = guid;
        _musics = new List<Music>();
        Names = new List<string>();
        Description = string.Empty;
    }

    public Playlist() : this(Guid.NewGuid())
    {
    }

    public Guid Guid { get; }

    public IList<string> Names { get; set; }

    public string Description { get; set; }

    #region Implemention Interface

    public void Add(Music item)
    {
        _musics.Add(item);
    }

    public void Clear()
    {
        _musics.Clear();
    }

    public bool Contains(Music item)
    {
        return _musics.Contains(item);
    }

    public void CopyTo(Music[] array, int arrayIndex)
    {
        _musics.CopyTo(array, arrayIndex);
    }

    public bool Remove(Music item)
    {
        return _musics.Remove(item);
    }

    public int IndexOf(Music item)
    {
        return _musics.IndexOf(item);
    }

    public void Insert(int index, Music item)
    {
        _musics.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _musics.RemoveAt(index);
    }

    public Music this[int index]
    {
        get => _musics[index];
        set => _musics[index] = value;
    }

    public bool IsReadOnly => false;

    int ICollection<Music>.Count => _musics.Count;

    int IReadOnlyCollection<Music>.Count => _musics.Count;

    public IEnumerator<Music> GetEnumerator()
    {
        return _musics.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}