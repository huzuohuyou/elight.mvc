using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace TestingCenterSystem.Utils.Hubs
{
    public class ProcessHub : Hub
    {
        private static ProcessHub Instance;
        public ProcessHub() { }
        public static ProcessHub GetInstance()
        {
            if (Instance == null)
            {
                Instance = new ProcessHub();
            }
            return Instance;
        }
        public void Send(string name, string percent, string title = "", string note = "", string executeList = "")
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ProcessHub>();
            context.Clients.All.addNewMessageToPage(name, percent, title, note, executeList);
        }
        public override Task OnConnected()
        {
            Trace.WriteLine("客户端连接成功");
            return base.OnConnected();

        }
    }
}