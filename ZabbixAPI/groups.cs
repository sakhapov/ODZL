using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Zabbix
{
    public class HostGroups:Result<HostGroup>
    {

        protected override void init()
        {
            method = "hostgroup.get";
            Params = new { output = "extend" };
        }
        public override void get()
        {
            base.get();
            foreach (HostGroup h in result)
            {
                h.hosts = new Hosts(server);
                h.hosts.getByGroupID(h.groupid);
            }
        }
        public HostGroups(ZabbixConnection Server) : base(Server) { }
#region Comments
        //public  ZabbixConnection server{get;set;}
        // public HostGroup[] result;
        //public string method = "hostgroup.get";
        //public object Params = new { output = "extend" };
        //public HostGroups(ZabbixConnection zabbixConnection)
        //{
        //    server = zabbixConnection;
        //}
        //public void get()
        //{
        //    string res=(server.CallApi("hostgroup.get", server.obj2json(new { output = "extend" })));
        //    JavaScriptSerializer serializer = new JavaScriptSerializer();
        //    result=serializer.Deserialize<HostGroups>(res).result;
        //}
#endregion
    }
    public class HostGroup
    {
        public string groupid;
        public string name;
        public int inter;
        public override string ToString()
        {
            return name;
        }
        public Hosts hosts;
    }
}
