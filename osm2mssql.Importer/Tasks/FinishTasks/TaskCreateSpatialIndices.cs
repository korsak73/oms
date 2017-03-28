using System.Diagnostics;
using System.Threading.Tasks;
using osm2mssql.Library.OsmReader;

namespace osm2mssql.Importer.Tasks
{
    public class TaskCreateSpatialIndices : TaskBase
    {
        public TaskCreateSpatialIndices(string name) : base(TaskType.FinishTask, name)
        {
        }

        protected async override void DoTaskWork(string osmFile, AttributeRegistry attributeRegistry)
        {

            var res = await QuerySqlCmd<string>("SELECT @@VERSION;");
            bool is2008Server = res.Contains("Server 2008");
            Task task1 = null, task2 = null;
            if (is2008Server)
            {
                task1 = Task.Factory.StartNew(() => ExecuteSqlCmd("CREATE SPATIAL INDEX idx ON tWay(line)"));
                task2 = Task.Factory.StartNew(() => ExecuteSqlCmd("CREATE SPATIAL INDEX idx ON tNode(location)"));
            }
            else
            {
                task1 = Task.Factory.StartNew(() => ExecuteSqlCmd("CREATE SPATIAL INDEX idx ON tWay(line) USING GEOGRAPHY_AUTO_GRID"));
                task2 = Task.Factory.StartNew(() => ExecuteSqlCmd("CREATE SPATIAL INDEX idx ON tNode(location) USING GEOGRAPHY_AUTO_GRID"));
            }

            Task.WaitAll(new[] { task1, task2 });

        }
    }
}
