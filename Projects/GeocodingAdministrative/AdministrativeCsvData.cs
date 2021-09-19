using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;
using NetTopologySuite.Geometries;

namespace ThailandAdministrative.GeocodingAdministrative
{
  public class AdministrativeCsvData {
    [Ignore]
    private static string[] UnwantedMPrefixes = new string[] {
      "ชุมชน", "หมู่บ้าน"
    };

    [Name("mcode")]
    public string MCode { get; set; }
    [Name("mname")]
    public string MName { get; set; }
    [Name("mno")]
    public string MNo { get; set; }
    [Name("mtype")]
    public string MType { get; set; }
    [Name("pcode")]
    public string PCode { get; set; }
    [Name("pname")]
    public string PName { get; set; }
    [Name("acode")]
    public string ACode { get; set; }
    [Name("aname")]
    public string AName { get; set; }
    [Name("tcode")]
    public string TCode { get; set; }
    [Name("tname")]
    public string TName { get; set; }
    [Name("orgcode")]
    public string OrgCode { get; set; }
    [Name("orgname")]
    public string OrgName { get; set; }
    [Name("orgtype")]
    public string OrgType { get; set; }
    [Name("remark")]
    public string Remark { get; set; }
    [Name("lastupdate")]
    public string Lastupdate { get; set; }
    [Name("lat")]
    public string Lat { get; set; }
    [Name("long")]
    public string Long { get; set; }
    [Name("log")]
    public string Log { get; set; }

    public AdministrativeCsvData() {}

    private static string GetWithoutPrefix(string value, string prefix) {
      return value.StartsWith(prefix) ? value.Substring(prefix.Length) : value;
    }

    private static string GetWithoutPrefixes(string value, IEnumerable<string> prefixes) {
      foreach (string prefix in prefixes) {
        value = GetWithoutPrefix(value, prefix);
      }
      return value;
    }

    public string GetTambon() {
      return GetWithoutPrefix(this.TName, "ตำบล");
    }

    public string GetAmphoe() {
      return GetWithoutPrefix(this.AName, "อำเภอ");
    }

    public string GetProvince() {
      return GetWithoutPrefix(this.PName, "จังหวัด");
    }

    public string GetCommunity() {
      return GetWithoutPrefixes(this.MName, UnwantedMPrefixes);
    }

    public void PrepareFinalOutput() {
      this.TName = GetTambon();
      this.AName = GetAmphoe();
      this.PName = GetProvince();
    }

    public void SetGeometry(Point point) {
      this.Lat = point.Y.ToString();
      this.Long = point.X.ToString();
    }
  }
}
