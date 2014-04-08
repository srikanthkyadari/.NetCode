using System;
using System.Configuration;

namespace ConsoleTest
{
    public static class GetGlobalValues
    {
        public string GetConnectionString(string Key)
        {
            return ConfigurationManager.ConnectionStrings["GetConstr"].ConnectionString;
        }
    }

}