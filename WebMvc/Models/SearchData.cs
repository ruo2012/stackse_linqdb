using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebMvc.Models
{
    public class SearchData
    {
        public DateTime CreationDate { get; set; }
        public string Query { get; set; }
        public List<Result> Links { get; set; }
        public bool NothingFound { get; set; }
        public NothingFoundReason Reason { get; set; }
        public long TotalRes { get; set; }
        public int IntersectionPercent { get; set; }
        public int CriteriasPercent { get; set; }
        public int SearchTimeMs { get; set; }
        public int FetchPercent { get; set; }
        public int DBPercent { get; set; }
        public int DeserializePercent { get; set; }
        public int CombinePercent { get; set; }
        public string TotalPagesCrawled { get; set; }
        public long Size_in_bytes { get; set; }
        public double Phase_fetch_index_ms { get; set; }
        public int StartPhase { get; set; }
        public int EndPhase { get; set; }
        public List<string> NodesAddresses { get; set; }
        public List<IntermResult> Nodes_best { get; set; }
        public double Nodes_Phase_fetch_index_ms { get; set; }
        public double Node_AvgIntersectionInMs { get; set; }
        public double Node_AvgCriteriasInMs { get; set; }
        public int SuccessfulNodes { get; set; }
        public int TotalSteps { get; set; }
        public string StepInfo { get; set; }
        public int StepSize { get; set; }
        public int PreDataPer { get; set; }
        public int IsNext { get; set; }
        public int IsPrev { get; set; }
        public int StepsToSkipForPrev { get; set; }
        public int Page { get; set; }
        public bool ShowNext { get; set; }
        public bool ShowPrev { get; set; }
        public bool NoButtons { get; set; }
        public string MetaRes { get; set; }
        public int MetaTimeMs { get; set; }
        public int MetaTimePerc { get; set; }
        public bool FromCache { get; set; }
        public bool Debug { get; set; }
        public int? Step { get; set; }
        public bool IsMeta { get; set; }
        public bool WasLoadingStuff { get; set; }
        public bool TitleSearch { get; set; }
    }

    public class PhaseResult
    {
        public List<IntermResult> Result { get; set; }
        public int IntersectionInMs { get; set; }
        public int CriteriasInMs { get; set; }
    }
    public class IntermResult
    {
        public int SiteKey { get; set; }
        public double Score { get; set; }
        public int? StartPosition { get; set; }
        public int TitleLength { get; set; }

        public string Url { get; set; }
        public int TotalRefs { get; set; }
        public ScoreDistribution Score_dist { get; set; }
        public bool IsMeta { get; set; }
    }
    public class ScoreDistribution
    {
        public double MainScore { get; set; }
        public double LinkScore { get; set; }
        public double TitleScore { get; set; }
        public double DescriptionScore { get; set; }
        public double UrlScore { get; set; }
        public double IndexScore { get; set; }
        public double RefsScore { get; set; }
    }

    public class Result
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public List<Word> Title { get; set; }
        public List<Word> Description { get; set; }
        public double Score { get; set; }
        public int Refs { get; set; }
        public ScoreDistribution Score_dist { get; set; }
        public int Title_length { get; set; }

        public bool IsMeta { get; set; }

        public string SiteKey { get; set; }
    }
    public class Word
    {
        public bool IsBold { get; set; }
        public string Token { get; set; }
    }
    public class Link
    {
        public string Href { get; set; }
        public string Text { get; set; }
    }
    public enum NothingFoundReason : int
    {
        NoData = 0,
        QueryTooLong = 1,
        QueryTooShort = 2
    }
}