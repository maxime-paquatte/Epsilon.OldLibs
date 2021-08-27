using System;
using System.Collections.Generic;
using System.Text;
using Epsilon.Messaging;
using Epsilon.Messaging.Sql;
using Epsilon.Model.Culture.Queries;

namespace Epsilon.Model.Culture
{


    public class SqlQueryJSonReader : SqlQueryJSonReaderBase, IQueryJSonReader<All>
    {
        public SqlQueryJSonReader(IConfig config) : base(config.ConnectionString)
        {
        }

        public string Read(IMessageContext context, All query)
        {
            return Read(context, query, "Ep.svCultureAll");
        }
        
    }
}
