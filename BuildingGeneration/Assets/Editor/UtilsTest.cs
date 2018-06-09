using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class UtilsTest {

    public class doubleComparer : IComparer
    {
        private double tolerance;
        public doubleComparer(double tolerance)
        {
            this.tolerance = tolerance;
        }

        // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
        int IComparer.Compare(System.Object x, System.Object y)
        {
            double diff = (double)x - (double)y;
            if (diff < tolerance)
            {
                return 0;
            } else if (diff < 0)
            {
                return -1;
            } else
            {
                return 1;
            }
        }
    }

    [Test]
	public void TestCalcCDFSimple() {
        // Use the Assert class to test conditions.
        double[] cdf = Utils.CalcCDF(new int[] { 1, 3, 1, 5});
        CollectionAssert.AreEqual(new double[] { 0.1, 0.4, 0.5, 1.0 }, cdf, new doubleComparer(0.0001));
	}

    [Test]
    public void TestCalcCDFZeros()
    {
        // Use the Assert class to test conditions.
        double[] cdf = Utils.CalcCDF(new int[] { 0, 0, 0, 0, 0});
        CollectionAssert.AreEqual(new double[] { 0.2, 0.4, 0.6, 0.8, 1.0 }, cdf, new doubleComparer(0.0001));
    }

    [Test]
    public void TestWeightedRandomIndexSingle()
    {
        // Part 1
        double[] cdf = new double[] { 1.0 };
        Assert.AreEqual(Utils.WeightedRandomIndex(cdf), 0);

        // Part 2
        cdf = new double[] { 0.0, 0.0, 1.0 };
        Assert.AreEqual(Utils.WeightedRandomIndex(cdf), 2);
    }

    [Test]
    public void TestWeightedRandomIndexDistribution()
    {
        // Use the Assert class to test conditions.
        int[] total = new int[] { 0, 0, 0, 0};
        int iters = 1000000;
        double[] cdf = new double[] { 0.1, 0.5, 0.9, 1.0 };
        double[] pdf = new double[] { 0.1, 0.4, 0.4, 0.1 };
        for (int i = 0; i < iters; i++)
        {
            total[Utils.WeightedRandomIndex(cdf)]++;
        }
        double[] dist = new double[] { 0.0, 0.0, 0.0, 0.0 };
        for (int i = 0; i < total.Length; i++)
        {
            dist[i] = ((double)total[i]) / iters;
        }
        CollectionAssert.AreEqual(pdf, dist, new doubleComparer(0.01));
    }

    [Test]
    public void TestFindLargerIndex()
    {
        double[] arr = new double[] { 1.0, 2.0, 3.0, 4.0 };
        Assert.AreEqual(Utils.FindLargerIndex(arr, 0.5), 0);
        Assert.AreEqual(Utils.FindLargerIndex(arr, 1.5), 1);
        Assert.AreEqual(Utils.FindLargerIndex(arr, 2.5), 2);
        Assert.AreEqual(Utils.FindLargerIndex(arr, 3.5), 3);
        Assert.AreEqual(Utils.FindLargerIndex(arr, 4.5), 3);
    }
}
