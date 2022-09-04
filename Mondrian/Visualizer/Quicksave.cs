using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Core;

namespace Visualizer
{
    public class Quicksave
    {
        public string ProblemId { get; set; }
        public List<Core.Rectangle> Rects { get; set; }

        public static void SaveRects(string problemId, List<Core.Rectangle> rects, string filePath)
        {
            var save = new Quicksave
            {
                ProblemId = problemId,
                Rects = rects
            };
            string contents = JsonSerializer.Serialize(save);
            
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            File.WriteAllText(filePath, contents);
        }

        public static (string ProblemId, List<Core.Rectangle> rects) RestoreRectsFromFile(string filePath)
        {
            Quicksave save = JsonSerializer.Deserialize<Quicksave>(File.ReadAllText(filePath));

            List<Core.Rectangle> r = save.Rects ?? new List<Core.Rectangle>();

            return (save.ProblemId, r);
        }

        public static (string ProblemId, List<Core.Rectangle> rects, string filePath) RestoreMostRecentStackFromDirectory(string directory, int problemId)
        {
            if (!Directory.Exists(directory))
            {
                return (null, new List<Core.Rectangle>(), null);
            }
            string file = Directory.GetFiles(directory, $"{problemId}_*.json").LastOrDefault();

            if (file == null)
            {
                return (null, new List<Core.Rectangle>(), null);
            }

            (string ProblemId, List<Core.Rectangle> rects) = RestoreRectsFromFile(file);
            return (ProblemId, rects, file);
        }
    }
}