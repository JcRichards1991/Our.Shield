using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Shield.Persistance.Helpers
{
    public class SqlHelper
    {
        {
            var config = Activator.CreateInstance<T>();

            var sql = new Sql();
            sql.Where<T>(x => x.Type == config.Type);

            config = ApplicationContext.Current.DatabaseContext.Database.Single<T>(sql);

            return config;
        }

        {
            var config = Activator.CreateInstance<T>();

            var sql = new Sql();
            sql.Where<T>(x => x.Type == config.Type);

            config = ApplicationContext.Current.DatabaseContext.Database.Single<T>(sql);

            return config;
        }

        {
            throw new NotImplementedException();
        }
    }
}
