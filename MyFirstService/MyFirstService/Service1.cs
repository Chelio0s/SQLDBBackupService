using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;
using System.Threading;
using System.Windows.Forms;
using System.Reflection.Emit;
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
            var dto = new DateTime(2017, 10, 10, 12, 02, 0);
            var dte = new DateTime(2017, 10, 10, 12, 15, 0);
            if (dtn.TimeOfDay > dto.TimeOfDay && dtn.TimeOfDay < dte.TimeOfDay)
            {       string path= Settings.Default.path;
                    string userid = Settings.Default.UserId;
                    string pas = Settings.Default.Password;
                    string dbname = Settings.Default.DBName;
                    string serverename = Settings.Default.ServerName;
                    string directoryname = path + @"\" + dbname + "_" + DateTime.Today.ToString().Split(' ')[0];
                try
                {
                    if (!Directory.Exists(directoryname))
                    {
                        Directory.CreateDirectory(directoryname);
                        string connect = 
                            (String.Format(@"Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3}",serverename,dbname,userid,pas)
                            );

                        using (SqlConnection connection = new SqlConnection(connect))
                        {
                            connection.Open();
                            string sql = @"BACKUP DATABASE belwestDB TO DISK = '" + directoryname +
                                         @"\belwestDB.bak'";
                            SqlCommand command = new SqlCommand(sql, connection);
                            var c = command.ExecuteNonQuery();

                        }
                    }
                }
                catch (Exception e)
                {
                    string data = directoryname + @"\err_log.txt";
                    var h = File.Open(data, FileMode.OpenOrCreate);
                    StreamWriter str = new StreamWriter(h);
                    str.WriteLine(e.Message);
                    str.Close();
                }
            }
        }
        public void start()
        {
            TimerCallback tm = new TimerCallback(addToDirectory);
            System.Threading.Timer timer = new System.Threading.Timer(tm, 0, 0, 2000);
        }
    }
}
