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


/*----------------------------------------------------------------
// 住宅小区综合评价系统
// 文件名称：色带.cs
// 功能描述：用于主窗体栅格图层的颜色选择
// 完成人：刘嘉澍1551160
// 作业日期：2018.08.26
//----------------------------------------------------------------*/

namespace GIS0728
{
    public partial class 色带 : Form
    {
        private Form1 mainform;
        public 色带(Form1 fff,IRasterLayer mlayer)
        {
            InitializeComponent();
            mainform = fff;
            rLayer=mlayer;
        }
        private ISymbologyStyleClass symbologyStyleClass = null;
        private List<IStyleGalleryItem> symbolArray = new List<IStyleGalleryItem>();
        string installationFolder = ESRI.ArcGIS.RuntimeManager.ActiveRuntime.Path;
       
        private void 色带_Load(object sender, EventArgs e)
        {
            sBC.Location = new System.Drawing.Point(10,10);
            sBC.Size = new Size(170, 20);
            this.Controls.Add(sBC);
            axSymbologyControl1.Visible = false;
            axSymbologyControl1.LoadStyleFile(installationFolder + "\\Styles\\ESRI.ServerStyle");
            sBC.SymbologyStyleClass = axSymbologyControl1.GetStyleClass(esriSymbologyStyleClass.esriStyleClassColorRamps);
            sBC.SelectedIndex = 0;
            sBC.Show();  
        }
          public void SelectColorRamps()
        {
            InitializeComponent();
        }
          public void SelectColorRamps(IRasterLayer rasterLayer)
        {
            rLayer = rasterLayer;
            InitializeComponent();
        }



        //private ISymbologyStyleClass symbologyStyleClass = null;
        //private List<IStyleGalleryItem> symbolArray = new List<IStyleGalleryItem>();
        symbolComboBox sBC = new symbolComboBox();
        IRasterLayer rLayer = null;
        public class symbolComboBox : ComboBox
        {
            public symbolComboBox()
            {
                this.DrawMode = DrawMode.OwnerDrawFixed;
                this.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            protected override void OnDrawItem(DrawItemEventArgs e)
            {
                e.DrawBackground();
                e.DrawFocusRectangle();
                try
                {
                    Image image = (Image)Items[e.Index];
                    Rectangle rect = e.Bounds;
                    rect.Height -= 2;
                    rect.Width -= 2;
                    rect.X += 1;
                    rect.Y += 1;
                    e.Graphics.DrawImage(image, rect);
                }
                catch
                {
                    if (e.Index != -1)
                    {
                        e.Graphics.DrawString(Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds.X, e.Bounds.Y);
                    }
                }
                finally
                {
                    base.OnDrawItem(e);
                }
            }
            private ISymbologyStyleClass symbologyStyleClass = null;
            private List<IStyleGalleryItem> symbolArray = new List<IStyleGalleryItem>();
            //符号选择属性
            public ISymbologyStyleClass SymbologyStyleClass
            {
                get
                {
                    return symbologyStyleClass;
                }
                set
                {
                    symbologyStyleClass = value;

                    if (symbologyStyleClass != null)
                    {
                        this.symbolArray.Clear();
                        this.BeginUpdate();
                        this.Items.Clear();
                        for (int i = 0; i < symbologyStyleClass.ItemCount; i++)
                        {
                            IStyleGalleryItem item = symbologyStyleClass.GetItem(i);
                           stdole.IPictureDisp picture = symbologyStyleClass.PreviewItem(item, this.Width, this.Height);
                            Image image = Image.FromHbitmap(new IntPtr(picture.Handle));
                            image = new Bitmap(image, new Size(image.Width - 10, image.Height));
                            this.Items.Add(image);
                            this.symbolArray.Add(item);
                        }
                        this.EndUpdate();
                    }
                }
            }
            public IStyleGalleryItem SelectSymbol
            {
                get
                {
                    if (this.SelectedIndex >= 0 && symbolArray.Count > this.SelectedIndex)
                        return symbolArray[this.SelectedIndex];
                    else
                        return null;
                }
            }
        }


