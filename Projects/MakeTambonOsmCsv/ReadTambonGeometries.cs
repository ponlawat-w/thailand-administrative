using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using ThailandAdministrative.GeocodingAdministrative;

namespace ThailandAdministrative.MakeTambonOsmCsv {
  public class TambonData {
    public string TCode;
    public string Tambon;
    public string Amphoe;
    public string Province;
    public Geometry Geometry;

    public TambonData(AdministrativeCsvData csvRow, Geometry geometry) {
      this.TCode = csvRow.TCode;
      this.Tambon = csvRow.GetTambon();
      this.Amphoe = csvRow.GetAmphoe();
      this.Province = csvRow.GetProvince();
      this.Geometry = geometry;
    }
  }

  public static class ReadTambonGeometries {
    public static TambonData[] GetGeometries() {
      List<TambonData> results = new List<TambonData>();
      AdministrativeShapeFile administrativeShapeFile = new AdministrativeShapeFile();
      Console.WriteLine("Reading administrative CSV file...");
      AdministrativeCsvData[] rows = AdministrativeCsvReader.Read();
      rows = rows.Select(row => row.TCode).Distinct()
        .Select(tCode => rows.Where(row => row.TCode == tCode).First())
        .ToArray();
      
      Console.WriteLine("Making shapefile dictionary...");
      Dictionary<string, Dictionary<string, Dictionary<string, Geometry>>> polygonDict = administrativeShapeFile.GetAdministrativeDict();

      int i = 0;
      foreach (AdministrativeCsvData row in rows) {
        if (++i % 100 == 0) {
          Console.Write($"\rGetting polygon {i} from {rows.Length}...");
        }
        Geometry geometry = AdministrativeShapeFile.GetGeometryFromDict(polygonDict, row.GetProvince(), row.GetAmphoe(), row.GetTambon());
        
        if (geometry == null) {
          throw new Exception($"Geometry for {row.GetTambon()} {row.GetAmphoe()} {row.GetProvince()} not found.");
        }
        
        results.Add(new TambonData(row, geometry));
      }
      Console.WriteLine();

      return results.ToArray();
    }
  }
}
