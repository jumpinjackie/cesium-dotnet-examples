using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CesiumAspNetDemo.Controllers
{
    public class DemoController : Controller
    {
        const string JSON_MIME = "application/json";

        public ActionResult Data_Zoning()
        {
            FileInfo fi = new FileInfo(Server.MapPath("~/App_Data/CDD_ZoningDistricts.geojson"));
            return new FileStreamResult(fi.OpenRead(), JSON_MIME);
        }

        public ActionResult Data_HealthClinics()
        {
            FileInfo fi = new FileInfo(Server.MapPath("~/App_Data/HEALTH_HealthClinics.geojson"));
            return new FileStreamResult(fi.OpenRead(), JSON_MIME);
        }

        public ActionResult Data_Playgrounds()
        {
            FileInfo fi = new FileInfo(Server.MapPath("~/App_Data/RECREATION_Playgrounds.geojson"));
            return new FileStreamResult(fi.OpenRead(), JSON_MIME);
        }

        public ActionResult Data_Rail()
        {
            FileInfo fi = new FileInfo(Server.MapPath("~/App_Data/TRANS_Rail.geojson"));
            return new FileStreamResult(fi.OpenRead(), JSON_MIME);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Hello3DWorld()
        {
            return View();
        }

        public ActionResult GeoJSON()
        {
            return View();
        }

        public ActionResult KML()
        {
            return View();
        }
    }
}