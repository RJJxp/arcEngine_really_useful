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
using ESRI.ArcGIS.SpatialAnalyst;
using ESRI.ArcGIS.SpatialAnalystTools;
using ESRI.ArcGIS.SpatialAnalystUI;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.DataSourcesRasterUI;
using ESRI.ArcGIS.GeoAnalyst;
using System.Collections;

/*----------------------------------------------------------------
// 住宅小区综合评价系统
// 文件名称：渲染.cs
// 功能描述：用于主窗体栅格数据颜色渲染的交互窗体（由于不可调式的bug，未使用）
// 完成人：刘嘉澍1551160
// 作业日期：2018.08.27
//----------------------------------------------------------------*/

namespace GIS0728
{
    public partial class 渲染 : Form
    {
        private Form1 mainform;
        private ArrayList EnumStyleItem = new ArrayList();
        private IGradientFillSymbol m_FillSymbol;
        private IColorRamp m_ColorRamp;

        public IRasterLayer rasterlayer;
        public AxMapControl axmapcontrol;
        public AxTOCControl axtoccontrol;
        string[] classmethod = new string[] { "自然断点分级", "等间距分级" };
        int[] classcount = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        public 渲染(Form1 fff, IRasterLayer rasLayer, AxMapControl mapcontrol, AxTOCControl toccontrol)
        {
            InitializeComponent();
            mainform = fff;
           
            DrawColorRamp();
            colorComboBox.SelectedIndex = 0;
            pictureBox1.Image = colorComboBox.SelectedItem as Image;
            colorComboBox1.SelectedIndex = 0;
            pictureBox2.Image = colorComboBox1.SelectedItem as Image;

            this.rasterlayer = rasLayer;
            axmapcontrol = mapcontrol;
            axtoccontrol = toccontrol;
        }
        private void 渲染_Load(object sender, EventArgs e)
        {
            cmbRasterBands.Items.AddRange(getBandName(rasterlayer).ToArray());
            cmbClassifyMethod.Items.AddRange(classmethod.ToArray());
            cmbRasterBands.SelectedIndex = 0;
            cmbClassifyMethod.SelectedIndex = 0;
        }
        private void DrawColorRamp()
        {

            string strDefaultStyleFileName = string.Format("{0}\\CNUStyles\\ESRI.ServerStyle", Application.StartupPath);
            IStyleGallery styleGallery = new ServerStyleGalleryClass();
            IStyleGalleryItem styleGalleryItem = new ServerStyleGalleryItemClass();
            IStyleGalleryStorage styleGalleryStorage = styleGallery as IStyleGalleryStorage;
            styleGalleryStorage.AddFile(strDefaultStyleFileName);
            IEnumStyleGalleryItem enumStyleGalleryItem = styleGallery.get_Items("Color Ramps", strDefaultStyleFileName, "");
            enumStyleGalleryItem.Reset();
            styleGalleryItem = enumStyleGalleryItem.Next();
            while (styleGalleryItem != null)
            {
                m_ColorRamp = (IColorRamp)styleGalleryItem.Item;
                EnumStyleItem.Add(m_ColorRamp);
                //新建m_FillSymbol和m_colorRamp
                m_FillSymbol = new GradientFillSymbol();
                m_FillSymbol.GradientPercentage = 1;
                m_FillSymbol.IntervalCount = 255;
                m_FillSymbol.GradientAngle = 0;
                m_FillSymbol.ColorRamp = m_ColorRamp;
                pictureBox1.Image = SymbolToBitmap(m_FillSymbol, 0, pictureBox1.Width, pictureBox1.Height);
                pictureBox2.Image = SymbolToBitmap(m_FillSymbol, 0, pictureBox1.Width, pictureBox1.Height);
                imageList1.Images.Add(m_ColorRamp.Name, pictureBox1.Image);
                imageList2.Images.Add(m_ColorRamp.Name, pictureBox1.Image);
                colorComboBox.Items.Add(pictureBox1.Image);
                colorComboBox1.Items.Add(pictureBox2.Image);
                styleGalleryItem = enumStyleGalleryItem.Next();
            }
        }

