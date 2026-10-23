using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Matrices
{
    public abstract class Matrix
    {
        public int Rows {  get; set; }
        public int Columns {  get; set; }
        public abstract float this[int x, int y] {  get; set; }
        protected void ValidateIndices(int row, int col)
        {
            if (row < 0 || row >= Rows)
                throw new IndexOutOfRangeException($"Wiersz {row} jest poza zakresem.");
            if (col < 0 || col >= Columns)
                throw new IndexOutOfRangeException($"Kolumna {col} jest poza zakresem.");
        }
        public Matrix(int  rows, int columns)
        {
            if(rows < 0 || columns < 0)
            {
                throw new ArgumentException("liczba wierszy i kolumn musi być całkowita dodatnia");
            }
            Rows = rows;
            Columns = columns;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Rows; i++) {
                for (int j = 0; j < Columns; j++) {
                    sb.Append(this[i,j].ToString());
                    if(j < Columns - 1)
                    {
                        sb.Append("\t");
                    }
                }
                if(i < Rows - 1)
                {
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
        public abstract void Identity();
        public abstract Matrix Transpose();
        public abstract float Norm();
        protected abstract Matrix GetInstance(int row, int col);
        public static Matrix operator +(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
            {
                throw new ArgumentException("złe wielkości");
            }
            var mat = a.GetInstance(a.Rows, a.Columns);
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Columns; j++)
                {
                    mat[i, j] = a[i, j] + b[i, j];
                }
            }
            return mat;
        }
        public static Matrix operator -(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
            {
                throw new ArgumentException("złe wielkości");
            }
            var mat = a.GetInstance(a.Rows, a.Columns);
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Columns; j++)
                {
                    mat[i, j] = a[i, j] - b[i, j];
                }
            }
            return mat;
        }
        public static Matrix operator *(Matrix a, Matrix b)
        {
            if(a.Columns != b.Rows)
            {
                throw new ArgumentException("złe wymiary");
            }
            int n = a.Columns;
            var mat = a.GetInstance(a.Rows, b.Columns);
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < b.Columns; j++)
                {
                    for (int k = 0; k < n; k++)
                    {
                        mat[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
            return mat;
        }
        public static Matrix operator *(Matrix a, float  b)
        {
            var mat = a.GetInstance(a.Rows, a.Columns);
            for(int i  = 0; i < a.Rows; ++i)
            {
                for (int j =0; j < a.Columns; ++j)
                {
                    mat[i,j] = a[i,j] * b;
                }
            }
            return mat;
        }
        public static Matrix operator *(float b, Matrix a)
        {
            var mat = a.GetInstance(a.Rows, a.Columns);
            for (int i = 0; i < a.Rows; ++i)
            {
                for (int j = 0; j < a.Columns; ++j)
                {
                    mat[i, j] = a[i, j] * b; ;
                }
            }
            return mat;
        }
    }


    public class DenseMatrix : Matrix
    {
        private float[,] matrix;
        public DenseMatrix(int rows, int columns) : base(rows, columns)
        {
            matrix = new float[rows, columns];
        }

        public override float this[int x, int y]
        {
            get
            {
                ValidateIndices(x, y);
                return matrix[x, y];
            }
            set
            {
                ValidateIndices(x, y);
                matrix[x, y] = value;
            }
        }

        public override void Identity()
        {
            if (Rows != Columns)
            {
                throw new InvalidOperationException("macierz musi być kwadratowa");
            }
            Array.Clear(matrix, 0, matrix.Length);
            for (int i = 0; i < Rows; i++)
            {
                matrix[i, i] = 1f;
            }
        }

        public override Matrix Transpose()
        {
            DenseMatrix DM = new DenseMatrix(Columns, Rows);
            for(int i  = 0; i < Rows; i++) { 
                for(int j = 0; j < Columns; j++)
                {
                    DM.matrix[j, i] = this.matrix[i, j];
                }
            }
            return DM;
        }
        public override float Norm()
        {
            float s = 0;
            for (int i = 0; i < Rows; i++) {
                for (int j = 0; j < Columns; j++) {
                    s += matrix[i, j] * matrix[i, j];
                }
            }
            return MathF.Sqrt(s);
        }
        protected override Matrix GetInstance(int row, int col)
        {
            return new DenseMatrix(row, col);
        }
    }

    public class SparseMatrix : Matrix
    {
        private Dictionary<(int, int), float> _elements;
        public SparseMatrix(int rows, int columns) : base(rows, columns)
        {
            _elements = new Dictionary<(int, int), float>();
        }
        public override float this[int x, int y] { get
            {
                ValidateIndices(x, y);
                if (_elements.TryGetValue((x, y), out float value)) {
                    return value;
            }
                return 0f;
            
            } set {
                ValidateIndices(x, y);
                if (value == 0f) { _elements.Remove((x, y)); } else
                {
                    _elements[(x,y)] = value;
                }
            }   
        }
        public override void Identity()
        {
            if (Rows != Columns)
            {
                throw new InvalidOperationException("macierz musi być kwadratowa");
            }
            _elements.Clear();
            for (int i = 0; i < Rows; i++) {
                _elements.Add((i, i), 1f);
            }
            
        }
        public override Matrix Transpose()
        {
            SparseMatrix SM = new SparseMatrix(Columns, Rows);
            foreach(var p in _elements)
            {
                int wiersz = p.Key.Item1;
                int kolumna = p.Key.Item2;
                float wartosc = p.Value;
                SM._elements.Add((kolumna, wiersz), wartosc);
            }
            return SM;
        }
        public override float Norm()
        {
            float s = 0;
            foreach(float val in _elements.Values)
            {
                s += val*val;
            }
            return MathF.Sqrt(s);
        }
        protected override Matrix GetInstance(int row, int col)
        {
            return new SparseMatrix(row, col);
        }
    }
    public class MatrixAlgorithms
    {
        public static Matrix PseudoInverse(Matrix A, int t = 1000)
        {
            var I = new SparseMatrix(A.Columns, A.Rows);
            I.Identity();
            float eps = 1e-7f;
            float norm = A.Norm();
            float alfa = 1f/(norm*norm);
            Matrix A2 = null;
            var A1 = A.Transpose() * alfa;
            for (int i = 1; i <= t; i++) {
                A2 = A1 + A1 * (I - A * A1);
                if ((A2 - A1).Norm() < eps) break;
                A1 = A2;
            }
            return A2;
        }
    }
}
