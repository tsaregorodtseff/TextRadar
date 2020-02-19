using TextRadarFuzzySearchDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TextRadarFuzzySearchDemo.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {

            return View();

        }

        public ActionResult Details(int Index, string SearchString, double Relevance)
        {

            string[] BookText = HttpContext.Application["BookText"] as string[];

            var Search = new Models.Search()
            {
                DataString = BookText[Index - 1],
                SearchString = SearchString,
                Index = Index - 1,
                Mode = 2
            };

            Search.CalculateRelevance();

            return View(Search);
        }

        public ActionResult TestSearch(int Mode, string DataString, string SearchString)
        {

            var Search = new Models.Search()
            {
                DataString = DataString,
                SearchString = SearchString,
                Mode = Mode
            };

            Search.CalculateRelevance();

            return View(Search);

        }

        public ActionResult Pagination()
        {

            return View(HttpContext.Application["BookTextNumberOfPages"]);
        }

        public ActionResult Page(int PageIndex)
        {

            string[] BookText = HttpContext.Application["BookText"] as string[];

            var Page = new Models.Page()
            {
                Id = PageIndex - 1,
                Text = BookText[PageIndex - 1]
            };

            return View(Page);
        }

        [HttpPost]
        public ActionResult MultiSearch(Models.SearchSourceData Sourse)
        {

            if (Sourse.SearchLocation == "InDataString")
            {

                var Search = new Models.Search()
                {
                    DataString = Sourse.DataString,
                    SearchString = Sourse.SearchString,
                    Mode = 3
                };

                Search.CalculateRelevance();

                return PartialView("Search", Search);

            }

            if (Sourse.SearchLocation == "InBookText")
            {

                var SearchInBook = new Models.SearchInBook(HttpContext.Application["BookText"] as string[]);

                SearchInBook.SearchString = Sourse.SearchString;
                SearchInBook.limit = 0.5; // корень извлечен

                SearchInBook.Search();

                return PartialView("SearchInBook", SearchInBook);
            }

            return PartialView();

        }

        public ActionResult About()
        {
            ViewBag.Message = "";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "";

            return View();
        }

    }
}