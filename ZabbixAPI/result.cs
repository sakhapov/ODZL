using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Zabbix;

namespace Zabbix
{
    /// <summary>
    /// Базовый класс для результатов запросов к серверу Zabbix
    /// </summary>
    /// <typeparam name="T">Тип одного элемента из коллекции результатов (например карта, хост, триггер и т.д.)</typeparam>
    public class Result<T>:IEnumerable<T>
    {
        /// <summary>
        /// храниться результат выполнения запроса к серверу       
        /// </summary>
        public T[] result;

        /// <summary>
        /// коллекция для использования в привязках данных в WPF проектах
        /// </summary>
       // public ObservableCollection<T> collection;   

        /// <summary>
        /// Активное соединение с сервером
        /// </summary>
        public ZabbixConnection server;              

        /// <summary>
        /// метод Из Zabbix API, например maps.get, triggers.update и т.д 
        /// </summary>
        protected string method;                     

        /// <summary>
        /// Объект с параметрами запроса к серверу 
        /// </summary>
        protected object Params;                       

        /// <summary>
        /// строковое значение ответа сервера, используется в основном для отладки
        /// </summary>
        public string stringResult;                    

        /// <summary>
        /// Объект для синхронизации доступа из разных потоков.
        /// используется в структурах типа:
        /// lock(var.SyncRoot)
        ///     { 
        ///         somework(var);
        ///     }
        /// </summary>
        public object SyncRoot;                      

        /// <summary>
        /// процедура для переопределения в Классах наследниках, используется для инициализации параметров запроса "method" и "Params"   
        /// </summary>
        /// <example>
        ///    method = "hostgroup.get";
        ///    Params = new { output = "extend" };
        /// </example>
        protected virtual void init() { }            

        public Result(ZabbixConnection Server)
        {
            init();
            server = Server;
            SyncRoot = new object();
           // collection = new ObservableCollection<T>();
        }
        public Result()
        {
            init();
        }
        /// <summary>
        /// Получение данных от сервера, результат заноситься в переменную result и collection
        /// </summary>
        public virtual void get()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            server.Update(new UpdateInfoMessage(this) { message = "Отправка запроса к серверу...", status = "INFO" });
            lock (SyncRoot)
            {
                stringResult = (server.CallApi(method, Params));
                server.Update(new UpdateInfoMessage(this) { message = "Обработка результата запроса", status = "INFO" });
                result = serializer.Deserialize<Result<T>>(stringResult).result;
                if (result==null){result=new T[1];}
                server.Update(new UpdateInfoMessage(this) { message = "Копирование результата в коллекцию", status = "INFO" });
                //collection.Clear();
                //foreach (T item in result)
                //{
                //    collection.Add(item);
                //}
                server.Update(new UpdateInfoMessage(this) { message = "", status = "INFO" });
            }
        }

        #region Индексаторы и итераторы
        /// <summary>
        /// Индексатор класса, используется для доступа к полю result
        /// </summary>
        /// <param name="index">Индекс элемента в массиве</param>
        /// <returns>атомарный элемент из массива результатов запроса к серверу Zabbix</returns>
        public T this[int index]
        {
            get
            {
                return result[index];
            }
        }
        //реализация интерфейса для использования класса в циклах foreach
        public IEnumerator<T> GetEnumerator()
        {
            foreach (T item in result)
            {
                yield return item;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }

    /// <summary>
    /// Простейший класс для хранения строковых результатов, например в процедурах авторизации или получения номера версии API
    /// </summary>
    public class simpleresult
    {
        public string result;
    }
}
