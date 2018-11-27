using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS0728
{
    class MyEventArgs : EventArgs
    {
        //传递主窗体的数据信息
        //响应主窗体鼠标位置数据传入最佳路径窗体
        public string[] Text
        {
            get;
            set;
        }
    }
}
