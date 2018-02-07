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
            string src = "XFiles";
            string outDir = "";
            if (outDir == "")
            {
                outDir = "out";
            }
            if (!(Directory.Exists(outDir))) {
                DirectoryInfo di = Directory.CreateDirectory(outDir);
            }
            Junkcode.ProcessDirectory(src, outDir);
            Console.ReadKey();
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
                    //if (tags.Contains("swap")) readText = SwapLines(readText);                    
                    //if (tags.Contains("junk")) readText = Junk(readText);
                    if (tags.Contains("encrypt")) readText = Encrypt(readText);
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
        private static string Encrypt(string readText)
        {
            List<String> elements = readText.Split('\n').ToList();
            readText = "";
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Contains("\"") && !elements[i].Contains("include") && !elements[i].Contains("system") && !elements[i].Contains("XORENGINE.XorStr"))
                {
                    // code here
                    
                    string replacement = "ENCODED";
                    string pattern = "[\"][^\\\"]*[\"]";
                    Regex rgx = new Regex(pattern);

                    foreach (Match match in rgx.Matches(elements[i])) {
                        Console.WriteLine("Вхождение в строке " + (i + 1) + " в столбце " + match.Index + " : " + match.Value);

                        byte[] strByte = CryptEncrypt(match.Value, "m");
                        string hex = "";
                        for (int j = 0; j < strByte.Length; j++) {
                            hex += "\\x" + strByte[j].ToString("X2");
                        }
                        Console.WriteLine("Заменено на: " + hex);
                        
                        //Тут меняем текст!====================================================================<<<<<<<<<<<<<<<<<<<<<<<<<<
                    }

                    string result = rgx.Replace(elements[i], replacement);
                    Console.WriteLine("Исходная строка:    " + elements[i]);
                    Console.WriteLine("Шифрованная строка: " + result);
                    readText += result + "\n";
                    continue; 
                }
                readText += elements[i] + "\n";
            }
            Console.WriteLine("\nТекст:\n" + readText);
            //Console.WriteLine(CryptDecrypt(CryptEncrypt("gui.ini", "m"), "m"));

            return readText;
        }
        public static byte[] CryptEncrypt(String pText, String pKey)
        {
            byte[] txt = System.Text.Encoding.UTF8.GetBytes(pText);
            byte[] key = System.Text.Encoding.UTF8.GetBytes(pKey);
            byte[] res = new byte[pText.Length];

            for (int i = 0; i < pText.Length; i++)
            {
                res[i] = (byte)(txt[i] ^ key[i % key.Length]);
            }
            
            //System.Console.WriteLine("Исходная строка (String): " + pText);
            //System.Console.WriteLine("Ключ шифрования (String): " + pKey);
            //string tmp = "";
            //for (int i = 0; i < txt.Length; i++)
            //    tmp += txt[i] + " ";
            //System.Console.WriteLine("Исходная строка (byte[]): " + tmp);
            //tmp = "";
            //for (int i = 0; i < res.Length; i++)
            //    tmp += res[i] + " ";
            //System.Console.WriteLine("Шифрованная строка (byte[]): " + tmp);            

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

            //string tmp = "";
            //for (int i = 0; i < pText.Length; i++)
            //    tmp += pText[i] + " ";
            //System.Console.WriteLine("");
            //System.Console.WriteLine("Исходная строка для дешифрования (byte[]): " + tmp);
            //System.Console.WriteLine("Ключ дешифровки (String): " + pKey);
            //tmp = "";
            //for (int i = 0; i < result.Length; i++)
            //    tmp += result[i];
            //System.Console.WriteLine("Дешифрованная строка (byte[]): " + tmp);

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
