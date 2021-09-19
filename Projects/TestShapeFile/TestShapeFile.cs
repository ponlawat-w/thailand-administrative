using System;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace ThailandAdministrative.TestShapeFile {
  public static class Program {
    public static void Main(string[] args) {
      ShapefileDataReader reader = Shapefile.CreateDataReader("data/shp/tha_admbnda_adm3_rtsd_20190221", new GeometryFactory());
      DbaseFileHeader h = reader.DbaseHeader;
      Console.WriteLine(String.Join(',', h.Fields.Select(x => x.Name)));
      reader.Read();
    }
  }
}
