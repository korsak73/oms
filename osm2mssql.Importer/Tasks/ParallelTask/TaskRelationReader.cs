using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Types;
using osm2mssql.Library;
using osm2mssql.Library.OpenStreetMapTypes;
using osm2mssql.Library.OsmReader;
using osm2mssql.Library.Protobuf;

namespace osm2mssql.Importer.Tasks
{
    public class TaskRelationReader : TaskBase
    {
        private double _countOfInsertedRelations;
        public TaskRelationReader(string name) : base(TaskType.ParallelTask, name)
        {
        }

        internal override void DurationRefresh()
        {
            base.DurationRefresh();
            AdditionalInfos = (int)(_countOfInsertedRelations / Duration.TotalSeconds) + " Relations / sec.";
        }

        protected override void DoTaskWork(string osmFile, AttributeRegistry attributeRegistry)
        {
            if (string.IsNullOrEmpty(osmFile))
                return;

            if (!File.Exists(osmFile))
                return;


            var reader = osmFile.EndsWith(".pbf") ?
                                                    (IOsmReader)new PbfOsmReader() :
                                                    (IOsmReader)new XmlOsmReader();


            ExecuteSqlCmd("TRUNCATE TABLE [tRelationCreation]");
            ExecuteSqlCmd("TRUNCATE TABLE [tRelationTag]");

            var dtRelationCreation = new DataTable { MinimumCapacity = MaxRowCountInMemory };
            dtRelationCreation.Columns.Add("relationId", typeof(long));
            dtRelationCreation.Columns.Add("ref", typeof(long));
            dtRelationCreation.Columns.Add("type");
            dtRelationCreation.Columns.Add("role");
            dtRelationCreation.Columns.Add("sort");

            var dtRelationTags = new DataTable { MinimumCapacity = MaxRowCountInMemory };
            dtRelationTags.Columns.Add("relationId", typeof(long));
            dtRelationTags.Columns.Add("Typ");
            dtRelationTags.Columns.Add("Info");

            foreach (var relation in reader.ReadRelations(osmFile, attributeRegistry))
            {
                _countOfInsertedRelations++;
                var sort = 0;
                foreach (var member in relation.Members)
                {
                    dtRelationCreation.Rows.Add(relation.RelationId, member.Ref, member.Type, member.Role, sort);
                    sort += 100000; //Offset for inserting Node Members in normlaized Table
                }

                foreach (var tag in relation.Tags)
                    dtRelationTags.Rows.Add(relation.RelationId, tag.Typ, tag.Value);

                if (dtRelationCreation.Rows.Count >= MaxRowCountInMemory)
                    WriteToDb("tRelationCreation", dtRelationCreation);

                if (dtRelationTags.Rows.Count >= MaxRowCountInMemory)
                    WriteToDb("tRelationTag", dtRelationTags);
            }

            WriteToDb("tRelationCreation", dtRelationCreation);
            WriteToDb("tRelationTag", dtRelationTags);

            WaitUntilFinished();
            Trace.WriteLine(string.Format("Inserted {0} relations", _countOfInsertedRelations));
        }
    }
}
