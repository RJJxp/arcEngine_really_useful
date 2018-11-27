///数据库导入功能
//张艾琳1551174
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data.Odbc;

namespace GIS0728
{
    public partial class mdbReadForm : Form
    {
        public mdbReadForm()
        {
            InitializeComponent();
        }
        private OleDbCommand cmd;
        OleDbConnection conn2;
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "打开mdb";
            openFileDialog1.InitialDirectory = Application.StartupPath + "\\Project_Data";
            openFileDialog1.Filter = "DB Documents(*.mdb)|*.mdb";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (dataGridView1.DataSource != null)
                {
                    DataTable dt = (DataTable)dataGridView1.DataSource;
                    dt.Rows.Clear();
                    dataGridView1.DataSource = dt;
                }
                else
                {
                    dataGridView1.Rows.Clear();
                }
                string dir = System.IO.Path.GetDirectoryName(openFileDialog1.FileName);
                textBox1.Text = openFileDialog1.FileName;
                string connStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + openFileDialog1.FileName;
                conn2 = new OleDbConnection(connStr);
                cmd = conn2.CreateCommand();
                conn2.Open();
                DataTable datatable = conn2.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                string sName = "";
                comboBox1.Items.Clear();
                for (int i = 0, maxI = datatable.Rows.Count; i < maxI; i++)
                {
                    // 获取第i个Access数据库中的表名
                    string sTempTableName = datatable.Rows[i]["TABLE_NAME"].ToString();
                    comboBox1.Items.Add(sTempTableName);
                }
                conn2.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string tableName = comboBox1.SelectedItem.ToString();
            cmd.CommandText = "select * from "+tableName;

                conn2.Open();
                OleDbDataReader dr = cmd.ExecuteReader();

                System.Data.DataTable dt = new System.Data.DataTable();
                if (dr.HasRows)
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        dt.Columns.Add(dr.GetName(i));
                    }
                    dt.Rows.Clear();
                }
                while (dr.Read())
                {
                    DataRow row = dt.NewRow();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        row[i] = dr[i];
                    }
                    dt.Rows.Add(row);
                }
                cmd.Dispose();
                conn2.Close();

                dataGridView1.DataSource = dt;


        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource != null)
            {
                DataTable dt = (DataTable)dataGridView1.DataSource;
                dt.Rows.Clear();
                dataGridView1.DataSource = dt;
            }
            else
            {
                dataGridView1.Rows.Clear();
            }
            openFileDialog1.Title = "打开dbf";
            openFileDialog1.InitialDirectory = "D:";
            openFileDialog1.Filter = "DB Documents(*.dbf)|*.dbf";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string dir = System.IO.Path.GetDirectoryName(openFileDialog1.FileName);
                string connStr = "Driver={Microsoft dBASE-Treiber (*.dbf)};SourceType=DBF;SourceDB=" + dir + ";Exclusive=No;";//空格都要有
                OdbcConnection conn2 = new OdbcConnection(connStr);
                conn2.Open();
                string sqlstr = "select * from " + openFileDialog1.FileName;
                OdbcDataAdapter daTJ = new OdbcDataAdapter(sqlstr, conn2);
                System.Data.DataTable daTB = new System.Data.DataTable();
                daTJ.Fill(daTB);
                dataGridView1.DataSource = daTB;

            }
        }
    }
}
