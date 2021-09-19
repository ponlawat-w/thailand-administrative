using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace ThailandAdministrative.GeocodingAdministrative
{
  public static class AdministrativeCsvReader {
    public static AdministrativeCsvData[] Read(string filePath = "data/list-all.csv") {
      StreamReader reader = new StreamReader(filePath);
      CsvReader csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) {
        PrepareHeaderForMatch = h => h.Header.ToLower()
      });
      AdministrativeCsvData[] results = csvReader.GetRecords<AdministrativeCsvData>().ToArray();

      reader.Close();

      return results;
    }

    public static string[] GetUniqueTCodes(string filePath = "data/list-all.csv") {
      return Read(filePath).Select(record => record.TCode).Distinct().ToArray();
    }
  }
}
