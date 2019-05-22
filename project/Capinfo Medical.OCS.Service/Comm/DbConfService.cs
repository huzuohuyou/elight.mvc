using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Service.comm.Interface;

namespace TestingCenterSystem.Service.comm
{
    public class DbConfService : BaseService<PDP_DB_CONF>, IDbConfService
    {
        private const string ConnctStr = "metadata=\"res://*/Unit.UTModel.csdl|res://*/Unit.UTModel.ssdl|res://*/Unit.UTModel.msl\";provider=System.Data.SqlClient;provider connection string=\"Data Source = {0}; User Id = {1}; Password = {2}; MultipleActiveResultSets = True; App = EntityFramework\";";
        public List<string> TestConnect(PDP_DB_CONF model)
        {
            try
            {
                var connect = string.Format(ConnctStr, model.SERVER_NAME, model.DB_USER, model.DB_PWD);
                var conn = new EntityConnection(connect);
                conn.Open();
                var sqlConnectStr = $"Data Source={model.SERVER_NAME};Initial Catalog=master;User ID={model.DB_USER};PWD={model.DB_PWD}";
                var adapter = new SqlDataAdapter("select name from master..sysdatabases", sqlConnectStr);
                var table = new DataTable();
                adapter.Fill(table);
                var dbNameList = (from DataRow row in table.Rows select row["name"].ToString()).OrderBy(o=>o.ToString()).ToList();
                
                return dbNameList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public int CDR_CONF_FLAG()
        {
            return 1;
        }

        public int UNIT_CONF_FLAG()
        {
            return 2;
        }

        public int CI_CONF_FLAG()
        {
            return 3;
        }
    }
}
