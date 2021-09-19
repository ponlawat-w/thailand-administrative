using System;
using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Geometries;
using OsmSharp;
using OsmSharp.Geo;
using OsmSharp.Streams;
using ThailandAdministrative.GeocodingAdministrative;

namespace ThailandAdministrative.MakeTambonOsmPbf {

  public static class Program {
    private static int AddFilteredSourceToTarget(OsmStreamSource source, PBFOsmStreamTarget target, Geometry geometry) {
      int n = 0;
      foreach (OsmGeo element in source) {
        if (element.Type == OsmGeoType.Node) {
          target.AddNode((Node)element);
          n++;
        } else if (element.Type == OsmGeoType.Way) {
          target.AddWay((Way)element);
          n++;
        } else if (element.Type == OsmGeoType.Relation) {
          target.AddRelation((Relation)element);
          n++;
        }
      }
      return n;
    }

    public static void Main(string[] args) {
      HashSet<string> processedTCode = new HashSet<string>();
      AdministrativeShapeFile administrativeShapeFile = new AdministrativeShapeFile();
      AdministrativeCsvData[] rows = AdministrativeCsvReader.Read();

      Directory.CreateDirectory("./out/tambon-osm");

      int i = 0;
      foreach (AdministrativeCsvData row in rows) {
        i++;
        if (processedTCode.Contains(row.TCode)) {
          continue;
        }
        processedTCode.Add(row.TCode);

        Console.Write($"{String.Format("{0:0.00}", (double)i / rows.Length * 100)}% - {row.TCode} {row.GetTambon()} {row.GetAmphoe()} {row.GetProvince()}...");

        string targetPath = $"./out/tambon-osm/{row.TCode}.osm.pbf";
        if (File.Exists(targetPath)) {
          Console.WriteLine("Skipped - output already exists");
          continue;
        }

        Geometry geometry = administrativeShapeFile.GetGeometry(row.GetProvince(), row.GetAmphoe(), row.GetTambon());
        if (geometry == null) {
          Console.WriteLine("Failed - No shp");
          continue;
        }
        if (!(geometry is Polygon)) {
          Console.WriteLine("Failed - shp not polygon");
          continue;
        }

        string intermediateTargetPath = "./out/intermediate.osm.pbf";

        int resultsCount;
        using (Stream targetFile = File.OpenWrite(intermediateTargetPath)) {
          PBFOsmStreamTarget targetPbf = new PBFOsmStreamTarget(targetFile);

          Stream sourceFile = File.OpenRead("./data/thailand-latest.osm.pbf");
          OsmStreamSource sourcePbf = new PBFOsmStreamSource(sourceFile);
          sourcePbf = sourcePbf.FilterSpatial((Polygon)geometry);

          resultsCount = AddFilteredSourceToTarget(sourcePbf, targetPbf, geometry);

          targetPbf.Flush();
          targetPbf.Close();

          sourceFile.Close();
        }

        File.Move(intermediateTargetPath, targetPath);
        Console.WriteLine($"OK - {resultsCount} added");
      }

      Console.WriteLine("Finished");
    }
  }
}
