using System;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using protectionUtils;
using System.Linq;

namespace consoleNonDi
{
    class Program
    {
        public static ProtectionNonDi protector;
        static void Main(string[] args)
        {
            protector = new ProtectionNonDi(new ProtectionConstants());
            //ProtectConfigurationFile("data/appsettings.json", "data/appsettings.json");
            //UnProtectConfigurationFile("data/appsettings.json", "data/appsettings.json");
            //UnProtectConfigurationFile("protectedConfiguration.json", "unprotectedConfiguration.json");

            ReadAndWriteValueFromJsonFile("data/appsettings.json","");
        }

        private static void ReadAndWriteValueFromJsonFile(string inputFileName, string jsonPath)
        {
            var innehall = ReadFileToString(inputFileName);
            //var varde = GetValueFromJson(innehall, "AzureAd:Instance");
            var varde = SetValueInJson(innehall, "AzureAd:Instance", true);
            Console.WriteLine($"Varde: {varde}");
        }

        private void UseJsonConfigurationFileParser()
        {
            using (var fs = File.OpenRead("konfiguration.json"))
            {
                var res = JsonConfigurationFileParser.Parse(fs);
                foreach(var pair in res)
                {
                    Console.WriteLine($"Key: {pair.Key}; Value: {pair.Value}");
                }
            }
        }

        private static void ProtectConfigurationFile(string inputFileName, string outputFileName)
        {
            //var protector = new ProtectionNonDi(new ProtectionConstants());
            string konfJson;

            using (var fs = File.OpenRead(inputFileName))
            using ( var tr = new StreamReader(fs))
            {
                var konfStr = tr.ReadToEnd();
                var konf = JsonConvert.DeserializeObject<dynamic>(konfStr);
                string value = konf["AzureAd"]["Instance"];
                konf["AzureAd"]["Instance"] = protector.Protect(value);

                konfJson = JsonConvert.SerializeObject(konf, Formatting.Indented);
            }
            Console.WriteLine(konfJson);
            WriteToFile(outputFileName, konfJson);
        }

        private static void UnProtectConfigurationFile(string inputFileName, string outputFileName)
        {
            var protector = new ProtectionNonDi(new ProtectionConstants());
            string konfJson;

            var konfStr = ReadFileToString(inputFileName);
            var konf = JsonConvert.DeserializeObject<dynamic>(konfStr);
            string value = konf["AzureAd"]["Instance"];
            konf["AzureAd"]["Instance"] = protector.Unprotect(value);

            konfJson = JsonConvert.SerializeObject(konf, Formatting.Indented);

            Console.WriteLine(konfJson);
            WriteToFile(outputFileName, konfJson);
        }
        private static void ReadAndWriteConfigurationFile()
        {
            using (var fs = File.OpenRead("konfiguration.json"))
            using ( var tr = new StreamReader(fs))
            using(var reader = new JsonTextReader(tr))
            {
                while(reader.Read())
                {
                    if(reader.Value != null)
                    {
                        Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
                    }
                    else
                    {
                        Console.WriteLine("Token: {0}", reader.TokenType);                        
                    }
                }
            }
        }

        private static void WriteProtectedResultToFile()
        {
            var protector = new ProtectionNonDi(new ProtectionConstants());

            var stringToProtect = "holaQueTal";
            var protectedString = protector.Protect(stringToProtect);
            Console.WriteLine($"String to protect: {stringToProtect}");
            Console.WriteLine($"Protected string: {protectedString}");

            WriteToFile("resultat.txt", stringToProtect, true);

        }

        private static string ReadFileToString(string inputFileName)
        {
            string content;

            using (var fs = File.OpenRead(inputFileName))
            using ( var tr = new StreamReader(fs))
            {
                content = tr.ReadToEnd();
            }

            return content;
        }

        private static void WriteToFile(string fileName, string message, bool addLabel = false)
        {
            using (var fs = File.Create(fileName))
            using(var writer = new StreamWriter(fs))
            {
                if (addLabel)
                {
                    writer.WriteLine($"String to protect: {message}");
                }
                else
                {
                    writer.WriteLine(message);
                }
            }
        }

        private static string GetValueFromJson(string json, string jsonPath)
        {
            var jsonObj = JsonConvert.DeserializeObject<dynamic>(json);
            var jsonPaths = jsonPath.Split(":");
            return GetValueFromJsonObject(jsonObj, jsonPaths);
        }

        private static string GetValueFromJsonObject(dynamic obj, string[] paths)
        {
            if(paths.Count() > 1)
            {
                var newPaths = paths.Where((ValueTuple, index)=> index != 0).ToArray();
                return GetValueFromJsonObject(obj[paths[0]],  newPaths);
            }
            return obj[paths[0]];
        }

        private static string SetValueInJson(string json, string jsonPath, bool protect)
        {
            var jsonObj = JsonConvert.DeserializeObject<dynamic>(json);
            var jsonPaths = jsonPath.Split(":");
            SetValueInJsonObject(jsonObj, jsonPaths, protect);
            return JsonConvert.SerializeObject(jsonObj);
        }

        private static void SetValueInJsonObject(dynamic obj, string[] paths, bool protect)
        {
            if(paths.Count() > 1)
            {
                var newPaths = paths.Where((ValueTuple, index)=> index != 0).ToArray();
                SetValueInJsonObject(obj[paths[0]],  newPaths, protect);
            }
            else
            {
                var oldValue = obj[paths[0]].ToString();
                var value = protect ? protector.Protect(oldValue) : protector.Unprotect(oldValue);
                obj[paths[0]] = value;
            }
        }


    }
}
