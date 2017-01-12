using NativeWifi;
using com.gseo.network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace com.gseo.network
{
    /// <summary>
    /// 個人電腦Wifi資源
    /// </summary>
    public class WifiNetwork : Network
    {
        private List<WifiSSID> _SSIDList; //掃出來的SSID
        private WlanClient _client;
        

        public WifiNetwork()
        {
            if (_client == null)
                _client = new WlanClient();

            _SSIDList = new List<WifiSSID>();
        }

        /// <summary>
        /// 掃描附件的WifiAP SSID
        /// </summary>
        public List<WifiSSID> ScanSSID()
        {
            foreach (WlanClient.WlanInterface iface in _client.Interfaces)
            {
                Wlan.WlanAvailableNetwork[] networks = iface.GetAvailableNetworkList(0);
                foreach (Wlan.WlanAvailableNetwork network in networks)
                {
                    WifiSSID ssid = new WifiSSID();
                    ssid.wlanInterface = iface;
                    ssid.SSID = GetStrForSSID(network.dot11Ssid);
                    ssid.profileName = network.profileName;
                    ssid.signalQuality = (int)network.wlanSignalQuality;
                    ssid.dot11DefaultAuthAlgorithm = network.dot11DefaultAuthAlgorithm;
                    ssid.dot11DefaultCipherAlgorithm = network.dot11DefaultCipherAlgorithm.ToString();
                    ssid.networkConnectable = network.networkConnectable;
                    ssid.notConnectableReason = network.wlanNotConnectableReason.ToString();
                    _SSIDList.Add(ssid);
                }
            }

            if (_SSIDList == null)
                return null;
            return _SSIDList;
        }

        /// <summary>
        /// 取得目前連接的SSID
        /// </summary>
        /// <returns>WifiSSID</returns>
        public WifiSSID GetCurrentConnected()
        {
            foreach (WlanClient.WlanInterface iface in _client.Interfaces)
            {
                if (iface.InterfaceState == Wlan.WlanInterfaceState.Connected)
                {

                    return GetSSIDbyProfileName(iface.CurrentConnection.profileName);
                }
            }
            return new WifiSSID();
        }

        /// <summary>
        /// 取得SSID
        /// </summary>
        /// <param name="name">SSID名稱</param>
        /// <returns>WifiSSID</returns>
        public WifiSSID GetSSID(string name)
        {
            if(name == null)
                return new WifiSSID();

            if (_SSIDList == null || _SSIDList.Count == 0)
                ScanSSID();

            foreach (WifiSSID ssid in _SSIDList)
            {
                if (ssid.SSID.Equals(name))
                    return ssid;
            }

            return new WifiSSID();
        }

        /// <summary>
        /// 取得SSID
        /// </summary>
        /// <param name="name">Profile名稱</param>
        /// <returns>WifiSSID</returns>
        public WifiSSID GetSSIDbyProfileName(string name)
        {
            if (name == null)
                return new WifiSSID();

            if (_SSIDList == null || _SSIDList.Count == 0)
                ScanSSID();

            foreach (WifiSSID ssid in _SSIDList)
            {
                if (ssid.profileName.Equals(name))
                    return ssid;
            }

            return new WifiSSID();
        }

        /// <summary>
        /// Retrieves XML configurations of existing profiles.
        /// </summary>
        /// <param name="name">Profile名稱</param>
        /// <returns>profile XML string</returns>
        public string GetStoredProfile(string name)
        {
            foreach (WlanClient.WlanInterface iface in _client.Interfaces)
            {
                foreach (Wlan.WlanProfileInfo profile in iface.GetProfiles())
                {
                    
                    if (profile.profileName.Equals(name))
                    {
                        Console.WriteLine("User profile with SSID {0}", profile.profileName);
                        return iface.GetProfileXml(profile.profileName);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 連接未加密SSID
        /// </summary>
        /// <param name="ssid">SSID</param>
        public void ConnectToSSID(WifiSSID ssid)
        {
            if (ssid == null)
                return;

            string profileName = ssid.SSID;
            string mac = StringToHex(profileName);
            string profileXml = string.Format("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><connectionMode>manual</connectionMode><MSM><security><authEncryption><authentication>open</authentication><encryption>none</encryption><useOneX>false</useOneX></authEncryption></security></MSM></WLANProfile>", profileName, mac);
            Connect(ssid, profileXml);
        }

        /// <summary>
        /// 連接加密SSID
        /// </summary>
        /// <param name="ssid">SSID</param>
        /// <param name="key">密碼</param>
        public void ConnectToSSID(WifiSSID ssid, string key)
        {
            if (ssid == null)
                return;
            if (key == null || key.Equals(""))
                throw new ArgumentNullException("You have to provide SSID Key!");

            string profileXml = GetStoredProfile(ssid.profileName);
            if (profileXml == null)
            {
                string profileName = ssid.SSID;
                string mac = StringToHex(profileName);
                string xml = "";
                switch (ssid.dot11DefaultAuthAlgorithm)
                {
                    case Wlan.Dot11AuthAlgorithm.RSNA_PSK:
                        xml = "<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><connectionMode>auto</connectionMode><MSM><security><authEncryption><authentication>WPAPSK</authentication><encryption>TKIP</encryption><useOneX>false</useOneX></authEncryption><sharedKey><keyType>passPhrase</keyType><protected>false</protected><keyMaterial>{1}</keyMaterial></sharedKey></security></MSM></WLANProfile>";
                        profileXml = string.Format(xml, profileName, key);
                        break;
                    case Wlan.Dot11AuthAlgorithm.WPA:
                        xml = "<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><connectionMode>auto</connectionMode><MSM><security><authEncryption><authentication>WPA</authentication><encryption>TKIP</encryption><useOneX>true</useOneX></authEncryption><OneX xmlns=\"http://www.microsoft.com/networking/OneX/v1\"><authMode>machineOrUser</authMode><EAPConfig><EapHostConfig xmlns=\"http://www.microsoft.com/provisioning/EapHostConfig\"><EapMethod><Type xmlns=\"http://www.microsoft.com/provisioning/EapCommon\">25</Type><VendorId xmlns=\"http://www.microsoft.com/provisioning/EapCommon\">0</VendorId><VendorType xmlns=\"http://www.microsoft.com/provisioning/EapCommon\">0</VendorType><AuthorId xmlns=\"http://www.microsoft.com/provisioning/EapCommon\">0</AuthorId></EapMethod><Config xmlns=\"http://www.microsoft.com/provisioning/EapHostConfig\"><Eap xmlns=\"http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1\"><Type>25</Type><EapType xmlns=\"http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1\"><ServerValidation><DisableUserPromptForServerValidation>false</DisableUserPromptForServerValidation><ServerNames></ServerNames><TrustedRootCA>de 28 f4 a4 ff e5 b9 2f a3 c5 03 d1 a3 49 a7 f9 96 2a 82 12 </TrustedRootCA></ServerValidation><FastReconnect>true</FastReconnect><InnerEapOptional>false</InnerEapOptional><Eap xmlns=\"http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1\"><Type>26</Type><EapType xmlns=\"http://www.microsoft.com/provisioning/MsChapV2ConnectionPropertiesV1\"><UseWinLogonCredentials>true</UseWinLogonCredentials></EapType></Eap><EnableQuarantineChecks>false</EnableQuarantineChecks><RequireCryptoBinding>false</RequireCryptoBinding><PeapExtensions><PerformServerValidation xmlns=\"http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2\">true</PerformServerValidation><AcceptServerName xmlns=\"http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2\">false</AcceptServerName></PeapExtensions></EapType></Eap></Config></EapHostConfig></EAPConfig></OneX></security></MSM></WLANProfile>";
                        profileXml = string.Format(xml, profileName, key);
                        break;
                    case Wlan.Dot11AuthAlgorithm.WPA_None:
                        profileXml = string.Format("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><MSM><security><authEncryption><authentication>open</authentication><encryption>WEP</encryption><useOneX>false</useOneX></authEncryption><sharedKey><keyType>networkKey</keyType><protected>false</protected><keyMaterial>{2}</keyMaterial></sharedKey><keyIndex>0</keyIndex></security></MSM></WLANProfile>", profileName, mac, key);
                        break;
                }
            }
            
            if(!profileXml.Equals(""))
                Connect(ssid, profileXml);
        }

        
        /// <summary>
        /// 連線
        /// </summary>
        /// <param name="ssid">SSID</param>
        /// <param name="profileXml">profile</param>
        private void Connect(WifiSSID ssid, string profileXml)
        {
            string profileName = ssid.SSID;
            ssid.wlanInterface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
            ssid.wlanInterface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName);
        }

        /// <summary>
        /// 從SSID取出名稱
        /// </summary>
        /// <param name="ssid">SSID</param>
        /// <returns>名稱</returns>
        private string GetStrForSSID(Wlan.Dot11Ssid ssid)
        {
            return Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }

        /// <summary>
        /// String to Hex
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string StringToHex(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.Default.GetBytes(str);
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString().ToUpper());
        }
    }
}
