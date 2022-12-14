using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core
{


    public static class Rest
    {
        static string key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InRoZWNhdGlzbnVtYmVyMUBnb29nbGVncm91cHMuY29tIiwiZXhwIjoxNjYyNDU5OTcwLCJvcmlnX2lhdCI6MTY2MjM3MzU3MH0.OWOg7-oKEwcGfTlecWIs9PXgB-NzHfSO_ruKVRTco-Q";
        static Dictionary<int, int>? best = new Dictionary<int, int>();
        static Dictionary<int, int>? current = new Dictionary<int, int>();

        public static void CacheBests()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
                string rez;
                try
                {
                    var task = client.GetStringAsync("https://robovinci.xyz/api/submissions");
                    rez = task.Result;
                } 
                catch
                {
                    return;
                }
                var json = JsonConvert.DeserializeObject<Dictionary<String, List<Dictionary<String, dynamic>>>>(rez);
                if (!json.ContainsKey("submissions")) return;
                var list = json["submissions"];
                best = new Dictionary<int, int>();
                current = new Dictionary<int, int>();

                foreach (var i in list)
                {
                    int prob = (int)i["problem_id"];
                    int score = (int)i["score"];
                    if (!current.ContainsKey(prob))
                    {
                        current[prob] = score;
                    }

                    if(!best.ContainsKey(prob))
                    {
                        best[prob] = score;
                    }

                    if (best[prob] > score)
                    {
                        best[prob] = score;
                    }
                }
            }
        }

        public static void Upload(int problem, string solution, int score)
        {
            if (current.GetValueOrDefault(problem, int.MaxValue) < score)
            {
                Console.WriteLine($"Not submitting because {score} > {current.GetValueOrDefault(problem, int.MaxValue)} ({best.GetValueOrDefault(problem, int.MaxValue)}?)");
                return;
            }

            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);

                formData.Add(new StringContent(solution), "file", "file");
                var task = client.PostAsync($"https://robovinci.xyz/api/problems/{problem}", formData);
                var rez = task.Result;
                Console.WriteLine(rez.Content.ReadAsStringAsync().Result);
            }
        }

        public static int BestForProblem(int problem)
        {
            return current.GetValueOrDefault(problem, int.MaxValue);
        }
    }
}
