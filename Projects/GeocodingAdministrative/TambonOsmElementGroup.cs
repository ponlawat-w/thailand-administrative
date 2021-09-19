using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using NetTopologySuite.Geometries;

namespace ThailandAdministrative.GeocodingAdministrative
{
  public class TambonOsmElementGroup : IEnumerable<TambonOsmElement>
  {
    IEnumerable<TambonOsmElement> Elements;

    public TambonOsmElementGroup(IEnumerable<TambonOsmElement> elements) {
      Elements = elements;
    }

    public TambonOsmElementGroup(string tCode, string dir = "./out/tambon-osmcsv") {
      LoadData($"{dir}/{tCode}.csv");
    }

    IEnumerator<TambonOsmElement> IEnumerable<TambonOsmElement>.GetEnumerator() {
      return Elements.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return Elements.GetEnumerator();
    }

    private void LoadData(string filePath) {
      using (StreamReader reader = new StreamReader(filePath))
      using (CsvReader csvReader = new CsvReader(reader, CultureInfo.InvariantCulture)) {
        Elements = csvReader.GetRecords<TambonOsmElement>().ToArray();
        reader.Close();  
      }
    }

    public Point Geocode(AdministrativeCsvData administrativeRecord, double ratioThreshold = 0.5) {
      string name = administrativeRecord.GetCommunity();
      List<double> xValues = new List<double>();
      List<double> yValues = new List<double>();
      List<double> weights = new List<double>();

      Point lastPoint = null;
      foreach (TambonOsmElement osmElement in Elements) {
        lastPoint = new Point(osmElement.Longitude, osmElement.Latitude);

        IEnumerable<string> texts = osmElement.GetTexts();
        if (texts.Any(text => text.Contains(name))) {
          xValues.Add(osmElement.Longitude);
          yValues.Add(osmElement.Latitude);
          weights.Add(1);
          continue;
        }

        double? similarityRatio = texts.Select(text => 1 - Levenshtein.GetDistanceRatio(name, text))
          .Where(similarityRatio => similarityRatio >= ratioThreshold)
          .OrderByDescending(x => x)
          .FirstOrDefault();
        if (similarityRatio == null || !similarityRatio.HasValue || similarityRatio < ratioThreshold) {
          continue;
        }

        xValues.Add(osmElement.Longitude * similarityRatio.Value);
        yValues.Add(osmElement.Latitude * similarityRatio.Value);
        weights.Add(similarityRatio.Value);
      }

      if (xValues.Count() == 0 || yValues.Count() == 0) {
        return null;
      }

      if (xValues.Count() != yValues.Count()) {
        throw new Exception("Why length not equal?");
      }

      if (xValues.Count() == 1) {
        return lastPoint;
      }

      double totalWeights = weights.Sum();
      return new Point(xValues.Sum() / totalWeights, yValues.Sum() / totalWeights);
    }
  }
}
