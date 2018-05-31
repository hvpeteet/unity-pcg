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
	public void UtilsTestSimplePasses() {
        // Use the Assert class to test conditions.
        double[] cdf = Utils.CalcCDF(new int[] { 1, 3, 1, 5});
        CollectionAssert.AreEqual(new double[] { 0.1, 0.4, 0.5, 1.0 }, cdf, new doubleComparer(0.0001));
	}
}
