using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace osm2mssql.WebApp.Controllers.API
{
    public class ReverseGeocodingController : ApiController
    {
        [HttpGet]
        public GeoPoint SearchNodeTags(int tagType, string text)
        {
            var db = new InfoDAL.osm2Entities();
            var query = db.tNodeTag.Where(x => x.tTagType.Typ == tagType)
                                   .Where(x => x.Info.Contains(text));
            var result = query.FirstOrDefault();
            return new GeoPoint
            {
                Latitude = result.tNode.Latitude,
                Longitude = result.tNode.Longitude
            };
        }
    }
}
