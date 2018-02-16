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
            string src = "";
            string outDir = "";

            if (args.Length == 0)
            {
                src = "src";
                outDir = "out";
                Console.WriteLine("Не заданы аргументы командной строки. Установлены значения по умолчанию.\n" +
                    "Ищем исходные данные в папке \"src\"");
            }
            if (args.Length == 1)
            {
                src = args[0];
                outDir = "out";
                //Console.WriteLine(src);
            }
            if (args.Length == 2)
            {
                src = args[0];
                outDir = args[1];
                //Console.WriteLine(src + " " + outDir);
            }
            
            if (!(Directory.Exists(outDir))) {
                DirectoryInfo di = Directory.CreateDirectory(outDir);
            }
            Junkcode.ProcessDirectory(src, outDir);
            //Console.ReadKey();
        }
    }
    class Junkcode
    {
        #region ProcessDirectory
        public static string currentFileName = "";

        public static void ProcessDirectory(string src, string outDir) {
            
            if (File.Exists(src))
            {                
                File.WriteAllText(outDir + "\\" + src, ProcessFile(src)); 
                Console.WriteLine(src + " успешно обработан.");
            }
            else if (Directory.Exists(src))
            {                
                string supportedExtensions =
                    "*.h,*.hpp,*.cpp";
                foreach (string srcFileName in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories)
                    .Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower())))
                {
                    string pathOutput = outDir + (srcFileName.Remove(0, src.Length));
                    int index = pathOutput.IndexOf(Path.GetFileName(pathOutput));
                    string newPath = pathOutput.Substring(0, index);
                    currentFileName = srcFileName;
                    DirectoryInfo di = Directory.CreateDirectory(newPath);
                    File.WriteAllText(pathOutput, ProcessFile(srcFileName));
                    Console.WriteLine(srcFileName + " успешно обработан.");   
                }
            }
        }
        #endregion

        #region ProcessFile
        public static string ProcessFile(string path)
        {           
            List<string> tags = SearchTags(path);
            string readText = "";
            // Файл в переменную
            try
            {
                readText = File.ReadAllText(path);
                // Операции с содержимым файла
                if (tags.Count != 0)
                {
                    if (tags.Contains("encrypt")) readText = Encrypt(readText);
                    if (tags.Contains("swap")) readText = SwapLines(readText);                    
                    if (tags.Contains("junk")) readText = Junk(readText);                    
                }             
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex);
            }
            return readText;
        }
        #endregion

        #region Encrypt

        public static char pKey = 'm';
        public static int index = 0;        

        static string EncodeText(Match m) {          

            byte[] strByte = CryptEncrypt(m.Groups[1].Value, pKey);
            string hex = "";
            for (int j = 0; j < strByte.Length; j++)
            {
                hex += "\\x" + strByte[j].ToString("X2");
            }
            int size = strByte.Length;            
            hex = "XORENGINE.XorStr(" + index + ", " + size + ", \"" + hex + "\")";
            index++;
            return hex;
        }
        
        private static string Encrypt(string readText)
        {
            List<String> elements = readText.Split('\n').ToList();
            readText = "";
            bool encryptEnable = false;
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Contains("[enc_string_disable /]"))
                    encryptEnable = false;
                if (elements[i].Contains("[enc_string_enable /]"))
                    encryptEnable = true;
                if (encryptEnable)
                    if (elements[i].Contains("\"") 
                        && !elements[i].Contains("include") 
                        && !elements[i].Contains("system") 
                        && !elements[i].Contains("XORENGINE.XorStr"))
                    {
                        // code here                           
                        Regex rx = new Regex("[\"]([^\\\"]*)[\"]");
                        string result = rx.Replace(elements[i], new MatchEvaluator(EncodeText));                    
                        readText += result + "\n";
                        continue; 
                    }
                readText += elements[i] + "\n";
            }

            //Console.WriteLine(readText);
            return readText;
        }

        public static byte[] CryptEncrypt(String pText, char pKey)
        {
            byte[] txt = System.Text.Encoding.UTF8.GetBytes(pText);
            //byte[] key = System.Text.Encoding.UTF8.GetBytes(pKey);
            byte[] res = new byte[pText.Length];

            for (int i = 0; i < pText.Length; i++)
            {
                //res[i] = (byte)(txt[i] ^ key[i % key.Length]);
                res[i] = (byte)(txt[i] ^ ((pKey + i) % 0xFF));
            }

            return res;
        }

        public static String CryptDecrypt(byte[] pText, String pKey)
        {
            byte[] res = new byte[pText.Length];
            byte[] key = System.Text.Encoding.UTF8.GetBytes(pKey);           

            for (int i = 0; i < pText.Length; i++)
            {
                res[i] = (byte)(pText[i] ^ key[i % key.Length]);
            }
            string result = System.Text.Encoding.UTF8.GetString(res);

            return result;
        }
        #endregion

        #region SearchTags
        public static List<string> SearchTags(string path)
        {
            List<string> tags = new List<string>();
            try
            {
                string readText = File.ReadAllText(path);
                if (readText.Contains("junk_enable")) tags.Add("junk");
                if (readText.Contains("swap_lines")) tags.Add("swap");
                if (readText.Contains("enc_string_enable")) tags.Add("encrypt");
                else tags.Add("clear");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex);
            }
            return tags;
        }
        #endregion

        #region Junk
        private static String[] availableTypes = new String[3] { "int", "char", "char*" }; // Типы переменной
        private const int length = 15; // Длина имени и значения переменной
        private const int countJunkLineMin = 5;  // Количество строк в одном экземпляре минимум
        private const int countJunkLineMax = 10; // Количество строк в одном экземпляре максимум
        private static Random random = new Random();

        public static string Junk(string readText)
        {            
            List<String> elements = readText.Split('\n').ToList();                        
            readText = "";
            bool junkEnable = true;
            for (int i = 0; i < elements.Count; i++){
                if (!String.IsNullOrWhiteSpace(elements[i])){
                    readText += "\n";
                    readText += elements[i];
                    if (elements[i].Contains("[junk_disable /]"))
                        junkEnable = false;
                    else
                        if (elements[i].Contains("[junk_enable /]"))
                            junkEnable = true;
                    if (junkEnable)
                        if (elements[i].Contains(";") && !elements[i].Contains("for ") && !elements[i].Contains("for(") && !elements[i].Contains("break;"))
                            readText += MakeJunk();               
                }                
            }
            return readText;
        }

        public static string MakeJunk()
        {
            string finalStr = "";
            int amountLines = random.Next(countJunkLineMin, countJunkLineMax);
            for (int i = 0; i < amountLines; i++)
            {
                string type = availableTypes[random.Next(0, 3)];
                const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                const string nameChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string numbers = "123456789";
                string name = new string(Enumerable.Repeat(nameChars, length).Select(s => s[random.Next(s.Length)]).ToArray());
                string value = "";

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
                    finalStr += '\n';
            }

            return finalStr;
        }
        #endregion

        #region SwapLines
        public static string SwapLines(string readText)
        {
            List<String> elements = readText.Split('\n').ToList<String>();
            List<String> linesToSwap = new List<String>();
            // Ищем строки для свопа во всём файле
            for (int i = 0; i < elements.Count; i++)
            {
                if (Regex.Match(elements[i], @"//\[swap_lines]").Success)
                {
                    int start = i + 1;
                    while (i < elements.Count - 1 && !Regex.Match(elements[i + 1], @"//\[/swap_lines]").Success)
                    {
                        linesToSwap.Add(elements[i + 1]);
                        i++;
                        if(i >= elements.Count - 1)
                        {
                            Console.WriteLine("Аварийное завершение!\nНе найден закрывающий тэг /swap_lines в файле: " + currentFileName);
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                    }
                    int stop = i;                                     
                    var rnd = new Random();
                    List<String> result = linesToSwap.OrderBy(item => random.Next()).ToList<String>();   
                    
                    foreach (string line in result)
                    {
                        elements.RemoveAt(start);
                        elements.Insert(start++, line);
                    }                       
                    linesToSwap.Clear();
                }               
            }
            readText = "";
            foreach (string line in elements)
                readText += line + "\n";
            return readText;
        }
        #endregion
    }
}
