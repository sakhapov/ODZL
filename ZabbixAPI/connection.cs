using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using System.Threading;

namespace Zabbix
{
    /// <summary>
    /// Событие для уведомления других объектов о происходящем внутри объекта, используется для отладки
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public delegate void AlertEvent(string message); 

    /// <summary>
    /// Событие для уведомления основного потока приложения об изменениях в потоке получения информации от сервера
    /// </summary>
    /// <param name="message">Структура указывающее тип события и коментарий к нему</param>
    public delegate void ThreadEvent(UpdateInfoMessage message);

    /// <summary>
    /// Класс используется для установления соединения с сервером Zabbix, 
    /// и доступа к информации полученной от него
    /// </summary>
    public class ZabbixConnection
    {
        #region Private section
        private string auth_hash = "";                 //хеш для авторизации на сервере
        private int id = 0;                            //идентифиактор запроса к серверу        
        private string url;                            //полный URL скрипта принимающего запросы Zabbix API,
                                                       //обычно имеет вид http://zabbix.host/zabbix/api_jsonrpc.php 
        private  string ServerRoot;                    //Корень сервера для доступа к Zabbix,
                                                       //обычно имеет вид http://zabbix.host/zabbix/ 
        private JavaScriptSerializer serializer        //Объект для сериализации и десереализации в JSON формат 
                         = new JavaScriptSerializer();

        private string _user;                          //имя пользователя для доступа к Zabbix API (пользователь должен входить в группу "API users") 
        private string _password;                      //пароль для доступа к Zabbix API
        #endregion

        #region Public Section
        public string lastError { get; private set; }  //Содержит описание последней ошибки
        public HostGroups hostgroups;                  //Список хостов по группам
        public Maps maps;                              //Список карт
        public Zabbix.Triggers triggers;               //Триггеры 
        public Events events;                          //События на Zabbix-сервере
        public AlertEvent onAlert { get; set; }        //Событие для уведомления других объектов о происходящем внутри объекта, используется для отладки
        public ThreadEvent onUpdate { get; set; }      //Класс используется для установления соединения с сервером Zabbix, 
                                                       // и доступа к информации полученной от него
        public Thread firstThread;                     //Поток получение первичной информации от сервера
        public Thread longThread;                      //Поток, постоянно обновляющий данные о картах и триггерах 
        #endregion
    
        /// <summary>
        /// Событие для уведомления основного потока приложения об изменениях в потоке получения информации от сервера
        /// </summary>
        /// <param name="message">Структура указывающее тип события и коментарий к нему</param>
        public void Update(UpdateInfoMessage msg)
        {
            if (onUpdate!=null)
            {
                onUpdate(msg);
            }
        }
        
        /// <summary>
        /// Остановка всех потоков
        /// </summary>
        public void stop()
        {
            longThread.Abort();
            firstThread.Abort();
        }

        /// <summary>
        /// Обновление данных от сервера.
        /// Процедура должна выполняться отдельным потоком
        /// внутри содержится бесконечный цикл
        /// </summary>
        private void getInfo()
        {
            int i = 1;          //используется для отладки, показывает количество обновлений данных с момента запуска соединения
            int err = 0;        //количество ошибок при попытке получения данных
            int sleep = 5000;   //пауза между обращениями к серверу, задается в милисекундах
            int lasterr=err;    //переменная для обнаружения была ли ошибка на предыдущей операции, чтобы заново перелогиниться
            while (true)
            {
                Thread.Sleep(sleep);
                try
                {
                    if (err > lasterr) { lasterr = err; login(); }
                    //this.triggers.refresh();
                    //this.maps.refresh();
                    RefreshTrigger();
                    RefreshMaps();

                    this.Update(new UpdateInfoMessage(this) { message = i.ToString(), status = "REFRESH" });
                }
                catch (Exception ex) //в случае неудачи уведомляем основной поток приложения об ошибке
                {
                    err++;
                    this.Update(new UpdateInfoMessage(this) { message = ex.Message, status = "REFRESH_ERROR" });
                }
                i++;
            }
        }
        private void RefreshTrigger()
        {
            GetTriggers();
        }
        private void RefreshMaps()
        {
            GetMaps();
        }
        private void getFirstInfo()
        {
            bool logged = login();
            while (!logged) {
                Update(new UpdateInfoMessage(this) { message = "пауза между попытками...", status = "LOGIN" });
                Thread.Sleep(3000);
                logged = login(); 
            }
            GetHosts();
            GetTriggers();
            GetMaps();
            Update(new UpdateInfoMessage(this) { message = "", status = "DEBUG" });
            //File.WriteAllText(@"c:\temp\odzl\maps.txt", maps.stringResult);
            //File.WriteAllText(@"c:\temp\odzl\hostgroups.txt", hostgroups.stringResult);
            //File.WriteAllText(@"c:\temp\odzl\triggers.txt", triggers.stringResult);
            Update(new UpdateInfoMessage(this) { message = "", status = "OK" });
            longThread.Start();
        }
        private void GetHosts()
        {
            Update(new UpdateInfoMessage(this) { message = "", status = "HOSTS" });
            hostgroups.get();
        }
        private void GetTriggers()
        {
            Update(new UpdateInfoMessage(this) { message = "", status = "TRIGGERS" });
            triggers.get();
        }

