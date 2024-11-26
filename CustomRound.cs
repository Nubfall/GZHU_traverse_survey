using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    internal class CustomRound
    {
        public static double CustomRound_(double value, int decimalPlaces)
        {
            double factor = Math.Pow(10, decimalPlaces);//保留小数位数decimalPlaces
            double scaledValue = value * factor;//放大倍数
            long integerPart = (long)scaledValue;
            double fractionalPart = scaledValue - integerPart;//舍入部分

            if (fractionalPart > 0.5)
            {
                // 小数部分大于0.5，则直接进位
                integerPart++;
            }
            else if (fractionalPart < 0.5)
            {
                // 小数部分小于0.5，则直接舍去
                // integerPart 保持不变
            }
            else
            {
                // 小数部分等于0.5，进行奇数进偶数舍的判断
                if (integerPart % 2 != 0)
                {
                    // 奇数，则进位
                    integerPart++;
                }
                // 偶数，则直接舍去，integerPart保持不变
            }

            return integerPart / factor;//转换为原来的倍数
        }

    }
}
