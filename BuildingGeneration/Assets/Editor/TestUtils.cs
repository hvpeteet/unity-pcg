using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUtils {

    public class DoubleComparer : IComparer
    {
        private double tolerance;
        public DoubleComparer(double tolerance)
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
            }
            else if (diff < 0)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }
}
