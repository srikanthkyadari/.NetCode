using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web.Services;

namespace ConsoleTest
{
    public class DBManager
    {
        string connectionstring = "";

        public DBManager()
        {
            connectionstring = ConfigurationManager.ConnectionStrings["GetConstr"].ConnectionString;
        }

        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["GetConstr"].ConnectionString;
        }
        
        
        public string getEventsList(string sqlquery)
        {
            //SQLiteConnection con = new SQLiteConnection(connectionstring);
            //SQLiteCommand slitecmd = null;
            //StringBuilder jsonobj = new StringBuilder();
            //try
            //{
            //    slitecmd = new SQLiteCommand("select * from files", con);
            //    if (con.State.ToString() == "Closed")
            //    { con.Open(); }
            //    SQLiteDataReader dr = slitecmd.ExecuteReader();                
            //    List<files> fileslist = new List<files>();

            //    while (dr.Read())
            //    {
            //        files fi = new files
            //        {
            //            file = dr["file"].ToString(),
            //            pages = Convert.ToInt32(dr["pages"]),
            //            size = Convert.ToInt32(dr["size"]),
            //            ocr = Convert.ToInt32(dr["ocr"]),
            //            exclude = Convert.ToInt32(dr["exclude"])                        
            //        };
            //        fileslist.Add(fi);                    
            //    }

            //    string json = JsonConvert.SerializeObject(fileslist, Formatting.Indented);

            //    if (con.State.ToString() == "Opened")
            //    { con.Close(); }

            //    return jsonobj.ToString();
            //}
            //catch (Exception ex)
            //{
            //    return ex.Message.ToString();
            //}
            //finally
            //{
            //    slitecmd = null;
            //    con.Dispose();
            //}

            return "testing";
            //return string.Empty;
        }
    }

    public class files
    {
        public string file;    
        public int pages;  
        public int size;  
        public int ocr;
        public int exclude;
    }


}


