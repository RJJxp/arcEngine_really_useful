using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.SystemUI;
using System.Threading;



namespace GIS0728
{
    public partial class Property : Form
    {
        public Property()
        {
            InitializeComponent();
        }

        private void Property_Load(object sender, EventArgs e)
        {
            DataTable pTable = new DataTable();
            // datagridview
            ILayer pLayer = global.p2DMap.get_Layer(global.PropertyID);
            IFeatureLayer pFLayer = pLayer as IFeatureLayer;
            IFeatureClass pFClass = pFLayer.FeatureClass;
            IFields pFields = pFClass.Fields;

            // copy the column name of the field
            for (int i = 0; i < pFields.FieldCount; i++)
            {
                string FieldName;
                FieldName = pFields.get_Field(i).AliasName;
                pTable.Columns.Add(FieldName);
            }

            IFeatureCursor pFeatureCursor;
            pFeatureCursor = pFClass.Search(null, false);

            IFeature pFeature;

            pFeature = pFeatureCursor.NextFeature();

            while (pFeature != null)
            {
                DataRow row = pTable.NewRow();
                for (int i = 0; i < pFields.FieldCount; i++)
                {
                    string FieldValue = null;
                    FieldValue = Convert.ToString(pFeature.get_Value(i));
                    row[i] = FieldValue;
                }
                pTable.Rows.Add(row);
                pFeature = pFeatureCursor.NextFeature();
            }

            dataGridView1.DataSource = pTable;
        
        }
        int[] myInt = null;
        int myIntCount = -1;

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            int count = 0;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Selected == true)
                {
                    count++;
                }
            }
            // 储存选中的FID
            myIntCount = count;
            if (myIntCount == 0) return;
            myInt = new int[count];
            count = 0;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Selected == true)
                {
                    count++;
                    myInt[count - 1] = i;
                }
            }

            // 在axmapcontrol里显示，通过查询的方式

            ILayer pLayer = global.p2DMap.get_Layer(global.PropertyID);
            IFeatureLayer pFLayer = pLayer as IFeatureLayer;
            IFeatureClass pFClass = pFLayer.FeatureClass;

            IQueryFilter queryFilter = new QueryFilterClass();

            string searchText = "";
            for (int i = 0; i < myIntCount; i++)
            {
                if (i == 0)
                {
                    searchText = "\"FID\"= " + myInt[i].ToString() + " ";
                }
                else
                {
                    searchText += "OR " + "\"FID\"= " + myInt[i].ToString() + " ";
                }
            }
            queryFilter.WhereClause = searchText;
            IFeatureCursor featureCursor = pFClass.Search(queryFilter, true);
            IFeature pfeature = featureCursor.NextFeature();
            global.p2DMap.ClearSelection();
            while (pfeature != null)
            {
                global.p2DMap.SelectFeature(pLayer, pfeature);
                pfeature = featureCursor.NextFeature();
            }
            (global.p2DMap as IActiveView).Refresh();
        }
    }
}
