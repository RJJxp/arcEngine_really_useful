using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//添加
using System.Threading;

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
using ESRI.ArcGIS.NetworkAnalysis;
using ESRI.ArcGIS.NetworkAnalyst;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Animation;

namespace GIS0728
{
    public partial class Form1 : Form
    {
        public static Form1 mainForm;
        public routeForm routeForm;
        
        public Form1()
        {
            InitializeComponent();
            //鼠标滚动为张艾琳1551174添加
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.axSceneControl1_Wheel);
           
            mainForm = this;
        }
        public _3D视图 form3d;
        public static bool flagCreateFeature;
        public static bool flagDeleteFeature;

        private IActiveView m_ipActiveView;
        public event EventHandler SendMsgEvent;//定义事件
        public string[] CoorPoint = null;//存储路径分析坐标位置

        private void Form1_Load(object sender, EventArgs e)
        {
            ///以下是刘嘉澍1551160的代码
            //this.KeyPreview = true;
            关联ToolStripMenuItem.Enabled = false;
            global.p2DMap = this.axMapControl1.Map;
            global.p3DMap = this.axSceneControl1.SceneGraph;
            pan.OnCreate(this.axMapControl1.Object);
            this.Text = "住宅小区综合评价系统";
            //axToolbarControl2.Visible = false;
            axTOCControl1.Parent = splitContainer1.Panel1;
            axTOCControl1.Dock = DockStyle.Fill;
            axTOCControl2.Parent = splitContainer1.Panel2;
            axTOCControl2.Dock = DockStyle.Fill;
            splitContainer4.Height = axToolbarControl1.Height;
            axToolbarControl1.Parent = splitContainer4.Panel1;
            axToolbarControl1.Dock = DockStyle.Fill;
            axToolbarControl2.Parent = splitContainer4.Panel2;
            axToolbarControl2.Dock = DockStyle.Fill;
            axSceneControl1.Visible = false;
            axTOCControl2.Visible = false;
            splitContainer1.SplitterDistance = splitContainer1.Height;
            splitContainer3.SplitterDistance = splitContainer3.Width;
            打开3D视图ToolStripMenuItem.Text = "打开3D视图"; 
            上下排布ToolStripMenuItem.Enabled = false; 
            左右排布ToolStripMenuItem.Enabled = false; 
            分离视图窗体ToolStripMenuItem.Enabled = false;
            this.axMapControl1.Map.Name = "2DMap";
            this.axTOCControl1.Update();
            this.axSceneControl1.Scene  .Name = "3DMap";
            this.axTOCControl2.Update();
            mainForm = this;
           
            
        }

        private void axLicenseControl1_Enter(object sender, EventArgs e)
        {

        }

        //刘嘉澍1551160设置参数 查看3D视图是否打开
        public bool flag_3D=false;

