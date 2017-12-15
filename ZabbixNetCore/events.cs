using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zabbix
{
    public class Events:Result<Event>
    {
    //    "params":{
    //"time_from":"1284910040",
    //"time_till": "1284991200",
    //"output": "extend",
    //"sortfield": "clock",
    //"sortorder": "desc",
    //"limit": 10
        protected override void init()
        {
            method = "event.get";
            Params = new { 
                    output = "extend",
                    sortfield="clock",
                    sortorder = "ASC",
                    select_hosts="shorten",
                    select_triggers="shorten",
                    limit="1000"

            };
        }
        
        public Events(ZabbixConnection Server) : base(Server) { }
    }
    public class Event
    {
    }
}
