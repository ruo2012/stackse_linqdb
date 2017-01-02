using Newtonsoft.Json;
using Search;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using WebMvc.Models;

namespace WebMvc.Controllers
{
    public class NodeController : Controller
    {

        [HttpPost]
        [ValidateInput(false)]
        public string SearchNode(string data_json)
        {
            var data = JsonConvert.DeserializeObject<SearchData>(data_json);
            var words = data.Query.Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

            if (data.TitleSearch)
            {
                Stopwatch sp = new Stopwatch();
                sp.Start();
                var tres = SearchApi.SearchTitles(data.Query);
                

                var title_links = tres.Select(f => new WebMvc.Models.Result()
                {
                    IsMeta = true,
                    Description = GetWords(f.Fragment, words),
                    Title = GetWords(f.Title, words),
                    Url = "http://stackoverflow.com/questions/" + f.Id,
                    Id = f.Id
                })
                .ToList();

                data.Links = title_links;
                sp.Stop();
                data.NodeTimeInMs = sp.ElapsedMilliseconds;
            }
            else
            {
                Stopwatch sp = new Stopwatch();
                sp.Start();
                var mres = SearchApi.SearchMain(data.Query);

                var main_links = mres.Select(z => new WebMvc.Models.Result()
                {
                    IsMeta = false,
                    Description = GetWords(z.Fragment, words),
                    Title = GetWords(z.Title, words),
                    Score = z.Score,
                    Url = "http://stackoverflow.com/questions/" + z.Id,
                    Id = z.Id
                })
                .ToList();
                data.Links = main_links;
                sp.Stop();
                data.NodeTimeInMs = sp.ElapsedMilliseconds;
            }
            return JsonConvert.SerializeObject(data);
        }

        List<WebMvc.Models.Word> GetWords(string text, List<string> words)
        {
            
            var twords = text.Split();
            var res = new List<WebMvc.Models.Word>();
            foreach (var w in twords)
            {
                res.Add(new WebMvc.Models.Word()
                {
                    IsBold = words.Any(f => w.ToLower().StartsWith(f.ToLower())),
                    Token = w
                });
            }
            return res;
        }
    }
}