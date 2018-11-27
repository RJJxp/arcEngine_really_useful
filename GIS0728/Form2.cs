/*----------------------------------------------------------------
// 住宅小区综合评价系统
// 文件名称：Form2.cs
// 功能描述：用于容积率计算、地块对比、梯度显示的交互窗体
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

namespace GIS0728
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 两面相交
        /// </summary>
        private void intersect()
        {
            //两面相交
            ESRI.ArcGIS.Geoprocessor.Geoprocessor GP = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();
            ESRI.ArcGIS.AnalysisTools.Intersect intersect = new ESRI.ArcGIS.AnalysisTools.Intersect();

            intersect.in_features = @"C:\Users\航\Desktop\同济新村shp数据\同济新村房屋.shp;C:\Users\航\Desktop\同济新村shp数据\生成地块.shp";
            intersect.out_feature_class = @"C:\Users\航\Desktop\同济新村shp数据\intersect.shp";

            IGPProcess GProcess1 = intersect;
            GP.Execute(GProcess1, null);

            //如果显示
            //Form1.mainForm.axMapControl1.AddShapeFile(@"C:\Users\航\Desktop\同济新村shp数据\", "intersect.shp");
            //Form1.mainForm.axMapControl1.Refresh();
        }

        public static double[] MyArray;

        /// <summary>
        /// 对比不同地块容积率
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)//所有文件夹部分需要更新！
        {
            string filepath =  @"C:\Users\航\Desktop\同济新村shp数据\intersect.shp";
            
            if (File.Exists(filepath))
            System.Console.WriteLine("相交文件已经存在！");
            
            else           
            intersect();
                        
            //相交后取关联字段
            IWorkspaceFactory pWSF = null;
            pWSF = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace pWS = pWSF.OpenFromFile(@"C:\Users\航\Desktop\同济新村shp数据", 0) as IFeatureWorkspace;
            
            IFeatureClass pFC1 = pWS.OpenFeatureClass("intersect.shp");
            IFeatureClass pFC2 = pWS.OpenFeatureClass("生成地块.shp");
            int locnum = pFC1.FindField("FID_生成地");//确定所属地块
            int floornum = pFC1.FindField("Floor");//确定层数
            int fidnum = pFC2.FindField("FID");
            
            IFeatureCursor pFeatureCur1 = pFC1.Search(null, false);
            IFeature pFeature1 = pFeatureCur1.NextFeature();
            IFeatureCursor pFeatureCur2 = pFC2.Search(null, false);
            IFeature pFeature2 = pFeatureCur2.NextFeature();

            double[] ratioArray = new double[16];
            int i = 0;

            while (pFeature2 != null)
            {
                string fidstr = pFeature2.get_Value(fidnum).ToString();
                int fid = int.Parse(fidstr);
                
                double floorArea = 0;
                double groundArea = 0;
                double plotRatio = 0;
                
                while (pFeature1 != null)
                {
                    string locstr = pFeature1.get_Value(locnum).ToString();
                    int loc = int.Parse(locstr);

                    if (pFeature1.Shape.GeometryType == esriGeometryType.esriGeometryPolygon && loc == fid)
                    {
                        string floorstr = pFeature1.get_Value(floornum).ToString();
                        int floor = int.Parse(floorstr);
                        IArea pArea = pFeature1.Shape as IArea;
                        floorArea = floorArea + pArea.Area * floor;
                    }

                    pFeature1 = pFeatureCur1.NextFeature();
                }

                IArea qArea = pFeature2.Shape as IArea;
                groundArea = qArea.Area;
                plotRatio = floorArea / groundArea;

                //导入数组
                ratioArray[i] = plotRatio;
                i = i + 1;

                //将游标清零
                pFeatureCur1 = pFC1.Search(null, false);
                pFeature1 = pFeatureCur1.NextFeature();

                pFeature2 = pFeatureCur2.NextFeature();
            }

            //添加字段
            IField pNewField = new FieldClass();
            IFieldEdit pFieldEdit = pNewField as IFieldEdit;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            pFieldEdit.AliasName_2 = "Plot_Ratio";
            pFieldEdit.Name_2 = "Plot_Ratio";
            
            //为字段赋值
            try
            {
                int theField = pFC2.Fields.FindField("Plot_Ratio");
                if (theField == -1)
                {
                    pFC2.AddField(pFieldEdit);
                    int k;
                    for (k = 0; k < 16; k++) //或者 k < pFeatureClass.FeatureCount(null);
                    {
                        IFeature pFeature = pFC2.GetFeature(k);
                        pFeature.set_Value(pFeature.Fields.FindField("Plot_Ratio"),ratioArray[k]);
                        pFeature.Store();   
                    }
                    //MessageBox.Show("字段添加成功！");
                    System.Console.WriteLine("字段添加成功！");
                }
                else
                {
                    //MessageBox.Show("字段已经存在！");
                    System.Console.WriteLine("字段已经存在！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("字段" + pFieldEdit.Name + "未能成功添加！(" + ex.Message + " )");
            }

            //导入静态数组
            MyArray = (double[])ratioArray.Clone();
            
            //清除默认的series
            chart1.Series.Clear();
            Series PRperBlock = new Series("地块容积率");

            //设置chart的类型
            PRperBlock.ChartType = SeriesChartType.Column;

            //给系列上的点进行赋值，分别对应横坐标和纵坐标的值
            int j;
            for (j = 1 ; j < 17 ; j++)
            {
                PRperBlock.Points.AddXY(j, ratioArray[j-1]);
            }

            chart1.Series.Add(PRperBlock);

            //设置坐标轴标题
            chart1.ChartAreas[0].AxisX.Title = "地块号";
            chart1.ChartAreas[0].AxisY.Title = "容积率";

            //设置标签的间隔和字体
            chart1.ChartAreas[0].AxisX.LabelStyle.Interval = 1;
            chart1.ChartAreas[0].AxisX.LabelStyle.Font = new Font("宋体", 10);

            //设置坐标轴标题的字体
            //chart1.ChartAreas[0].AxisX.TitleFont = new Font("宋体", 10);
            //chart1.ChartAreas[0].AxisY.TitleFont = new Font("宋体", 10);
        }

        public static NameValueCollection myCol = new NameValueCollection();  

        private void Form2_Load(object sender, EventArgs e)
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
                System.Console.WriteLine(ws.PathName + @"\"+ layer.Name + ".shp");
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
        /// 计算容积率
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
            double floorArea = 0;
            double plotRatio = 0;

            try
            {
                pWSF = new ShapefileWorkspaceFactoryClass();
                IFeatureWorkspace pWS = pWSF.OpenFromFile(@strShapeFolder, 0) as IFeatureWorkspace;
                IFeatureClass pFC1 = pWS.OpenFeatureClass(filename1);
                IFeatureClass pFC2 = pWS.OpenFeatureClass(filename2);
                int floornum = pFC2.FindField("Floor");//确定层数

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
                        string floorstr = pFeature2.get_Value(floornum).ToString();
                        int floor = int.Parse(floorstr);
                        IArea pArea = pFeature2.Shape as IArea;
                        floorArea = floorArea + pArea.Area * floor;
                    }

                    pFeature2 = pFeatureCur2.NextFeature();
                }

                plotRatio = floorArea / groundArea;
                label3.Text = plotRatio.ToString("#0.00");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("计算失败！");
            }

            //同一图层时的处理方法：

            //// 获取选择集
            //ISelection pSelection = Form1.mainForm.axMapControl1.Map.FeatureSelection;
            //// 打开属性标签 
            //IEnumFeatureSetup pEnumFeatureSetup = pSelection as IEnumFeatureSetup;
            //pEnumFeatureSetup.AllFields = true;
            //// 获取要素
            //IFeatureClass pFeatureClass = pSelection as IFeatureClass;
            //IEnumFeature pEnumFeature = pSelection as IEnumFeature;
            //IFeature pFeature = pEnumFeature.Next();
            //double area = 0;
            //double floorArea = 0;
            //double plotRatio = 0;
            //int typenum = pFeatureClass.FindField("Type");//确定shp文件类型
            //int floornum = pFeatureClass.FindField("Floor");//确定层数
            
            //while (pFeature != null)
            //{
            //    string type = pFeature.get_Value(typenum).ToString();
                
            //    if (pFeature.Shape.GeometryType == esriGeometryType.esriGeometryPolygon && type == "土地")
            //    {
            //        //计算面积
            //        IArea pArea = pFeature.Shape as IArea;
            //        area = area + pArea.Area;//得到的面积单位是平方米
            //    }

            //    else if (pFeature.Shape.GeometryType == esriGeometryType.esriGeometryPolygon && type == "楼房")
            //    {
            //        string floorstr = pFeature.get_Value(floornum).ToString();
            //        int floor = int.Parse(floorstr);
            //        IArea pFloorArea = pFeature.Shape as IArea;
            //        floorArea = floorArea + pFloorArea.Area * floor;
            //    }
            //    else
            //        break;
            //};
            //plotRatio = floorArea / area;
            //label3.Text = plotRatio.ToString("#0.00") + "%";   
        }

        /// <summary>
        /// 根据容积率大小进行梯度颜色显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            //梯度颜色算法
            IAlgorithmicColorRamp pAlgoColorRamp = new AlgorithmicColorRampClass();
            pAlgoColorRamp.Size = MyArray.Length;

            IRgbColor pFromColor = new RgbColorClass(), pToColor = new RgbColorClass();

            pFromColor.Red = 255;
            pFromColor.Green = 255;
            pFromColor.Blue = 255;

            pToColor.Red = 0;
            pToColor.Green = 255;
            pToColor.Blue = 255;

            pAlgoColorRamp.FromColor = pFromColor;
            pAlgoColorRamp.ToColor = pToColor;

            bool ok = true;
            pAlgoColorRamp.CreateRamp(out ok);


            IClassBreaksRenderer pRender = new ClassBreaksRendererClass();
            pRender.BreakCount = MyArray.Length;
            pRender.Field = "Plot_Ratio";

            ISimpleFillSymbol pSym;
            System.Array.Sort(MyArray);

            for (int i = 0; i < MyArray.Length; i++)
            {
                pRender.set_Break(i, (double)MyArray.GetValue(i));
                pSym = new SimpleFillSymbolClass();
                pSym.Color = pAlgoColorRamp.get_Color(i);
                pRender.set_Symbol(i, (ISymbol)pSym);
            }

            IGeoFeatureLayer pGeoLyr = (IGeoFeatureLayer)Form1.mainForm.axMapControl1.get_Layer(1);//确保先加入地块图层
            pGeoLyr.Renderer = (IFeatureRenderer)pRender;

            Form1.mainForm.axMapControl1.Refresh();
            Form1.mainForm.axTOCControl1.Update();
        }
    }
}
