using System.Collections;

namespace Trinity.PaymentPlatform.Model.Util;

public static class Extensions
{
    public static IReadOnlyCollection<T> AsReadOnly<T>(this ICollection<T> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        return source as IReadOnlyCollection<T> ?? new ReadOnlyCollectionAdapter<T>(source);
    }

    sealed class ReadOnlyCollectionAdapter<T> : IReadOnlyCollection<T>
    {
        readonly ICollection<T> _source;
        public ReadOnlyCollectionAdapter(ICollection<T> source) => this._source = source;
        public int Count => _source.Count;
        public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}