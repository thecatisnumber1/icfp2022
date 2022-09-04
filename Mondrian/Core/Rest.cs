﻿using System;
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
        static string key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InRoZWNhdGlzbnVtYmVyMUBnb29nbGVncm91cHMuY29tIiwiZXhwIjoxNjYyMjgyNDMyLCJvcmlnX2lhdCI6MTY2MjE5NjAzMn0.YlIMvrYY1ARl--WvOVKRYzvlqHD0XumWfhQ5vyVpwPQ";
        static Dictionary<int, int>? best;
        static Dictionary<int, int>? current;

        public static void CacheBests()
        {
            best = new Dictionary<int, int>();
            current = new Dictionary<int, int>();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
                var task = client.GetStringAsync("https://robovinci.xyz/api/submissions");
                var rez = task.Result;
                var json = JsonConvert.DeserializeObject<Dictionary<String, List<Dictionary<String, dynamic>>>>(rez);
                var list = json["submissions"];

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
            if (current[problem] < score)
            {
                Console.WriteLine($"Not submitting because {score} > {current[problem]} ({best[problem]}?)");
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
    }
}
