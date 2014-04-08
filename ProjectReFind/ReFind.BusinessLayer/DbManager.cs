using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SQLite;

namespace ReFind.BusinessLayer
{
    public class DbManager
    {
        string connectionstring = "";

        public DbManager()
        {
            connectionstring = ConfigurationManager.ConnectionStrings["GetConstr"].ConnectionString;
        }

        public static string GetConnectionString()
        {
             return ConfigurationManager.ConnectionStrings["GetConstr"].ConnectionString;
        }

        public string GetEventsList()
        {
            SQLiteConnection con = new SQLiteConnection(connectionstring);
            SQLiteCommand slitecmd = null;
            StringBuilder jsonobj = new StringBuilder();
            try
            {
                slitecmd = new SQLiteCommand("select * from events", con);
                if (con.State.ToString() == "Closed")
                { con.Open(); }
                SQLiteDataReader dr = slitecmd.ExecuteReader();

                while (dr.Read())
                {
                    jsonobj.Append("FileHashCode" + ":" + dr["file"].ToString());
                }                

                if (con.State.ToString() == "Opened")
                { con.Close(); }

                return jsonobj.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
            finally
            {
                slitecmd = null;
                con.Dispose();
            }

            return string.Empty;
        }
    }
}
