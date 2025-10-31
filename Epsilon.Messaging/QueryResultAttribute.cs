using System;

namespace Epsilon.Messaging;

public class QueryResultAttribute : Attribute
{
    public Type ResultType { get; set; }

    public QueryResultAttribute(Type resultType)
    {
        ResultType = resultType;
    }
}
