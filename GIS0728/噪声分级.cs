using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using System.Data.OleDb;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.SpatialAnalyst;
using ESRI.ArcGIS.SpatialAnalystTools;
using ESRI.ArcGIS.SpatialAnalystUI;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.DataSourcesRasterUI;
using ESRI.ArcGIS.GeoAnalyst;



/*----------------------------------------------------------------
// 住宅小区综合评价系统
// 文件名称：噪声分级.cs
// 功能描述：对密度计算后的噪声分部栅格数据进行多值分类的交互窗体
// 完成人：刘嘉澍1551160
// 作业日期：2018.08.26
//----------------------------------------------------------------*/

namespace GIS0728
{
    public partial class 噪声分级 : Form
    {
        private Form1 mainForm;
        public 噪声分级(Form1 fff)
        {
            InitializeComponent();
            mainForm = fff;
        }
        DataTable datatable1;
        IRasterStatistics rasterStatic;
        private void 噪声分级_Load(object sender, EventArgs e)
        {
            //向图层comboBox1中预置噪音图层
            for (int i = 0; i < global.p2DMap.LayerCount; i++)
            {
                ILayer pLayer = global.p2DMap.get_Layer(i);
                if (pLayer != null)
                {
                    if (pLayer is IRasterLayer)
                    {
                        comboBox1.Items.Add(pLayer.Name);
                    }
                }
            }
            comboBox1.SelectedIndex = 0;

            //向分级数comBobox2中预置分级数，默认为5
            for (int i = 1; i < 7; i++)
            {
                comboBox2.Items.Add(i);
            }
         

            //栅格像元统计
            IRasterBandCollection bandCollection;
            ILayer mlayer = GetLayerByName(comboBox1.SelectedItem.ToString());
            IRasterLayer rasterLayer = mlayer as IRasterLayer;
            IRaster2 raster = rasterLayer.Raster as IRaster2;
            IRasterDataset rd = raster.RasterDataset;
            bandCollection = rd as IRasterBandCollection;
              IEnumRasterBand enumband=bandCollection.Bands;
            IRasterBand rasterBand=enumband.Next();
             rasterStatic=null;
            if (rasterBand != null && rasterBand.Statistics != null)
            {
                rasterStatic  = rasterBand.Statistics;
            }
                   
             datatable1 = new DataTable();
           
            DataColumn datacolumn1 = new DataColumn("区间", System.Type.GetType("System.String"));
            DataColumn datacolumn2 = new DataColumn("值", System.Type.GetType("System.Int32"));
            datatable1.Columns.Add(datacolumn1);
            datatable1.Columns.Add(datacolumn2);
            comboBox2.SelectedIndex = 4;
            //for (int i = 0; i < 5; i++)
            //{
            //    DataRow datarow1 = datatable1.NewRow();
            //    datarow1[0] = ((rasterStatic.Maximum - rasterStatic.Minimum) * i / 5).ToString()+"-"+ ((rasterStatic.Maximum - rasterStatic.Minimum) * (i + 1) / 5).ToString();
            //    datarow1[1] = i + 1;
            //    datatable1.Rows.Add(datarow1);

            //}

            dataGridView1.DataSource = datatable1 ;
            textBox1.Text = System.Environment.CurrentDirectory + "noiseclassification.tif";         
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            DialogResult dialogRuslt = saveFileDialog.ShowDialog();
            if (dialogRuslt == DialogResult.OK)
            {
                textBox1.Text = saveFileDialog.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IReclassOp reCla;
            reCla = new RasterReclassOp() as IReclassOp;
            //符号化
            IRemap pRemap;
            INumberRemap pSRemap;
            pSRemap = new NumberRemap() as INumberRemap;
            for (int i = 1; i <= Convert.ToInt32(comboBox2.SelectedItem.ToString()); i++)
            {
                try
                {
                    string str;
                    str = dataGridView1.Rows[i - 1].Cells[0].Value.ToString();
                    float fValue, tValue;
                    int p;
                    p = str.LastIndexOf("-");
                   
                    fValue = Convert.ToSingle(str.Substring(0, p));
                    tValue = Convert.ToSingle(str.Substring(p + 1, str.Length - p - 1));
                    pSRemap.MapRange(fValue, tValue, Convert.ToInt32(dataGridView1.Rows[i - 1].Cells[1].Value));
                }
                catch
                {
                    MessageBox.Show("Error!");
                    return;
                }
            }
            pRemap = (IRemap)pSRemap;

            //获取栅格图层
            IRasterLayer play = (IRasterLayer)GetLayerByName(comboBox1.SelectedItem.ToString());
            IRaster pRster = play.Raster;
            IGeoDataset pOutputRaster = null;
            try
            {
                pOutputRaster = reCla.ReclassByRemap((IGeoDataset)pRster, pRemap, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            try
            {
                IWorkspaceFactory pWKSF = new RasterWorkspaceFactoryClass();
                IWorkspace pWorkspace = pWKSF.OpenFromFile(System.IO.Path.GetDirectoryName(textBox1.Text), 0);
                ISaveAs pSaveAs = pOutputRaster as ISaveAs;
                pSaveAs.SaveAs(System.IO.Path.GetFileName(textBox1.Text), pWorkspace, "TIFF");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            IRasterLayer pRlayer = new RasterLayer();
            pRlayer.CreateFromRaster((IRaster)pOutputRaster);
            pRlayer.Name = System.IO.Path.GetFileName(textBox1.Text);
            global.p2DMap.AddLayer(pRlayer);
            global.p3DMap.Scene .AddLayer(pRlayer);
            mainForm.axTOCControl1.Update();
            mainForm.axTOCControl2.Update();

           
        }
        //
        public static ILayer GetLayerByName(string name)
        {
            IEnumLayer Temp_AllLayer = global.p2DMap.Layers;
            ILayer Each_Layer = Temp_AllLayer.Next();
            while (Each_Layer != null)
            {
                if (Each_Layer.Name.Contains(name))
                    return Each_Layer;
                Each_Layer = Temp_AllLayer.Next();
            }
            return null;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            datatable1.Clear();
            for (int i = 0; i <Convert.ToInt32(comboBox2.Text); i++)
            {
                DataRow datarow1 = datatable1.NewRow();
                datarow1[0] = ((rasterStatic.Maximum - rasterStatic.Minimum) * i / Convert.ToInt32(comboBox2.Text)).ToString() + "-" + ((rasterStatic.Maximum - rasterStatic.Minimum) * (i + 1) / Convert.ToInt32(comboBox2.Text)).ToString();
                datarow1[1] = i + 1;
                datatable1.Rows.Add(datarow1);

            }
        }
    }
}
