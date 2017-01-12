using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace com.gseo.network
{
    /// <summary>
    /// 個人電腦網路資源
    /// </summary>
    public class Network
    {
        private string _validNetworkIPStartWith = "192"; //合法的IP開頭

        /// <summary>
        /// 取得IP
        /// </summary>
        /// <returns>IP address</returns>
        public string GetIP()
        {
            //if (GetCurrentConnected() == null)
            //    return "";

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ///合法IP段才回覆IP內容
                    if (ip.ToString().StartsWith(_validNetworkIPStartWith))
                        return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        /// <summary>
        /// 取得機器名稱
        /// </summary>
        /// <returns></returns>
        public string GetMachineName()
        {
            return System.Environment.MachineName;
        }

        

        
    }
}
