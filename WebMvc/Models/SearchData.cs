using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebMvc.Models
{
    public class SearchData
    {
        public string Query { get; set; }
        public List<Result> Links { get; set; }
        public string MetaRes { get; set; }
       
        public bool TitleSearch { get; set; }
        public long TimeInMs { get; set; }
        public string Node { get; set; }
        public long NodeTimeInMs { get; set; }
    }

    public class Result
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public List<Word> Title { get; set; }
        public List<Word> Description { get; set; }
        public double Score { get; set; }
        public bool IsMeta { get; set; }
    }
    public class Word
    {
        public bool IsBold { get; set; }
        public string Token { get; set; }
    }
    public enum NothingFoundReason : int
    {
        NoData = 0,
        QueryTooLong = 1,
        QueryTooShort = 2
    }
}