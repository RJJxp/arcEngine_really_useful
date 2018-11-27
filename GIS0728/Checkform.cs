using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GIS0728
{
    public partial class Checkform : Form
    {
        private Form1 mainForm;
        public Checkform(Form1 fff)
        {
            InitializeComponent();
            mainForm = fff;
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
           
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked==true && checkBox2.Checked==false )
            {mainForm.openmode = 1;}
            else if (checkBox1.Checked == false && checkBox2.Checked == true)
            { mainForm.openmode = 2; }
            else if(checkBox1.Checked==true&& checkBox2.Checked==true)
            { mainForm.openmode = 3; }
            else
            { mainForm.openmode = -1; }
            this.Close();
        }

        private void Checkform_FormClosing(object sender, FormClosingEventArgs e)
        {
            mainForm.openmode = -1;
        }

        private void Checkform_Load(object sender, EventArgs e)
        {

        }
    }
}
