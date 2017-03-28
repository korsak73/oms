using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osm2mssql.Importer.Tasks
{
    class TaskRelationIndices : TaskBase
    {
        public TaskRelationIndices(string name)
            : base(TaskType.ParallelFinishTask, name)
        {

        }

        protected override void DoTaskWork(string osmFile, Library.OsmReader.AttributeRegistry attributeRegistry)
        {
            ExecuteSqlCmd("ALTER TABLE tRelation ADD CONSTRAINT PK_tRelation PRIMARY KEY CLUSTERED (id, role) " +
                          "WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];");

            ExecuteSqlCmd("ALTER TABLE [tRelationCreation] ADD CONSTRAINT PK_tRelationCreation PRIMARY KEY CLUSTERED (relationId, sort) " +
                          "WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
        }
    }
}
