using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epsilon.BackgroundTask
{
    public class ServicesStore
    {
        public IDictionary<string, Type> Types { get; private set; }

        public ServicesStore(IEnumerable<Type> types)
        {
            Types = types.ToDictionary(p => p.FullName, p => p);
        }
    }
}
