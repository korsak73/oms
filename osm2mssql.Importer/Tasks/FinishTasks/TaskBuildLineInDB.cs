using System.Data.SqlClient;
using System.Threading.Tasks;
using osm2mssql.Library.OsmReader;

namespace osm2mssql.Importer.Tasks
{
    public class TaskCreateLineInDB : TaskBase
    {
        public TaskCreateLineInDB(string name) : base(TaskType.FinishTask, name)
        {
        }

        protected override void DoTaskWork(string osmFile, AttributeRegistry attributeRegistry)
        {
            ExecuteSqlCmd("INSERT INTO dbo.tWay(Id, line) " +
                          "SELECT wayId, dbo.CreateLineString(Latitude, Longitude, sort) " +
                          "from tWayCreation INNER JOIN " +
                          "tNode on tNode.Id = tWayCreation.nodeId group by wayId");
        }
    }
}
