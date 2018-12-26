using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.IO;
using System.Data.SqlClient;
using System.Threading;
using MyFirstService.Properties;


namespace SQLBackUpService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
           MyServiceClass msc = new MyServiceClass();
           Thread mscThread = new Thread(new ThreadStart(msc.start));
           mscThread.Start();
        }

        protected override void OnStop()
        {

        }
    }

    class MyServiceClass
    {
        private void addToDirectory(object obj)
        {
            var dtn = DateTime.Now;
            if (dtn.Hour==12 && dtn.Minute<=15)
            {       string path= Settings.Default.path;
                    string userid = Settings.Default.UserId;
                    string pas = Settings.Default.Password;
                    string dbname = Settings.Default.DBName;
                    string serverename = Settings.Default.ServerName;
                    string directoryname = path + @"\" + "backup" + "_" + DateTime.Today.ToString().Split(' ')[0];

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                try
                {
                    if (!Directory.Exists(directoryname))
                    {
                        Directory.CreateDirectory(directoryname);
                        string connect =
                            String.Format(@"Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3}",
                                serverename, dbname, userid, pas);

                        using (SqlConnection connection = new SqlConnection(connect))
                        {
                            connection.Open();
                            string getDBsCommandStr = "select name from master.sys.databases";
                            SqlCommand getDbsCommand = new SqlCommand(getDBsCommandStr, connection);
                            var READER = getDbsCommand.ExecuteReader();
                            List<string> listDB = new List<string>();
                            while (READER.Read())
                            {
                                listDB.Add(READER.GetString(0));
                            }

                            listDB.Remove("tempdb");
                            READER.Close();
                            foreach (var item in listDB)
                            {
                                string sql = string.Format(@"BACKUP DATABASE {0} TO DISK = '" + directoryname +
                                                           @"\{0}.bak'", item);
                                SqlCommand command = new SqlCommand(sql, connection);
                                var c = command.ExecuteNonQuery();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string data = directoryname + @"\err_log.txt";
                    var h = File.Open(data, FileMode.OpenOrCreate);
                    StreamWriter str = new StreamWriter(h);
                    str.WriteLine(ex.Message);
                    str.Close();
                }
            }
        }
        public void start()
        {
            TimerCallback tm = new TimerCallback(addToDirectory);
            Timer timer = new Timer(tm, 0, 0, 2000);
        }
    }
}
