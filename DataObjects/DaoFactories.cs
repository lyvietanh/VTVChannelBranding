using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataObjects
{
    public class DaoFactories
    {
        public static IDaoFactory GetFactory(string dataProvider)
        {
            // return the requested DaoFactory

            switch (dataProvider.ToLower())
            {
                //case "ado.net":
                //    return new AdoNet.DaoFactory();
                //case "linq2sql":
                //    return new Linq2Sql.DaoFactory();
                case "entityframework":
                default:
                    return new EF.DaoFactory();
            }
        }
    }
}
