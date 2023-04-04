# FastDtw.CSharp

Fast Dynamic Time Warping (DTW) algorithm implementation in .NET C# focusing on performance.

Reference: [Fast DTW: Toward Accurate Dynamic Time Warping in Linear Time and and Space](https://cs.fit.edu/~pkc/papers/tdm04.pdf) 

Further development:

- Add constrains

## Usage

For double precision results:

```csharp
double[] seriesA = new double[] {0, 1, 2, 3};
double[] seriesB = new double[] {1, 2, 3, 4, 5};

FastDtw.CSharp.Dtw.GetScore(seriesA, seriesB)
```

For single precision results:

```csharp
float[] seriesA = new float[] { 41.98f, 41.65f, 42.01f, 42.35f, 44.4f };
float[] seriesB = new float[] { 95.07f, 93.5f, 96.67f, 96.28f, 102.47f, 94.24f, 95.12f, 87.06f };

FastDtw.CSharp.Dtw.GetScoreF(seriesA, seriesB)
```

## Performance results

At the time of investigating `FastDtw` (v 1.1.3) package was the fastest one, and therefore was used as a baseline. `ADN.TimeSeries` was not able to process larger dataset without errors. Compared to FastDtw v 1.1.3 a significant improvement was achieved for short and long time series. 

### Summary

|                    Method | Series length AxB |               Mean | Ratio |    Allocated | Alloc Ratio |
|-------------------------- |------------------------ |-------------------:|------:|-------------:|------------:|
|                   FastDtw |               8163x8089 |   852.8 ms |  1.00 |  529705064 B |       1.000 |
|                      NDtw |               8163x8089 | 2,363.0 ms |  2.77 | 2114538016 B |       3.992 |
|   FastDtw.CSharp (double) |               8163x8089 |   261.7 ms |  0.31 |     130840 B |       0.000 |
|    FastDtw.CSharp (float) |               8163x8089 |   279.7 ms |  0.33 |      67096 B |       0.000 |
|            ADN.TimeSeries |               8163x8089 |                 NA | NA |            - |           ? |
|                           |                         |                    |              |             |
|                   FastDtw |                      10 |         1,887.9 ns |  1.00 |       2760 B |        1.00 |
|                      NDtw |                      10 |         2,119.2 ns |  1.12 |       5832 B |        2.11 |
|   FastDtw.CSharp (double) |                      10 |           381.0 ns |  0.20 |        408 B |        0.15 |
|    FastDtw.CSharp (float) |                      10 |           402.4 ns |  0.21 |        240 B |        0.09 |
|            ADN.TimeSeries |                      10 |         6,365.9 ns |  3.82 |       2112 B |        0.77 |
|                           |                         |                    |       |              |             |
|                   FastDtw |                     500 |     3,702.9 µs |  1.00 |    2086744 B |       1.000 |
|                      NDtw |                     500 |     6,804.2 µs |  1.84 |    8104805 B |       3.884 |
|   FastDtw.CSharp (double) |                     500 |       939.5 µs |  0.25 |      16088 B |       0.008 |
|    FastDtw.CSharp (float) |                     500 |     1,030.9 µs |  0.28 |       8081 B |       0.004 |
|            ADN.TimeSeries |                     500 |    15,957.7 µs |  4.31 |    4016530 B |       1.925 |

Note: *Smaller is better

### Full results

|                    Method | BenchmarkSequenceLength |               Mean |            Error |           StdDev | Ratio | RatioSD |        Gen0 |        Gen1 |      Gen2 |    Allocated | Alloc Ratio |
|-------------------------- |------------------------ |-------------------:|-----------------:|-----------------:|------:|--------:|------------:|------------:|----------:|-------------:|------------:|
|                   FastDtw |               8163x8089 |   852,770,223.1 ns | 14,650,159.15 ns | 12,233,546.40 ns |  1.00 |    0.00 |           - |           - |         - |  529705064 B |       1.000 |
|                      NDtw |               8163x8089 | 2,363,016,883.3 ns | 24,067,817.16 ns | 18,790,568.52 ns |  2.77 |    0.04 | 340000.0000 | 175000.0000 | 7000.0000 | 2114538016 B |       3.992 |
|   FastDtw.CSharp (double) |               8163x8089 |   261,675,781.1 ns |  5,174,377.41 ns |  8,786,485.53 ns |  0.31 |    0.01 |           - |           - |         - |     130840 B |       0.000 |
|    FastDtw.CSharp (float) |               8163x8089 |   279,699,110.0 ns |  5,400,750.08 ns |  5,051,864.97 ns |  0.33 |    0.01 |           - |           - |         - |      67096 B |       0.000 |
|            ADN.TimeSeries |               8163x8089 |                 NA |               NA |               NA |     ? |       ? |           - |           - |         - |            - |           ? |
|                           |                         |                    |                  |                  |       |         |             |             |           |              |             |
|                   FastDtw |                      10 |         1,887.9 ns |         36.80 ns |         34.43 ns |  1.00 |    0.00 |      0.4387 |      0.0019 |         - |       2760 B |        1.00 |
|                      NDtw |                      10 |         2,119.2 ns |         22.72 ns |         18.97 ns |  1.12 |    0.02 |      0.9270 |      0.0191 |         - |       5832 B |        2.11 |
|   FastDtw.CSharp (double) |                      10 |           381.0 ns |          1.78 ns |          1.39 ns |  0.20 |    0.00 |      0.0648 |           - |         - |        408 B |        0.15 |
|    FastDtw.CSharp (float) |                      10 |           402.4 ns |          2.85 ns |          2.67 ns |  0.21 |    0.00 |      0.0381 |           - |         - |        240 B |        0.09 |
|            ADN.TimeSeries |                      10 |         6,365.9 ns |        351.47 ns |      1,025.26 ns |  3.82 |    0.83 |      0.3357 |           - |         - |       2112 B |        0.77 |
|                           |                         |                    |                  |                  |       |         |             |             |           |              |             |
|                   FastDtw |                     500 |     3,702,870.0 ns |     27,765.07 ns |     25,971.46 ns |  1.00 |    0.00 |    496.0938 |    496.0938 |  496.0938 |    2086744 B |       1.000 |
|                      NDtw |                     500 |     6,804,232.9 ns |    114,975.57 ns |     89,765.37 ns |  1.84 |    0.03 |   1335.9375 |    671.8750 |  257.8125 |    8104805 B |       3.884 |
|   FastDtw.CSharp (double) |                     500 |       939,511.9 ns |      5,399.81 ns |      4,509.08 ns |  0.25 |    0.00 |      1.9531 |           - |         - |      16088 B |       0.008 |
|    FastDtw.CSharp (float) |                     500 |     1,030,855.3 ns |     18,986.04 ns |     15,854.20 ns |  0.28 |    0.00 |           - |           - |         - |       8081 B |       0.004 |
|            ADN.TimeSeries |                     500 |    15,957,675.2 ns |    224,745.02 ns |    199,230.60 ns |  4.31 |    0.07 |    968.7500 |    968.7500 |  968.7500 |    4016530 B |       1.925 |

# License

FastDtw.CSharp is licensed under the [MIT license](https://github.com/kkartavenka/FastDtw.CSharp/blob/master/LICENSE.txt).
