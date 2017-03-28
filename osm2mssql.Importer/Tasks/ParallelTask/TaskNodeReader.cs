using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.SqlServer.Types;
using osm2mssql.Importer.Classes;
using osm2mssql.Library;
using osm2mssql.Library.OpenStreetMapTypes;
using osm2mssql.Library.Protobuf;
using osm2mssql.Library.OsmReader;
using System.Threading.Tasks;

namespace osm2mssql.Importer.Tasks
{
    public class TaskNodeReader : TaskBase
    {
        private int _countOfInsertedNodes;
        public TaskNodeReader(string name) : base(TaskType.ParallelTask, name)
        {
        }

        internal override void DurationRefresh()
        {
            base.DurationRefresh();
            AdditionalInfos = (int)(_countOfInsertedNodes / Duration.TotalSeconds) + " Nodes / sec.";
        }

        protected override void DoTaskWork(string osmFile, AttributeRegistry attributeRegistry)
        {
            if (string.IsNullOrEmpty(osmFile))
                return;

            if (!File.Exists(osmFile))
                return;

            ExecuteSqlCmd("TRUNCATE TABLE tNode");

            var loadingNodeTable = new DataTable();
            loadingNodeTable.Columns.Add("NodeId", typeof(long));
            loadingNodeTable.Columns.Add("location", typeof(SqlGeography));
            loadingNodeTable.Columns.Add("Latitude", typeof(double));
            loadingNodeTable.Columns.Add("Longitude", typeof(double));
            loadingNodeTable.MinimumCapacity = MaxRowCountInMemory;

            var dtNodeTags = new DataTable();
            dtNodeTags.MinimumCapacity = MaxRowCountInMemory;
            dtNodeTags.Columns.Add("NodeId", typeof(long));
            dtNodeTags.Columns.Add("Typ");
            dtNodeTags.Columns.Add("Info");

            var reader = osmFile.EndsWith(".pbf") ? 
                                                    (IOsmReader)new PbfOsmReader() : 
                                                    (IOsmReader)new XmlOsmReader();

            foreach (var node in reader.ReadNodes(osmFile, attributeRegistry))
            {
                _countOfInsertedNodes++;

                loadingNodeTable.Rows.Add(node.NodeId, node.ToSqlGeographyPoint(), node.Latitude, node.Longitude);
                foreach (var tag in node.Tags)
                {
                    dtNodeTags.Rows.Add(node.NodeId, tag.Typ, tag.Value);
                }
             
                if (loadingNodeTable.Rows.Count >= MaxRowCountInMemory)
                    WriteToDb("tNode", loadingNodeTable);
                if (dtNodeTags.Rows.Count >= MaxRowCountInMemory)
                    WriteToDb("tNodeTag", dtNodeTags);
            }

            WriteToDb("tNode", loadingNodeTable);
            WriteToDb("tNodeTag", dtNodeTags);
            WaitUntilFinished();

            Trace.WriteLine(string.Format("Inserted {0} nodes", _countOfInsertedNodes));
        }
    }
}
