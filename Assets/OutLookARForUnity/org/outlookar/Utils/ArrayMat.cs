namespace OutLookAR
{
    public struct ArrayMat
    {
        double[,] _data;
        public double[,] ToArray { get { return _data; } }
        public int Rows { get { return _data.GetLength(0); } }
        public int Cols { get { return _data.GetLength(1); } }
        public double At(int row, int col) { return _data[row, col]; }
        public void At(int row, int col, double pt) { _data[row, col] = pt; }
        public ArrayMat(int rows, int cols)
        {
            _data = new double[rows, cols];
        }
        public ArrayMat(int rows, int cols, double[] data)
        {
            if (data.Length != rows * cols)
                throw new OutLookARException("dataサイズが異なります");
            _data = new double[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    _data[r, c] = data[r * cols + c];
                }
            }
        }
        public ArrayMat(double[,] data)
        {
            _data = data;
        }
        public static ArrayMat operator *(ArrayMat z, ArrayMat w)
        {
            if (z.Cols != w.Rows)
            {
                throw new OutLookARException("Mat type must bu equal to the number of w.rows and z.cols.");
            }
            ArrayMat v = new ArrayMat(z.Rows, w.Cols);
            for (int r = 0; r < v.Rows; r++)
            {
                for (int c = 0; c < v.Cols; c++)
                {
                    double at = 0;
                    for (int b = 0; b < w.Rows; b++)
                    {
                        at += z.At(r, b) * w.At(b, c);
                    }
                    v.At(r, c, at);
                }
            }
            return v;
        }
        public static ArrayMat operator +(ArrayMat z, ArrayMat w)
        {
            if (z.Cols != w.Rows)
            {
                throw new OutLookARException("Mat type must bu equal to the number of w.rows and z.cols.");
            }
            ArrayMat v = new ArrayMat(z.Rows, w.Cols);
            for (int r = 0; r < v.Rows; r++)
            {
                for (int c = 0; c < v.Cols; c++)
                {
                    v.At(r, c, z.At(r, c) + w.At(r, c));
                }
            }
            return v;
        }
         public static ArrayMat zeros(int rows, int cols)
        {
            double[,] data = new double[rows, cols];
            return new ArrayMat(data);
        }
        public static ArrayMat eye(int rows, int cols)
        {
            double[,] data = new double[rows, cols];
            int min = (rows < cols) ? rows : cols;
            for (int i = 0; i < min; i++)
                data[i, i] = 1;
            return new ArrayMat(data);
        }
        public override string ToString()
        {
            string text = "{\n";
            for (int r = 0; r < Rows; r++)
            {
                text += "[";
                for (int c = 0; c < Cols; c++)
                {
                    text += _data[r, c] + ",";
                }
                text += "],\n";
            }
            text += "}";
            return string.Format(
                "Rows : {0} Cols : {1}\n" +
                "Data : {2}",
            Rows, Cols, text);
        }
        public ArrayMat inv()
        {
            int i, j;
            int n = Rows;
            if (n != Cols)
                throw new OutLookARException("正方行列ではありません.");
            double[,] _invData = new double[Rows, Cols];
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    _invData[i, j] = (i == j) ? 1.0 : 0.0;
                }
            }
            double buf;
            double[,] a = _data;
            for (i = 0; i < n; i++)
            {
                buf = 1 / a[i, i];
                for (j = 0; j < n; j++)
                {
                    a[i, j] *= buf;
                    _invData[i, j] *= buf;
                }
                for (j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        buf = a[j, i];
                        for (int k = 0; k < n; k++)
                        {
                            a[j, k] -= a[i, k] * buf;
                            _invData[j, k] -= _invData[i, k] * buf;
                        }
                    }
                }
            }
            return new ArrayMat(_invData);
        }
        public ArrayMat t()
        {
            ArrayMat tMat = new ArrayMat(Cols, Rows);
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if(r!=c)
                        tMat.At(c,r,At(r,c));
                }
            }
            return tMat;
        }
    }
}