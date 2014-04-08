using System;

public class Class1
{
	public Class1()
	{
        public int InsertFileDetails(string FilePath,string FileName,int pages)
        {
            FileHash fh = new FileHash();
            string HashCode = fh.HashFile(FilePath);

            SQLiteConnection con = new SQLiteConnection(@"data source=D:\Inklii.sqlite");
            con.ConnectionString = @"Data Source=D:\Inklii.sqlite;Version=3;;Version=2;New=True;Compress=True;";
            con.Open();            
            FileInfo fi = new FileInfo(FilePath);
            var fsize = fi.Length;

            SQLiteCommand cmd = new SQLiteCommand("insert into files(file,pages,size,ocr,exclude) values('" + HashCode + "'," + pages + "," + fsize + "," + 12323223 + "," + 0 + ")");
            int result = cmd.ExecuteNonQuery();

            return result;
        }
	}
}
