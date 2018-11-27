using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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

///此部分由刘嘉澍编写
namespace GIS0728
{
    public partial class _3D视图 : Form
    {
        private Form1 mainForm;
        public _3D视图(Form1 fff)
        {
            InitializeComponent();
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.axSceneControl1_Wheel);
            this.mainForm =fff;
        }

        private void _3D视图_Load(object sender, EventArgs e)
       {
           for (int i = 0; i < global.p3DMap.Scene.LayerCount; i++)
           {
               axSceneControl1.SceneGraph.Scene.AddLayer(global.p3DMap.Scene.get_Layer(i));
           }
           axSceneControl1.Camera.Rotate(-50); 
        }

        private void _3D视图_FormClosed(object sender, FormClosedEventArgs e)
        {
            global.open3dform = false;
            
           
            if (mainForm.splitContainer3.Orientation == Orientation.Horizontal)
            { mainForm.splitContainer3.SplitterDistance = mainForm.splitContainer3. Height/ 2; }
            else { mainForm.splitContainer3.SplitterDistance = mainForm.splitContainer3.Width / 2; }
            

        }
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

        //传递主窗体axSceneControl1控件
        public ESRI.ArcGIS.Controls.AxSceneControl get3DAxSceneControl()
        {
            return axSceneControl1;
        }
    }
}
