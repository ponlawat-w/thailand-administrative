using System;

namespace ThailandAdministrative.GeocodingAdministrative {
  public static class Levenshtein {
    public static int GetDistance(string str1, string str2) {
      int[,] matrix = new int[str2.Length + 1, str1.Length + 1];
      for (int i = 0; i <= str1.Length; i++) {
        matrix[0, i] = i;
      }
      for (int j = 0; j <= str2.Length; j++) {
        matrix[j, 0] = j;
      }

      for (int i = 1; i <= str1.Length; i++) {
        for (int j = 1; j <= str2.Length; j++) {
          matrix[j, i] = Math.Min(
            Math.Min(matrix[j, i - 1] + 1, matrix[j - 1, i] + 1),
            matrix[j - 1, i - 1] + (str1[i - 1] == str2[j - 1] ? 0 : 1)
          );
        }
      }

      return matrix[str2.Length, str1.Length];
    }

    public static double GetDistanceRatio(string str1, string str2) {
      double distance = (double)GetDistance(str1, str2);
      return Math.Min(distance / str2.Length, distance / str1.Length);
    }
  }
}
