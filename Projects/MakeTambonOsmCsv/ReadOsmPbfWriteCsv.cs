using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetTopologySuite.Geometries;
using OsmSharp;
using OsmSharp.Streams;
using ThailandAdministrative.GeocodingAdministrative;

namespace ThailandAdministrative.MakeTambonOsmCsv
{
  public class ReadOsmPbfWriteCsv {

    CsvTargetGroup TargetGroup;

    TambonData[] Tambons;

    IDictionary<long, Point> NodePointDict;
    IDictionary<long, Point[]> WayPointsDict;
    IDictionary<long, Point[]> RelationPointsDict;

    bool StartedNode = false;
    bool StartedWay = false;
    bool StartedRelation = false;

    public ReadOsmPbfWriteCsv(TambonData[] tambons) {
      Tambons = tambons;
    }

    private IEnumerable<string> GetTCodesFromPoint(Point point) {
      return Tambons.Where(t => t.Geometry.Covers(point)).Select(t => t.TCode);
    }

    private IEnumerable<TambonData> GetTambonsFromPoints(MultiPoint multiPoint) {
      return Tambons.Where(t => t.Geometry.Covers(multiPoint));
    }

    private void ExecuteNode(Node node) {
      if (!StartedNode) {
        Console.WriteLine("Executing nodes...");
        StartedNode = true;
      }
      if (node.Id == null) {
        return;
      }

      Point point = new Point(node.Longitude.Value, node.Latitude.Value);
      NodePointDict[node.Id.Value] = point;
      if (TambonOsmElement.HasMetadata(node)) {
        TargetGroup.AddRecordsTo(node, point, GetTCodesFromPoint(point));
      }
    }

    private void ExecuteWay(Way way) {
      if (!StartedWay) {
        Console.WriteLine("\nExecuting ways...");
        StartedWay = true;
      }
      if (way.Id == null) {
        return;
      }

      Point[] points = way.Nodes.Select(nodeID => NodePointDict[nodeID]).ToArray();
      WayPointsDict[way.Id.Value] = points;
      if (TambonOsmElement.HasMetadata(way)) {
        TargetGroup.AddRecordsTo(way, points, GetTambonsFromPoints(new MultiPoint(points)));
      }
    }

    private void ExecuteRelation(Relation relation) {
      if (!StartedRelation) {
        Console.WriteLine("\nExecuting relations...");
        StartedRelation = true;
      }
      if (relation.Id == null) {
        return;
      }

      Point[] points;

      {
        List<Point> pointsList = new List<Point>();

        foreach (RelationMember member in relation.Members) {
          if (member.Type == OsmGeoType.Node && NodePointDict.ContainsKey(member.Id)) {
            pointsList.Add(NodePointDict[member.Id]);
          } else if (member.Type == OsmGeoType.Way && WayPointsDict.ContainsKey(member.Id)) {
            pointsList.AddRange(WayPointsDict[member.Id]);
          } else if (member.Type == OsmGeoType.Relation && RelationPointsDict.ContainsKey(member.Id)) {
            pointsList.AddRange(RelationPointsDict[member.Id]);
          }
        }

        points = pointsList.ToArray();
      }

      RelationPointsDict[relation.Id.Value] = points;
      if (TambonOsmElement.HasMetadata(relation)) {
        TargetGroup.AddRecordsTo(relation, points, GetTambonsFromPoints(new MultiPoint(points)));
      }
    }

    public void Execute() {
      Directory.CreateDirectory("./out/tambon-osmcsv");

      TargetGroup = new CsvTargetGroup(Tambons.Select(t => t.TCode));

      NodePointDict = new Dictionary<long, Point>();
      WayPointsDict = new Dictionary<long, Point[]>();
      RelationPointsDict = new Dictionary<long, Point[]>();

      using (FileStream fileStream = File.OpenRead("./data/thailand-latest.osm.pbf"))
      using (OsmStreamSource osmSource = new PBFOsmStreamSource(fileStream)) {
        Console.Write("Counting elements...");
        long elementCount = osmSource.LongCount();
        Console.WriteLine($" {elementCount:n0} found");

        osmSource.Reset();
        long i = 0;
        long paceCounter = 0;
        DateTime startTime = DateTime.Now;
        TimeSpan interval = new TimeSpan(0, 0, 1);
        foreach (OsmGeo element in osmSource) {
          ++i;
          if (DateTime.Now - startTime >= interval) {
            TimeSpan realInterval = DateTime.Now - startTime;
            double percent = (double)i / elementCount * 100;
            double pace = (double)(i - paceCounter) / realInterval.TotalSeconds;
            TimeSpan estimate = new TimeSpan(0, 0, (int)Math.Round((elementCount - i) / pace));
            Console.Write($"\r{i:n0} from {elementCount:n0} ({percent:n2}%) processed ({pace:n2} elements/sec, remaining {estimate:hh\\:mm\\:ss})...");

            startTime = DateTime.Now;
            paceCounter = i;
          }

          if (element.Type == OsmGeoType.Node) {
            ExecuteNode((Node)element);
          } else if (element.Type == OsmGeoType.Way) {
            ExecuteWay((Way)element);
          } else if (element.Type == OsmGeoType.Relation) {
            ExecuteRelation((Relation)element);
          }
        }

        fileStream.Close();
        Console.WriteLine("\nTask finished.");
      }

      Console.Write("Closing streams...");
      TargetGroup.FlushAndClose();
      Console.WriteLine("OK");
    }
  }
}
