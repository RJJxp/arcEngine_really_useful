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
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.DataSourcesRasterUI;
using ESRI.ArcGIS.GeoAnalyst;


namespace GIS0728
{
    public partial class raster2shp : Form
    {
        private Form1 mainForm;
        public raster2shp(Form1 fff)
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

        private void raster2shp_Load(object sender, EventArgs e)
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
        }

        string myFolderPath;

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                myFolderPath = folderBrowserDialog1.SelectedPath;
                textBox1.Text = myFolderPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IGeoDataset inGeoDataSet;
            IGeoDataset outGeoDataSet;
            IConversionOp conversionOp = new RasterConversionOpClass();
            
            // 构造inGeoDataSet
            ILayer layer = GetLayerByName(comboBox1.SelectedItem.ToString());
            IRasterLayer rasterLayer = layer as IRasterLayer;
            IRaster raster = rasterLayer.Raster;
            inGeoDataSet = raster as IGeoDataset;

            IWorkspaceFactory2 pWorkspaceFactoryShp = new ShapefileWorkspaceFactoryClass();
            IWorkspace pWorkspace = pWorkspaceFactoryShp.OpenFromFile(textBox1.Text, 0);
            ISpatialReference pSpatialReference = inGeoDataSet.SpatialReference;

            outGeoDataSet = conversionOp.RasterDataToPolygonFeatureData(
                inGeoDataSet,
                pWorkspace,
                textBox2.Text, 
                true
                );

            IDataset pDataset1 = outGeoDataSet as IDataset;
            IFeatureClass pFeatureClass = pDataset1 as IFeatureClass;

            IFeatureLayer pFeatureLayer = new FeatureLayerClass();
            pFeatureLayer.FeatureClass = pFeatureClass;
            pFeatureLayer.Name = textBox2.Text;

            mainForm.axMapControl1.AddLayer(pFeatureLayer);
            mainForm.axMapControl1.Refresh();
        }



    }
}
