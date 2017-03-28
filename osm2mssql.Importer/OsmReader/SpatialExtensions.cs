using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Data.SqlTypes;
using osm2mssql.Library.OpenStreetMapTypes;
using Microsoft.SqlServer.Types;

namespace osm2mssql.Library.OsmReader
{
    public static class SpatialExtensions
    {
        public static SqlGeography ToSqlGeographyPoint(this Node node)
        {
            var point = string.Format(CultureInfo.InvariantCulture, "POINT({1} {0})", node.Latitude, node.Longitude);
            return SqlGeography.STPointFromText(new SqlChars(point), 4326);
        }
    }
}