        private void GetMaps()
        {
            this.Update(new UpdateInfoMessage(this) { message = "", status = "MAPS" });
            maps.get();
        }
        /// <summary>
        /// Процедура авторизации
        /// </summary>
        /// <returns></returns>
        public bool login()
        {
            bool res;
            onUpdate(new UpdateInfoMessage(this) { message = "Попытка авторизоваться", status = "LOGIN" });
            try
            {
                var userinfo = new { user = _user, password = _password };
                string result = CallApi("user.authenticate", userinfo);
                auth_hash = (serializer.Deserialize<simpleresult>(result)).result;
                res=true;
                Update(new UpdateInfoMessage(this) { message = "Авторизация произошла успешно", status = "LOGIN" });
            }
            catch (Exception ex)
            {
                Update(new UpdateInfoMessage(this) { message = "Ошибка авторизации:" + ex.Message, status = "LOGIN" });
                res= false;
            }
            return res;
        }

        public ZabbixConnection(string api_url, string user, string password)
        {
            _user = user;
            _password = password;
            ServerRoot=api_url;
            url = ServerRoot + @"/api_jsonrpc.php";
            hostgroups = new HostGroups(this);
            maps= new Maps(this);
            triggers = new Triggers(this);
            events = new Events(this);
            firstThread = new Thread(getFirstInfo);
            longThread = new Thread(getInfo);
        }

        private string GetWebRequest(string body)
        {
            WebRequest wb = WebRequest.Create(url);
            wb.ContentType = @"application/json-rpc";
            wb.Credentials = CredentialCache.DefaultCredentials;
            ASCIIEncoding encoding = new ASCIIEncoding();
            string postData = body;
            byte[] data = encoding.GetBytes(postData);
            wb.Method = "POST";
            wb.ContentLength = data.Length;
            //wb.Timeout = 10000;
            Stream newStream = wb.GetRequestStream();
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            HttpWebResponse response = (HttpWebResponse)wb.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            return responseFromServer;
        }
        private Stream GetWebFile(string url)
        {
            
            WebRequest wb = WebRequest.Create(url);
            wb.Credentials = CredentialCache.DefaultCredentials;
            HttpWebResponse response = (HttpWebResponse)wb.GetResponse();
            Stream dataStream = response.GetResponseStream();
            return dataStream;
        }
        public Stream GetImageByID(string imageID)
        {
            return GetWebFile(ServerRoot + @"/imgstore.php?iconid=" + imageID);
        }

        public string obj2json(object obj)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(obj);
        }

        public string CallApi(string method, object param)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            object Query = new
            {
                jsonrpc = "2.0",
                auth = auth_hash,
                id = id.ToString(),
                method = method,
                Params = param
            };
            String qr =  obj2json(Query);
            qr=qr.Replace("Params", "params");
            id++;
            Alert("request:\n" + qr);
                // onAlert( }
            string result = GetWebRequest(qr);
            Alert("response:\n" + result);
            return result;
        }
        public void connect()
        {
            firstThread.Start();
        }


        public string ApiVersion()
        {
            string result = CallApi("apiinfo.version", null);
            return (serializer.Deserialize<simpleresult>(result)).result; ;
        }

        public void Alert(string message)
        {
            try
            {
                onAlert(message);
            }
            catch(NullReferenceException ex)
            {
            }
        }

    }
    public static class center
    {
        public static ZabbixConnection connection;

    }
    public class UpdateInfoMessage
    {
        public string status { get; set; }
        public string message { get; set; }
        public object sender { get; set; }
        public UpdateInfoMessage(object Sender, string Status = "", string Message = "")
        {
            status = Status;
            message = Message;
            sender = Sender;
        }

    }
}