        /// <summary>
        /// 打开3D视图
        /// 完成人：刘嘉澍1551160
        /// </summary>      
        /// <parma name ="sender"></parma>
        /// <parma name ="e"></parma>
        private void 打开3D视图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flag_3D = !flag_3D;
            if (flag_3D == true)
            { 打开3D视图ToolStripMenuItem.Text = "关闭3D视图"; 打开3D视图ToolStripMenuItem.Name = "打开3D视图"; 上下排布ToolStripMenuItem.Enabled = true; 左右排布ToolStripMenuItem.Enabled = true; 分离视图窗体ToolStripMenuItem.Enabled = true; axSceneControl1.Visible = true; axTOCControl2.Visible = true; splitContainer1.SplitterDistance = splitContainer1.Height * 2 / 3; splitContainer3.SplitterDistance = splitContainer3.Width / 2; 关联ToolStripMenuItem.Enabled = true; }
            else
            {
                打开3D视图ToolStripMenuItem.Text = "打开3D视图"; 上下排布ToolStripMenuItem.Enabled = false; 左右排布ToolStripMenuItem.Enabled = false; 分离视图窗体ToolStripMenuItem.Enabled = false; axSceneControl1.Visible  = false; axTOCControl2.Visible = false; splitContainer1.SplitterDistance = splitContainer1.Height;
                splitContainer3.SplitterDistance = splitContainer3.Width;
                关联ToolStripMenuItem.Enabled = false; 
            }


            
        }
        Checkform form_ck;
        public int openmode = -1;
        bool rotateflag = false;

        /// <summary>
        /// 载入shp文件
        /// 完成人：张艾琳1551174
        /// 修改或完善：刘嘉澍1551160
        /// </summary>      
        /// <parma name ="sender"></parma>
        /// <parma name ="e"></parma>
        private void 打开shp文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "shp文件(*.shp)|*.shp";
            openFileDialog1.Multiselect = false ;
            openFileDialog1.InitialDirectory = Application.StartupPath + "\\Project_Data";
            if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            string FileFullPath = openFileDialog1.FileName;
            string pFolder = System.IO.Path.GetDirectoryName(FileFullPath);//可以从文件名里面获取路径
            string pFileName = System.IO.Path.GetFileName(FileFullPath);//获取文件名

            ILayerFactoryHelper pLayerFactoryHelper = new LayerFactoryHelperClass();
            IFileName fileN = new FileNameClass();
            fileN.Path = FileFullPath;
            IEnumLayer pEnumLayer = pLayerFactoryHelper.CreateLayersFromName(fileN as IName);
            ILayer pLayer;
            pEnumLayer.Reset();
            pLayer = pEnumLayer.Next();
            form_ck=new Checkform(mainForm );
            form_ck.ShowDialog();

         
          //  while (pLayer != null)
            {
                if (openmode == 1)
                { axMapControl1.AddLayer(pLayer); }
                else if (openmode == 2)
                {
                    axSceneControl1.SceneGraph.Scene.AddLayer(pLayer);
                    if (rotateflag == false)
                    { axSceneControl1.Camera.Rotate(-50); rotateflag = true; }
                    if (global.open3dform == true) { form3d.axSceneControl1.SceneGraph.Scene.AddLayer(pLayer); }
                   
                }
                else if(openmode==3)
                {
                    axMapControl1.AddLayer(pLayer);
                    axSceneControl1.SceneGraph.Scene.AddLayer(pLayer);
                   if (rotateflag==false)
                   { axSceneControl1.Camera.Rotate(-50); rotateflag = true; }
                   if (global.open3dform == true) { form3d.axSceneControl1.SceneGraph.Scene.AddLayer(pLayer); }

                   
                }
                
              //  pLayer = pEnumLayer.Next();
                axSceneControl1.SceneGraph.RefreshViewers();
              
               // axSceneControl1.Refresh();
                //axTOCControl1.Refresh();
                //axTOCControl2.Refresh();
                axMapControl1.Refresh();
            }
        }

        /// <summary>
        /// 布局窗口上下排布功能
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 上下排布ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer3.Orientation = Orientation.Horizontal;
            splitContainer3.SplitterDistance = splitContainer3.Height / 2;
            splitContainer3.Refresh();
        }

        /// <summary>
        /// 布局窗口左右排布功能
        /// 完成人：刘嘉澍
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void 左右排布ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer3.Orientation = Orientation.Vertical;
            splitContainer3.SplitterDistance = splitContainer3.Width / 2;
            splitContainer3.Refresh();
        }
       
        /// <summary>
        /// 打开独立的3D窗体
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 分离视图窗体ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            form3d = new _3D视图(mainForm);
            form3d.Show();
            global.open3dform = true;
            //axSceneControl1.Visible = false; 
            //splitContainer1.SplitterDistance = splitContainer1.Height;
            splitContainer3.SplitterDistance = splitContainer3.Width;
            //axToolbarControl2.Visible = false;

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
        ControlsMapPanTool pan = new ControlsMapPanToolClass();
        
        /// <summary>
        /// 对2D控件pan功能的记录，完成2D与3D的联动平移
        /// 完成人：张艾琳 1551174
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void axMapControl1_OnMouseUp(object sender, IMapControlEvents2_OnMouseUpEvent e)
        {
    

            if (global.MapMouseDown == 1 && linkflag == true)
            {
                axMapControl1.CurrentTool =(ITool) pan;
                //axMapControl1.Pan();
                ICamera pCamera = this.axSceneControl1.Camera as ICamera;
                global.currentPoint.PutCoords(e.x, e.y);
                pCamera.Pan(global.mapPoint, global.currentPoint);
                ISceneViewer pSceneViewer = this.axSceneControl1.SceneGraph.ActiveViewer as ISceneViewer;
                pSceneViewer.Redraw(false);
            }
        }

        /// <summary>
        /// 对3D控件pan功能的记录，完成3D与2D的联动平移
        /// 完成人：张艾琳1551174
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void axSceneControl1_OnMouseUp(object sender, ISceneControlEvents_OnMouseUpEvent e)
        {
            if (global.MouseDown == 1 && linkflag==true)
            {
                //axSceneControl1.CurrentTool = null;
                //axSceneControl1.CurrentTool=(ITool)pan;
                IActiveView pActiveView = this.axMapControl1.Map as IActiveView;
                IEnvelope pEnvelope = (IEnvelope)pActiveView.Extent;
                pEnvelope.Offset(global.scenePoint.X - e.x, -global.scenePoint.Y + e.y);

                pActiveView.Extent = pEnvelope;
                pActiveView.Refresh();
                global.MouseDown = 0;

                

            }
        }

        /// <summary>
        /// 对3D控件pan功能鼠标状态的记录，完成3D与2D的联动平移
        /// 完成人：张艾琳1551174
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void axSceneControl1_OnMouseDown(object sender, ISceneControlEvents_OnMouseDownEvent e)
        {
            global.MouseDown = 1;
            global.scenePoint.PutCoords(e.x, e.y);
        }

        /// <summary>
        /// 对3D控件pan功能鼠标状态的记录，完成3D与2D的联动平移，获取路径分析起点、终点位置坐标
        /// 完成人：张艾琳1551174
        /// 修改或完善：蔡昕芷1551169
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            global.MapMouseDown = 1;
            global.mapPoint.PutCoords(e.x, e.y);

            if (global.networkAnalysis == true)
            {


                //this.Cursor = new System.Windows.Forms.Cursor("..\\..\\Resources\\locate.cur");
                IPointCollection points;//输入点集合
                IPoint point;
                points = new MultipointClass();
                point = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
                object o = Type.Missing;
                points.AddPoint(point, ref o, ref o);

                CreateFeature(global.inputFClass, points);//或者用鼠标点击最近点

                //把最近的点显示出来
                IElement element;
                ITextElement textelement = new TextElementClass();
                element = textelement as IElement;
                ITextSymbol textSymbol = new TextSymbol();

                textelement.Symbol = textSymbol;
                global.clickedcount++;
                textelement.Text = global.clickedcount.ToString();
                element.Geometry = m_ipActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
                global.PGC.AddElement(element, 0);
                m_ipActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                CoorPoint[global.clickedcount - 1] = e.x.ToString() + " , " + e.y.ToString();
                SendMsgEvent(this, new MyEventArgs() { Text = CoorPoint });
            }
        }
        /// <summary>
        /// 实现Scene控件的滚轮缩放功能
        /// 完成人；张艾琳1551174
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void axSceneControl1_Wheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (axSceneControl1.Visible == true)
            {
                try
                {
                    System.Drawing.Point pSceLoc = axSceneControl1.PointToScreen(this.axSceneControl1.Location);
                    System.Drawing.Point Pt = this.PointToScreen(e.Location);
                    if (Pt.X < pSceLoc.X | Pt.X > pSceLoc.X + axSceneControl1.Width | Pt.Y <
                          pSceLoc.Y | Pt.Y > pSceLoc.Y + axSceneControl1.Height) return;
                    double scale = 0.2;
                    if (e.Delta < 0) scale = -0.25;
                    //if (e.Delta > 0) scale = -0.2;
                    ICamera pCamera = axSceneControl1.Camera;
                    IPoint pPtObs = pCamera.Observer;
                    IPoint pPtTar = pCamera.Target;
                    pPtObs.X += (pPtObs.X - pPtTar.X) * scale;
                    pPtObs.Y += (pPtObs.Y - pPtTar.Y) * scale;
                    pPtObs.Z += (pPtObs.Z - pPtTar.Z) * scale;
                    pCamera.Observer = pPtObs;
                    
                    axSceneControl1.SceneGraph.RefreshViewers();
                }
                catch (Exception ex)
                {
                }
            }
        }
        bool linkflag = false;

        /// <summary>
        /// 设置2D与3D视图的联动平移状态
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 关联ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (linkflag == false)
            { linkflag = true; 关联ToolStripMenuItem.Text = "解除平移关联"; }
            else { linkflag = false; 关联ToolStripMenuItem.Text = "平移关联"; } 
        }

        /// <summary>
        /// 图层列表中鼠标右键菜单功能，更改图层可见性
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 更改可见性ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            esriTOCControlItem pTocItem = new esriTOCControlItem();
            IBasicMap pBasicMap = new MapClass();
            ILayer pLayer = new FeatureLayerClass();
            object obj1 = new object();
            object obj2 = new object();

            axTOCControl1.GetSelectedItem(ref pTocItem, ref pBasicMap, ref pLayer, ref obj1, ref obj2);
            if (pTocItem == esriTOCControlItem.esriTOCControlItemLayer)
            {
                int iIndex;
                for (iIndex = 0; iIndex < axMapControl1.LayerCount; iIndex++)
                {
                    if (axMapControl1.get_Layer(iIndex) == pLayer)
                    {
                        pLayer.Visible = !(pLayer.Visible);
                        axMapControl1.Refresh();
                        axTOCControl1.Update();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 图层列表中鼠标右键菜单功能，删除图层
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 删除图层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            esriTOCControlItem pTocItem = new esriTOCControlItem();
            IBasicMap pBasicMap = new MapClass();
            ILayer pLayer = new FeatureLayerClass();
            object obj1 = new object();
            object obj2 = new object();

            axTOCControl1.GetSelectedItem(ref pTocItem, ref pBasicMap, ref pLayer, ref obj1, ref obj2);
            if (pTocItem == esriTOCControlItem.esriTOCControlItemLayer)
            {
                int iIndex;
                for (iIndex = 0; iIndex < axMapControl1.LayerCount; iIndex++)
                {
                    if (axMapControl1.get_Layer(iIndex) == pLayer)
                    {
                        axMapControl1.DeleteLayer(iIndex);
                       
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 图层列表中鼠标右键菜单功能，改变层序
        /// 完成人：刘嘉澍155160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 上移图层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            esriTOCControlItem pTocItem = new esriTOCControlItem();
            IBasicMap pBasicMap = new MapClass();
            ILayer pLayer = new FeatureLayerClass();
            object obj1 = new object();
            object obj2 = new object();

            axTOCControl1.GetSelectedItem(ref pTocItem, ref pBasicMap, ref pLayer, ref obj1, ref obj2);
            if (pTocItem == esriTOCControlItem.esriTOCControlItemLayer)
            {
                int iIndex;
                for (iIndex = 0; iIndex < axMapControl1.LayerCount; iIndex++)
                {
                    if (axMapControl1.get_Layer(iIndex) == pLayer && iIndex != 0)
                    {
                        axMapControl1.MoveLayerTo(iIndex, iIndex - 1);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 图层列表中鼠标右键菜单功能，改变层序
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 下移图层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            esriTOCControlItem pTocItem = new esriTOCControlItem();
            IBasicMap pBasicMap = new MapClass();
            ILayer pLayer = new FeatureLayerClass();
            object obj1 = new object();
            object obj2 = new object();

            axTOCControl1.GetSelectedItem(ref pTocItem, ref pBasicMap, ref pLayer, ref obj1, ref obj2);
            if (pTocItem == esriTOCControlItem.esriTOCControlItemLayer)
            {
                int iIndex;
                for (iIndex = 0; iIndex < axMapControl1.LayerCount; iIndex++)
                {
                    if (axMapControl1.get_Layer(iIndex) == pLayer && iIndex != axMapControl1.LayerCount - 1)
                    {
                        axMapControl1.MoveLayerTo(iIndex, iIndex + 1);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 图层列表中鼠标右键菜单功能，将图层移至最上层
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 移至顶层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            esriTOCControlItem pTocItem = new esriTOCControlItem();
            IBasicMap pBasicMap = new MapClass();
            ILayer pLayer = new FeatureLayerClass();
            object obj1 = new object();
            object obj2 = new object();

            axTOCControl1.GetSelectedItem(ref pTocItem, ref pBasicMap, ref pLayer, ref obj1, ref obj2);
            if (pTocItem == esriTOCControlItem.esriTOCControlItemLayer)
            {
                int iIndex;
                for (iIndex = 0; iIndex < axMapControl1.LayerCount; iIndex++)
                {
                    if (axMapControl1.get_Layer(iIndex) == pLayer && iIndex != 0)
                    {
                        axMapControl1.MoveLayerTo(iIndex, 0);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 图层列表中鼠标右键菜单功能，将图层移至最下层
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 移至底层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            esriTOCControlItem pTocItem = new esriTOCControlItem();
            IBasicMap pBasicMap = new MapClass();
            ILayer pLayer = new FeatureLayerClass();
            object obj1 = new object();
            object obj2 = new object();

            axTOCControl1.GetSelectedItem(ref pTocItem, ref pBasicMap, ref pLayer, ref obj1, ref obj2);
            if (pTocItem == esriTOCControlItem.esriTOCControlItemLayer)
            {
                int iIndex;
                for (iIndex = 0; iIndex < axMapControl1.LayerCount; iIndex++)
                {
                    if (axMapControl1.get_Layer(iIndex) == pLayer && iIndex != axMapControl1.LayerCount - 1)
                    {
                        axMapControl1.MoveLayerTo(iIndex, axMapControl1.LayerCount - 1);
                        break;
                    }
                }
            }
        }
        编辑图层 bjtcform = new 编辑图层();
        public int editLayernum;
        ILayer editLayer = new FeatureLayerClass();
        /// <summary>
        /// 编辑图层功能
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 编辑图层ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            编辑图层 bjtcform = new 编辑图层();
            esriTOCControlItem pTocItem = new esriTOCControlItem();
            IBasicMap pBasicMap = new MapClass();
            ILayer pLayer = new FeatureLayerClass();
            object obj1 = new object();
            object obj2 = new object();
            editLayernum = 0;
            bjtcform.Show();
            axTOCControl1.GetSelectedItem(ref pTocItem, ref pBasicMap, ref pLayer, ref obj1, ref obj2);
            editLayer = pLayer;
            if (pTocItem == esriTOCControlItem.esriTOCControlItemLayer)
            {
                int iIndex;
                for (iIndex = 0; iIndex < axMapControl1.LayerCount; iIndex++)
                {
                    if (axMapControl1.get_Layer(iIndex) == pLayer)
                    {
                        editLayernum = iIndex;
                        break;
                    }
                }
            }
        }
        private Property propertyList;

        /// <summary>
        /// 图层列表中鼠标右键菜单功能，打开FeatureLayer图层属性表
        /// 完成人：任家平1551127
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 属性表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            esriTOCControlItem pTocItem = new esriTOCControlItem();
            IBasicMap pBasicMap = new MapClass();
            ILayer pLayer = new FeatureLayerClass();
            object obj1 = new object();
            object obj2 = new object();

            axTOCControl1.GetSelectedItem(ref pTocItem, ref pBasicMap, ref pLayer, ref obj1, ref obj2);
            if (pTocItem == esriTOCControlItem.esriTOCControlItemLayer)
            {
                
                int iIndex;
                for (iIndex = 0; iIndex < axMapControl1.LayerCount; iIndex++)
                {
                    if (axMapControl1.get_Layer(iIndex) == pLayer)
                    {
                        global.PropertyID = iIndex;
                        propertyList = new Property();
                        if(pLayer is RasterLayerClass)
                        {
                            属性表ToolStripMenuItem.Enabled = false;
                        }
                        propertyList.Show();

                    }
                }

            }
        }

        /// <summary>
        /// 图层列表中鼠标右键菜单功能，打开图层设置属性
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 属性ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            esriTOCControlItem pTocItem = new esriTOCControlItem();
            IBasicMap pBasicMap = new MapClass();
            ILayer pLayer = new FeatureLayerClass();
            object obj1 = new object();
            object obj2 = new object();

            axTOCControl1.GetSelectedItem(ref pTocItem, ref pBasicMap, ref pLayer, ref obj1, ref obj2);
            if (pTocItem == esriTOCControlItem.esriTOCControlItemLayer)
            {
                int iIndex;
                for (iIndex = 0; iIndex < axMapControl1.LayerCount; iIndex++)
                {
                    if (axMapControl1.get_Layer(iIndex) == pLayer)
                    {
                        SetupFeaturePropertySheet(pLayer);

                        break;
                    }
                }
            }
            axMapControl1.Refresh();
            axTOCControl1.Update();
        }

        /// <summary>
        /// 图层属性设置窗体设置
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="layer">已选中图层</param>
        /// <returns></returns>
        private bool SetupFeaturePropertySheet(ILayer layer)
        {
            if (layer == null) return false;
            ESRI.ArcGIS.Framework.IComPropertySheet pComPropSheet = new ESRI.ArcGIS.Framework.ComPropertySheet();
            pComPropSheet.Title = layer.Name + " - 属性";
            ESRI.ArcGIS.esriSystem.UID pPPUID = new ESRI.ArcGIS.esriSystem.UIDClass();
            pComPropSheet.AddCategoryID(pPPUID);

            // General.... 
            ESRI.ArcGIS.Framework.IPropertyPage pGenPage = new ESRI.ArcGIS.CartoUI.GeneralLayerPropPageClass();
            pComPropSheet.AddPage(pGenPage);

            //Source
            ESRI.ArcGIS.Framework.IPropertyPage pSrcPage = new ESRI.ArcGIS.CartoUI.FeatureLayerSourcePropertyPageClass();
            pComPropSheet.AddPage(pSrcPage);

            // Selection... 
            ESRI.ArcGIS.Framework.IPropertyPage pSelectPage = new ESRI.ArcGIS.CartoUI.FeatureLayerSelectionPropertyPageClass();
            pComPropSheet.AddPage(pSelectPage);

            // Display.... 
            ESRI.ArcGIS.Framework.IPropertyPage pDispPage = new ESRI.ArcGIS.CartoUI.FeatureLayerDisplayPropertyPageClass();
            pComPropSheet.AddPage(pDispPage);

            // Symbology.... 
            ESRI.ArcGIS.Framework.IPropertyPage pDrawPage = new ESRI.ArcGIS.CartoUI.LayerDrawingPropertyPageClass();
            pComPropSheet.AddPage(pDrawPage);

            // Fields...
            ESRI.ArcGIS.Framework.IPropertyPage pFieldsPage = new ESRI.ArcGIS.CartoUI.LayerFieldsPropertyPageClass();
            pComPropSheet.AddPage(pFieldsPage);

            // Definition Query...
            ESRI.ArcGIS.Framework.IPropertyPage pQueryPage = new ESRI.ArcGIS.CartoUI.LayerDefinitionQueryPropertyPageClass();
            pComPropSheet.AddPage(pQueryPage);

            // Labels....
            //ESRI.ArcGIS.Framework.IPropertyPage pSelPage = new ESRI.ArcGIS.CartoUI.LayerLabelsPropertyPageClass(); 
            //pComPropSheet.AddPage(pSelPage);

            // Joins & Relates.... 
            // ESRI.ArcGIS.Framework.IPropertyPage pJoinPage = new ESRI.ArcGIS.SystemUI .JoinRelatePageClass();
            //pComPropSheet.AddPage(pJoinPage); 

            // Setup layer link 
            ESRI.ArcGIS.esriSystem.ISet pMySet = new ESRI.ArcGIS.esriSystem.SetClass();
            pMySet.Add(layer); pMySet.Reset();

            // make the symbology tab active 
            pComPropSheet.ActivePage = 0;

            // show the property sheet
            bool bOK = pComPropSheet.EditProperties(pMySet, 0);
            // m_activeView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_activeView.Extent); 
            return (bOK);
        }

        /// <summary>
        /// 在2D图层列表上的鼠标右键事件，打开右键菜单
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            属性表ToolStripMenuItem.Enabled = true;
            if (e.button == 2)//右键事件,先判断是否是图层，再进行操作，优化
            {

                esriTOCControlItem pTocItem = new esriTOCControlItem();
                IBasicMap pBasicMap = new MapClass();
                ILayer pLayer = new FeatureLayerClass();
                object obj1 = new object();
                object obj2 = new object();

                axTOCControl1.GetSelectedItem(ref pTocItem, ref pBasicMap, ref pLayer, ref obj1, ref obj2);
                if (pTocItem == esriTOCControlItem.esriTOCControlItemLayer)
                {
                    if (pLayer is RasterLayerClass) { 属性表ToolStripMenuItem.Enabled = false; }
                    System.Drawing.Point mousept = new System.Drawing.Point(MousePosition.X, MousePosition.Y);
                    编辑图层ToolStripMenuItem1.Visible = false;
                    contextMenuStrip1.Show(mousept);
                    // contextMenuStrip1.Show(e.x + this.Left+axTOCControl1.Left, e.y + this.Top + axTOCControl1.Top);考虑窗体在屏幕中的位置、axTOCControl1在窗体中的位置以及border的厚度
                }
            }
        }

        /// <summary>
        /// 按ESC键将2D与3D控件现用工具清空
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode  == Keys.Escape)
            {
                axMapControl1.CurrentTool = null;
                axSceneControl1.CurrentTool = null;
            }
        }

        /// <summary>
        /// 在3D图层列表上的鼠标右键事件，打开右键菜单
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void axTOCControl2_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            if (e.button == 2)//右键事件,先判断是否是图层，再进行操作，优化
            {

                esriTOCControlItem pTocItem = new esriTOCControlItem();
                IBasicMap pBasicMap = new MapClass();
                ILayer pLayer = new FeatureLayerClass();
                object obj1 = new object();
                object obj2 = new object();

                axTOCControl2.GetSelectedItem(ref pTocItem, ref pBasicMap, ref pLayer, ref obj1, ref obj2);
                if (pTocItem == esriTOCControlItem.esriTOCControlItemLayer)
                {
                    System.Drawing.Point mousept = new System.Drawing.Point(MousePosition.X, MousePosition.Y);
                    contextMenuStrip2.Show(mousept); 
                    // contextMenuStrip1.Show(e.x + this.Left+axTOCControl1.Left, e.y + this.Top + axTOCControl1.Top);考虑窗体在屏幕中的位置、axTOCControl1在窗体中的位置以及border的厚度
                }
            }
        }
        /// <summary>
        /// 在3D图层列表的右键菜单，更改图层可见性
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            esriTOCControlItem pTocItem = new esriTOCControlItem();
            IBasicMap pBasicMap = new MapClass();
            ILayer pLayer = new FeatureLayerClass();
            object obj1 = new object();
            object obj2 = new object();

            axTOCControl2.GetSelectedItem(ref pTocItem, ref pBasicMap, ref pLayer, ref obj1, ref obj2);
            if (pTocItem == esriTOCControlItem.esriTOCControlItemLayer)
            {
                int iIndex;
                for (iIndex = 0; iIndex < axSceneControl1.SceneGraph.Scene .LayerCount; iIndex++)
                {
                    if (axSceneControl1.SceneGraph.Scene.get_Layer(iIndex) == pLayer)
                    {
                        pLayer.Visible = !(pLayer.Visible);
                        axSceneControl1.SceneGraph.RefreshViewers();
                        axTOCControl2.Update();
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// 完成栅格数据的读取和载入
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 载入栅格数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "栅格文件(*.img)|*.img|栅格文件(*.tif)|*.tif|栅格文件(*.jpg)|*.jpg";  //Filter是过滤器，双引号内的内容为 显示内容|过滤内容
            openFileDialog1.Multiselect = false;  //禁止多选
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;//判断ShowDialog1的返回值是不是有文件，没有则退出

            string fileFullPath = openFileDialog1.FileName;  //用fileFullPath代表文件的路径，需要解析为文件名和路径
            string pFolder = System.IO.Path.GetDirectoryName(fileFullPath);  //系统方法，从完整路径获取文件路径
            string pFileName = System.IO.Path.GetFileName(fileFullPath);  //系统方法，从完整路径获取文件名

            IRasterLayer pLayer = new RasterLayerClass();
            pLayer.CreateFromFilePath(fileFullPath);
            pLayer.Name = pFileName;

            form_ck = new Checkform(mainForm);
            form_ck.ShowDialog();

            {
                if (openmode == 1)
                { axMapControl1.AddLayer(pLayer); }
                else if (openmode == 2)
                {
                    axSceneControl1.SceneGraph.Scene.AddLayer(pLayer);
                    if (rotateflag == false)
                    { axSceneControl1.Camera.Rotate(-50); rotateflag = true; }
                    if (global.open3dform == true) { form3d.axSceneControl1.SceneGraph.Scene.AddLayer(pLayer); }

                }
                else if (openmode == 3)
                {
                    axMapControl1.AddLayer(pLayer);
                    axSceneControl1.SceneGraph.Scene.AddLayer(pLayer);
                    if (rotateflag == false)
                    { axSceneControl1.Camera.Rotate(-50); rotateflag = true; }
                    if (global.open3dform == true) { form3d.axSceneControl1.SceneGraph.Scene.AddLayer(pLayer); }


                }

                //  pLayer = pEnumLayer.Next();
                axSceneControl1.SceneGraph.RefreshViewers();

                // axSceneControl1.Refresh();
                axTOCControl1.Update();
                axTOCControl2.Update();

                //axTOCControl2.Refresh();
                axMapControl1.Refresh();
            }


            
        }

        /// <summary>
        ///在3D图层列表的右键菜单，删除图层
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            esriTOCControlItem pTocItem = new esriTOCControlItem();
            IBasicMap pBasicMap = new MapClass();
            ILayer pLayer = new FeatureLayerClass();
            object obj1 = new object();
            object obj2 = new object();

            axTOCControl2.GetSelectedItem(ref pTocItem, ref pBasicMap, ref pLayer, ref obj1, ref obj2);
            if (pTocItem == esriTOCControlItem.esriTOCControlItemLayer)
            {
                int iIndex;
                for (iIndex = 0; iIndex < axSceneControl1.SceneGraph.Scene.LayerCount; iIndex++)
                {
                    if (axSceneControl1.SceneGraph.Scene.get_Layer(iIndex) == pLayer)
                    {
                        axSceneControl1.SceneGraph.Scene.DeleteLayer(pLayer);
                        axTOCControl2.Update();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 噪音分析、噪音分级等功能的弹出窗体交互
        /// 完成人：刘嘉澍1551160
        /// </summary>
        #region
        噪声分析 zsfx;
        private void 道路噪声分布ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            zsfx = new 噪声分析(mainForm);
            zsfx.Show();
        }

        噪声分级 zsfj;
        private void 噪声分级可视化ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            zsfj = new 噪声分级(mainForm);
            zsfj.Show();

        }
        #endregion


        色带 sd;

        /// <summary>
        /// 对2D图层列表的双击操作，更改符号样式、更改栅格色带
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void axTOCControl1_OnDoubleClick(object sender, ITOCControlEvents_OnDoubleClickEvent e)
        {
            esriTOCControlItem pTocItem = new esriTOCControlItem();
            IBasicMap pBasicMap = new MapClass();
            ILayer pLayer = new FeatureLayerClass();
            object obj1 = new object();
            object obj2 = new object();

            axTOCControl1.GetSelectedItem(ref pTocItem, ref pBasicMap, ref pLayer, ref obj1, ref obj2);
            try
            {
                if (pTocItem == esriTOCControlItem.esriTOCControlItemLayer && !(pLayer is FeatureLayerClass))
                {
                    sd = new 色带(mainForm, (RasterLayer)pLayer);
                    sd.ShowDialog();
                    axTOCControl1.Update();
                    axMapControl1.Refresh();

                    axTOCControl2.Update();
                    axSceneControl1.SceneGraph.RefreshViewers();
                }
                else
                {

                    esriTOCControlItem toccItem = esriTOCControlItem.esriTOCControlItemNone;
                    ILayer iLayer = null;
                    IBasicMap iBasicMap = null;
                    object unk = null;
                    object data = null;
                    if (e.button == 1)
                    {
                        axTOCControl1.HitTest(e.x, e.y, ref toccItem, ref iBasicMap, ref iLayer, ref unk,
                            ref data);
                        System.Drawing.Point pos = new System.Drawing.Point(e.x, e.y);
                        if (toccItem == esriTOCControlItem.esriTOCControlItemLegendClass)
                        {
                            ESRI.ArcGIS.Carto.ILegendClass pLC = new LegendClassClass();
                            ESRI.ArcGIS.Carto.ILegendGroup pLG = new LegendGroupClass();
                            if (unk is ILegendGroup)
                            {
                                pLG = (ILegendGroup)unk;
                            }
                            pLC = pLG.get_Class((int)data);
                            ISymbol pSym;
                            pSym = pLC.Symbol;
                            ESRI.ArcGIS.DisplayUI.SymbolSelector pSS = new
                                ESRI.ArcGIS.DisplayUI.SymbolSelectorClass();
                            bool bOK = false;
                            pSS.AddSymbol(pSym);
                            bOK = pSS.SelectSymbol(0);
                            if (bOK)
                            {
                                pLC.Symbol = pSS.GetSymbolAt(0);
                            }
                            this.axMapControl1.ActiveView.Refresh();
                            this.axTOCControl1.Update();

                            axTOCControl2.Update();
                            axSceneControl1.SceneGraph.RefreshViewers();
                        }
                    }
                }
            }
            catch { };
           



        }
        /// <summary>
        /// 对3D图层列表的双击操作，更改符号样式、更改栅格色带
        /// 完成人：刘嘉澍1551160
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void axTOCControl2_OnDoubleClick(object sender, ITOCControlEvents_OnDoubleClickEvent e)
        {
            try
            {
                esriTOCControlItem pTocItem = new esriTOCControlItem();
                IBasicMap pBasicMap = new MapClass();
                ILayer pLayer = new FeatureLayerClass();
                object obj1 = new object();
                object obj2 = new object();

                axTOCControl2.GetSelectedItem(ref pTocItem, ref pBasicMap, ref pLayer, ref obj1, ref obj2);
                if (pTocItem == esriTOCControlItem.esriTOCControlItemLayer && (!(pLayer is FeatureLayerClass)))
                {
                    sd = new 色带(mainForm, (RasterLayer)pLayer);
                    sd.ShowDialog();
                    axTOCControl2.Update();
                    axSceneControl1.SceneGraph.Scene.DeleteLayer(pLayer);
                    axSceneControl1.SceneGraph.Scene.AddLayer(pLayer);

                    this.axMapControl1.ActiveView.Refresh();
                    this.axTOCControl1.Update();
                    this.axSceneControl1.SceneGraph.RefreshViewers();
                    this.axTOCControl2.Update();
                    //this.axSceneControl1.SceneGraph.RefreshViewers();
                }

                esriTOCControlItem toccItem = esriTOCControlItem.esriTOCControlItemNone;
                ILayer iLayer = null;
                IBasicMap iBasicMap = null;
                object unk = null;
                object data = null;
                if (e.button == 1)
                {
                    axTOCControl2.HitTest(e.x, e.y, ref toccItem, ref iBasicMap, ref iLayer, ref unk,
                        ref data);
                    System.Drawing.Point pos = new System.Drawing.Point(e.x, e.y);
                    if (toccItem == esriTOCControlItem.esriTOCControlItemLegendClass)
                    {
                        ESRI.ArcGIS.Carto.ILegendClass pLC = new LegendClassClass();
                        ESRI.ArcGIS.Carto.ILegendGroup pLG = new LegendGroupClass();
                        if (unk is ILegendGroup)
                        {
                            pLG = (ILegendGroup)unk;
                        }
                        pLC = pLG.get_Class((int)data);
                        ISymbol pSym;
                        pSym = pLC.Symbol;
                        ESRI.ArcGIS.DisplayUI.SymbolSelector pSS = new
                            ESRI.ArcGIS.DisplayUI.SymbolSelectorClass();
                        bool bOK = false;
                        pSS.AddSymbol(pSym);
                        bOK = pSS.SelectSymbol(0);
                        if (bOK)
                        {
                            pLC.Symbol = pSS.GetSymbolAt(0);
                        }
                        //this.axSceneControl1.Refresh();]
                        axSceneControl1.SceneGraph.Scene.DeleteLayer(iLayer);
                        axSceneControl1.SceneGraph.Scene.AddLayer(iLayer);

                        this.axSceneControl1.SceneGraph.RefreshViewers();
                        this.axTOCControl2.Update();
                    }
                }
            }
            catch { }
        }

        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 传递主窗体axMapControl1控件
        /// 完成人：蔡昕芷1551169
        /// </summary>
        /// <returns></returns>
        public ESRI.ArcGIS.Controls.AxMapControl getMainAxMapControl()
        {
            return axMapControl1;
        }

        /// <summary>
        /// 载入网络数据集文件
        /// 完成人：蔡昕芷1551169
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 打开网络数据集ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = Application.StartupPath; ;
            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK) return;

            string folderPath = folderBrowserDialog1.SelectedPath;

            //打开工作区间
            global.pFWorkspace = OpenWorkspace(folderPath) as IFeatureWorkspace;
            //打开网络数据集
            global.networkDataset = OpenNetworkDataset(global.pFWorkspace as IWorkspace, "同济新村道路_ND", "同济新村道路");
            //创建网络分析上下文，建立一种解决关系
            global.m_NAContext = CreateSolverContext(global.networkDataset);

            //打开停靠点数据集
            global.inputFClass = global.pFWorkspace.OpenFeatureClass("stops");

            //TEST_ND_JUNCTIONS图层
            IFeatureLayer vertex = new FeatureLayerClass();
            vertex.FeatureClass = global.pFWorkspace.OpenFeatureClass("同济新村道路_ND_Junctions");
            vertex.Name = vertex.FeatureClass.AliasName;
            axMapControl1.AddLayer(vertex, 0);
            //street图层
            IFeatureLayer road;
            road = new FeatureLayerClass();
            road.FeatureClass = global.pFWorkspace.OpenFeatureClass("同济新村道路");
            road.Name = road.FeatureClass.AliasName;
            axMapControl1.AddLayer(road, 0);

            //为networkdataset生成一个图层，并将该图层添加到axmapcontrol中
            ILayer pLayer;//网络图层
            INetworkLayer pNetworkLayer;
            pNetworkLayer = new NetworkLayerClass();
            pNetworkLayer.NetworkDataset = global.networkDataset;
            pLayer = pNetworkLayer as ILayer;
            pLayer.Name = "Network Dataset";
            axMapControl1.AddLayer(pLayer, 0);
            //生成一个网络分析图层并添加到axmaptrol1中
            ILayer layer1;
            INALayer nalayer = global.m_NAContext.Solver.CreateLayer(global.m_NAContext);
            layer1 = nalayer as ILayer;
            layer1.Name = global.m_NAContext.Solver.DisplayName;
            axMapControl1.AddLayer(layer1, 0);
            m_ipActiveView = axMapControl1.ActiveView;
            global.p2DMap = m_ipActiveView.FocusMap;
            global.PGC = global.p2DMap as IGraphicsContainer;
        }

        /// <summary>
        /// 打开工作空间
        /// 完成人：蔡昕芷1551169
        /// </summary>
        /// <param name="strGDBName"></param>
        /// <returns></returns>
        public IWorkspace OpenWorkspace(string strGDBName)
        {
            IWorkspaceFactory workspaceFactory;
            workspaceFactory = new FileGDBWorkspaceFactoryClass();
            return workspaceFactory.OpenFromFile(strGDBName, 0);
        }

        /// <summary>
        /// 打开网络数据集
        /// 完成人：蔡昕芷1551169
        /// </summary>
        /// <param name="networkDatasetWorkspace"></param>
        /// <param name="networkDatasetName"></param>
        /// <param name="featureDatasetName"></param>
        /// <returns></returns>
        public INetworkDataset OpenNetworkDataset(IWorkspace networkDatasetWorkspace, System.String networkDatasetName, System.String featureDatasetName)
        {
            if (networkDatasetWorkspace == null || networkDatasetName == "" || featureDatasetName == null)
            {
                return null;
            }
            IDatasetContainer3 datasetContainer3 = null;

            switch (networkDatasetWorkspace.Type)
            {
                case ESRI.ArcGIS.Geodatabase.esriWorkspaceType.esriFileSystemWorkspace:

                    // Shapefile or SDC network dataset workspace
                    IWorkspaceExtensionManager workspaceExtensionManager = networkDatasetWorkspace as ESRI.ArcGIS.Geodatabase.IWorkspaceExtensionManager; // Dynamic Cast
                    ESRI.ArcGIS.esriSystem.UID networkID = new ESRI.ArcGIS.esriSystem.UIDClass();

                    networkID.Value = "esriGeoDatabase.NetworkDatasetWorkspaceExtension";
                    ESRI.ArcGIS.Geodatabase.IWorkspaceExtension workspaceExtension = workspaceExtensionManager.FindExtension(networkID);
                    datasetContainer3 = workspaceExtension as IDatasetContainer3; // Dynamic Cast
                    break;

                case ESRI.ArcGIS.Geodatabase.esriWorkspaceType.esriLocalDatabaseWorkspace:

                // Personal Geodatabase or File Geodatabase network dataset workspace

                case ESRI.ArcGIS.Geodatabase.esriWorkspaceType.esriRemoteDatabaseWorkspace:

                    // SDE Geodatabase network dataset workspace
                    ESRI.ArcGIS.Geodatabase.IFeatureWorkspace featureWorkspace = networkDatasetWorkspace as ESRI.ArcGIS.Geodatabase.IFeatureWorkspace; // Dynamic Cast
                    global.featureDataset = featureWorkspace.OpenFeatureDataset(featureDatasetName);
                    ESRI.ArcGIS.Geodatabase.IFeatureDatasetExtensionContainer featureDatasetExtensionContainer = global.featureDataset as ESRI.ArcGIS.Geodatabase.IFeatureDatasetExtensionContainer; // Dynamic Cast
                    ESRI.ArcGIS.Geodatabase.IFeatureDatasetExtension featureDatasetExtension = featureDatasetExtensionContainer.FindExtension(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTNetworkDataset);
                    datasetContainer3 = featureDatasetExtension as ESRI.ArcGIS.Geodatabase.IDatasetContainer3; // Dynamic Cast
                    break;
            }

            if (datasetContainer3 == null)
                return null;

            ESRI.ArcGIS.Geodatabase.IDataset dataset = datasetContainer3.get_DatasetByName(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTNetworkDataset, networkDatasetName);

            return dataset as ESRI.ArcGIS.Geodatabase.INetworkDataset; // Dynamic Cast
        }

        /// <summary>
        /// 创建网络分析上下文
        /// 完成人：蔡昕芷1551169
        /// </summary>
        /// <param name="networkDataset"></param>
        /// <returns></returns>
        public INAContext CreateSolverContext(INetworkDataset networkDataset)
        {
            //获取创建网络分析上下文所需的IDENETWORKDATASET类型参数
            IDENetworkDataset deNDS = GetDENetworkDataset(networkDataset);
            INASolver naSolver;
            naSolver = new NARouteSolver();
            INAContextEdit contextEdit = naSolver.CreateContext(deNDS, naSolver.Name) as INAContextEdit;
            contextEdit.Bind(networkDataset, new GPMessagesClass());
            return contextEdit as INAContext;
        }

        /// <summary>
        /// 得到创建网络分析上下文所需的IDENETWORKDATASET类型参数
        /// 完成人：蔡昕芷1551169
        /// </summary>
        /// <param name="networkDataset"></param>
        /// <returns></returns>
        public IDENetworkDataset GetDENetworkDataset(INetworkDataset networkDataset)
        {
            //将网络分析数据集QI添加到DATASETCOMPOENT
            IDatasetComponent dstComponent;
            dstComponent = networkDataset as IDatasetComponent;
            //获得数据元素
            return dstComponent.DataElement as IDENetworkDataset;

        }

        /// <summary>
        /// 打开独立的最佳路径分析窗口
        /// 完成人：蔡昕芷1551169
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 最佳路径ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CoorPoint = null;
            CoorPoint = new string[50];
            routeForm = new routeForm(mainForm);
            SendMsgEvent += routeForm.MainFormTxtChanged;
            routeForm.Show();
        }

        /// <summary>
        /// 获取距离鼠标点击最近的点
        /// 完成人：蔡昕芷1551169
        /// </summary>
        /// <param name="featureClass"></param>
        /// <param name="PointCollection"></param>
        public void CreateFeature(IFeatureClass featureClass, IPointCollection PointCollection)
        {
            //是否为点图层
            if (featureClass.ShapeType != esriGeometryType.esriGeometryPoint)
            {
                return;
            }
            //创建点要素
            for (int i = 0; i < PointCollection.PointCount; i++)
            {
                IFeature feature = featureClass.CreateFeature();
                feature.Shape = PointCollection.get_Point(i);
                IRowSubtypes rowSubtypes = (IRowSubtypes)feature;
                feature.Store();
            }
        }

        /// <summary>
        /// 根据路径创建飞行动画
        /// 完成人：蔡昕芷1551169
        /// </summary>
        /// <param name="pScene"></param>
        /// <param name="_pPolyline"></param>
        /// <param name="_pType"></param>
        /// <param name="_pDuration"></param>
        public void CreateAnimationFromPath(IScene pScene, IPolyline _pPolyline, int _pType, double _pDuration)
        {
            IScene _pScene = pScene;

            // 获取动画扩展对象
            ESRI.ArcGIS.Analyst3D.IBasicScene2 pBasicScene2 = (ESRI.ArcGIS.Analyst3D.IBasicScene2)_pScene; // Explicit Cast
            ESRI.ArcGIS.Animation.IAnimationExtension pAnimationExtension = pBasicScene2.AnimationExtension;

            //创建两个对象，一个用于导入路径，一个用于播放
            ESRI.ArcGIS.Animation.IAGAnimationUtils pAGAnimationUtils = new ESRI.ArcGIS.Animation.AGAnimationUtilsClass();
            ESRI.ArcGIS.Animation.IAGImportPathOptions pAGImportPathOptions = new ESRI.ArcGIS.Animation.AGImportPathOptionsClass();

            // 设置参数
            pAGImportPathOptions.BasicMap = (ESRI.ArcGIS.Carto.IBasicMap)_pScene;
            pAGImportPathOptions.AnimationTracks = pAnimationExtension.AnimationTracks;
            pAGImportPathOptions.AnimationType = new AnimationTypeCameraClass();
            pAGImportPathOptions.VerticalOffset = 10;
            pAGImportPathOptions.LookaheadFactor = 1;

            pAGImportPathOptions.PutAngleCalculationMethods(esriPathAngleCalculation.esriAngleAddRelative,
                              esriPathAngleCalculation.esriAngleAddRelative,
                              esriPathAngleCalculation.esriAngleAddRelative);

            pAGImportPathOptions.AnimatedObject = _pScene.SceneGraph.ActiveViewer.Camera;
            pAGImportPathOptions.PathGeometry = _pPolyline;

                //都移动
            if (_pType == 1)
            {
                pAGImportPathOptions.ConversionType = ESRI.ArcGIS.Animation.esriFlyFromPathType.esriFlyFromPathObsAndTarget;
            }
                //观察者移动
            else if (_pType == 2)
            {
                pAGImportPathOptions.ConversionType = ESRI.ArcGIS.Animation.esriFlyFromPathType.esriFlyFromPathObserver;
            }
                //路径移动
            else
            {
                pAGImportPathOptions.ConversionType = ESRI.ArcGIS.Animation.esriFlyFromPathType.esriFlyFromPathTarget;
            }

            pAGImportPathOptions.RollFactor = 0;
            pAGImportPathOptions.AnimationEnvironment = pAnimationExtension.AnimationEnvironment;
            pAGImportPathOptions.ReversePath = false;

            ESRI.ArcGIS.Animation.IAGAnimationContainer AGAnimationContainer = pAnimationExtension.AnimationTracks.AnimationObjectContainer;
            pAGAnimationUtils.CreateFlybyFromPath(AGAnimationContainer, pAGImportPathOptions);
            //该接口相当于播放的界面
            IAGAnimationPlayer pAGAplayer = pAGAnimationUtils as IAGAnimationPlayer;
            IAGAnimationEnvironment pAGAeviroment = new AGAnimationEnvironmentClass();
            pAGAeviroment.AnimationDuration = _pDuration;
            pAGAeviroment.PlayMode = esriAnimationPlayMode.esriAnimationPlayOnceForward;
            pAGAplayer.PlayAnimation(_pScene as IAGAnimationTracks, pAGAeviroment, null);

        }

        /// <summary>
        /// 打开路径shp文件，在独立3D视图窗体进行飞行动画模拟
        /// 完成人：蔡昕芷1551169
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 动画模拟ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            form3d = new _3D视图(mainForm);
            form3d.Show();
            global.open3dform = true;
            global.p3DMap.Scene.ClearLayers();
            try
            {
                form3d.get3DAxSceneControl().LoadSxFile("..\\..\\..\\Data\\3D.sxd");
                form3d.get3DAxSceneControl().Refresh();

            }
            catch { }

            openFileDialog1.Filter = "shp文件(*.shp)|*.shp";
            openFileDialog1.Multiselect = false;
            openFileDialog1.InitialDirectory = Application.StartupPath + "\\Project_Data";
            if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            string FileFullPath = openFileDialog1.FileName;
            string pFolder = System.IO.Path.GetDirectoryName(FileFullPath);//可以从文件名里面获取路径
            string pFileName = System.IO.Path.GetFileName(FileFullPath);//获取文件名

            ILayerFactoryHelper pLayerFactoryHelper = new LayerFactoryHelperClass();
            IFileName fileN = new FileNameClass();
            fileN.Path = FileFullPath;
            IEnumLayer pEnumLayer = pLayerFactoryHelper.CreateLayersFromName(fileN as IName);
            ILayer layer;
            pEnumLayer.Reset();
            layer = pEnumLayer.Next();

            while (layer != null)
            {

                form3d.get3DAxSceneControl().Scene.AddLayer(layer, false);

                layer = pEnumLayer.Next();

                axSceneControl1.SceneGraph.RefreshViewers();

            }

            IScene pScene = form3d.get3DAxSceneControl().Scene;
            ILayer pLayer = form3d.get3DAxSceneControl().Scene.get_Layer(4);

            //获取路径  
            IFeatureLayer featureLayer = pLayer as IFeatureLayer;
            IFeatureClass featureClass = featureLayer.FeatureClass;
            IFeature feature = featureClass.GetFeature(0);
            IPolyline pPolyline = (IPolyline)feature.Shape;
            CreateAnimationFromPath(pScene, pPolyline, 1, 10);
        }

        private void 容积率ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 fm2 = new Form2();
            fm2.Show();
        }

        private void 绿地率ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 fm3 = new Form3();
            fm3.Show();
        }

        private void 拉框选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IMap pMap = axMapControl1.Map;
            IActiveView pActiveView = pMap as IActiveView;
            IEnvelope pEnv = axMapControl1.TrackRectangle();
            pMap.SelectByShape(pEnv, null, false);
            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }
        /// 数据库载入及导出功能
        //张艾琳1551174
        private void 载入数据库ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            mdbReadForm mdbrf = new mdbReadForm();
            mdbrf.Show();
        }

        private void 导出到数据库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mdbWriteForm mdbwf = new mdbWriteForm(this);
            mdbwf.Show();
        }

        private void 矢量转栅格ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            shp2raster f = new shp2raster(this);
            f.Show();
        }

        private void 地图代数ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapAlgebra f = new mapAlgebra(this);
            f.Show();
        }

        private void 坡向ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aspectForm f = new aspectForm(this);
            f.Show();
        }

        private void 栅格计算器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rasterCal f = new rasterCal(this);
            f.Show();
        }

        private void 山体阴影ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HillshadeForm f = new HillshadeForm(this);
            f.Show();
        }

        private void 栅格转矢量ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            raster2shp f = new raster2shp(this);
            f.Show();

        }


    }
}
