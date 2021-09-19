using System;

namespace ThailandAdministrative.MakeTambonOsmCsv {
  public static class Program {
    public static void Main(string[] args) {
      Console.WriteLine("Getting tambon geometries...");
      TambonData[] tambons = ReadTambonGeometries.GetGeometries();
      Console.WriteLine("Starting task...");
      ReadOsmPbfWriteCsv task = new ReadOsmPbfWriteCsv(tambons);
      task.Execute();

      Console.WriteLine("Finished");
    }
  }
}
