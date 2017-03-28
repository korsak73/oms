using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using osm2mssql.Importer.Classes;
using osm2mssql.Library;
using osm2mssql.Library.OpenStreetMapTypes;
using osm2mssql.Library.OsmReader;
using osm2mssql.Library.Protobuf;

namespace osm2mssql.Importer.Tasks
{
    public class TaskWayReader : TaskBase
    {
        private int _countOfInsertedWays;
        private double? _timeOffset = null;
        public TaskWayReader(string name) : base(TaskType.ParallelTask, name)
        {
        }

        internal override void DurationRefresh()
        {
            base.DurationRefresh();
            if (_timeOffset.HasValue)
                AdditionalInfos = (int)(_countOfInsertedWays / (Duration.TotalSeconds-_timeOffset)) + " Ways / sec.";
        }

        protected override void DoTaskWork(string osmFile, AttributeRegistry attributeRegistry)
        {
            if (string.IsNullOrEmpty(osmFile))
                return;
            if (!File.Exists(osmFile))
                return;

            var watch = Stopwatch.StartNew();
            ExecuteSqlCmd("TRUNCATE TABLE tWayCreation");

            var dtWays = new DataTable();
            dtWays.Columns.Add("wayId", typeof(long));
            dtWays.Columns.Add("nodeId", typeof(long));
            dtWays.Columns.Add("sort");
            dtWays.MinimumCapacity = MaxRowCountInMemory;

            var dtWayTags = new DataTable();
            dtWayTags.MinimumCapacity = MaxRowCountInMemory;
            dtWayTags.Columns.Add("WayId", typeof(long));
            dtWayTags.Columns.Add("Typ", typeof(int));
            dtWayTags.Columns.Add("Info", typeof(string));
            
            var reader = osmFile.EndsWith(".pbf") ?
                                                    (IOsmReader)new PbfOsmReader() :
                                                    (IOsmReader)new XmlOsmReader();

            foreach (var way in reader.ReadWays(osmFile, attributeRegistry))
            {
                if(!_timeOffset.HasValue)
                {
                    watch.Stop();
                    _timeOffset = watch.Elapsed.TotalSeconds;
                }
                _countOfInsertedWays++;
                var sort = 0;
                foreach(var node in way.NodeRefs)
                    dtWays.Rows.Add(way.WayId, node, sort++);
                   
                var allText = string.Empty;
                foreach (var tag in way.Tags)
                {
                    allText += tag.Value + " ";
                    dtWayTags.Rows.Add(way.WayId, tag.Typ, tag.Value);
                }
                

                if (dtWays.Rows.Count >= MaxRowCountInMemory)
                    WriteToDb("tWayCreation", dtWays);
                    

                if (dtWayTags.Rows.Count >= MaxRowCountInMemory)
                    WriteToDb("tWayTag", dtWayTags);
            }
            WriteToDb("tWayCreation", dtWays);
            WriteToDb("tWayTag", dtWayTags);
            WaitUntilFinished();

            Trace.WriteLine(string.Format("Inserted {0} ways", _countOfInsertedWays));
        }
    }
}
