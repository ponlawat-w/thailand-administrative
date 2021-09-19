using System.Globalization;
using System.IO;
using CsvHelper;
using ThailandAdministrative.GeocodingAdministrative;

namespace ThailandAdministrative.MakeTambonOsmCsv
{
  public class CsvTarget {
    public CsvWriter CsvWriter;
    public StreamWriter Writer;

    public CsvTarget(string filePath) {
      this.Writer = new StreamWriter(filePath);
      this.CsvWriter = new CsvWriter(this.Writer, CultureInfo.InvariantCulture);
      this.CsvWriter.WriteHeader<TambonOsmElement>();
      this.CsvWriter.NextRecord();
    }

    public void Close() {
      this.CsvWriter.Flush();
      this.Writer.Close();
    }
  }
}
