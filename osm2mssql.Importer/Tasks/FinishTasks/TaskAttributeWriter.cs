using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osm2mssql.Library.OpenStreetMapTypes;
using osm2mssql.Library.OsmReader;

namespace osm2mssql.Importer.Tasks
{
    public class TaskAttributeWriter : TaskBase
    {
        public TaskAttributeWriter(string name)
            : base(TaskType.FinishTask, name)
        {
        }

        protected override void DoTaskWork(string osmFile, AttributeRegistry attributeRegistry)
        {
            var tagTypes = attributeRegistry.GetAttributeValues(OsmAttribute.TagType);

            var tagTypeDataTable = new DataTable("tTagType");
            tagTypeDataTable.Columns.Add("Typ", typeof(int));
            tagTypeDataTable.Columns.Add("Name", typeof(string));
            foreach (var tagType in tagTypes)
                tagTypeDataTable.Rows.Add(tagType.Key, tagType.Value);
            WriteToDb(tagTypeDataTable.TableName, tagTypeDataTable);

            var memberRoles = attributeRegistry.GetAttributeValues(OsmAttribute.MemberRole);

            var memberRoleDataTable = new DataTable("tMemberRole");
            memberRoleDataTable.Columns.Add("id", typeof(int));
            memberRoleDataTable.Columns.Add("name", typeof(string));
            foreach (var tagType in memberRoles)
                memberRoleDataTable.Rows.Add(tagType.Key, tagType.Value);
            WriteToDb(memberRoleDataTable.TableName, memberRoleDataTable);

            var memberTypes = attributeRegistry.GetAttributeValues(OsmAttribute.MemberType);

            var memberTypesDataTable = new DataTable("tMemberType");
            memberTypesDataTable.Columns.Add("id", typeof(int));
            memberTypesDataTable.Columns.Add("name", typeof(string));
            foreach (var tagType in memberTypes)
                memberTypesDataTable.Rows.Add(tagType.Key, tagType.Value);
            WriteToDb(memberTypesDataTable.TableName, memberTypesDataTable);

            WaitUntilFinished();
        }
    }
}
