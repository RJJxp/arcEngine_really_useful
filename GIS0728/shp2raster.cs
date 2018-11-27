using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataSourcesRaster;

namespace GIS0728
{
    public partial class shp2raster : Form
    {
        private Form1 mainForm;
        public shp2raster(Form1 fff)
        {
            InitializeComponent();
            mainForm = fff;
        }

        private void shp2raster_Load(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < mainForm.axMapControl1.LayerCount; i++)
                {
                    ILayer pLyr = mainForm.axMapControl1.get_Layer(i);
                    if (pLyr != null)
                    {
                        if (pLyr is IFeatureLayer)
                        {
                            comboBox1.Items.Add(pLyr.Name);
                        }
                    }
                }
                if (comboBox1.Items.Count > 0) comboBox1.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                comboBox2.Items.Clear();
                ILayer selectedLayer = mainForm.axMapControl1.get_Layer(comboBox1.SelectedIndex);
                IFeatureLayer selectedFeatureLayer = selectedLayer as IFeatureLayer;
                IFeatureClass selectedFeatureClass = selectedFeatureLayer.FeatureClass;
                for (int i = 0; i < selectedFeatureClass.GetFeature(0).Fields.FieldCount; i++)
                {
                    comboBox2.Items.Add(selectedFeatureClass.GetFeature(0).Fields.get_Field(i).Name);
                }
                if (comboBox2.Items.Count > 0) comboBox2.SelectedIndex = 0;
            }
            catch (Exception error)
            {
                Console.Write(error);
            }
        }

        string myFilePath;

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                myFilePath = folderBrowserDialog1.SelectedPath;
                textBox1.Text = myFilePath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string myListBoxContent;
            double myCellSize;
            myCellSize = System.Convert.ToDouble(textBox4.Text);
            //try
            {
                myListBoxContent = "正在转换所选ShpFile到栅格数据...";
                textBox3.Text += myListBoxContent;
                ILayer selectedLayer = mainForm.axMapControl1.get_Layer(comboBox1.SelectedIndex);
                IFeatureLayer selectedFeatureLayer = selectedLayer as IFeatureLayer;
                IFeatureClass selectedFeatureClass = selectedFeatureLayer.FeatureClass;
                Geoprocessor pGeoprocessor = new Geoprocessor();
                pGeoprocessor.OverwriteOutput = true;

                if (selectedFeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
                {
                    ESRI.ArcGIS.ConversionTools.PointToRaster pPointToRaster =
                    new ESRI.ArcGIS.ConversionTools.PointToRaster();
                    pGeoprocessor.SetEnvironmentValue("workspace", myFilePath);
                    pPointToRaster.cellsize = myCellSize;
                    pPointToRaster.in_features = selectedFeatureLayer;
                    pPointToRaster.value_field = selectedFeatureClass.Fields.get_Field(comboBox2.SelectedIndex);
                    pPointToRaster.out_rasterdataset = textBox2.Text + ".tif";
                    pGeoprocessor.Execute(pPointToRaster, null);
                    myListBoxContent = "转换成功！";
                    textBox3.Text += Environment.NewLine + Environment.NewLine + myListBoxContent;
                }
                else if (selectedFeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                {
                    ESRI.ArcGIS.ConversionTools.PolylineToRaster pPolylineToRaster =
                    new ESRI.ArcGIS.ConversionTools.PolylineToRaster();
                    pGeoprocessor.SetEnvironmentValue("workspace", myFilePath);
                    pPolylineToRaster.cellsize = myCellSize;
                    pPolylineToRaster.in_features = selectedFeatureLayer;
                    pPolylineToRaster.value_field = selectedFeatureClass.Fields.get_Field(comboBox2.SelectedIndex);
                    pPolylineToRaster.out_rasterdataset = textBox2.Text + ".tif";
                    pGeoprocessor.Execute(pPolylineToRaster, null);
                    myListBoxContent = "转换成功！";
                    textBox3.Text += Environment.NewLine + Environment.NewLine + myListBoxContent;
                }
                else if (selectedFeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                {
                    ESRI.ArcGIS.ConversionTools.PolygonToRaster pPolygonToRaster =
                    new ESRI.ArcGIS.ConversionTools.PolygonToRaster();
                    pGeoprocessor.SetEnvironmentValue("workspace", myFilePath);
                    pPolygonToRaster.cellsize = myCellSize;
                    pPolygonToRaster.in_features = selectedFeatureLayer;
                    pPolygonToRaster.value_field = selectedFeatureClass.Fields.get_Field(comboBox2.SelectedIndex);
                    pPolygonToRaster.out_rasterdataset = textBox2.Text + ".tif";
                    pGeoprocessor.Execute(pPolygonToRaster, null);
                    myListBoxContent = "转换成功！";
                    textBox3.Text += Environment.NewLine + Environment.NewLine + myListBoxContent;
                }
                else { MessageBox.Show("请选择正确的ShapeFile文件"); }

                if (MessageBox.Show("是否将生成的新Raster文件添加到图层", "Confirm Message",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    IRasterLayer myRasterLayer = new RasterLayerClass();
                    try
                    {
                        myRasterLayer.CreateFromFilePath(myFilePath + "\\" + textBox2.Text + ".tif");
                        mainForm.axMapControl1.AddLayer(myRasterLayer, 0);
                    }
                    catch
                    {
                        MessageBox.Show("打开文件错误！");
                    }
                }
            }
        }





    }
}
