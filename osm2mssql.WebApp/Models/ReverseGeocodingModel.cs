using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace osm2mssql.WebApp.Models
{
    public class ReverseGeocodingModel
    {
        public int SelectedTagTyp { get; set; }
        public IEnumerable<InfoDAL.tTagType> TagTypes { get; set; }
    }
}