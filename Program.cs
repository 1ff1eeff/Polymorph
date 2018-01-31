using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Polymorph
{
    class Program
    {
        static void Main(string[] args)
        {
            string src = "src";
            string outDir = "";
            if (outDir == "")
            {
                outDir = "output";
            }
            if (!(Directory.Exists(outDir))) {
                DirectoryInfo di = Directory.CreateDirectory(outDir);
            }
            Junkcode.ProcessDirectory(src, outDir);
            //File.WriteAllText(fileOut, lines);

            Console.ReadKey();
        }
       
    }
    class Junkcode
    {
        #region ProcessDirectory
        public static void ProcessDirectory(string src, string outDir) {
            
            if (File.Exists(src))
            {                
                File.WriteAllText(outDir + "\\" + src, Junkcode.ProcessFile(src)); 
                Console.WriteLine(src + " успешно обработан.");
            }
            else if (Directory.Exists(src))
            {                
                string supportedExtensions =
                    "*.cpp,*.cs,*.c,*.h,*.hpp,*.inl";
                foreach (string srcFileName in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories)
                    .Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower())))
                {
                    string pathOutput = outDir + (srcFileName.Remove(0, src.Length));
                    int index = pathOutput.IndexOf(Path.GetFileName(pathOutput));
                    string newPath = pathOutput.Substring(0, index);
                    DirectoryInfo di = Directory.CreateDirectory(newPath);
                    File.WriteAllText(pathOutput, Junkcode.ProcessFile(srcFileName));
                    Console.WriteLine(srcFileName + " успешно обработан.");   // ОЧЕНЬ ДОЛГО РАБОТАЕТ
                }
            }

        }
        #endregion

        #region ProcessFile
        public static string ProcessFile(string filename)
        {
            List<string> tags = SearchTags(filename);
            string lines = "";
            if (tags.Count != 0)
            {
                if (tags.Contains("junk")) {
                    try
                    {
                        using (StreamReader sr = new StreamReader(filename))
                        {
                            string line;
                            while (!sr.EndOfStream)
                            {
                                line = sr.ReadLine();
                                if (Regex.Match(line, @";$").Success)
                                {
                                    lines += line + Junkcode.Junk();
                                }
                                else
                                {
                                    lines += line;
                                }
                                if (line != "") lines += Environment.NewLine;
                            }                            
                        }
                    }
                    catch (FileNotFoundException ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            return lines;
        }
        #endregion

        #region SearchTags
        public static List<string> SearchTags(string filename)
        {
            List<string> tags = new List<string>();
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (Regex.Match(line, @"\[junk_enable /]").Success)
                        {
                            //Console.WriteLine(line);
                            tags.Add("junk");
                        }
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex);
            }
            return tags;
        }
        #endregion

        #region JUNK
        private static String[] availableTypes = new String[3] { "int", "char", "char*" }; // Типы переменной
        private const int length = 15; // Длина имени и значения переменной
        private const int countJunkLineMin = 5;  // Количество строк в одном экземпляре минимум
        private const int countJunkLineMax = 10; // Количество строк в одном экземпляре максимум
        private static Random random = new Random();

        // Создаёт строки содержащие инициализации переменных
        public static string Junk()
        {
            string finalStr = Environment.NewLine;
            int amountLines = random.Next(countJunkLineMin, countJunkLineMax);
            for (int i = 0; i < amountLines; i++)
            {
                string type = availableTypes[random.Next(0, 3)];

                const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                const string nameChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string numbers = "123456789";

                string name = new string(Enumerable.Repeat(nameChars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                string value;                
                if (type == "int")
                {
                    value = new string(Enumerable.Repeat(numbers, length).Select(s => s[random.Next(s.Length)]).ToArray());
                    finalStr += type + " " + name + " = " + value + ";";
                }
                if (type == "char")
                {
                    value = new string(Enumerable.Repeat(chars, 1).Select(s => s[random.Next(s.Length)]).ToArray());
                    finalStr += type + " " + name + " = \'" + value + "\';";
                }
                if (type == "char*")
                {
                    value = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
                    finalStr += type + " " + name + " = \"" + value + "\";";
                }                
                if (i < amountLines - 1)
                    finalStr += Environment.NewLine;
                
            }
            return finalStr;
        }
        #endregion
    }
}
