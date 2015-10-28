using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Configuration;

namespace HydrologyExportService
{
    public partial class HydrologyExport : ServiceBase
    {
        public HydrologyExport()
        {
            InitializeComponent();
            
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("OnStart");
            timer1_Elapsed(null, null);
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("OnStop");

        }

        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                eventLog1.WriteEntry("timer1_Elapsed");


                eventLog1.WriteEntry(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

                string setting = ConfigurationManager.AppSettings["DataExport"];

                if (setting == null)

                    setting = "Setting not found";

                eventLog1.WriteEntry(setting);

                string strAgkDataFolder = setting;
                strAgkDataFolder.TrimEnd(new char[] {'\\'});



                Hydro.HydroServiceClient theClient = new Hydro.HydroServiceClient();
                const int TYPE_AGK = 6;
                
                foreach (var Site in theClient.GetSiteList(TYPE_AGK))
                {
                    string strSiteFolder = strAgkDataFolder + "\\" + Convert.ToInt32(Site.SiteCode).ToString( ("00000"));
                    if (!Directory.Exists(strSiteFolder))
                    {
                        Directory.CreateDirectory(strSiteFolder);
                    }
                    strSiteFolder += "\\уровни";
                    if (!Directory.Exists(strSiteFolder))
                    {
                        Directory.CreateDirectory(strSiteFolder);
                    }

                    int countPrevMonth = 12;
                    for ( ;  countPrevMonth>0; countPrevMonth--)
                    {

                        DateTime bgnDate = new DateTime(DateTime.Now.AddMonths(-countPrevMonth).Year,
                            DateTime.Now.AddMonths(-countPrevMonth).Month, countPrevMonth);
                        DateTime endDate = new DateTime(DateTime.Now.AddMonths(-countPrevMonth).Year,
                            DateTime.Now.AddMonths(-countPrevMonth).Month,
                            DateTime.DaysInMonth(DateTime.Now.AddMonths(-countPrevMonth).Year, DateTime.Now.AddMonths(-countPrevMonth).Month));
                        const int WATER_LEVEL = 2;

                        string strFileName = strSiteFolder + "\\" + Site.SiteCode.ToString() + "_" + bgnDate.ToString("yyyy_MM") + ".asc";

                        if (File.Exists(strFileName))
                        {
                            continue;
                        }

                        var fileHandle = File.CreateText(strFileName);
                        var DataValueList = theClient.GetDataValues(Site.SiteId, bgnDate, endDate, WATER_LEVEL, null, null, null);
                        if (DataValueList != null)
                        {
                            foreach (var item in DataValueList)
                            {
                                string line = item.Date.ToString("dd.MM.yyyy ") + item.Date.ToLongTimeString() + "\t" + item.Value.ToString() + "\tcм";
                                fileHandle.WriteLine(line);
                            }
                        }
                        fileHandle.Close();
                    }
                    
                }


            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry(ex.Message);
                eventLog1.WriteEntry(ex.StackTrace);
            }
        }


    }
}
