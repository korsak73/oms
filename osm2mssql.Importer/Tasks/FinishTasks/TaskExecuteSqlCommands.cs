using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osm2mssql.Importer.Tasks
{
    class TaskExecuteSqlCommands : TaskBase
    {
        public TaskExecuteSqlCommands(string name) : base(TaskType.FinishTask, name)
        {

        }

        protected override void DoTaskWork(string osmFile, Library.OsmReader.AttributeRegistry attributeRegistry)
        {
            var sql = File.ReadAllText("SQLCommands.sql");
            sql = sql.Replace("[OSM]", Connection.InitialCatalog);
            ExecuteSqlCmd(sql);
        }
    }
}
