using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;


// 这里存放跨平台交互的逻辑
public class Platform
{
    private static Platform _me;
    public static Platform Me
    {
        get
        {
            if (_me == null)
            {
                _me = new Platform();
            }
            return _me;
        }
    }

}
