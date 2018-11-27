using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using ESRI.ArcGIS.NetworkAnalysis;
using ESRI.ArcGIS.NetworkAnalyst;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;


namespace GIS0728
{
    class global
    {
        public static bool open3dform = false;
        public static IMap p2DMap = null;
        public static ISceneGraph  p3DMap = null;
        public static bool scenePan = false;
        public static IPoint scenePoint = new PointClass();
        public static IPoint mapPoint = new PointClass();
        public static IPoint currentPoint = new PointClass();
        public static IActiveView pActiveView = null;
        public static int MouseDown = 0;
        public static int MapMouseDown = 0;
        public static ICamera pSceneCamera = null;

        //任家平
        public static int PropertyID = -1;
        public static List<string> map2list = new List<string>();// 从axmap到属性表

        //蔡昕芷
        public static INAContext m_NAContext;//网络分析上下文
        public static INetworkDataset networkDataset;//网络数据集
        public static IFeatureWorkspace pFWorkspace;
        public static IFeatureClass inputFClass;//打开stops数据集
        public static IFeatureDataset featureDataset;
        public static int clickedcount = 0;//mapcontrol加点显示点数
        public static IGraphicsContainer PGC;
        public static bool networkAnalysis = false;
    }
}
