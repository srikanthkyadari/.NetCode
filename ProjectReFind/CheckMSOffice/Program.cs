using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace CheckMSOffice
{
    public class Program
    {
        public Boolean IsMSOfficeInstalled()
        {
            RegistryKey localMachine = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Office");            

			string version = string.Empty;

			foreach(string key in localMachine.GetSubKeyNames())
			{
				if (key == "7.0") 
					version = "1995";
				else if (key == "8.0")
					version = "1997";
				else if (key == "9.0")
					version = "2000";
				else if (key == "10.0")
					version = "XP";
				else if (key == "11.0")
					version = "2003";
				else if (key == "12.0")
					version = "2007";
				else if (key == "14.0")
					version = "2010";

				if (!string.IsNullOrEmpty(version))
				{
                    return true;
				}
			}

            return false;
		}

        static void Main(string[] args)
        {
           

        }
    }  
    
}
