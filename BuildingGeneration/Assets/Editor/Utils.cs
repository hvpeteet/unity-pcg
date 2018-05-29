using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils {

    public static System.Random rng = new System.Random();

    // --------- Math Helpers ---------
    public static int WeightedRandomIndex(double[] cdf)
    {
        double val = rng.NextDouble();
        int low = 0;
        int high = cdf.Length - 1;

        for (int i = 0; i < cdf.Length; i++)
        {
            int mid = (low + high) / 2;
            if (cdf[mid] < val)
            {
                low = mid;
            }
            else
            {
                high = mid;
            }
            if (low == high - 1)
            {
                break;
            }
        }
        return high;
    }

    public static double[] CalcCDF(int[] values)
    {
        double[] cdf = new double[values.Length];
        int total_points = 0;

        for (int i = 0; i < values.Length; i++)
        {
            total_points += values[i];
        }

        if (total_points == 0)
        {
            // If there are no values then return a uniform distribution cdf.
            for (int i = 0; i < values.Length; i++)
            {
                cdf[i] = 1.0 / values.Length * (i + 1);
            }
        }
        else
        {
            double cumulative_points = 0.0;
            for (int i = 0; i < cdf.Length; i++)
            {
                cdf[i] = ((double)values[i]) / total_points + cumulative_points;
                cumulative_points += cdf[i];
            }
        }
        return cdf;
    }
}
