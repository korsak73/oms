﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osm2mssql.Importer.Tasks
{
    class TaskWayIndices : TaskBase
    {
        public TaskWayIndices(string name) : base(TaskType.ParallelFinishTask, name)
        {

        }

        protected override void DoTaskWork(string osmFile, Library.OsmReader.AttributeRegistry attributeRegistry)
        {
            ExecuteSqlCmd("ALTER TABLE tWay ADD CONSTRAINT PK_tWay PRIMARY KEY CLUSTERED (Id) " +
                          "WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];");

            ExecuteSqlCmd("ALTER TABLE [tWayCreation] ADD CONSTRAINT PK_tWayCreation PRIMARY KEY CLUSTERED (wayId,	nodeId,	sort) " +
                          "WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");

            ExecuteSqlCmd("CREATE CLUSTERED INDEX [idxWay] ON [dbo].[tWayTag] ([WayId] ASC,[Typ] ASC)WITH (STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]");
        }
    }
}
