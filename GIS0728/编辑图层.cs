using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

/*----------------------------------------------------------------
// 住宅小区综合评价系统
// 文件名称：编辑图层.cs
// 功能描述：用于主窗体编辑图层开关的交互窗体（为简化程序不必要功能，未使用）
// 完成人：刘嘉澍1551160
// 作业日期：2018.08.23-24
//----------------------------------------------------------------*/

namespace GIS0728
{
    public partial class 编辑图层 : Form
    {
        public 编辑图层()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button1.Enabled = true;
            button4.Enabled = false;
            button3.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            button3.Enabled = false;
            button2.Enabled = false;
            button1.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form1.flagCreateFeature = true;
            Form1.flagDeleteFeature = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form1.flagDeleteFeature = true;
            Form1.flagCreateFeature = false;
        }

        private void 编辑图层_Load(object sender, EventArgs e)
        {

        }
    }
}
