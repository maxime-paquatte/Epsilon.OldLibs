using System;
using System.Linq;
using System.Reflection;

namespace Epsilon.Messaging.Host
{
    public class QueryJSonReader : IQueryJSonBus
    {
        private readonly IStore _store;
        private readonly IMessageContextFactory _contextFactory;
        private readonly IClaimsValidator _claimsValidator;

        public QueryJSonReader(IStore store, IMessageContextFactory contextFactory, IClaimsValidator claimsValidator)
        {
            _store = store;
            _contextFactory = contextFactory;
            _claimsValidator = claimsValidator;
        }


        public string Read<T>(T query) where T : IQuery
        {
            using (var scope = _contextFactory.GetScope())
            {
                var ctx = scope.GetContext();
                var claimsAttr = typeof(T).GetCustomAttribute<AnyClaimsAttribute>();
                if (claimsAttr != null && !_claimsValidator.ValidateAny(ctx, claimsAttr.Claims))
                    throw new UnauthorizedAccessException("Claims no validated: " + string.Join(", ", claimsAttr.Claims));

                var readers = _store.ResolveReader(typeof(T)).ToArray();
                foreach (IQueryJSonReader<T> reader in readers)   
                {
                    var r = reader.Read(ctx, query);
                    if (r != null) return r;
                }
                throw new InvalidOperationException("No reader found for query: " + typeof(T).FullName);
            }
        }
    }
}
