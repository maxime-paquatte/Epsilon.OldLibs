using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Queries
{
    public class ValueBySystemKey : IQuery
    {

        public string RepoSystemKey { get; set; }

        public string ValueSystemKey { get; set; }
    }
}
