using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace Zabbix
{
    public class Triggers:Result<Trigger>
    {
        protected override void init()
        {
            method = "trigger.get";
            Params = new 
                    {
                        output = "extend",
                        select_hosts = "extend",
                        monitored="1",
                        templated="0",
        //                only_true="1",
                        sortfield = "lastchange"
                    };
        }
         public Triggers(ZabbixConnection Server) : base(Server) { }
         public override void get()
         {
             base.get();
             foreach (Trigger tr in result)
             {
                 tr.host = tr.hosts[0];
             }
             result[0].value = "11";
 
         }
         public List<Trigger> getByHostid(String hostid)
         {
             var q = from res in result
                          where res.host.hostid == hostid
                          select res;
             List<Trigger> trgs = new List<Trigger>(q);
             return trgs;
         }
         public void refresh()
         {
             get();
             //foreach (Map m in server.maps.result)
             //{
             //    foreach (MapElement elem in m.selements)
             //    {
             //        List<Trigger> ls = server.triggers.getByHostid(elem.elementid);
             //        elem.triggers = ls.ToArray();
             //    }
             //}
             //result[0].value = "15";
         }


    }
    public class Trigger
    {
        public Host[] hosts { get; set; }
        public string triggerid { get; set; }
        public string expression { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string status { get; set; }
        public string value { get; set; }
        public string priority { get; set; }
        public string lastchange { get; set; }
        public DateTime lastchangeDateTime
        { get
            {
                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return origin.AddSeconds(double.Parse(lastchange));
            }
        }
        public string dep_level { get; set; }
        public string comments { get; set; }
        public string error { get; set; }
        public string templateid { get; set; }
        public string type { get; set; }
        public Host host { get; set; }
    }
}
