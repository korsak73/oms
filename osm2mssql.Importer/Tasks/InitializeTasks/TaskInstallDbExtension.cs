using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osm2mssql.Importer.Tasks
{
    class TaskInstallDbExtension : TaskBase
    {
        public TaskInstallDbExtension(string name) : base(TaskType.InitializeTask, name)
        {

        }

        protected override async void DoTaskWork(string osmFile, Library.OsmReader.AttributeRegistry attributeRegistry)
        {
            var res = await QuerySqlCmd<string>("SELECT @@VERSION;");
            bool is2008Server = res.Contains("Server 2008");
            var createExtensions = App.GetResourceFileText("osm2mssql.Importer.SQL.CreateDbExtension.sql");
            var file = Directory.GetCurrentDirectory() + @"\osm2mssql.OsmDb.dll";
            if (is2008Server)
                file = Directory.GetCurrentDirectory() + @"\osm2mssql.OsmDb2008.dll";

            var buffer = File.ReadAllBytes(file);
            var data = "0x" + string.Join("", buffer.Select(x => x.ToString("X2")));
            createExtensions = createExtensions.Replace("[OSM]", Connection.InitialCatalog);
            createExtensions = createExtensions.Replace("[DllExtension]", data);
            ExecuteSqlCmd(createExtensions);
        }
    }
}
