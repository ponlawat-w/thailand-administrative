using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using OsmSharp;
using ThailandAdministrative.GeocodingAdministrative;

namespace ThailandAdministrative.MakeTambonOsmCsv
{
  public class CsvTargetGroup {
    private IDictionary<string, CsvTarget> Dictionary;

    public CsvTargetGroup(IEnumerable<string> tCodes) {
      this.Dictionary = tCodes.ToDictionary(
        tCode => tCode,
        tCode => new CsvTarget($"./out/tambon-osmcsv/{tCode}.csv")
      );
    }

    public void AddRecordTo(TambonOsmElement record, string tCode) {
      this.Dictionary[tCode].CsvWriter.WriteRecord<TambonOsmElement>(record);
      this.Dictionary[tCode].CsvWriter.NextRecord();
    }

    public void AddRecordsTo(OsmGeo element, Point point, IEnumerable<string> tCodes) {
      foreach (string tCode in tCodes) {
        TambonOsmElement record = new TambonOsmElement(element, tCode, point);
        this.AddRecordTo(record, tCode);
      }
    }

    public void AddRecordsTo(OsmGeo element, Point[] points, IEnumerable<TambonData> tambons) {
      foreach (TambonData tambon in tambons) {
        MultiPoint filteredPoints = new MultiPoint(points.Where(p => tambon.Geometry.Covers(p)).ToArray());
        TambonOsmElement record = new TambonOsmElement(element, tambon.TCode, filteredPoints.Centroid);
        this.AddRecordTo(record, tambon.TCode);
      }
    }

    public void Flush() {
      foreach (string tCode in this.Dictionary.Keys) {
        this.Dictionary[tCode].CsvWriter.Flush();
      }
    }

    public void FlushAndClose() {
      foreach (string tCode in this.Dictionary.Keys) {
        this.Dictionary[tCode].Close();
      }
    }
  }
}
