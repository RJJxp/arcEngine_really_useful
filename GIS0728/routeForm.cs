using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

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
using ESRI.ArcGIS.NetworkAnalysis;
using ESRI.ArcGIS.NetworkAnalyst;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;


/*----------------------------------------------------------------
// 住宅小区综合评价系统
// 文件名称：routeForm.cs
// 功能描述：最佳路径分析的交互窗口
// 完成人：蔡昕芷1551169
// 作业日期：2018.08.25-26
//----------------------------------------------------------------*/

namespace GIS0728
{
    public partial class routeForm : Form
    {
        public Form1 pMainForm = new Form1();
        public INASolver naSolver;

        public routeForm(Form1 mainForm)
        {
            InitializeComponent();

            this.TopMost = true;

            pMainForm = mainForm;


        }

        internal void MainFormTxtChanged(object sender, EventArgs e)
        {
            MyEventArgs arg = e as MyEventArgs;
            this.textBox1.Text = arg.Text[0];
            this.textBox2.Text = arg.Text[global.clickedcount - 1];

        }

        //定位
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            global.networkAnalysis = true;

        }

        //路径分析
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (global.networkAnalysis && global.clickedcount > 1)
            {
                global.networkAnalysis = false;
                listBox1.Items.Add("开始进行路径分析求解...");

                IGPMessages gpMessages = new GPMessagesClass();
                loadNANetworkLocations("Stops", global.inputFClass, 80);
                naSolver = global.m_NAContext.Solver;
                naSolver.Solve(global.m_NAContext, gpMessages, null);
                listBox1.Items.Add("done!");

                //获取路径结果
                INAStreetDirectionsAgent2 streetAgent;
                streetAgent = global.m_NAContext.Agents.get_ItemByName("StreetDirectionsAgent") as INAStreetDirectionsAgent2;
                //设置处理结果为中文
                //streetAgent.Language = "zh-CN";
                streetAgent.Execute(null, null);

                INAStreetDirectionsContainer directionsContainer;
                directionsContainer = streetAgent.DirectionsContainer as INAStreetDirectionsContainer;
                //directionsContainer.SaveAsXML("route.xml");

                int directionsCount = directionsContainer.DirectionsCount;
                string routeIdName = string.Empty;
                string routeString;
                for (int index = 0; index < directionsCount; index++)
                {
                    INAStreetDirections naStreetDirections = directionsContainer.get_Directions(index);
                    routeString = getDirectionsString(naStreetDirections);
                    getDataTableFromRouteString(routeString);
                }

                //解决完后，删除图层内容
                ITable pTable_inputFClass = global.inputFClass as ITable;
                pTable_inputFClass.DeleteSearchedRows(null);
                (global.p2DMap as IActiveView).Refresh();

                if (gpMessages != null)
                {
                    for (int i = 0; i < gpMessages.Count; i++)
                    {
                        switch (gpMessages.GetMessage(i).Type)
                        {

                            case esriGPMessageType.esriGPMessageTypeError:
                                listBox1.Items.Add("错误 " + gpMessages.GetMessage(i).ErrorCode.ToString() + " " + gpMessages.GetMessage(i).Description);
                                break;
                            case esriGPMessageType.esriGPMessageTypeWarning:
                                listBox1.Items.Add("警告 " + gpMessages.GetMessage(i).Description);
                                break;
                            default:
                                listBox1.Items.Add("信息 " + gpMessages.GetMessage(i).Description);
                                break;
                        }
                    }
                }

                (global.p2DMap as IActiveView).Refresh();

            }
            else if (global.clickedcount == 1)
            {
                MessageBox.Show("未选取终点，请继续定位！");
            }
            else
            {
                MessageBox.Show("未选取或更新起点和终点，请重新定位！");
            }
        }

        public void loadNANetworkLocations(string strNAClassName, IFeatureClass inputFC, double snapTolerance)
        {
            INAClass naClass;
            INamedSet classes;
            classes = global.m_NAContext.NAClasses;
            naClass = classes.get_ItemByName(strNAClassName) as INAClass;
            //删除naClasses中添加的项
            naClass.DeleteAllRows();
            //加载网络分析对象，设置容差值
            INAClassLoader classLoader = new NAClassLoader();
            classLoader.Locator = global.m_NAContext.Locator;
            if (snapTolerance > 0) classLoader.Locator.SnapTolerance = snapTolerance;
            classLoader.NAClass = naClass;
            //创建INAclassFieldMap,用于字段映射
            INAClassFieldMap fieldMap;
            fieldMap = new NAClassFieldMap();
            //加载网络分析类
            int rowsln = 0;
            int rowsLocated = 0;
            IFeatureCursor featureCursor = inputFC.Search(null, true);
            classLoader.Load((ICursor)featureCursor, null, ref rowsln, ref rowsLocated);
            ((INAContextEdit)global.m_NAContext).ContextChanged();
        }

        //清除
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            listBox1.Items.Clear();
            clearAll();
        }

        private void clearAll()
        {
            global.networkAnalysis = false;
            pMainForm.getMainAxMapControl().CurrentTool = null;
            ITable pTable = global.inputFClass as ITable;
            pTable.DeleteSearchedRows(null);
            //提取路径前，删除上一次路径route网络上下文
            IFeatureClass routesFC;
            routesFC = global.m_NAContext.NAClasses.get_ItemByName("Routes") as IFeatureClass;
            ITable pTable1 = routesFC as ITable;
            pTable1.DeleteSearchedRows(null);
            //提取路径前，删除上一次路径Stops网络上下文
            INAClass stopsNAClass = global.m_NAContext.NAClasses.get_ItemByName("Stops") as INAClass;
            ITable ptable2 = stopsNAClass as ITable;
            ptable2.DeleteSearchedRows(null);
            //提取路径前，删除上一次barries网络上下文
            INAClass barriesNAClass = global.m_NAContext.NAClasses.get_ItemByName("Barriers") as INAClass;
            ITable pTable3 = barriesNAClass as ITable;
            pTable3.DeleteSearchedRows(null);
            global.PGC.DeleteAllElements();
            global.clickedcount = 0;
            (global.p2DMap as IActiveView).Refresh();
        }

        //获取方向信息
        private string getDirectionsString(INAStreetDirections serverDirections)
        {
            // 得到总的距离和时间
            INAStreetDirection direction = serverDirections.Summary;
            string totallength = null, totaltime = null;
            for (int k = 0; k < direction.StringCount; k++)
            {
                if (direction.get_StringType(k) == esriDirectionsStringType.esriDSTLength)
                    totallength = direction.get_String(k);
                if (direction.get_StringType(k) == esriDirectionsStringType.esriDSTTime)
                    totaltime = direction.get_String(k);
            }
            //MessageBox.Show("Directions for CFRoute [" + (1) + "] - Total Distance: " + totallength + " Total Time: " + totaltime);            
            string directionString = string.Empty;
            // 加节点到方向
            for (int directionIndex = 0; directionIndex < serverDirections.DirectionCount; directionIndex++)
            {
                direction = serverDirections.get_Direction(directionIndex);
                for (int stringIndex = 0; stringIndex < direction.StringCount; stringIndex++)
                {
                    if (direction.get_StringType(stringIndex) == esriDirectionsStringType.esriDSTGeneral ||
                        direction.get_StringType(stringIndex) == esriDirectionsStringType.esriDSTDepart ||
                        direction.get_StringType(stringIndex) == esriDirectionsStringType.esriDSTArrive ||
                        direction.get_StringType(stringIndex) == esriDirectionsStringType.esriDSTSummary
                        )
                    {
                        if (stringIndex == 0)
                        {
                            directionString += direction.get_String(stringIndex).ToString() + "|";
                        }
                        else if (stringIndex == 2)
                        {
                            directionString += direction.get_String(stringIndex).ToString() + "@";
                        }
                    }
                }
            }
            return directionString;

        }

        //导航信息显示
        private void getDataTableFromRouteString(string routesString)
        {
            routesString = routesString.Substring(0, routesString.Length - 1);
            string[] routeArray = routesString.Split(new Char[] { '@' });
            string firstSecond = routeArray[0];
            string[] firstSeconds = firstSecond.Split(new Char[] { '|' });
            string firstString = firstSeconds[0];
            string secondDirection = firstSeconds[1];
            string secondLength = firstSeconds[2];
            string endString = routeArray[routeArray.Length - 1];
            listBox1.Items.Add("");
            listBox1.Items.Add("导航信息");
            listBox1.Items.Add("");
            listBox1.Items.Add(firstString);
            listBox1.Items.Add("");
            listBox1.Items.Add(secondDirection + "   " + secondLength);
            listBox1.Items.Add("");
            for (int i = 1; i < routeArray.Length - 1; i++)
            {
                //MessageBox.Show(routeArray[i]);   
                string[] temps = routeArray[i].Split(new Char[] { '|' });
                string direction = temps[0];
                string length = temps[1];
                listBox1.Items.Add(direction + "   " + length);
                listBox1.Items.Add("");
            }
            listBox1.Items.Add(endString);
        }

        private void routeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            clearAll();
            this.Dispose();
            this.Close();
        }

        //导出路径
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "shapefile（*.shp）|*.shp";
            saveFileDialog1.FileName = "route_export";
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                saveFileDialog1.AddExtension = true;
                string file = saveFileDialog1.FileName.Substring(0, saveFileDialog1.FileName.LastIndexOf('\\'));
                string name = System.IO.Path.GetFileNameWithoutExtension(saveFileDialog1.FileName);
                if (!System.IO.Directory.Exists(file))
                {
                    System.IO.Directory.CreateDirectory(file);
                }
                try
                {
                    IFeatureClass routesFC;
                    routesFC = global.m_NAContext.NAClasses.get_ItemByName("Routes") as IFeatureClass;
                    ExportFeature(routesFC, file, name);
                }
                catch
                {
                    MessageBox.Show("导出失败！");
                }
            }
        }

        //创建要素
        public void ExportFeature(IFeatureClass apFeatureClass, string ExportFilePath, string ExportFileShortName)
        {

            if (apFeatureClass == null)
            {

                MessageBox.Show("分析出错，请检查路径分析结果", "系统提示");
                return;

            }

            //设置导出要素类的参数
            IFeatureClassName pOutFeatureClassName = new FeatureClassNameClass();
            IDataset pOutDataset = (IDataset)apFeatureClass;
            pOutFeatureClassName = (IFeatureClassName)pOutDataset.FullName;
            //创建一个输出shp文件的工作空间
            IWorkspaceFactory pShpWorkspaceFactory = new ShapefileWorkspaceFactory();
            IWorkspaceName pInWorkspaceName = new WorkspaceNameClass();
            pInWorkspaceName = pShpWorkspaceFactory.Create(ExportFilePath, ExportFileShortName, null, 0);

            //创建一个要素集合
            IFeatureDatasetName pInFeatureDatasetName = null;
            //创建一个要素类
            IFeatureClassName pInFeatureClassName = new FeatureClassNameClass();
            IDatasetName pInDatasetClassName;
            pInDatasetClassName = (IDatasetName)pInFeatureClassName;
            pInDatasetClassName.Name = ExportFileShortName;//作为输出参数
            pInDatasetClassName.WorkspaceName = pInWorkspaceName;
            //自定义字段
            AddField(apFeatureClass, "Elevation", "", esriFieldType.esriFieldTypeInteger);
            //通过FIELDCHECKER检查字段的合法性，为输出SHP获得字段集合
            long iCounter;
            IFields pOutFields, pInFields;
            IFieldChecker pFieldChecker;
            IField pGeoField;
            IEnumFieldError pEnumFieldError = null;
            pInFields = apFeatureClass.Fields;
            pFieldChecker = new FieldChecker();
            pFieldChecker.Validate(pInFields, out pEnumFieldError, out pOutFields);
            //通过循环查找几何字段
            pGeoField = null;
            for (iCounter = 0; iCounter < pOutFields.FieldCount; iCounter++)
            {
                if (pOutFields.get_Field((int)iCounter).Type == esriFieldType.esriFieldTypeGeometry)
                {
                    pGeoField = pOutFields.get_Field((int)iCounter);
                    break;
                }
            }
            //得到几何字段的几何定义
            IGeometryDef pOutGeometryDef;
            IGeometryDefEdit pOutGeometryDefEdit;
            pOutGeometryDef = pGeoField.GeometryDef;
            //设置几何字段的空间参考和网格
            pOutGeometryDefEdit = (IGeometryDefEdit)pOutGeometryDef;
            pOutGeometryDefEdit.GridCount_2 = 1;
            pOutGeometryDefEdit.set_GridSize(0, 1500000);
            try
            {
                //开始导入
                IFeatureDataConverter pShpToClsConverter = new FeatureDataConverterClass();
                pShpToClsConverter.ConvertFeatureClass(pOutFeatureClassName, null, pInFeatureDatasetName, pInFeatureClassName, pOutGeometryDef, pOutFields, "", 1000, 0);
                MessageBox.Show("导出成功！");
            }
            catch
            {
            }
        }

        private void AddField(IFeatureClass pFeatureClass, string name, string aliasName, esriFieldType FieldType)
        {
            //若存在，则不需添加
            if (pFeatureClass.Fields.FindField(name) > -1) return;
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit.AliasName_2 = aliasName;
            pFieldEdit.Name_2 = name;
            pFieldEdit.Type_2 = FieldType;

            IClass pClass = pFeatureClass as IClass;
            pClass.AddField(pField);
        }
    }
}
