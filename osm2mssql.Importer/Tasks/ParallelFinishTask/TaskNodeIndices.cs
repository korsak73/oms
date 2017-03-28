using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osm2mssql.Importer.Tasks
{
    class TaskNodeIndices : TaskBase
    {
        public TaskNodeIndices(string name) : base(TaskType.ParallelFinishTask, name)
        {

        }

        protected override void DoTaskWork(string osmFile, Library.OsmReader.AttributeRegistry attributeRegistry)
        {
            ExecuteSqlCmd("ALTER TABLE tNode ADD CONSTRAINT PK_tNode PRIMARY KEY CLUSTERED (Id) " +
                          "WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];");
            ExecuteSqlCmd("CREATE CLUSTERED INDEX [idxNode] ON [dbo].[tNodeTag] ([NodeId] ASC,[Typ] ASC) WITH (STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]");
        }
    }
}
