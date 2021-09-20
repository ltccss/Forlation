using System;
using XLua;
public static class MathUtil
{

    public static T Min<T>(T t1, T t2) where T : IComparable
    {
        return t1.CompareTo(t2) < 0 ? t1 : t2;
    }
    public static T Max<T>(T t1, T t2) where T : IComparable
    {
        return t1.CompareTo(t2) > 0 ? t1 : t2;
    }
    public static T Clamp<T>(T t1, T t2, T t3) where T : IComparable
    {
        T min = Min(t2, t3);
        T max = Max(t2, t3);
        return Min(max, Max(t1, min));
    }
    //将一个数字映射到一个范围内
    [LuaCallCSharp]
    public static int Round(int a, int min, int max)
    {
        if (min == max)
        {
            return min;
        }
        if (min > max)
        {
            int temp = max;
            max = min;
            min = temp;
        }

        int diff = max - min + 1;
        //diff 一定是正数
        int result = a;
        while (result < min || result > max)
        {
            if (result < min)
            {
                int mindiff = min - result;
                //mindiff 一定是正数
                if (mindiff > diff)
                {
                    //比一个diff的差距都大
                    result = result + (mindiff / diff) * diff;
                }
                else
                {
                    result = result + diff;
                }

            }
            else
            {
                int maxdiff = result - max;
                if (maxdiff > diff)
                {
                    //比一个diff的差距都大
                    result = result - (maxdiff / diff) * diff;
                }
                else
                {
                    result = result - diff;
                }
            }
        }
        return result;
    }
    [LuaCallCSharp]
    public static int BitCount(int i)
    {
        int ret = 0;
        for (; i != 0; i &= (i - 1))
        {
            ret++;
        }
        return ret;
    }
    //获取传入数字的位数
    public static int Digits_IfChain(long n)
    {
        if (n >= 0)
        {
            if (n < 10L) return 1;
            if (n < 100L) return 2;
            if (n < 1000L) return 3;
            if (n < 10000L) return 4;
            if (n < 100000L) return 5;
            if (n < 1000000L) return 6;
            if (n < 10000000L) return 7;
            if (n < 100000000L) return 8;
            if (n < 1000000000L) return 9;
            if (n < 10000000000L) return 10;
            if (n < 100000000000L) return 11;
            if (n < 1000000000000L) return 12;
            if (n < 10000000000000L) return 13;
            if (n < 100000000000000L) return 14;
            if (n < 1000000000000000L) return 15;
            if (n < 10000000000000000L) return 16;
            if (n < 100000000000000000L) return 17;
            if (n < 1000000000000000000L) return 18;
            return 19;
        }
        else
        {
            if (n > -10L) return 2;
            if (n > -100L) return 3;
            if (n > -1000L) return 4;
            if (n > -10000L) return 5;
            if (n > -100000L) return 6;
            if (n > -1000000L) return 7;
            if (n > -10000000L) return 8;
            if (n > -100000000L) return 9;
            if (n > -1000000000L) return 10;
            if (n > -10000000000L) return 11;
            if (n > -100000000000L) return 12;
            if (n > -1000000000000L) return 13;
            if (n > -10000000000000L) return 14;
            if (n > -100000000000000L) return 15;
            if (n > -1000000000000000L) return 16;
            if (n > -10000000000000000L) return 17;
            if (n > -100000000000000000L) return 18;
            if (n > -1000000000000000000L) return 19;
            return 20;
        }
    }

	public static bool IsVectorCross(UnityEngine.Vector2 src, UnityEngine.Vector2 tar)
    {
        float x1 = Min(src.x, src.y);
        float y1 = Max(src.x, src.y);
        float x2 = Min(tar.x, tar.y);
        float y2 = Max(tar.x, tar.y);
        return (x1 >= x2 && x1 <= y2) || (x1 < x2 && x2 <= y1);
    }
}