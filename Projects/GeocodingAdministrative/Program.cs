using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using NetTopologySuite.Geometries;

namespace ThailandAdministrative.GeocodingAdministrative {
  public static class Program {
    public static void Main(string[] args) {
      string resultPath = "./out/list-all.csv";
      Console.WriteLine("Starting writer...");
      using (StreamWriter writer = new StreamWriter(resultPath))
      using (CsvWriter csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture)) {
        csvWriter.WriteHeader<AdministrativeCsvData>();
        csvWriter.NextRecord();

        Console.WriteLine("Getting TCodes...");
        AdministrativeCsvData[] administrativeRecords = AdministrativeCsvReader.Read();
        string[] tCodes = AdministrativeCsvReader.GetUniqueTCodes().ToArray();

        int i = 0;
        foreach (string tCode in tCodes) {
          TambonOsmElementGroup osmGroup = new TambonOsmElementGroup(tCode);
          Console.Write($"Processing {tCode} with {osmGroup.Count()} elements ({++i} from {tCodes.Length})...");
          AdministrativeCsvData[] communities = administrativeRecords.Where(r => r.TCode == tCode).ToArray();
          int success = 0;
          foreach (AdministrativeCsvData community in communities) {
            Point p = osmGroup.Geocode(community, 0.4);
            if (p != null) {
              community.SetGeometry(p);
              success++;
            }
            community.PrepareFinalOutput();
            csvWriter.WriteRecord<AdministrativeCsvData>(community);
            csvWriter.NextRecord();
          }
          Console.WriteLine($"{success} success, {communities.Length - success} failed");
          csvWriter.Flush();
        }

        Console.WriteLine("Closing stream...");
      }

      Console.WriteLine("Finished");
    }
  }
}
