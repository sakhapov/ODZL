using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZabbixNET
{
    public class settingsController
    {
        public string user { get; set; }
        public string pass { get; set; }
        public string host { get; set; }
        public bool isFirstRun { get; set; }


        public settingsController()
        {
            isFirstRun = ZabbixNET.Properties.Settings.Default.isfirstrun;
            host = ZabbixNET.Properties.Settings.Default.zhost;
            user = ZabbixNET.Properties.Settings.Default.zuser;
            pass = ZabbixNET.Properties.Settings.Default.zpass;
        }
        public void save()
        {
            isFirstRun = false;
            ZabbixNET.Properties.Settings.Default.isfirstrun = isFirstRun;
            ZabbixNET.Properties.Settings.Default.zhost = host;
            ZabbixNET.Properties.Settings.Default.zuser = user;
            ZabbixNET.Properties.Settings.Default.zpass = pass;
            ZabbixNET.Properties.Settings.Default.Save();
        }
        public void reload()
        {
            ZabbixNET.Properties.Settings.Default.Reload();
            ZabbixNET.Properties.Settings.Default.isfirstrun=isFirstRun;
            ZabbixNET.Properties.Settings.Default.zhost=host ;
            ZabbixNET.Properties.Settings.Default.zuser=user;
            ZabbixNET.Properties.Settings.Default.zpass=pass;
        }

    }
}
