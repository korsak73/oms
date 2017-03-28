using System.Collections.Generic;

namespace osm2mssql.Library.OpenStreetMapTypes
{
    public class Node 
    {
        public long NodeId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<Tag> Tags { get; set; }

        public Node()
        {
            Tags = new List<Tag>();
        }
    }
}
