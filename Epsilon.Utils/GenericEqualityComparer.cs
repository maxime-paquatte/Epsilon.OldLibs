using System;
using System.Collections.Generic;

namespace Epsilon.Utils;

public class GenericEqualityComparer<T> : EqualityComparer<T>
{
    private readonly Func<T, object> _selector;

    public GenericEqualityComparer(Func<T, object> selector)
    {
        _selector = selector;
    }


    public override bool Equals(T x, T y)
    {
        return _selector(x).Equals(_selector(y));
    }

    public override int GetHashCode(T obj)
    {
        return _selector(obj).GetHashCode();
    }
}