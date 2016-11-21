using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataObjects.EF
{
    public partial class VTVChannelBrandingEntities : DbContext
    {
        public VTVChannelBrandingEntities(ConnectionString connectionString) : base(GetSqlConnection(connectionString), true)
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        public static DbConnection GetSqlConnection(ConnectionString connectionString)
        {
            // Initialize the EntityConnectionStringBuilder. 
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();

            // Set the provider name. 
            entityBuilder.Provider = "System.Data.SqlClient";

            // Set the provider-specific connection string. 
            entityBuilder.ProviderConnectionString = connectionString.ToString();

            // Set the Metadata location. 
            entityBuilder.Metadata = "res://*/EF.VTVChannelBrandingModel.csdl|res://*/EF.VTVChannelBrandingModel.ssdl|res://*/EF.VTVChannelBrandingModel.msl";

            return new EntityConnection(entityBuilder.ToString());
        }

    }
}
