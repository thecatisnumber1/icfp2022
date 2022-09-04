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
        public Stack<Core.Rectangle> Rects { get; set; }

        public static void SaveRects(string problemId, Stack<Core.Rectangle> rects, string filePath)
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

        public static (string ProblemId, Stack<Core.Rectangle> rects) RestoreRectsFromFile(string filePath)
        {
            Quicksave save = JsonSerializer.Deserialize<Quicksave>(File.ReadAllText(filePath));

            Stack<Core.Rectangle> r = save.Rects ?? new Stack<Core.Rectangle>();
            Stack<Core.Rectangle> toReturn = new Stack<Rectangle>();
            foreach (var rect in r)
            {
                toReturn.Push(rect); // Deal with stack deserializing in reverse order.
            }

            return (save.ProblemId, toReturn);
        }

        public static (string ProblemId, Stack<Core.Rectangle> rects, string filePath) RestoreMostRecentStackFromDirectory(string directory, int problemId)
        {
            if (!Directory.Exists(directory))
            {
                return (null, new Stack<Core.Rectangle>(), null);
            }
            string file = Directory.GetFiles(directory, $"{problemId}_*.json").LastOrDefault();

            if (file == null)
            {
                return (null, new Stack<Core.Rectangle>(), null);
            }

            (string ProblemId, Stack<Core.Rectangle> rects) = RestoreRectsFromFile(file);
            return (ProblemId, rects, file);
        }
    }
}