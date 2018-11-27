using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.SpatialAnalyst;
using ESRI.ArcGIS.SpatialAnalystUI;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.DataSourcesRasterUI;
using ESRI.ArcGIS.GeoAnalyst;

namespace GIS0728
{
    public partial class mapAlgebra : Form
    {
        private Form1 mainForm;
        public mapAlgebra(Form1 fff)
        {
            InitializeComponent();
            mainForm = fff;
        }

        private void mapAlgebra_Load(object sender, EventArgs e)
        {
            textBox2.Text = System.Environment.CurrentDirectory + "\\mapA01.tif";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            DialogResult dialogResult = openFileDialog1.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            DialogResult dialogRuslt = saveFileDialog.ShowDialog();
            if (dialogRuslt == DialogResult.OK)
            {
                textBox2.Text = saveFileDialog.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string inputFullFilePath = textBox1.Text;
            string outputFullFilePath = textBox2.Text;
            // deal with the outputFullFilePath
            int _index = outputFullFilePath.LastIndexOf("\\");
            string outputRasterName = (outputFullFilePath.Substring(_index + 1));//name
            string outputRasterDirectory = outputFullFilePath.Remove(_index);//directory

            //deal with input
            IRasterLayer pRasterLayer = new RasterLayerClass();
            pRasterLayer.CreateFromFilePath(inputFullFilePath);
            IRaster raster = pRasterLayer.Raster;

            IMapAlgebraOp mapAlgebraOp;
            mapAlgebraOp = new RasterMapAlgebraOpClass();
            //控制raster分析的环境
            IRasterAnalysisEnvironment rasterAnalysisEnvironment = default(IRasterAnalysisEnvironment);
            rasterAnalysisEnvironment = (IRasterAnalysisEnvironment)mapAlgebraOp;

            IWorkspaceFactory workspaceFactory = new RasterWorkspaceFactoryClass();
            IWorkspace workspace = workspaceFactory.OpenFromFile(outputRasterDirectory, 0);//这里应该是输出raster的路径
            rasterAnalysisEnvironment.OutWorkspace = workspace;

            try
            {
                mapAlgebraOp.BindRaster((IGeoDataset)raster, "this");
                //定义表达式（elevationMode为要减去的数值）不要忘了"[ ]"
                //"CON(ISNULL([this]),0,[this])"
                //"~(([this]>=92)&([this]<=272))&([this]>=2)"
                string strOut = textBox3.Text;
                IRaster outRaster = (IRaster)mapAlgebraOp.Execute(strOut);
                ISaveAs2 saveAs;
                saveAs = (ISaveAs2)outRaster;
                saveAs.SaveAs(outputRasterName, workspace, "TIFF");//输出名称(注意：名称中加后缀名，例：test.tif)，工作空间，格式
                MessageBox.Show("栅格计算器成功！");

                IRasterLayer pRlayer = new RasterLayer();
                pRlayer.CreateFromRaster(outRaster);
                pRlayer.Name = System.IO.Path.GetFileName(outputFullFilePath);
                mainForm.axMapControl1.AddLayer(pRlayer, 0);
            }
            catch (Exception ex)
            {

            }

        }


    }
}
