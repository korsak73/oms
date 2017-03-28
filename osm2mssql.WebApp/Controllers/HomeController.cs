using osm2mssql.InfoDAL;
using osm2mssql.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace osm2mssql.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            osm2Entities db = new osm2Entities();
            var model = new ReverseGeocodingModel
            {
                TagTypes = db.tTagType.OrderBy(x => x.Typ).ToList()
            };
            return View(model);
        }
    }
}
