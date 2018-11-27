/// 数据库导出功能
//张艾琳1551174

using System;
using System.Windows.Forms;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;

namespace GIS0728
{
    public partial class mdbWriteForm : Form
    {
        private Form1 mainForm;
        public mdbWriteForm(Form1 fff)
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
        
        private void mdbWriteForm_Load(object sender, EventArgs e)
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

            comboBox1.SelectedIndex = -1;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            DialogResult dialogRuslt = saveFileDialog.ShowDialog();
            if (dialogRuslt == DialogResult.OK)
            {
                textBox1.Text = saveFileDialog.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string shpName = Application.StartupPath + "\\Project_Data\\" + comboBox1.SelectedItem.ToString() + ".shp";
            string mdbName = textBox1.Text;
            
            string FileFullPath = shpName;
            string pFolder = System.IO.Path.GetDirectoryName(FileFullPath);//可以从文件名里面获取路径
            string pFileName = System.IO.Path.GetFileName(FileFullPath);//获取文件名
            string mdbFileName = textBox1.Text;
            string dir = System.IO.Path.GetDirectoryName(mdbFileName);
            mdbFileName = System.IO.Path.GetFileName(mdbFileName);
            string Getname = System.IO.Path.GetFileNameWithoutExtension(pFileName);
            IWorkspaceFactory shpWorkspaceFactory = new ShapefileWorkspaceFactoryClass();

            IWorkspace workspace = shpWorkspaceFactory.OpenFromFile(pFolder, 0);
            IFeatureWorkspace featureworkSpace = workspace as IFeatureWorkspace;
            IFeatureClass shpFeatureClass = featureworkSpace.OpenFeatureClass(Getname);



            IWorkspaceFactory mdbWorkspaceFactory = new AccessWorkspaceFactoryClass();
            if (System.IO.File.Exists(textBox1.Text))
            {
                System.IO.File.Delete(dir + mdbFileName);
            }
            IWorkspaceName workspaceName = mdbWorkspaceFactory.Create(dir, mdbFileName, null, 0);
            IName name = (IName)workspaceName;
            IWorkspace mdbWorkspace = (IWorkspace)name.Open();
            IFeatureWorkspace mdbFeatureWorkspace = mdbWorkspace as IFeatureWorkspace;
            IFeatureClass newFeatureClass = mdbFeatureWorkspace.CreateFeatureClass(shpFeatureClass.AliasName, shpFeatureClass.Fields, null, null,
            esriFeatureType.esriFTSimple, "Shape", "");
            IFeatureCursor featureCursor = shpFeatureClass.Search(null, true);
            IFeature feature = featureCursor.NextFeature();

            while (feature != null)
            {
                IFeature newfeature = newFeatureClass.CreateFeature();
                for (int i = 0; i < feature.Fields.FieldCount; i++)
                {
                    if (feature.Fields.get_Field(i).Editable)
                        newfeature.set_Value(i, feature.get_Value(i));
                }
                newfeature.Store();
                feature = featureCursor.NextFeature();

            }
        }
    }
}