        private Image SymbolToBitmap(IGradientFillSymbol iSymbol, int iStyle, int iWidth, int iHeight)
        {
            IntPtr iHDC = new IntPtr();
            Bitmap iBitmap = new Bitmap(iWidth, iHeight);
            Graphics iGraphics = System.Drawing.Graphics.FromImage(iBitmap);
            tagRECT itagRECT;
            IEnvelope iEnvelope = new EnvelopeClass() as IEnvelope;
            IDisplayTransformation iDisplayTransformation;
            IPoint iPoint;
            IGeometryCollection iPolyline;
            IGeometryCollection iPolygon;
            IRing iRing;
            ISegmentCollection iSegmentCollection;
            IGeometry iGeometry = null;
            object Missing = Type.Missing;
            iEnvelope.PutCoords(0, 0, iWidth, iHeight);
            itagRECT.left = 0;
            itagRECT.right = iWidth;
            itagRECT.top = 0;
            itagRECT.bottom = iHeight;
            iDisplayTransformation = new DisplayTransformationClass();
            iDisplayTransformation.VisibleBounds = iEnvelope;
            iDisplayTransformation.Bounds = iEnvelope;
            iDisplayTransformation.set_DeviceFrame(ref itagRECT);//DeviceFrame
            iDisplayTransformation.Resolution = iGraphics.DpiX / 100000;
            iHDC = iGraphics.GetHdc();
            //获取Geometry
            if (iSymbol is ESRI.ArcGIS.Display.IMarkerSymbol)
            {
                switch (iStyle)
                {
                    case 0:
                        iPoint = new ESRI.ArcGIS.Geometry.Point();
                        iPoint.PutCoords(iWidth / 2, iHeight / 2);
                        iGeometry = iPoint;
                        break;
                    default:
                        break;
                }
            }
            else if (iSymbol is ESRI.ArcGIS.Display.ILineSymbol)
            {
                iSegmentCollection = new ESRI.ArcGIS.Geometry.Path() as ISegmentCollection;
                iPolyline = new ESRI.ArcGIS.Geometry.Polyline() as IGeometryCollection;
                switch (iStyle)
                {
                    case 0:
                        iSegmentCollection.AddSegment(CreateLine(0, iHeight / 2, iWidth, iHeight / 2) as ISegment, ref Missing, ref Missing);
                        iPolyline.AddGeometry(iSegmentCollection as IGeometry, ref Missing, ref Missing);
                        iGeometry = iPolyline as IGeometry;
                        break;
                    case 1:
                        iSegmentCollection.AddSegment(CreateLine(0, iHeight / 4, iWidth / 4, 3 * iHeight / 4) as ISegment, ref Missing, ref Missing);
                        iSegmentCollection.AddSegment(CreateLine(iWidth / 4, 3 * iHeight / 4, 3 * iWidth / 4, iHeight / 4) as ISegment, ref Missing, ref Missing);
                        iSegmentCollection.AddSegment(CreateLine(3 * iWidth / 4, iHeight / 4, iWidth, 3 * iHeight / 4) as ISegment, ref Missing, ref Missing);
                        iPolyline.AddGeometry(iSegmentCollection as IGeometry, ref Missing, ref Missing);
                        iGeometry = iPolyline as IGeometry;
                        break;
                    default:
                        break;
                }
            }
            else if (iSymbol is ESRI.ArcGIS.Display.IFillSymbol)
            {
                iSegmentCollection = new ESRI.ArcGIS.Geometry.Ring() as ISegmentCollection;
                iPolygon = new ESRI.ArcGIS.Geometry.Polygon() as IGeometryCollection;
                switch (iStyle)
                {
                    case 0:
                        iSegmentCollection.AddSegment(CreateLine(1, iHeight - 1, iWidth - 2, iHeight - 1) as ISegment, ref Missing, ref Missing);
                        iSegmentCollection.AddSegment(CreateLine(iWidth - 2, iHeight - 1, iWidth - 2, 2) as ISegment, ref Missing, ref Missing);
                        iSegmentCollection.AddSegment(CreateLine(iWidth - 2, 2, 1, 2) as ISegment, ref Missing, ref Missing);
                        iRing = iSegmentCollection as IRing;
                        iRing.Close();
                        iPolygon.AddGeometry(iSegmentCollection as IGeometry, ref Missing, ref Missing);
                        iGeometry = iPolygon as IGeometry;
                        break;
                    default:
                        break;
                }
            }
            else if (iSymbol is ESRI.ArcGIS.Display.ISimpleTextSymbol)
            {
                switch (iStyle)
                {
                    case 0:
                        iPoint = new ESRI.ArcGIS.Geometry.Point();
                        iPoint.PutCoords(iWidth / 2, iHeight / 2);
                        iGeometry = iPoint;
                        break;
                    default:
                        break;
                }
            }
            if (iGeometry == null)
            {
                MessageBox.Show("几何对象不符合要求！", "错误");
                return null;
            }
            ISymbol pOutputSymbol = iSymbol as ISymbol;
            pOutputSymbol.SetupDC(iHDC.ToInt32(), iDisplayTransformation);
            pOutputSymbol.Draw(iGeometry);
            pOutputSymbol.ResetDC();
            iGraphics.ReleaseHdc(iHDC);
            iGraphics.Dispose();
            return iBitmap;
        }

