using System;

namespace of2.Math
{
    public static class GaussianElim
    {
        public static bool InvertMatrix(ref float[] A, uint n)
        {
            uint[] swap; // which row have we swapped with the current one?
            swap = new uint[n];

            // do one pass for each diagonal element
            for (uint pivot = 0; pivot < n; ++pivot )
            {
                uint row, col; // counters

                // find the largest magnitude element in the current column
                uint maxrow = pivot;
                float maxelem = (A[maxrow + n * pivot]);
                for (row = pivot + 1; row < n; ++row)
                {
                    float elem = System.Math.Abs(A[row + n * pivot]);
                    if (elem > maxelem)
                    {
                        maxelem = elem;
                        maxrow = row;
                    }
                }

                // if max is zero, stop!
                if (maxelem == 0.0f)
                {
                    
                    Console.WriteLine("::Inverse() -- singular matrix\n");
                    return false;
                }

                // if not in the current row, swap rows
                swap[pivot] = maxrow;
                if (maxrow != pivot)
                {
                    // swap the row
                    for (col = 0; col < n; ++col)
                    {
                        float temp = A[maxrow + n * col];
                        A[maxrow + n * col] = A[pivot + n * col];
                        A[pivot + n * col] = temp;
                    }
                }

                // multiply current row by 1/pivot to "set" pivot to 1
                float pivotRecip = 1.0f / A[n * pivot + pivot];
                for (col = 0; col < n; ++col)
                {
                    A[pivot + n * col] *= pivotRecip;
                }

                // copy 1/pivot to pivot point (doing inverse in place)
                A[pivot + n * pivot] = pivotRecip;

                // now zero out pivot column in other rows 
                for (row = 0; row < n; ++row)
                {
                    // don't subtract from pivot row
                    if (row == pivot)
                        continue;

                    // subtract multiple of pivot row from current row,
                    // such that pivot column element becomes 0
                    float factor = A[row + n * pivot];

                    // clear pivot column element (doing inverse in place)
                    // will end up setting this element to -factor*pivotInverse
                    A[row + n * pivot] = 0.0f;

                    // subtract multiple of row
                    for (col = 0; col < n; ++col)
                    {
                        A[row + n * col] -= factor * A[pivot + n * col];
                    }
                }
            }

            return true;
        }
    }
}