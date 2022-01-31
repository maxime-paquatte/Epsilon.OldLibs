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
                if (claimsAttr != null && !_claimsValidator.ValidateAny(ctx, claimsAttr.RequiredClaims))
                    throw new UnauthorizedAccessException("Claims no validated: " + claimsAttr.RequiredClaims);
                
                var featureAttr = typeof(T).GetCustomAttribute<FeatureAttribute>();
                if (featureAttr != null && !_claimsValidator.ValidateFeature(ctx, featureAttr.Feature))
                    throw new UnauthorizedAccessException("Can not access to feature : " + featureAttr.Feature);

                var readers = _store.ResolveReader(typeof(T)).ToArray();
                if(readers.Length == 0)
                    throw new InvalidOperationException("No reader found for query: " + typeof(T).FullName);

                foreach (IQueryJSonReader<T> reader in readers)   
                {
                    var r = reader.Read(ctx, query);
                    if (r == string.Empty) return "{}";
                    if (r != null) return r;
                }
                throw new InvalidOperationException("no reader returned results: " + typeof(T).FullName);
            }
        }
    }
}
