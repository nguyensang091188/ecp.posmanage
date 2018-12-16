using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace ePOS3.Utils
{
    public class BuildMessage
    {
        public static string GetInfor(object[] objInfors, string errorMessage, string functionName)
        {
            try
            {
                string valueObjects = $"\r\n--------------Execute {functionName} at {DateTime.Now} -------------\r\n";
                if (objInfors != null)
                {
                    foreach (Object objInfor in objInfors)
                    {
                        if (objInfor != null)
                        {
                            Type myType = objInfor.GetType();
                            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
                            foreach (PropertyInfo prop in props)
                            {
                                object propValue = prop.GetValue(objInfor, null);
                                if (!string.IsNullOrEmpty(prop.Name))
                                {
                                    string jsonValue = JsonConvert.SerializeObject(propValue);
                                    valueObjects += $"{prop.Name}:{jsonValue},";
                                }

                            }
                        }

                    }
                }
                if (!string.IsNullOrEmpty(errorMessage))
                    valueObjects += $"\r\nError: {errorMessage}\r\n";
                valueObjects += $"\r\n-----------End Execute {functionName} at {DateTime.Now} -------------\r\n";
                return valueObjects;
            }
            catch (Exception ex)
            {
                try
                {
                    return $"{JsonConvert.SerializeObject(objInfors)}\r\n{errorMessage}\r\n{ex.Message}";
                }
                catch (Exception exx)
                {
                    return $"{errorMessage}\r\n{exx.Message}";
                }
            }
        }

        public static string GetInforEx(string[] objInfors, string errorMessage, string functionName)
        {
            string valueObjects = $"\r\n--------------Execute {functionName} at {DateTime.Now} -------------\r\n";
            try
            {
                foreach (string infor in objInfors)
                {
                    valueObjects += $"{infor}";
                }
                valueObjects += $"\r\n--------------End Execute {functionName} at {DateTime.Now} -------------\r\n";
                return valueObjects;
            }
            catch (Exception ex)
            {
                return $"{errorMessage}\r\n{ex.Message}";
            }

        }
    }
}