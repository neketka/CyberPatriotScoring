using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.IO.Compression;

namespace ScoringGenerator
{
    class Program
    {
        static void RaiseError(string err)
        {
            Console.WriteLine(err);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        public static void Main(string[] args)
        {
            string imgFolderPath = "";
            string imgPath = "";
            if (args.Length == 0)
            {
                Console.Write("Enter image folder path: ");
                imgFolderPath = Console.ReadLine();
            }
            else imgFolderPath = args[0];

            imgFolderPath = Path.GetFullPath(imgFolderPath);

            if (!Directory.Exists(imgFolderPath))
            {
                RaiseError("Given folder path does not exist!");
                return;
            }

            imgPath = imgFolderPath + "\\image.json";
            if (!File.Exists(imgPath))
            {
                RaiseError("Image file does not exist!");
                return;
            }

            JObject finalImage = new JObject();

            JsonReader reader = new JsonTextReader(new StreamReader(imgPath));
            JObject mainImageFile = JObject.Load(reader);

            JArray pip = new JArray();
            JObject checks = new JObject();
            JObject extChecks = new JObject();
            JObject defaults = new JObject();

            JObject imgData = new JObject();
            JArray imgDefaults = new JArray();
            JArray imgVulns = new JArray();
            JArray imgPens = new JArray();

            string platform = mainImageFile["platform"].Value<string>();

            SHA256Managed sha = new SHA256Managed();
            foreach (JProperty prop in ((JObject)mainImageFile["hashes"]).Properties())
            {
                byte[] data = File.ReadAllBytes(imgFolderPath + "\\" + prop.Value.Value<string>());
                data = sha.ComputeHash(data);
                string hash = "";
                foreach (byte b in data)
                    hash += b.ToString("x2");
                imgData.Add(prop.Name, hash);
            }

            foreach (JProperty prop in ((JObject)mainImageFile["files"]).Properties())
            {
                byte[] data = File.ReadAllBytes(imgFolderPath + "\\" + prop.Value.Value<string>());
                imgData.Add(prop.Name, Convert.ToBase64String(data));
            }

            foreach (string s in mainImageFile["modules"])
            {
                string modPath = imgFolderPath + "\\" + s + "\\module.json";
                if (!File.Exists(modPath))
                {
                    RaiseError("Module " + s + " does not exist!");
                    return;
                }

                JsonReader modReader = new JsonTextReader(new StreamReader(modPath));
                JObject module = JObject.Load(modReader);
                modReader.Close();

                JArray pipArr = module["pipModules"]["ubuntu"] as JArray;
                if (pipArr != null)
                {
                    foreach (string str in pipArr)
                        pip.Add(str);
                }

                JArray defaultsArr = module["defaults"] as JArray;
                if (defaultsArr != null)
                {
                    foreach (JObject def in defaultsArr)
                    {
                        if (!def.ContainsKey("platforms") || ((JArray)def["platforms"]).Values<string>().Contains(platform))
                        {
                            JObject newDef = new JObject();
                            defaults[def["name"].Value<string>()] = newDef;
                            newDef["src"] = File.ReadAllText(imgFolderPath + "\\" + s + "\\" + def["src"].Value<string>());
                        }
                    }
                }

                JArray checksArr = module["checks"] as JArray;
                if (checksArr != null)
                {
                    foreach (JObject ch in checksArr)
                    {
                        if (!ch.ContainsKey("platforms") || ((JArray)ch["platforms"]).Values<string>().Contains(platform))
                        {
                            JObject newCh = new JObject();
                            if (!ch.ContainsKey("extends"))
                            {
                                checks[ch["name"].Value<string>()] = newCh;
                                newCh["src"] = File.ReadAllText(imgFolderPath + "\\" + s + "\\" + ch["src"].Value<string>());
                            }
                            else
                            {
                                extChecks[ch["name"].Value<string>()] = newCh;
                                newCh["extends"] = ch["extends"];
                                newCh["params"] = ch.ContainsKey("params") ? ch["params"] : new JObject();
                                newCh["args"] = ch.ContainsKey("args") ? ch["args"] : new JObject();

                                List<string> refs = new List<string>();
                                foreach (JProperty prop in ((JObject)newCh["args"]).Properties())
                                    Parser.GetRefs(refs, prop.Value.Value<string>(), '$');

                                newCh["refs"] = new JArray(refs);
                            }
                        }
                    }
                }
            }

            foreach (JObject def in mainImageFile["defaults"])
            {
                List<string> refs = new List<string>();
                foreach (JProperty prop in ((JObject)def["args"]).Properties())
                    if (prop.Value.Type == JTokenType.String)
                        Parser.GetRefs(refs, prop.Value.Value<string>(), '$');
                def["refs"] = new JArray(refs);
                if (!def.ContainsKey("args"))
                    def["args"] = new JObject();
                imgDefaults.Add(def);
            }

            int maxPoints = 0;
            foreach (JObject vuln in mainImageFile["vulns"])
            {
                List<string> refs = new List<string>();
                int count = 0;
                string condStr = "";
                foreach (JObject check in vuln["checks"])
                {
                    foreach (JProperty prop in ((JObject)check["args"]).Properties())
                        if (prop.Value.Type == JTokenType.String)
                            Parser.GetRefs(refs, prop.Value.Value<string>(), '$');
                    if (!check.ContainsKey("args"))
                        check["args"] = new JObject();
                    condStr = count + " " + condStr + "& ";
                    ++count;
                }
                vuln["refs"] = new JArray(refs);

                if (vuln.ContainsKey("condition"))
                    vuln["condition"] = Parser.ToPostfix(vuln["condition"].Value<string>());
                else vuln["condition"] = condStr;

                maxPoints += vuln["points"].Value<int>();

                imgVulns.Add(vuln);
            }

            foreach (JObject pens in mainImageFile["penalties"])
            {
                List<string> refs = new List<string>();
                int count = 0;
                string condStr = "";
                foreach (JObject check in pens["checks"])
                {
                    foreach (JProperty prop in ((JObject)check["args"]).Properties())
                        if (prop.Value.Type == JTokenType.String)
                            Parser.GetRefs(refs, prop.Value.Value<string>(), '$');
                    if (!check.ContainsKey("args"))
                        check["args"] = new JObject();
                    condStr = count + " " + condStr + "& ";
                    ++count;
                }
                pens["refs"] = new JArray(refs);
                if (pens.ContainsKey("condition"))
                    pens["condition"] = Parser.ToPostfix(pens["condition"].Value<string>());
                else pens["condition"] = condStr;

                imgPens.Add(pens);
            }

            finalImage["pipModules"] = pip;
            finalImage["checks"] = checks;
            finalImage["extChecks"] = extChecks;
            finalImage["defaults"] = defaults;

            finalImage["imgData"] = imgData;
            finalImage["imgDefaults"] = imgDefaults;
            finalImage["imgVulns"] = imgVulns;
            finalImage["imgPens"] = imgPens;

            finalImage["scoringServer"] = mainImageFile["scoringServer"];
            finalImage["platform"] = mainImageFile["platform"];
            finalImage["pollingRate"] = mainImageFile["pollingRate"];
            finalImage["readme"] = File.ReadAllText(imgFolderPath + "\\" + mainImageFile["readme"].Value<string>());
            finalImage["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

            finalImage["totalPoints"] = maxPoints;
            //string imageFilePath = ".\\data\\" + platform + "\\scoring" + "\\imagefile.dat";
            string imageFilePath = imgFolderPath + "\\imagefile.dat";

            JsonWriter writer = new JsonTextWriter(new StreamWriter(imageFilePath, false));
            finalImage.WriteTo(writer);
            writer.Flush();
            writer.Close();
            /*
            ZipFile.CreateFromDirectory(".\\data\\" + platform, imgFolderPath + "\\ScoringEngine.zip");

            File.Delete(imageFilePath);*/
        }
    }
}
