using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCommon.Helper
{
    public class ConvertUtils
    {
        public static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
