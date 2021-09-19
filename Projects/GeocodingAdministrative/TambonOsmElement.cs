using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using NetTopologySuite.Geometries;
using OsmSharp;
using OsmSharp.Tags;

namespace ThailandAdministrative.GeocodingAdministrative
{
  public class TambonOsmElement {
    [Ignore]
    private static string[] InterstedTags = new string[] {
      "name",
      "ADDRESS",
      "name:th",
      "operator"
    };

    [Name("type")]
    public int Type { get; set; }
    [Name("id")]
    public long ID { get; set; }
    [Name("name")]
    public string Name { get; set; }
    [Name("address")]
    public string Address { get; set; }
    [Name("name_th")]
    public string NameTH { get; set; }
    [Name("operator")]
    public string Operator { get; set; }
    [Name("tcode")]
    public string TCode { get; set; }
    [Name("latitude")]
    public double Latitude { get; set; }
    [Name("longitude")]
    public double Longitude { get; set; }

    public bool IsEmpty() {
      return this.Name.Trim() == "" && this.Address.Trim() == "" && this.NameTH.Trim() == "" && this.Operator.Trim() == "";
    }

    public TambonOsmElement() {}

    public TambonOsmElement(OsmGeo osmGeo, string tCode, Point point) {
      TagsCollectionBase tags = osmGeo.Tags;
      this.Type = (int)osmGeo.Type;
      this.ID = osmGeo.Id.Value;
      this.Name = tags.ContainsKey("name") ? tags["name"] : "";
      this.Address = tags.ContainsKey("ADDRESS") ? tags["ADDRESS"] : "";
      this.NameTH = tags.ContainsKey("name:th") ? tags["name:th"] : "";
      this.Operator = tags.ContainsKey("operator") ? tags["operator"] : "";
      this.TCode = tCode;
      this.Latitude = point.Y;
      this.Longitude = point.X;
    }

    public IEnumerable<string> GetTexts() {
      return new List<string>{this.Name, this.Address, this.NameTH, this.Operator}
        .Where(text => text != null && text.Trim() != "");
    }

    public static bool HasMetadata(OsmGeo osmGeo) {
      return (osmGeo.Tags.ContainsKey("name") && osmGeo.Tags["name"].Trim() != "")
        || (osmGeo.Tags.ContainsKey("ADDRESS") && osmGeo.Tags["ADDRESS"].Trim() != "")
        || (osmGeo.Tags.ContainsKey("name:th") && osmGeo.Tags["name:th"].Trim() != "")
        || (osmGeo.Tags.ContainsKey("operator") && osmGeo.Tags["operator"].Trim() != "");
    }
  }
}
