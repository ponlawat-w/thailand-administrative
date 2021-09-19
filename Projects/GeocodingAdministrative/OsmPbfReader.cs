using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetTopologySuite.Geometries;
using OsmSharp;
using OsmSharp.Streams;

namespace ThailandAdministrative.GeocodingAdministrative {
  public class ThailandOsmPbfReader {
    public OsmStreamSource Source;
    public Stream FileStream;

    private Polygon Envelope = null;

    public ThailandOsmPbfReader(string filePath = "./data/thailand-latest.osm.pbf") {
      this.FileStream = File.OpenRead(filePath);
      this.Source = new PBFOsmStreamSource(this.FileStream);
    }

    public void Filter(Polygon polygon) {
      this.Envelope = GetEnvelope(polygon);
    }

    public bool ExistsElementWithName(string name) {
      string[] tags = new string[] {
        "name",
        "ADDRESS",
        "name:th",
        "operator"
      };

      OsmStreamSource filteredSource = this.Envelope == null ? this.Source : this.Source.FilterBox(
        (float)this.Envelope.Coordinates[0].X,
        (float)this.Envelope.Coordinates[2].Y,
        (float)this.Envelope.Coordinates[1].X,
        (float)this.Envelope.Coordinates[0].Y
      );

      Console.WriteLine($"Left: {this.Envelope.Coordinates[0].X}");
      Console.WriteLine($"Top: {this.Envelope.Coordinates[2].Y}");
      Console.WriteLine($"Right: {this.Envelope.Coordinates[1].X}");
      Console.WriteLine($"Bottom: {this.Envelope.Coordinates[0].Y}");

      Console.WriteLine(this.Source.Count());
      Console.WriteLine(filteredSource.Count());

      IEnumerable<OsmGeo> results = filteredSource.Where(element => {
        foreach (string tag in tags) {
          if (element.Tags.ContainsKey(tag)) {
            if (element.Tags[tag].ToString().Contains(name)) {
              return true;
            }
          }
        }
        return false;
      });

      return results.FirstOrDefault() != null;
    }

    public void Close() {
      this.FileStream.Close();
    }

    public static Polygon GetEnvelope(Polygon polygon) {
      Geometry envelope = polygon.Envelope; // (minx, miny), (maxx, miny), (maxx, maxy), (minx, maxy), (minx, miny).
      if (!(envelope is Polygon)) {
        throw new Exception("Envelope is not a polygon.");
      }
      
      return (Polygon)envelope;
    }
  }
}
