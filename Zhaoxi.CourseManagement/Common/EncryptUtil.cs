using System.Text;
using System.Security.Cryptography;

namespace DataMonitoringSystem.Common
{
    public static class EncryptUtil
    {
        //加密算法HmacSHA256
        public static string HmacSHA256(string secret, string signKey)
        {
            string signRet = string.Empty;
            using (HMACSHA256 mac = new HMACSHA256(Encoding.UTF8.GetBytes(signKey)))
            {
                byte[] hash = mac.ComputeHash(Encoding.UTF8.GetBytes(secret));
                //signRet = Convert.ToBase64String(hash);
                signRet = ToHexString(hash);
            }
            return signRet;
        }

        //byte[]转16进制格式string
        private static string ToHexString(byte[] bytes)
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();
                foreach (byte b in bytes)
                {
                    strB.AppendFormat("{0:x2}", b);
                }
                hexString = strB.ToString();
            }
            return hexString;
        }
    }
}
