using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Zabbix
{
    public class Hosts:Result<Host>
    {
        protected override void init()
        {
            method = "host.get";
            Params = new { output = "extend" };
        }
        public void getByGroupID(string GroupID)
        {
            Params = new { output = "extend", groupids = GroupID };
            base.get();

        }
        public Hosts(ZabbixConnection Server) : base(Server) { }

    }
    public class Host
    {
        public string host;
        public string hostid;
        public string ip;
        public override string ToString()
        {
            return host;
        }
    }
}