        private ILine CreateLine(int x1, int y1, int x2, int y2)
        {
            IPoint pnt1 = new PointClass();
            pnt1.PutCoords(x1, y1);
            IPoint pnt2 = new PointClass();
            pnt2.PutCoords(x2, y2);
            ILine ln = new LineClass();
            ln.PutCoords(pnt1, pnt2);
            return ln;
        }

        private void colorComboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();//绘制背景
            e.DrawFocusRectangle();//绘制焦点框
            //绘制图例
            Rectangle iRectangle = new Rectangle(e.Bounds.Left, e.Bounds.Top, 215, 27);
            Bitmap getBitmap = new Bitmap(imageList2.Images[e.Index]);
            e.Graphics.DrawImage(getBitmap, iRectangle);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public List<string> getBandName(IRasterLayer rasterlayer)
        {
            string fullpath = rasterlayer.FilePath;
            string filePath = System.IO.Path.GetDirectoryName(fullpath);
            string fileName = System.IO.Path.GetFileName(fullpath);
            IWorkspaceFactory wsf = new RasterWorkspaceFactoryClass();
            IWorkspace ws = wsf.OpenFromFile(filePath, 0);
            IRasterWorkspace rasterws = ws as IRasterWorkspace;
            IRasterDataset rastdataset = rasterws.OpenRasterDataset(fileName);
            IRasterBandCollection bandcoll = rastdataset as IRasterBandCollection;
            List<string> itemband = new List<string>();
            for (int i = 0; i < bandcoll.Count; i++)
            {
                itemband.Add(bandcoll.Item(i).Bandname);
            }
            return itemband;
        }
        private void ChangeRender2RasterStretchColorRampRender(IRasterLayer rasterLayer)
        {
            if (rasterlayer == null)
                return;

            //计算统计图
            IRasterBand band = GetBand(rasterLayer);
            if (band.Histogram == null)
            {
                band.ComputeStatsAndHist();
            }

            IRaster raster = rasterLayer.Raster;
            IRasterStretchColorRampRenderer rasterStretchColorRampRenderer = new RasterStretchColorRampRendererClass();
            IRasterRenderer rasterRenderer = rasterStretchColorRampRenderer as IRasterRenderer;
            rasterRenderer.Raster = raster;
            rasterRenderer.Update();

            //IAlgorithmicColorRamp algorithmicColorRamp = new AlgorithmicColorRampClass();
            //algorithmicColorRamp.Size = 255;
            //algorithmicColorRamp.FromColor = getRGBColor(255, 0, 0);
            //algorithmicColorRamp.ToColor = getRGBColor(0, 255, 0);
            //bool btrue;
            //algorithmicColorRamp.CreateRamp(out btrue);

            rasterStretchColorRampRenderer.BandIndex = cmbRasterBands.SelectedIndex;
            IColorRamp pColorRamp = (IColorRamp)EnumStyleItem[colorComboBox.SelectedIndex];
            rasterStretchColorRampRenderer.ColorRamp = pColorRamp;
            rasterRenderer.Update();
            rasterLayer.Renderer = rasterStretchColorRampRenderer as IRasterRenderer;

            axmapcontrol.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            axtoccontrol.Update();
            axmapcontrol.Extent = rasterlayer.AreaOfInterest;
        }
        public IRasterBand GetBand(IRasterLayer rasterlayer)
        {
            string fullpath = rasterlayer.FilePath;
            string filePath = System.IO.Path.GetDirectoryName(fullpath);
            string fileName = System.IO.Path.GetFileName(fullpath);
            IWorkspaceFactory wsf = new RasterWorkspaceFactoryClass();
            IWorkspace ws = wsf.OpenFromFile(filePath, 0);
            IRasterWorkspace rasterws = ws as IRasterWorkspace;
            IRasterDataset rastdataset = rasterws.OpenRasterDataset(fileName);
            IRasterBandCollection bandcoll = rastdataset as IRasterBandCollection;
            return bandcoll.Item(0);
        }
        private void ChangeRender2ClassifyColorRampRenderer(IRasterLayer rasterLayer, int classCount, IColorRamp pColorRamp)
        {
            if (rasterlayer == null)
                return;

            //计算统计图
            IRasterBand band = GetBand(rasterLayer);
            if (band.Histogram == null)
            {
                band.ComputeStatsAndHist();
            }


            IRaster raster = rasterLayer.Raster;
            //创建分类
            IRasterClassifyColorRampRenderer rasterClassifyColorRampRenderer = new RasterClassifyColorRampRendererClass();
            IRasterRenderer rasterRenderer = rasterClassifyColorRampRenderer as IRasterRenderer;
            rasterRenderer.Raster = raster;
            rasterRenderer.Update();



            //IClassify classify = null;

            //switch (classMethod)
            //{
            //    case "等间距分级":
            //        classify = new EqualIntervalClass();
            //        break;
            //    case "自然断点分级":
            //        classify = new NaturalBreaksClass();
            //        break;
            //}
            //int numClass = classCount;
            //classify.Classify(ref numClass);

            //double[] Classes = classify.ClassBreaks as double[];
            //UID pUid = classify.ClassID;
            //IRasterClassifyUIProperties rasClassifyUI = rasterClassifyColorRampRenderer as IRasterClassifyUIProperties;
            //rasClassifyUI.ClassificationMethod = pUid;
            rasterClassifyColorRampRenderer.ClassCount = classCount;



            //IAlgorithmicColorRamp algorithmicColorRamp = new AlgorithmicColorRampClass();
            //algorithmicColorRamp.Size = 20;
            //algorithmicColorRamp.FromColor = getRGBColor(255, 0, 0);
            //algorithmicColorRamp.ToColor = getRGBColor(0, 255, 0);
            //bool btrue;
            //algorithmicColorRamp.CreateRamp(out btrue);
            //IEnumColors enumColors = pColorRamp.Colors;


            IEnumColors enumColors = pColorRamp.Colors;

            IFillSymbol fillSymbol = new SimpleFillSymbolClass();
            for (int i = 0; i < rasterClassifyColorRampRenderer.ClassCount - 1; i++)
            {
                fillSymbol.Color = enumColors.Next();
                rasterClassifyColorRampRenderer.set_Symbol(i, fillSymbol as ISymbol);
                //rasterClassifyColorRampRenderer.set_Label(i, "Class" + i.ToString());

            }

            //ISimpleFillSymbol fillSymbol = new SimpleFillSymbolClass();
            //IColor pColor;
            //for (int i = 0; i < classCount - 1; i++)
            //{
            //    pColor = pColorRamp.get_Color(i * (pColorRamp.Size / classCount));
            //    fillSymbol = new SimpleFillSymbolClass();
            //    fillSymbol.Color = pColor;
            //    rasterClassifyColorRampRenderer.set_Symbol(i, fillSymbol as ISymbol);
            //    //rasterRenderer.Update();
            //    //rasterClassifyColorRampRenderer.set_Break(i, rasterClassifyColorRampRenderer.get_Break(i));
            //    //rasterClassifyColorRampRenderer.set_Label(i, "Class" + i.ToString());
            //    rasterRenderer.Update();

            //}
            rasterRenderer.Update();
            rasterLayer.Renderer = rasterClassifyColorRampRenderer as IRasterRenderer;
        }
        public void RasterClassify(IRasterLayer rastlayer, string classMethod, int count, IColorRamp ramp)
        {
            try
            {
                //计算统计图
                IRasterBand band = GetBand(rastlayer);
                if (band.Histogram == null)
                {
                    band.ComputeStatsAndHist();
                }
                IRasterClassifyColorRampRenderer rasClassifyRender = new RasterClassifyColorRampRendererClass();
                IRasterRenderer rasRender = rasClassifyRender as IRasterRenderer;
                rasRender.Raster = rastlayer.Raster;
                rasRender.Update();

                int numClasses = count;
                IRasterHistogram pRasterHistogram = band.Histogram;
                double[] dblValues = pRasterHistogram.Counts as double[];
                int intValueCount = dblValues.GetUpperBound(0) + 1;
                double[] vValues = new double[intValueCount];
                IRasterStatistics pRasterStatistic = band.Statistics;
                double dMaxValue = pRasterStatistic.Maximum;
                double dMinValue = pRasterStatistic.Minimum;

                double BinInterval = Convert.ToDouble((dMaxValue - dMinValue) / intValueCount);
                for (int i = 0; i < intValueCount; i++)
                {
                    vValues[i] = i * BinInterval + pRasterStatistic.Minimum;
                }
                long[] longvalues = new long[dblValues.Length];
                for (int i = 0; i <= dblValues.Length - 1; i++)
                {
                    longvalues[i] = long.Parse(Convert.ToString(dblValues[i]));
                }
                //IClassifyGEN classify = null;
                IClassify classify = null;

                switch (classMethod)
                {
                    case "等间距分级":
                        classify = new EqualIntervalClass();
                        break;
                    case "自然断点分级":
                        classify = new NaturalBreaksClass();
                        break;
                }
                classify.Classify(ref numClasses);

                double[] Classes = classify.ClassBreaks as double[];
                UID pUid = classify.ClassID;
                IRasterClassifyUIProperties rasClassifyUI = rasClassifyRender as IRasterClassifyUIProperties;
                rasClassifyUI.ClassificationMethod = pUid;
                rasClassifyRender.ClassCount = count;
                IColor pColor;
                ISimpleFillSymbol pSym;

                for (int j = 0; j < count; j++)
                {
                    pColor = ramp.get_Color(j * (ramp.Size - 1) / (count - 1));
                    pSym = new SimpleFillSymbolClass();
                    pSym.Color = pColor;
                    rasClassifyRender.set_Symbol(j, (ISymbol)pSym);
                    rasRender.Update();
                    rasClassifyRender.set_Break(j, rasClassifyRender.get_Break(j));
                    //rasClassifyRender.set_Label(j, Classes[j].ToString("0.000") + "-" + Classes[j + 1].ToString("0.000"));
                    rasRender.Update();
                }

                //IRasterProps rasterProps = (IRasterProps)rastlayer.Raster;
                //rasterProps.NoDataValue = 0;
                //IRasterDisplayProps props = rasClassifyRender as IRasterDisplayProps;
                //props.NoDataColor = GET(255, 255, 255);
                //rasRender.Update();
                rastlayer.Renderer = rasClassifyRender as IRasterRenderer;
            }
            catch
            {
                MessageBox.Show("唯一值数量已达到限制（65536）");
            }
        }

