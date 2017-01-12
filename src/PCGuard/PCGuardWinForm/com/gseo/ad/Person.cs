using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace com.gseo.ad
{
    /// <summary>
    /// 個人在AD內的資訊
    /// </summary>
    public class Person
    {
        private UserPrincipal _userPrincipal = null;

        public Person()
        {
            if (_userPrincipal == null)
            {
                PrincipalContext domain = new PrincipalContext(ContextType.Domain);
                _userPrincipal = UserPrincipal.FindByIdentity(domain, GetADAccount());
            }
        }

        /// <summary>
        /// 取得登入AD帳號
        /// </summary>
        /// <returns></returns>
        public string GetADAccount()
        {
            string name = new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent()).Identity.Name.ToString();
            //string name1 = System.Environment.UserName;
            //string name2 = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            return name;
        }

        /// <summary>
        /// 取得AD內的部門名稱
        /// </summary>
        /// <returns>部門名稱</returns>
        public string GetDepartment()
        {
            
            string deptName = GetProperty("department");
            return deptName;
        }

        /// <summary>
        /// 列出所有UserPrincipal屬性及其值
        /// </summary>
        public void ListAllPrincipalProperties()
        {
            /*
            Property:	objectClass 	top
            Property:	objectClass 	person
            Property:	objectClass 	organizationalPerson
            Property:	objectClass 	user
            Property:	cn 	T1863
            Property:	sn 	劉
            Property:	title 	工程師
            Property:	description 	資訊處系統發展部-劉政鋊
            Property:	userCertificate 	System.Byte[]
            Property:	givenName 	政鋊
            Property:	distinguishedName 	CN=T1863,OU=系統發展部,OU=資訊處,OU=台廠,DC=gseo,DC=com
            Property:	instanceType 	4
            Property:	whenCreated 	2014/9/15 上午 06:02:49
            Property:	whenChanged 	2017/1/3 上午 00:41:12
            Property:	displayName 	zk.liu-劉政鋊
            Property:	uSNCreated 	System.__ComObject
            Property:	memberOf 	CN=gstwpgp_user,OU=台廠,DC=gseo,DC=com
            Property:	memberOf 	CN=TW_WLAN_Users,DC=gseo,DC=com
            Property:	memberOf 	CN=台廠全體同仁-gseotwall,OU=台廠,DC=gseo,DC=com
            Property:	memberOf 	CN=XMIT资讯处只读,OU=资讯处,OU=共享盘权限,OU=XM厦门厂,DC=gseo,DC=com
            Property:	memberOf 	CN=T1200-資訊處,OU=台廠,DC=gseo,DC=com
            Property:	memberOf 	CN=T1210-資訊處系統發展部,OU=台廠,DC=gseo,DC=com
            Property:	uSNChanged 	System.__ComObject
            Property:	department 	資訊處系統發展部
            Property:	company 	玉晶光電股份有限公司
            Property:	mDBUseDefaults 	True
            Property:	name 	T1863
            Property:	objectGUID 	System.Byte[]
            Property:	userAccountControl 	512
            Property:	badPwdCount 	0
            Property:	codePage 	0
            Property:	countryCode 	0
            Property:	badPasswordTime 	System.__ComObject
            Property:	lastLogoff 	System.__ComObject
            Property:	lastLogon 	System.__ComObject
            Property:	scriptPath 	資訊處.bat
            Property:	pwdLastSet 	System.__ComObject
            Property:	primaryGroupID 	513
            Property:	objectSid 	System.Byte[]
            Property:	accountExpires 	System.__ComObject
            Property:	logonCount 	3031
            Property:	sAMAccountName 	t1863
            Property:	sAMAccountType 	805306368
            Property:	userPrincipalName 	t1863@gseo.com
            Property:	lockoutTime 	System.__ComObject
            Property:	objectCategory 	CN=Person,CN=Schema,CN=Configuration,DC=gseo,DC=com
            Property:	msNPAllowDialin 	True
            Property:	dSCorePropagationData 	1601/1/1 上午 00:00:00
            Property:	lastLogonTimestamp 	System.__ComObject
            Property:	mail 	zk.liu@gseo.com
            Property:	manager 	CN=T1715,OU=系統發展部,OU=資訊處,OU=台廠,DC=gseo,DC=com
            Property:	msExchHomeServerName 	/o=GSEO/ou=GSEO/cn=Configuration/cn=Servers/cn=AOS02
            Property:	msExchUserAccountControl 	0
            Property:	nTSecurityDescriptor 	System.__ComObject
             */
            DirectoryEntry directoryEntry = GetDirectoryEntry();

            foreach (string key in directoryEntry.Properties.PropertyNames)
            {
                foreach (object value in directoryEntry.Properties[key])
                {
                    Debug.WriteLine(String.Format("Property:\t{0} \t{1}", key, value));
                }
            }
        }

        /// <summary>
        /// 取得屬性值
        /// </summary>
        /// <param name="property">屬性名稱</param>
        /// <returns>屬性值</returns>
        private String GetProperty(String property)
        {
            DirectoryEntry directoryEntry = GetDirectoryEntry();

            if (directoryEntry.Properties.Contains(property))
                return directoryEntry.Properties[property].Value.ToString();
            else
                return String.Empty;
        }

        private DirectoryEntry GetDirectoryEntry()
        {
            DirectoryEntry directoryEntry = _userPrincipal.GetUnderlyingObject() as DirectoryEntry;
            return directoryEntry;
        }
    }
}
