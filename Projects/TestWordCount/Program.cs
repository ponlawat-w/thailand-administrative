using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThailandAdministrative.GeocodingAdministrative;
using ThaiStringTokenizer;

namespace ThailandAdministrative {
  public static class Program {
    public static void Main(string[] args) {
      AdministrativeCsvData[] rows = AdministrativeCsvReader.Read();
      ThaiTokenizer tokenizer = new ThaiTokenizer(null, MatchingMode.Longest);

      Dictionary<string, int> wordCounts = new Dictionary<string, int>();

      int i = 0;
      foreach (AdministrativeCsvData row in rows) {
        List<string> words = tokenizer.Split(row.MName);
        foreach (string word in words) {
          if (!wordCounts.ContainsKey(word)) {
            wordCounts[word] = 1;
          } else {
            wordCounts[word]++;
          }
        }
        if (++i % 100 == 0) {
          Console.WriteLine($"{i.ToString()} entries processed.");
        }
      }

      wordCounts = wordCounts.OrderByDescending(x => x.Value).ToArray().ToDictionary(x => x.Key, x => x.Value);

      StreamWriter writer = new StreamWriter("./out/wordcounts.csv");
      writer.WriteLine("word,count");
      foreach (KeyValuePair<string, int> result in wordCounts) {
        writer.WriteLine($"\"{result.Key}\",{result.Value.ToString()}");
      }

      writer.Close();
      Console.WriteLine("OK");
    }
  }
}