        private void colorComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();//绘制背景
            e.DrawFocusRectangle();//绘制焦点框
            //绘制图例
            Rectangle iRectangle = new Rectangle(e.Bounds.Left, e.Bounds.Top, 215, 27);
            Bitmap getBitmap = new Bitmap(imageList1.Images[e.Index]);
            e.Graphics.DrawImage(getBitmap, iRectangle);
        }

        private void colorComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = colorComboBox.SelectedItem as Image;
        }

        private void colorComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox2.Image = colorComboBox1.SelectedItem as Image;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                try
                {
                    ChangeRender2RasterStretchColorRampRender(rasterlayer);
                    axmapcontrol.Refresh();
                    axtoccontrol.Update();
                    axmapcontrol.Extent = rasterlayer.AreaOfInterest;
                }
                catch (Exception ec)
                {
                    MessageBox.Show(ec.Message);
                }
            }
            else
            {
                try
                {
                    IColorRamp pColorRamp = (IColorRamp)EnumStyleItem[colorComboBox1.SelectedIndex];
                    RasterClassify(rasterlayer, cmbClassifyMethod.Text, Convert.ToInt32(nudClassCount.Text), pColorRamp);
                    //ChangeRender2ClassifyColorRampRenderer(rasterlayer, Convert.ToInt32(nudClassCount.Text), pColorRamp);
                    axmapcontrol.Refresh();
                    axtoccontrol.Update();
                    axmapcontrol.Extent = rasterlayer.AreaOfInterest;
                }
                catch (Exception ec)
                {
                    MessageBox.Show(ec.Message);
                }
            }
        }

    }
}
