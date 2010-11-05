using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;

namespace Zabbix
{
    public class Maps:Result<Map>
    {

        protected override void init()
        {
            method = "Map.get";
            Params = new 
                    {
                        select_selements= "extend" ,
                        select_links= "extend",
                        output= "extend",
                       // nodeids=new int[] {1,2}

                    };
        }

        public Maps(ZabbixConnection Server) : base(Server) { }
        
        public override void get()
         {
             base.get();
             foreach (Map m in result)
             {
                 foreach (MapElement elem in m.selements)
                 {
                     List<Trigger> ls ;
                     lock (server.triggers.SyncRoot)
                     {
                         ls= server.triggers.getByHostid(elem.elementid);
                     }
                     elem.triggers = ls.ToArray();
                     if (elem.elementid == "200200000010102") { elem.elementid = "200200000010102"; }
                 }
                 m.server = server;
             }

         }
         public void refresh()
         {
             get();
         }
    }
    public class Map
    {
        public string sysmapid;
        public string name;// { get { return "SomeMap " + sysmapid; } set {} }
        public int width{get;set;}
        public int height{get;set;}
        public MapElement[] selements { get; set; }
        public link[] links { get; set; }
        public ZabbixConnection server { get; set; }
        public override string ToString()
        {
            return name;
        }
        public MapElement getById(string id)
        {

           var result= from res in selements
                   where res.selementid == id
                   select res;
           List<MapElement> lnks = new List<MapElement>();
           foreach (MapElement ln in result)
           {
               lnks.Add(ln);
           }
           return lnks[0];
        }
        public List<link> getAllElementLinks(string elementId)
        {
            var result = from res in links
                        where res.selementid1 == elementId || res.selementid2 == elementId
                         select res;
            List<link> lnks=new List<link>();
            foreach (link ln in result)
            {
                lnks.Add(ln);
            }

            return lnks;
        }
        public void update()
        {
            string method = "map.update";
            MapElement[] temp =(MapElement[])selements.Clone();
            foreach (MapElement m in temp)
            {
                m.Tag = null;
            }
            object Params = new
            {
                sysmapid = sysmapid,
                //name="Test234",
                selements=temp
                // nodeids=new int[] {1,2}

            };
            string r=server.CallApi(method, Params);
            File.WriteAllText("q.txt", server.obj2json(Params));
            File.WriteAllText("tmp.txt", r);
        }

    }
    public class MapElement
    {
        public Map map;
        public string selementid;
        public string sysmapid;
        public string elementid;
        public string elementtype;
        public string label;
        public int x;
        public int y;
        public string url;
        public string iconid_off;
        public string iconid_on;
        public string iconid_disabled;
        public string iconid_maintance;
        public object Tag;
        public Trigger[] triggers { get; set; }

        public override string ToString()
        {
            return label;
        }


    }
    public class link
    {
        public string linkid;
        public string sysmapid;
        public string selementid1;
        public string selementid2;
        public string drawtype;
        public string color;
        public string label;
        public object Tag;
    }

}
