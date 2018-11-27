using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SpatialAnalyst;

namespace GIS0728
{
    public partial class rasterCal : Form
    {
        private Form1 mainForm;
        public rasterCal(Form1 fff)
        {
            InitializeComponent();
            mainForm = fff;
        }

        private IGeoDataset inGeoDataSet1;//输入数据1
        private IGeoDataset inGeoDataSet2;//输入数据2
        private IGeoDataset result;//计算结果
        private IMathOp mathOp;//数学计算对象
        private ILogicalOp logicalOp;//逻辑运算对象;
        private ITrigOp triOp;//三角函数运算对象

        public ILayer GetLayerByName(string name)
        {
            IEnumLayer Temp_AllLayer = mainForm.axMapControl1.Map.Layers;
            ILayer Each_Layer = Temp_AllLayer.Next();
            while (Each_Layer != null)
            {
                if (Each_Layer.Name.Contains(name))
                    return Each_Layer;
                Each_Layer = Temp_AllLayer.Next();
            }
            return null;
        }

        private void ShowAndSave(IGeoDataset geoDataset)
        {
            IRasterLayer rasterLayer = new RasterLayerClass();
            IRaster raster = new Raster();
            raster = (IRaster)geoDataset;
            rasterLayer.CreateFromRaster(raster);
            try
            {
                rasterLayer.Name = System.IO.Path.GetFileName(textBox1.Text);
                mainForm.axMapControl1.AddLayer((ILayer)rasterLayer, 0);
                mainForm.axMapControl1.ActiveView.Refresh();
                IWorkspaceFactory pWKSF = new RasterWorkspaceFactoryClass();
                IWorkspace pWorkspace = pWKSF.OpenFromFile(System.IO.Path.GetDirectoryName(textBox1.Text), 0);
                ISaveAs pSaveAs = raster as ISaveAs;
                pSaveAs.SaveAs(System.IO.Path.GetFileName(textBox1.Text), pWorkspace, "TIFF");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void rasterCal_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < mainForm.axMapControl1.LayerCount; i++)
            {
                ILayer pLyr = mainForm.axMapControl1.get_Layer(i);
                if (pLyr != null)
                {
                    if (pLyr is IRasterLayer)
                    {
                        comboBox1.Items.Add(pLyr.Name);
                        comboBox2.Items.Add(pLyr.Name);
                    }
                }
            }
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;

            mathOp = new RasterMathOpsClass();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ILayer layer = GetLayerByName(comboBox1.SelectedItem.ToString());
            IRasterLayer rasterLayer = layer as IRasterLayer;
            IRaster raster = rasterLayer.Raster;
            inGeoDataSet1 = raster as IGeoDataset;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ILayer layer = GetLayerByName(comboBox2.SelectedItem.ToString());
            IRasterLayer rasterLayer = layer as IRasterLayer;
            IRaster raster = rasterLayer.Raster;
            inGeoDataSet2 = raster as IGeoDataset;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            DialogResult dialogRuslt = saveFileDialog.ShowDialog();
            if (dialogRuslt == DialogResult.OK)
            {
                textBox1.Text = saveFileDialog.FileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            result = mathOp.Times(inGeoDataSet1,inGeoDataSet2);
            ShowAndSave(result);
        }


    }
}