        //IRasterStatistics rasterStatic;
        private void SetRasterSymbol(IRasterLayer rasterLayer)
        {
            //获取选择的序号
            int index = sBC.SelectedIndex;
            ISymbologyStyleClass symbologyStyleClass = axSymbologyControl1.GetStyleClass(esriSymbologyStyleClass.esriStyleClassColorRamps);
            IStyleGalleryItem mStyleGalleryItem = symbologyStyleClass.GetItem(index);
            //获取选择的符号
            IColorRamp select = (IColorRamp)mStyleGalleryItem.Item;
            IRasterStretchColorRampRenderer rasterStretchColorRampRenderer = new RasterStretchColorRampRendererClass();
            IRasterRenderer rasterRenderer = rasterStretchColorRampRenderer as IRasterRenderer;
            rasterRenderer.Raster =( rasterLayer).Raster;

            if (checkBox1.Checked==true )
            {
                //修改像元值 线性拉伸
                IRaster2 pRaster2 = rasterLayer.Raster as IRaster2;
                IPnt pPntBlock = new PntClass();

                pPntBlock.X = 128;
                pPntBlock.Y = 128;

                IRasterCursor pRasterCursor = pRaster2.CreateCursorEx(pPntBlock);

                IRasterEdit pRasterEdit = pRaster2 as IRasterEdit;
                if (pRasterEdit.CanEdit())
                {
                    // IRasterBandCollection pBands = rasterLayer as IRasterBandCollection;
                    IPixelBlock3 pPixelblock3 = null;
                    int pBlockwidth = 0;
                    int pBlockheight = 0;
                    System.Array pixels;
                    IPnt pPnt = null;
                    object pValue;
                    // long pBandCount = pBands.Count;

                    //获取Nodata
                    //IRasterProps pRasterPro = pRaster2 as IRasterProps;

                    //object pNodata = pRasterPro.NoDataValue;
                    do
                    {
                        pPixelblock3 = pRasterCursor.PixelBlock as IPixelBlock3;
                        pBlockwidth = pPixelblock3.Width;
                        pBlockheight = pPixelblock3.Height;

                        //   for (int k = 0; k < pBandCount; k++)

                        //IRasterBandCollection bandCollection;
                        //IRasterLayer irasterLayer = rasterLayer;
                        //IRaster2 raster = rasterLayer.Raster as IRaster2;
                        //IRasterDataset rd = raster.RasterDataset;
                        //bandCollection = rd as IRasterBandCollection;
                        //IEnumRasterBand enumband = bandCollection.Bands;
                        //IRasterBand rasterBand = enumband.Next();
                        //rasterStatic = null;
                        //if (rasterBand != null && rasterBand.Statistics != null)
                        //{
                        //    rasterStatic = rasterBand.Statistics;
                        //}


                        int pixelmax, pixelmin;


                        {
                            pixels = (System.Array)pPixelblock3.get_PixelData(0);
                            pixelmax = Convert.ToInt32(pixels.GetValue(0, 0));
                            pixelmin = pixelmax;

                            for (int i = 0; i < pBlockwidth; i++)
                            {
                                for (int j = 0; j < pBlockheight; j++)
                                {
                                    pValue = pixels.GetValue(i, j);


                                    {
                                        // pixels.SetValue(Convert.ToByte(Convert.ToInt32(pValue) * strscale), i, j);
                                        if (Convert.ToInt32(pValue) > pixelmax) { pixelmax = Convert.ToInt32(pValue); }
                                        if (Convert.ToInt32(pValue) < pixelmin) { pixelmin = Convert.ToInt32(pValue); }
                                    }
                                }
                            }
                            double strscale = ((double)pixelmax - (double)pixelmin) / 255.0;

                            for (int i = 0; i < pBlockwidth; i++)
                            {
                                for (int j = 0; j < pBlockheight; j++)
                                {
                                    pValue = pixels.GetValue(i, j);


                                    {
                                        pixels.SetValue(Convert.ToByte((Convert.ToInt32(pValue) - pixelmin) / (strscale)), i, j);

                                    }
                                }
                            }


                            pPixelblock3.set_PixelData(0, pixels);
                        }
                        pPnt = pRasterCursor.TopLeft;
                        pRasterEdit.Write(pPnt, (IPixelBlock)pPixelblock3);
                    }
                    while (pRasterCursor.Next());
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pRasterEdit);
                   // MessageBox.Show("done");
                }
            }




            rasterRenderer.Update();
            rasterStretchColorRampRenderer.ColorRamp = select;
            rasterRenderer.Update();
            ((IRasterLayer)rasterLayer).Renderer = rasterRenderer;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (sBC.SelectSymbol != null)
            {
                if (rLayer != null)
                {
                    SetRasterSymbol(rLayer);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("error");
                    // this.Close();
                }
            }
            else
            {
                MessageBox.Show("未选择色带!");
            }
        }

    }
}
