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
    public partial class HillshadeForm : Form
    {
        private Form1 mainForm;
        private double verticalAngle;
        private double azimuthAngle;

        public HillshadeForm(Form1 fff)
        {
            InitializeComponent();
            mainForm = fff;
        }

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


        private void HillshadeForm_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < mainForm.axMapControl1.LayerCount; i++)
            {
                ILayer pLyr = mainForm.axMapControl1.get_Layer(i);
                if (pLyr != null)
                {
                    if (pLyr is IRasterLayer)
                    {
                        comboBox1.Items.Add(pLyr.Name);
                    }
                }
            }
            comboBox1.SelectedIndex = -1;
            textBox1.Text = System.Environment.CurrentDirectory + "\\hillShade.tif";
            verticalBox.Text = "45";
            azimuthBox.Text = "315";
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
            IGeoDataset inGeoDataSet;
            IGeoDataset outGeoDataSet;
            ISurfaceOp SurfaceOp = new RasterSurfaceOpClass();

            ILayer layer = GetLayerByName(comboBox1.SelectedItem.ToString());
            IRasterLayer rasterLayer = layer as IRasterLayer;
            IRaster raster = rasterLayer.Raster;
            inGeoDataSet = raster as IGeoDataset;

            azimuthAngle = System.Convert.ToDouble(azimuthBox.Text);
            verticalAngle = System.Convert.ToDouble(verticalBox.Text);

            outGeoDataSet = SurfaceOp.HillShade(inGeoDataSet, azimuthAngle, verticalAngle, true);

            try
            {
                IWorkspaceFactory pWKSF = new RasterWorkspaceFactoryClass();
                IWorkspace pWorkspace = pWKSF.OpenFromFile(System.IO.Path.GetDirectoryName(textBox1.Text), 0);
                ISaveAs pSaveAs = outGeoDataSet as ISaveAs;
                pSaveAs.SaveAs(System.IO.Path.GetFileName(textBox1.Text), pWorkspace, "TIFF");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            IRasterLayer pRlayer = new RasterLayer();
            pRlayer.CreateFromRaster((IRaster)outGeoDataSet);
            pRlayer.Name = System.IO.Path.GetFileName(textBox1.Text);
            mainForm.axMapControl1.AddLayer(pRlayer, 0);
        }
    }
}
