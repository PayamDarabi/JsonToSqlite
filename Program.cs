using ConsoleApp1;
using JsonToSqlite.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JsonToSqlite
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string filesFormat = "*.json";
            string filesPath = @"C:\Users\MyPC\Desktop\New folder";
            using (var sqlite2 = new SQLiteConnection(@"Data Source=C:\Users\MyPC\Desktop\ExpertsDicNew - Copy.db"))
            {
                sqlite2.Open();
                string[] filePaths = Directory.GetFiles(filesPath, filesFormat);

                foreach (var file in filePaths)
                {
                    // Get File Name And Set It To SheetName
                    string majorName = Path.GetFileNameWithoutExtension(file);
                    Console.WriteLine("Reading File: {0}", majorName);

                    using (StreamReader r = new StreamReader(file))
                    {
                        string jsonContent = r.ReadToEnd();

                        //JsonObject: Root Object Of Json
                        var jsonObj = JsonConvert.DeserializeObject<JsonObject>(jsonContent);

                        List<Vocab> vocabsList = ToList(jsonObj.Vocabs);

                        //ReadMajorsTitle
                        string[] majorsSplitedTitle = majorName.Split('-');
                        string majorPersianTitle = majorsSplitedTitle[0];
                        string majorEnglishTitle = majorsSplitedTitle[1];
                        Major major = new Major
                        {
                            EnglishTitle = EnglishStringEditor(majorEnglishTitle),
                            PersianTitle = PersianStringEditor(majorPersianTitle)
                        };
                        string sql = "Insert Into Majors (EnglishTitle, PersianTitle) Values(@EnglishTitle, @PersianTitle); " +
                                     "SELECT Id FROM Majors ORDER BY Id DESC";
                        int lastMajorId = 0;
                        using (var cmd = sqlite2.CreateCommand())
                        {
                            cmd.CommandText = sql;
                            cmd.Parameters.AddWithValue("@EnglishTitle", major.EnglishTitle);
                            cmd.Parameters.AddWithValue("@PersianTitle", major.PersianTitle);
                            lastMajorId = Convert.ToInt32(cmd.ExecuteScalar());
                            Console.WriteLine("Inserted Major In Db: {0}", major.EnglishTitle + ", " + major.PersianTitle);
                        }
                        foreach (var item in vocabsList)
                        {
                            int lastVocabId = 0;
                            sql = "Insert Into Vocabs(English, Persian) Values(@English, @Persian);" +
                                     " SELECT Id FROM Vocabs ORDER BY Id DESC";
                            using (var cmd = sqlite2.CreateCommand())
                            {
                                cmd.CommandText = sql;
                                cmd.Parameters.AddWithValue("@English", item.English);
                                cmd.Parameters.AddWithValue("@Persian", item.Persian);
                                lastVocabId = Convert.ToInt32(cmd.ExecuteScalar());
                                Console.WriteLine("Inserted Vocab In Db: {0}", item.English + ", " + item.Persian);
                                sql = "Insert Into MajorsVocabs(MajorId, VocabId) Values(@MajorId, @VocabId)";

                                cmd.CommandText = sql;
                                cmd.Parameters.AddWithValue("@MajorId", lastMajorId);
                                cmd.Parameters.AddWithValue("@VocabId", lastVocabId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }
        public static List<Vocab> ToList(List<Vocab> vocabsList)
        {
            var vocabs = new List<Vocab>();
            vocabsList = vocabsList.OrderBy(x => x.English).ToList();
            Regex regex = new Regex("[\u0600-\u06ff]|[\u0750-\u077f]|[\ufb50-\ufc3f]|[\ufe70-\ufefc]");

            foreach (var vocab in vocabsList)
            {
                string newEnglish = vocab.English;
                vocab.English = (newEnglish.FirstCharToUpper());
                if (regex.IsMatch(vocab.English))
                    vocabs.Add(new Vocab { Persian = PersianStringEditor(vocab.English), English = EnglishStringEditor(vocab.Persian) });

                else
                    vocabs.Add(new Vocab { English = EnglishStringEditor(vocab.English), Persian = PersianStringEditor(vocab.Persian) });
            }
            return vocabs;
        }
        public static string PersianStringEditor(string persian)
        {
            var newPersian = persian.Trim().Replace("'", "`");
            newPersian = newPersian.Replace('ي', 'ی');
            newPersian = newPersian.Replace('ك', 'ک');
            return newPersian;
        }
        public static string EnglishStringEditor(string english)
        {
            var newEnglish = english.Trim().Replace("'", "`");
            return newEnglish;
        }
    }
}
