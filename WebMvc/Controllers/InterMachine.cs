using Newtonsoft.Json;
using Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using WebMvc.Models;

namespace WebMvc.Controllers
{
    public class InterMachine
    {
        public static void IndexLinks(string node_url, List<Link> links)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(node_url);

                var content = new StringContent(Compress(JsonConvert.SerializeObject(links)), Encoding.UTF8);
                var result = client.PostAsync("/Node/IndexLinks", content).Result;
                var t = result.Content.ReadAsStringAsync().Result;
            }
        }
        public static void NodePutSynonyms(string node_url, SynonymsData data)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(node_url);

                var content = new StringContent(Compress(JsonConvert.SerializeObject(data)), Encoding.UTF8);
                var result = client.PostAsync("/Node/PutSynonyms", content).Result;
                var t = result.Content.ReadAsStringAsync().Result;
            }
        }

        //public static void NodeIndexThese(string node_url, List<string> links)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(node_url);

        //        var content = new StringContent(Compress(JsonConvert.SerializeObject(links)), Encoding.UTF8);
        //        var result = client.PostAsync("/Node/IndexThese", content).Result;
        //        var t = result.Content.ReadAsStringAsync().Result;
        //    }
        //}

        //public static void MasterProceedResults(string master_url, List<CrawlResult> results)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(master_url);
        //        var content = new StringContent(Utils.Util.Compress(JsonConvert.SerializeObject(results)), Encoding.UTF8);
        //        var result = client.PostAsync("/Master/MasterProceedResults", content).Result;
        //        var t = result.Content.ReadAsStringAsync().Result;
        //    }
        //}

        //public static int GetNodesLinksCount(string node_url)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(node_url);
        //        var res = client.GetAsync("/Node/GetLinkNumber").Result;
        //        return Convert.ToInt32(res.Content.ReadAsStringAsync().Result);
        //    }
        //}

        //public static string GetNodesStats(string node_url)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(node_url);
        //        var res = client.GetAsync("/Node/GetStats").Result;
        //        return res.Content.ReadAsStringAsync().Result;
        //    }
        //}

        public static string SearchNode(string node_url, SearchData data)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(node_url);
                var content = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("data_json", JsonConvert.SerializeObject(data))
            });
                var result = client.PostAsync("/Node/SearchNode", content).Result;
                return result.Content.ReadAsStringAsync().Result;
            }
        }

        public static string GetNodesLog(string node_url)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(node_url);
                var result = client.GetAsync("/Node/GetNodesLog").Result;
                return result.Content.ReadAsStringAsync().Result;
            }
        }
        public static string GetNodesSlow(string node_url)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(node_url);
                var result = client.GetAsync("/Node/GetNodesSlow").Result;
                return result.Content.ReadAsStringAsync().Result;
            }
        }

        public static string GetNodesErrors(string node_url)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(node_url);
                var result = client.GetAsync("/Node/GetNodesErrors").Result;
                return result.Content.ReadAsStringAsync().Result;
            }
        }

        public static string Compress(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            byte[] buffer = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream();
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;
            //MemoryStream outStream = new MemoryStream();

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return Convert.ToBase64String(gzBuffer);
        }

        public static string Decompress(string compressedText)
        {
            if (string.IsNullOrEmpty(compressedText))
                return "";

            byte[] gzBuffer = Convert.FromBase64String(compressedText);
            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }
        public static string CalculateMD5Hash(string input)
        {
            var md5 = MD5.Create();
            byte[] inputBytes = Encoding.Unicode.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}