using Core;
using System.Text.Json;

namespace Mondrian
{
    public static class InitialConfigs
    {
        private static string? configDir = null;
        private static int? configCount = null;

        public static InitialConfig? GetInitialConfig(int num)
        {
            return LoadFile(LocationFor($"{num}.initial.json"));
        }

        public static InitialConfig? LoadFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            string text = File.ReadAllText(path);
            return JsonSerializer.Deserialize<InitialConfig>(text);
        }

        public static int InitialConfigCount()
        {
            return configCount ??= Directory.GetFiles(ConfigDirectory()).Length;
        }

        private static string ConfigDirectory()
        {
            string FindConfigDirectory()
            {
                bool ContainsScores(string dirName)
                {
                    var dirs = Directory.GetDirectories(dirName);
                    return (from dir in dirs select Path.GetFileName(dir)).Contains("InitialConfigs");
                }

                string dir = ".";
                while (!ContainsScores(dir))
                {
                    dir = Path.Join(dir, "..");

                    if (dir.Length > 100)
                    {
                        throw new Exception("Couldn't find Initial Configs");
                    }
                }

                return Path.Join(dir, "InitialConfigs");
            }

            return configDir ??= FindConfigDirectory();
        }

        private static string LocationFor(string filename)
        {
            return Path.Join(ConfigDirectory(), filename);
        }
    }
}
