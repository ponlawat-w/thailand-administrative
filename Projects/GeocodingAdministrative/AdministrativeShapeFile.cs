using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace ThailandAdministrative.GeocodingAdministrative {
  public class AdministrativeShapeFile {
    private ShapefileDataReader DataReader;

    private int ProvinceIndex;
    private int AmphoeIndex;
    private int TambonIndex;

    public AdministrativeShapeFile(string shapePath = "data/shp/tha_admbnda_adm3_rtsd_20190221",
      string provinceField = "ADM1_TH", string amphoeField = "ADM2_TH", string tambonField = "ADM3_TH") {
      DbaseFileHeader.DefaultEncoding = Encoding.UTF8;
      this.DataReader = new ShapefileDataReader(shapePath, new GeometryFactory(), Encoding.UTF8);
      GetFieldIndices(provinceField, amphoeField, tambonField);
    }

    private void GetFieldIndices(string provinceField, string amphoeField, string tambonField) {
      DbaseFileHeader tableHeader = this.DataReader.DbaseHeader;
      this.ProvinceIndex = -1;
      this.AmphoeIndex = -1;
      this.TambonIndex = -1;
      for (int i = 0; i < tableHeader.NumFields; i++) {
        string fieldName = tableHeader.Fields[i].Name;
        
        if (this.ProvinceIndex < 0 && provinceField == fieldName) {
          this.ProvinceIndex = i + 1;
        }
        if (this.AmphoeIndex < 0 && amphoeField == fieldName) {
          this.AmphoeIndex = i + 1;
        }
        if (this.TambonIndex < 0 && tambonField == fieldName) {
          this.TambonIndex= i + 1;
        }
      }

      if (this.ProvinceIndex < 0 || this.AmphoeIndex < 0 || this.TambonIndex < 0) {
        throw new Exception("Field(s) not found");
      }
    }

    public Geometry GetGeometry(string searchProvince, string searchAmphoe, string searchTambon) {
      this.DataReader.Reset();
      while (this.DataReader.Read()) {
        string province = this.DataReader.GetString(this.ProvinceIndex);
        string amphoe = this.DataReader.GetString(this.AmphoeIndex);
        string tambon = this.DataReader.GetString(this.TambonIndex);

        if (searchProvince == province && searchAmphoe == amphoe && searchTambon == tambon) {
          return this.DataReader.Geometry;
        }
      }

      return null;
    }

    public Dictionary<string, Dictionary<string, Dictionary<string, Geometry>>> GetAdministrativeDict() {
      this.DataReader.Reset();
      Dictionary<string, Dictionary<string, Dictionary<string, Geometry>>> dict = new Dictionary<string, Dictionary<string, Dictionary<string, Geometry>>>();
      while (this.DataReader.Read()) {
        string province = this.DataReader.GetString(this.ProvinceIndex);
        string amphoe = this.DataReader.GetString(this.AmphoeIndex);
        string tambon = this.DataReader.GetString(this.TambonIndex);

        if (!dict.ContainsKey(province)) {
          dict[province] = new Dictionary<string, Dictionary<string, Geometry>>();
        }
        if (!dict[province].ContainsKey(amphoe)) {
          dict[province][amphoe] = new Dictionary<string, Geometry>();
        }
        if (!dict[province][amphoe].ContainsKey(tambon)) {
          dict[province][amphoe][tambon] = (Geometry)this.DataReader.Geometry;
        }
      }
      return dict;
    }

    public static bool ExistsInAdministrativeDict(Dictionary<string, Dictionary<string, Dictionary<string, Geometry>>> dict,
      string searchProvince, string searchAmphoe, string searchTambon) {
      return dict.ContainsKey(searchProvince) && dict[searchProvince].ContainsKey(searchAmphoe)
        && dict[searchProvince][searchAmphoe].ContainsKey(searchTambon) && dict[searchProvince][searchAmphoe][searchTambon] != null;
    }

    public static Geometry GetGeometryFromDict(Dictionary<string, Dictionary<string, Dictionary<string, Geometry>>> dict,
      string searchProvince, string searchAmphoe, string searchTambon) {
        return dict.ContainsKey(searchProvince) && dict[searchProvince].ContainsKey(searchAmphoe)
          && dict[searchProvince][searchAmphoe].ContainsKey(searchTambon) && dict[searchProvince][searchAmphoe][searchTambon] != null ?
          dict[searchProvince][searchAmphoe][searchTambon] : null;
      }
  }
}
