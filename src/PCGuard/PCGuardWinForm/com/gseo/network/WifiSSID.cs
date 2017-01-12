using NativeWifi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.gseo.network
{
    /// <summary>
    /// 定義一個Wifi AP的SSID資訊
    /// </summary>
    public class WifiSSID
    {
        public string SSID = "NONE";
        public Wlan.Dot11AuthAlgorithm dot11DefaultAuthAlgorithm;
        public string dot11DefaultCipherAlgorithm;
        public bool networkConnectable = false;
        public string notConnectableReason = "";
        public int signalQuality = 0;
        public WlanClient.WlanInterface wlanInterface = null;
        public string profileName = "";
    }
}
