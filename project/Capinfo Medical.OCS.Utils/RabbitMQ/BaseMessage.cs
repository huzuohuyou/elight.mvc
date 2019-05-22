using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingCenterSystem.Utils.RabbitMQ
{
    public delegate void NotifyEventHandler(BaseMessage sender);
    public class BaseMessage
    {
        public NotifyEventHandler NotifyEvent;
        public string MessageID { get; set; }

        public string MessageTitle { get; set; }

        public string MessageRouter { get; set; }

        public string MessageExecuteString { get; set; }

        public string ExcuteDetailString
        {
            get
            {
                var reault = string.Empty;
                foreach (var item in Dict.Keys)
                {
                    reault += Dict[item].ToString();
                }
                return reault;
            }
        }
        private Dictionary<string, Jobs> dict = new Dictionary<string, Jobs>();
        public Dictionary<string, Jobs> Dict { get { return dict; } set { dict = null; dict = value; } }
        public int Percent { get; set; }

        public class Jobs
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public string Status { get; set; }
            public string Exception { get; set; }
            public string TimeConsum { get; set; }
            public override string ToString()
            {
                if (Exception != string.Empty)
                {
                    return $"[{Status.PadLeft(6, ' ')}] {TimeConsum} {Code} {Name} 异常：{Exception}\n";
                }
                return $"[{Status.PadLeft(6, ' ')}] {TimeConsum} {Code} {Name} \n";
            }
        }

        public static class Tags
        {
            public static string Waitting = "等    待";
            public static string Done = "完    成";
            public static string Error = "错    误";
            public static string Exception = "异    常";
            public static string Cancel = "取    消";
        }



        #region 新增对订阅号列表的维护操作
        public void AddObserver(NotifyEventHandler ob)
        {
            NotifyEvent += ob;
        }
        public void RemoveObserver(NotifyEventHandler ob)
        {
            NotifyEvent -= ob;
        }

        #endregion

        public void Update()
        {
            if (NotifyEvent != null)
            {
                NotifyEvent(this);
            }
        }
    }
}
