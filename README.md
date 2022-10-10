# FastDtw.CSharp

Fast Dynamic Time Warping (DTW) algorithm implementation in .NET C# focusing on performance.

Reference: [Fast DTW: Toward Accurate Dynamic Time Warping in Linear Time and and Space](https://cs.fit.edu/~pkc/papers/tdm04.pdf) 

Further development:

- Add to nuget repository
- Add constrains
- Add tests

## Usage

For double precision results:

```csharp
double[] seriesA = new double[] {0, 1, 2, 3};
double[] seriesB = new double[] {1, 2, 3, 4};

FastDtw.CSharp.Dtw.GetScore(seriesA, seriesB)
```

For single precision results:

```csharp
float[] seriesA = new float[] {0, 1, 2, 3};
float[] seriesB = new float[] {1, 2, 3, 4};

FastDtw.CSharp.Dtw.GetScoreF(seriesA, seriesB)
```

## Performance results

At the time of investigating `FastDtw` (v 1.1.3) package was the fastest one, and therefore was used as a baseline. `ADN.TimeSeries` was not able to process larger dataset without errors. Compared to FastDtw v 1.1.3 a significant improvement was achieved for short and long time series. 

### Short summary

|                    Method | Time series lengthes | Ratio | Alloc Ratio |
|-------------------------- |------------------------ |------:|------------:|
|                   FastDtw |               8163x8089 |  1.00 |        1.00 |
|                      NDtw |               8163x8089 |  2.94 |        3.99 |
|   FastDtw.CSharp (double) |               8163x8089 |  **0.62** |        1.00 |
|    FastDtw.CSharp (float) |               8163x8089 |  **0.47** |        0.50 |
|            ADN.TimeSeries |               8163x8089 |     ? |           ? |
|                           |                         |                    |                  |	
|                   FastDtw |                   10x10 |  1.00 |        1.00 |
|                      NDtw |                   10x10 |  1.09 |        2.11 |
|   FastDtw.CSharp (double) |                   10x10 |  **0.26** |        0.56 |
|    FastDtw.CSharp (float) |                   10x10 |  **0.25** |        0.37 |
|            ADN.TimeSeries |                   10x10 |  2.86 |        0.77 |
|                           |                         |       |             |
|                   FastDtw |                 500x500 |  1.00 |        1.00 |
|                      NDtw |                 500x500 |  1.88 |        3.88 |
|   FastDtw.CSharp (double) |                 500x500 |  **0.31** |        0.97 |
|    FastDtw.CSharp (float) |                 500x500 |  **0.28** |        0.49 |
|            ADN.TimeSeries |                 500x500 |  5.11 |        1.92 |


### Full reults

|                    Method | Time series lengthes |               Mean |            Error |           StdDev | Ratio | RatioSD |        Gen0 |        Gen1 |      Gen2 |    Allocated | Alloc Ratio |
|-------------------------- |------------------------ |-------------------:|-----------------:|-----------------:|------:|--------:|------------:|------------:|----------:|-------------:|------------:|
|                   FastDtw |               8163x8089 |   624,484,657.1 ns |  4,331,375.65 ns |  3,839,651.70 ns |  1.00 |    0.00 |           - |           - |         - |  529705832 B |        1.00 |
|                      NDtw |               8163x8089 | 1,836,728,353.8 ns | 28,146,343.29 ns | 23,503,471.40 ns |  2.94 |    0.05 | 257000.0000 | 137000.0000 | 7000.0000 | 2114535544 B |        3.99 |
|   FastDtw.CSharp (double) |               8163x8089 |   389,984,823.8 ns |  7,615,794.07 ns |  9,066,061.09 ns |  **0.62** |    0.01 |  66000.0000 |  36000.0000 | 4000.0000 |  528634712 B |        1.00 |
|    FastDtw.CSharp (float) |               8163x8089 |   294,769,032.1 ns |  4,625,446.50 ns |  4,100,337.85 ns |  **0.47** |    0.01 |  33500.0000 |  18000.0000 | 3000.0000 |  264448712 B |        0.50 |
|            ADN.TimeSeries |               8163x8089 |                 NA |               NA |               NA |     ? |       ? |           - |           - |         - |            - |           ? |
|                           |                         |                    |                  |                  |       |         |             |             |           |              |             |
|                   FastDtw |                   10x10 |         1,417.2 ns |         18.93 ns |         16.78 ns |  1.00 |    0.00 |      0.3281 |           - |         - |       2760 B |        1.00 |
|                      NDtw |                   10x10 |         1,544.6 ns |         25.11 ns |         23.49 ns |  1.09 |    0.02 |      0.6962 |      0.0134 |         - |       5832 B |        2.11 |
|   FastDtw.CSharp (double) |                   10x10 |           369.6 ns |          3.54 ns |          3.14 ns |  **0.26** |    0.00 |      0.1855 |      0.0005 |         - |       1552 B |        0.56 |
|    FastDtw.CSharp (float) |                   10x10 |           347.6 ns |          2.56 ns |          2.00 ns |  **0.25** |    0.00 |      0.1230 |           - |         - |       1032 B |        0.37 |
|            ADN.TimeSeries |                   10x10 |         4,051.3 ns |         18.19 ns |         17.02 ns |  2.86 |    0.03 |      0.2518 |           - |         - |       2112 B |        0.77 |
|                           |                         |                    |                  |                  |       |         |             |             |           |              |             |
|                   FastDtw |                 500x500 |     2,582,169.6 ns |      9,156.40 ns |      8,116.91 ns |  1.00 |    0.00 |    496.0938 |    496.0938 |  496.0938 |    2086744 B |        1.00 |
|                      NDtw |                 500x500 |     4,847,824.4 ns |     49,959.14 ns |     46,731.81 ns |  1.88 |    0.02 |    968.7500 |    468.7500 |         - |    8104555 B |        3.88 |
|   FastDtw.CSharp (double) |                 500x500 |       793,969.0 ns |      3,421.62 ns |      3,033.18 ns |  **0.31** |    0.00 |    242.1875 |    105.4688 |         - |    2032112 B |        0.97 |
|    FastDtw.CSharp (float) |                 500x500 |       726,095.7 ns |      4,534.84 ns |      4,241.89 ns |  **0.28** |    0.00 |    122.0703 |     46.8750 |         - |    1026112 B |        0.49 |
|            ADN.TimeSeries |                 500x500 |    13,198,765.8 ns |     49,636.05 ns |     44,001.07 ns |  5.11 |    0.03 |    984.3750 |    984.3750 |  984.3750 |    4016530 B |        1.92 |

# License

FastDtw.CSharp is licensed under the [MIT license](https://github.com/kkartavenka/FastDtw.CSharp/blob/master/LICENSE.txt).
