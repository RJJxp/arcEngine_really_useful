/*----------------------------------------------------------------
// 住宅小区综合评价系统
// 文件名称：Form3.cs
// 功能描述：用于绿地率计算、地类分布统计的交互窗体
// 作者：梁雍（1451162）
// 作业日期：2018.08.24-30
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.DataSourcesRaster;
using System.Threading;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Geoprocessing;
using GIS0728;

namespace GIS0728
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 计算地类分布
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            IWorkspaceFactory pWSF = null;
            pWSF = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace pWS = pWSF.OpenFromFile(@"C:\Users\航\Desktop\同济新村shp数据", 0) as IFeatureWorkspace;

            IFeatureClass pFC1 = pWS.OpenFeatureClass("同济新村房屋.shp");
            IFeatureClass pFC2 = pWS.OpenFeatureClass("生成地块.shp");
            IFeatureClass pFC3 = pWS.OpenFeatureClass("绿化面数据.shp");

            IFeatureCursor pFeatureCur1 = pFC1.Search(null, false);
            IFeature pFeature1 = pFeatureCur1.NextFeature();
            IFeatureCursor pFeatureCur2 = pFC2.Search(null, false);
            IFeature pFeature2 = pFeatureCur2.NextFeature();
            IFeatureCursor pFeatureCur3 = pFC3.Search(null, false);
            IFeature pFeature3 = pFeatureCur3.NextFeature();

            double residenceArea = 0;
            double groundArea = 0;
            double greenLand = 0;
            double residenceRatio = 0;
            double greenRatio = 0;
            double otherRatio = 0;

            while (pFeature1 != null)
            {
                if (pFeature1.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                {
                    IArea pArea = pFeature1.Shape as IArea;
                    residenceArea = residenceArea + pArea.Area;
                }

                pFeature1 = pFeatureCur1.NextFeature();
            }

            while (pFeature2 != null)
            {
                if (pFeature2.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                {
                    IArea pArea = pFeature2.Shape as IArea;
                    groundArea = groundArea + pArea.Area;
                }

                pFeature2 = pFeatureCur2.NextFeature();
            }

            while (pFeature3 != null)
            {
                if (pFeature3.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                {
                    IArea pArea = pFeature3.Shape as IArea;
                    greenLand = greenLand + pArea.Area;
                }

                pFeature3 = pFeatureCur3.NextFeature();
            }

            residenceRatio = residenceArea / groundArea;
            greenRatio = greenLand / groundArea;
            otherRatio = 1 - residenceRatio - greenRatio;

            //清除默认的series
            chart1.Series.Clear();
            Series LandType = new Series("用地类型");

            //设置chart的类型
            LandType.ChartType = SeriesChartType.Pie;

            //给系列上的点进行赋值，分别对应横坐标和纵坐标的值
            LandType.Points.AddXY("住宅", residenceRatio);
            LandType.Points.AddXY("绿地", greenRatio);
            LandType.Points.AddXY("其它", otherRatio);

            chart1.Series.Add(LandType);

        }

        public static NameValueCollection myCol = new NameValueCollection();

        private void Form3_Load(object sender, EventArgs e)
        {
            //获取图层
            IEnumLayer layers = GetFeatureLayers();
            layers.Reset();
            ILayer layer = null;
            while ((layer = layers.Next()) != null)
            {
                comboBox1.Items.Add(layer.Name);
                comboBox2.Items.Add(layer.Name);

                //错误写法
                //IFeatureLayer pFeatureLayer = (IFeatureLayer)layer;
                //IFeatureClass fc = (IFeatureClass)pFeatureLayer.FeatureClass;
                //IFeatureDataset fds = (IFeatureDataset)fc.FeatureDataset;
                //IWorkspace ws = (IWorkspace)fds.Workspace;

                IDataLayer pDLayer = (IDataLayer)layer;
                IWorkspaceName ws = ((IDatasetName)(pDLayer.DataSourceName)).WorkspaceName;

                myCol.Add(layer.Name, ws.PathName + @"\" + layer.Name + ".shp");
                System.Console.WriteLine(ws.PathName + @"\" + layer.Name + ".shp");
            }

            label3.Text = "";
        }

        /// <summary>
        /// 由小区道路生成内部地块
        /// </summary>
        /// <param name="getpolygon_fc"></param>
        /// <param name="fromline_fl"></param>
        private void createpolygonfromline(IFeatureClass getpolygon_fc, IFeatureLayer fromline_fl)
        {
            //判断getpolygon_fc是否为面
            if (getpolygon_fc.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                MessageBox.Show("目标图层不是多边形图层！");
                System.Console.WriteLine("目标图层不是多边形图层！");
                return;
            }

            //得到选择的Feature的指针
            IFeatureSelection pFeatureSelection = fromline_fl as IFeatureSelection;
            ISelectionSet pSelectionSet = pFeatureSelection.SelectionSet;
            ICursor pCursor;
            pSelectionSet.Search(null, false, out pCursor);
            IFeatureCursor pFeatureCursor = pCursor as IFeatureCursor;
            IGeoDataset pGeoDataset = getpolygon_fc as IGeoDataset;
            IEnvelope pEnvelope = pGeoDataset.Extent;
            IInvalidArea pInvalidArea = new InvalidAreaClass();
            IFeatureConstruction pFeatureConstruction = new FeatureConstructionClass();
            IDataset pDataset = getpolygon_fc as IDataset;
            IWorkspace pWorkspace = pDataset.Workspace;
            IWorkspaceEdit pWorkspaceEdit = pWorkspace as IWorkspaceEdit;

            if (pWorkspaceEdit.IsBeingEdited() != true)
            {
                pWorkspaceEdit.StartEditing(true);
                pWorkspaceEdit.StartEditOperation();
            }
            //开始
            try
            {
                pFeatureConstruction.ConstructPolygonsFromFeaturesFromCursor(null, getpolygon_fc, pEnvelope, true, false, pFeatureCursor, pInvalidArea, -1, null);
                pWorkspaceEdit.StopEditOperation();
                pWorkspaceEdit.StopEditing(true);
            }
            catch
            {
                MessageBox.Show("构造多边形失败！");
                System.Console.WriteLine("构造多边形失败！");
                pWorkspaceEdit.AbortEditOperation();
            }

            //获取FeatureClass
            IWorkspace editedWorkspace = pWorkspaceEdit as IWorkspace;
            IEnumDataset FeatureEnumDataset = editedWorkspace.get_Datasets(esriDatasetType.esriDTFeatureClass);
            if (FeatureEnumDataset == null) return;
            FeatureEnumDataset.Reset();
            IDataset editedDataset = FeatureEnumDataset.Next();
            IFeatureClass pFC = editedDataset as IFeatureClass;

            //错误写法
            //IFeatureWorkspace pFeatureWorkspace = pWorkspaceEdit as IFeatureWorkspace;
            //IFeatureDataset pFeatureDataset = pFeatureWorkspace as IFeatureDataset;
            //IFeatureClass pFC = pFeatureDataset as IFeatureClass;

            //将生成面元素显示在图层中
            IFeatureLayer pFLayer = new FeatureLayerClass();
            pFLayer.FeatureClass = pFC;
            pFLayer.Name = pFC.AliasName;//保存为“生成地块.shp”
            ILayer pLayer = pFLayer as ILayer;
            Form1.mainForm.axMapControl1.Map.AddLayer(pLayer);
            Form1.mainForm.axMapControl1.ActiveView.Refresh();
        }

        /// <summary>
        /// 根据选择的要素判断其所属图层
        /// </summary>
        /// <param name="pMap"></param>
        /// <returns></returns>
        public static IFeatureLayer ReturnFeatureSelectedLayer(IMap pMap)
        {
            try
            {
                IFeatureLayer pFeatureLayer = null;
                for (int i = 0; i < pMap.LayerCount; i++)
                {
                    if (pMap.get_Layer(i) is FeatureLayer)
                    {
                        pFeatureLayer = pMap.get_Layer(i) as IFeatureLayer;
                        IFeatureSelection pFeatureSel = pFeatureLayer as IFeatureSelection;
                        ISelectionSet pSelSet = pFeatureSel.SelectionSet;
                        if (pSelSet.Count != 0)
                            break;
                    }
                    else
                    { MessageBox.Show("找不到选择的要素！"); return null; }
                }
                return pFeatureLayer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
         public string show_save_filedialog(string type)
        {
            string localFilePath = "";
            SaveFileDialog sfd = new SaveFileDialog();
            //设置文件类型 
            sfd.Filter = "文件（*." + type + " ）|*." + type;

            //设置默认文件类型显示顺序 
            sfd.FilterIndex = 1;

            //保存对话框是否记忆上次打开的目录 
            sfd.RestoreDirectory = true;

            //点了保存按钮进入 
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                localFilePath = sfd.FileName.ToString(); //获得文件路径 
                string fileNameExt = localFilePath.Substring(localFilePath.LastIndexOf("\\") + 1); //获取文件名，不带路径
            }

            return localFilePath;
        }

        /// <summary>
        /// 建立新多变形要素类
        /// </summary>
        /// <param name="file_path"></param>
        /// <returns></returns>
        public IFeatureClass createPolygonFeatureClass(string file_path)
        {
            string strShapeFolder = System.IO.Path.GetDirectoryName(file_path);//从文件名中获取路径
            string strShapeFieldName = System.IO.Path.GetFileName(file_path);//从文件名中获取文件名
            
            //打开工作空间  
            //const string strShapeFieldName = "shape";
            IWorkspaceFactory pWSF = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace pWS = (IFeatureWorkspace)pWSF.OpenFromFile(strShapeFolder, 0);

            //设置字段集  
            IFields pFields = new FieldsClass();
            IFieldsEdit pFieldsEdit = (IFieldsEdit)pFields;

            //设置字段  
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = (IFieldEdit)pField;

            //创建类型为几何类型的字段  
            pFieldEdit.Name_2 = strShapeFieldName;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;

            //为esriFieldTypeGeometry类型的字段创建几何定义，包括类型和空间参照   
            IGeometryDef pGeoDef = new GeometryDefClass();     //The geometry definition for the field if IsGeometry is TRUE.  
            IGeometryDefEdit pGeoDefEdit = (IGeometryDefEdit)pGeoDef;
            pGeoDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
            pGeoDefEdit.SpatialReference_2 = new UnknownCoordinateSystemClass();

            pFieldEdit.GeometryDef_2 = pGeoDef;
            pFieldsEdit.AddField(pField);

            //添加其他的字段  
            pField = new FieldClass();
            pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = "Area";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            pFieldEdit.Precision_2 = 7;//数值精度  
            pFieldEdit.Scale_2 = 3;//小数点位数  
            pFieldsEdit.AddField(pField);

            //创建要素类  
            IFeatureClass newfclass = pWS.CreateFeatureClass(strShapeFieldName, pFields, null, null, esriFeatureType.esriFTSimple, strShapeFieldName, "");

            return newfclass;
        }

        /// <summary>
        /// 根据道路生成地块
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (Form1.mainForm.axMapControl1.Map.FeatureSelection == null)
            {
                MessageBox.Show("未选择线要素！");
                return;
            }
            else
            {
                string file_path = show_save_filedialog("shp");
                if (file_path == "")
                {
                    return;
                }
                if (File.Exists(file_path))
                {
                    File.Delete(file_path);
                }
                if (File.Exists(file_path.Substring(0, file_path.Count() - 1) + "x"))
                {
                    File.Delete(file_path.Substring(0, file_path.Count() - 1) + "x");
                }
                if (File.Exists(file_path.Substring(0, file_path.Count() - 3) + "dbf"))
                {
                    File.Delete(file_path.Substring(0, file_path.Count() - 3) + "dbf");
                }

                IFeatureClass pFeatureClass = createPolygonFeatureClass(file_path);
                IFeatureLayer selectedFeatureLayer = ReturnFeatureSelectedLayer(Form1.mainForm.axMapControl1.Map);
                createpolygonfromline(pFeatureClass, selectedFeatureLayer);
            }
        }

        /// <summary>
        /// 遍历要素图层
        /// </summary>
        /// <returns></returns>
        public IEnumLayer GetFeatureLayers()
        {
            UID uid = new UIDClass();
            uid.Value = "{40A9E885-5533-11d0-98BE-00805F7CED21}";//FeatureLayer
            IEnumLayer layers = Form1.mainForm.axMapControl1.Map.get_Layers(uid, true);
            return layers;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 计算绿地率
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            string file_path1 = myCol[comboBox1.Text];
            string file_path2 = myCol[comboBox2.Text];
            string strShapeFolder = System.IO.Path.GetDirectoryName(file_path1);
            string filename1 = System.IO.Path.GetFileName(file_path1);
            string filename2 = System.IO.Path.GetFileName(file_path2);
            
            IWorkspaceFactory pWSF = null;
            double groundArea = 0;
            double greenLand = 0;
            double greenRatio = 0;

            try
            {
                pWSF = new ShapefileWorkspaceFactoryClass();
                IFeatureWorkspace pWS = pWSF.OpenFromFile(@strShapeFolder, 0) as IFeatureWorkspace;
                IFeatureClass pFC1 = pWS.OpenFeatureClass(filename1);
                IFeatureClass pFC2 = pWS.OpenFeatureClass(filename2);

                IFeatureCursor pFeatureCur1 = pFC1.Search(null, false);
                IFeature pFeature1 = pFeatureCur1.NextFeature();
                while (pFeature1 != null)
                {
                    if (pFeature1.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                    {
                        IArea pArea = pFeature1.Shape as IArea;
                        groundArea = groundArea + pArea.Area;
                    }

                    pFeature1 = pFeatureCur1.NextFeature();
                }

                IFeatureCursor pFeatureCur2 = pFC2.Search(null, false);
                IFeature pFeature2 = pFeatureCur2.NextFeature();
                while (pFeature2 != null)
                {
                    if (pFeature2.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                    {
                        IArea pArea = pFeature2.Shape as IArea;
                        greenLand = greenLand + pArea.Area;
                    }

                    pFeature2 = pFeatureCur2.NextFeature();
                }

                greenRatio = greenLand / groundArea * 100;
                label3.Text = greenRatio.ToString("#0.00") + "%";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("计算失败！");
            }
        }
    }
}
