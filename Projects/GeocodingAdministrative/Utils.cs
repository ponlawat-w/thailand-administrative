using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetTopologySuite.Geometries;

namespace ThailandAdministrative.GeocodingAdministrative {
  public static class Utils {
    private static void CheckTambonShapeFileFromCSV() {
      HashSet<string> notFoundList = new HashSet<string>();
      AdministrativeCsvData[] rows = AdministrativeCsvReader.Read();
      AdministrativeShapeFile administrativeShapeFile = new AdministrativeShapeFile();
      Dictionary<string, Dictionary<string, Dictionary<string, Geometry>>> administrativeDict = administrativeShapeFile.GetAdministrativeDict();

      HashSet<string> uniqueTambons = new HashSet<string>();
      foreach (AdministrativeCsvData row in rows) {
        string province = row.GetProvince();
        string amphoe = row.GetAmphoe();
        string tambon = row.GetTambon();
        uniqueTambons.Add($"{tambon}/{amphoe}/{province}");
      }

      foreach (string uniqueTambon in uniqueTambons) {
        string[] tokens = uniqueTambon.Split('/');
        string tambon = tokens[0];
        string amphoe = tokens[1];
        string province = tokens[2];
        Console.Write($"{tambon} {amphoe} {province}...");
        if (AdministrativeShapeFile.ExistsInAdministrativeDict(administrativeDict, province, amphoe, tambon)) {
          Console.WriteLine("Yes");
        } else {
          Console.WriteLine("No");
          notFoundList.Add($"{tambon},{amphoe},{province}");
        }
      }

      Console.WriteLine($"Not found = {notFoundList.Count()}");

      Directory.CreateDirectory("./out");
      StreamWriter writer = new StreamWriter("./out/not-found.csv");
      writer.WriteLine("ตำบล,อำเภอ,จังหวัด");
      foreach (string notFoundItem in notFoundList) {
        writer.WriteLine(notFoundItem);
      }
      writer.Close();
    }
    

    private static void CheckOSMNameFromCSV() {
      AdministrativeShapeFile shp = new AdministrativeShapeFile();
      AdministrativeCsvData[] rows = AdministrativeCsvReader.Read();

      StreamWriter writer = new StreamWriter("./out/osm-no-name.csv");
      writer.WriteLine("m,t,a,p,why");

      Console.WriteLine("Ready");
      int i = 0;
      foreach (AdministrativeCsvData row in rows) {
        if (++i % 1000 == 0) {
          Console.WriteLine($"{i} rows processed.");
        }

        Polygon tambonPolygon = (Polygon)shp.GetGeometry(row.GetProvince(), row.GetAmphoe(), row.GetTambon());
        if (tambonPolygon == null) {
          writer.WriteLine($"{row.MName},{row.GetTambon()},{row.GetAmphoe()},{row.GetProvince()},NO_POLYGON");
          continue;
        }

        ThailandOsmPbfReader osmReader = new ThailandOsmPbfReader();
        osmReader.Filter(tambonPolygon);
        if (!osmReader.ExistsElementWithName(row.MName)) {
          writer.WriteLine($"{row.MName},{row.GetTambon()},{row.GetAmphoe()},{row.GetProvince()},NO_RESULTS");
        }
        osmReader.Close();
      }

      writer.Close();

      Console.WriteLine("OK");
    }
  }
}
