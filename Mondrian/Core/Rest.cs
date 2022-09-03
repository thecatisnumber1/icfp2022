using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class Rest
    {
        static string key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InRoZWNhdGlzbnVtYmVyMUBnb29nbGVncm91cHMuY29tIiwiZXhwIjoxNjYyMjgyNDMyLCJvcmlnX2lhdCI6MTY2MjE5NjAzMn0.YlIMvrYY1ARl--WvOVKRYzvlqHD0XumWfhQ5vyVpwPQ";

        public static void Upload(int problem, string solution)
        {
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
