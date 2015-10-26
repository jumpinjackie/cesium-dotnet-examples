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
        public ActionResult Index() => View();

        public ActionResult Hello3DWorld() => View();

        public ActionResult GeoJSON() => View();

        public ActionResult KML() => View();

        public ActionResult CZML() => View();

        public ActionResult SignalR() => View();

        public ActionResult CZMLTimeDynamic() => View();
    }
}