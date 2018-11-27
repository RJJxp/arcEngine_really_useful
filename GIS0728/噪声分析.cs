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
// 文件名称：噪声分析.cs
// 功能描述：用于对道路噪声的密度分析功能的交互窗体
// 完成人：刘嘉澍1551160
// 作业日期：2018.08.27
//----------------------------------------------------------------*/

namespace GIS0728
{
    public partial class 噪声分析 : Form
    {
        Form1 mainform;
        public 噪声分析(Form1 fff)
        {
            InitializeComponent();
            mainform=fff;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 噪声分析_Load(object sender, EventArgs e)
        {
            //向图层comboBox1中预置噪音来源
            for (int i = 0; i < global.p2DMap.LayerCount; i++)
            {
                ILayer pLayer = global.p2DMap.get_Layer(i);
                if (pLayer != null)
                {
                    if (pLayer is IFeatureLayer)
                    {
                        comboBox1.Items.Add(pLayer.Name);
                    }
                }
            }
            comboBox1.SelectedIndex = 0;

            textBox2.Text = "50";
            textBox3.Text = "2";

            textBox1.Text = System.Environment.CurrentDirectory + "noise.tif";        
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();

            ILayer pLayer = global.p2DMap.get_Layer(comboBox1.SelectedIndex);
            IFeatureLayer pFLayer = pLayer as IFeatureLayer;
            IFeatureClass pFClass = pFLayer.FeatureClass;
            for (int i = 0; i < pFClass.Fields.FieldCount; i++)
            {
                comboBox2.Items.Add(pFClass.Fields.get_Field(i).Name);

            }
            comboBox2.SelectedIndex = 0;
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
            try 
            {
                ILayer selectedLayer = global.p2DMap.get_Layer(comboBox1.SelectedIndex);
                IFeatureLayer selectedFeatureLayer = selectedLayer as IFeatureLayer;
                IFeatureClass selectedFeatureClass = selectedFeatureLayer.FeatureClass;

                IRasterAnalysisEnvironment rasterEnv = new RasterDensityOp();

                 double r = Convert.ToDouble(textBox2.Text);
                object dSearchD = r;  //搜索半径

                //设置输出栅格大小
                double cellSize = Convert.ToDouble(textBox3.Text);
               // object cellSizeObj = cellSize;
               // rasterEnv.SetCellSize(esriRasterEnvSettingEnum.esriRasterEnvValue, ref cellSizeObj);

                  //获得图层
            ILayer layer = null;
            IMap map = global.p2DMap;
            for(int i=0;i<map.LayerCount;i++)
            {
                ILayer temp = map.Layer[i];
                if (temp.Name == comboBox1.SelectedItem.ToString())
                    layer = temp;                   
            }
            IFeatureLayer fLayer = layer as IFeatureLayer;
            IFeatureClass fClass = fLayer.FeatureClass;

                  //设置空间处理范围
            object extentProObj = layer;
            rasterEnv.SetExtent(esriRasterEnvSettingEnum.esriRasterEnvValue, ref extentProObj);
            rasterEnv.SetCellSize(esriRasterEnvSettingEnum.esriRasterEnvValue, 1);
                // IRasterRadius pRadius = new RasterRadiusClass();??
                 // pRadius.SetVariable(pSearchCount, ref dSearchD);


                  //设置要素数据
            IFeatureClassDescriptor feaDes;
            feaDes = new FeatureClassDescriptorClass();
            feaDes.Create(fClass, null, comboBox2.SelectedItem.ToString());
            IGeoDataset inGeodataset;
            inGeodataset = feaDes as IGeoDataset;
               
                
                //设置输出栅格
                //IRaster outraster;
            IGeoDataset outGeoDataset;

                IDensityOp densityOp = rasterEnv as IDensityOp;
                outGeoDataset = densityOp.KernelDensity(inGeodataset, r);//., 1);

                 try
            {
                IWorkspaceFactory pWKSF = new RasterWorkspaceFactoryClass();
                IWorkspace pWorkspace = pWKSF.OpenFromFile(System.IO.Path.GetDirectoryName(textBox1.Text), 0);
                ISaveAs pSaveAs = outGeoDataset as ISaveAs;
                pSaveAs.SaveAs(System.IO.Path.GetFileName(textBox1.Text), pWorkspace, "TIFF");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            IRasterLayer pRlayer = new RasterLayer();
            pRlayer.CreateFromRaster((IRaster)outGeoDataset);
            pRlayer.Name = System.IO.Path.GetFileName(textBox1.Text);
            global.p2DMap.AddLayer(pRlayer);
            global.p3DMap.Scene.AddLayer(pRlayer);

            
            }

           catch { }
            this.Close();
        }
    }
}
