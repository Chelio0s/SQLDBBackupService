using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.IO;
using System.Data.SqlClient;
using System.Threading;
using SQLBackUpService.Properties;


namespace SQLBackUpService
{
    public partial class BackupService : ServiceBase
    {
        public BackupService()
        {
            InitializeComponent();
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
           ServiceClass msc = new ServiceClass();
           Thread mscThread = new Thread(new ThreadStart(msc.StartTimer));
           mscThread.Start();
        }

        protected override void OnStop()
        {

        }
    }

    class ServiceClass
    {
        private void AddToDirectory(object obj)
        {
            var dtn = DateTime.Now;
            if (dtn.Hour == 12 && dtn.Minute <= 20)
            {
                string path = Settings.Default.path;
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
                            String.Format(
                                @"Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3}",
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
                    else CreateLog(directoryname, "Не удалось создать бэкап");
                }
                catch (Exception ex)
                {
                    CreateLog(directoryname, ex.Message);
                }
            }
        }
        public void StartTimer()
        {
            TimerCallback tm = new TimerCallback(AddToDirectory);
            System.Threading.Timer timer = new System.Threading.Timer(tm, 0, 0, 20000);
        }

        void CreateLog(string directoryname, string ex)
        {
            string data = directoryname + @"\err_log.txt";
            var h = File.Open(data, FileMode.OpenOrCreate);
            StreamWriter str = new StreamWriter(h);
            str.WriteLine(ex);
            str.Close();
        }
    }
}

