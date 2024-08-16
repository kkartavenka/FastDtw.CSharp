# FastDtw.CSharp

Fast Dynamic Time Warping (DTW) algorithm implementation in .NET C# with a focus on performance.

Reference: [Fast DTW: Toward Accurate Dynamic Time Warping in Linear Time and and Space](https://cs.fit.edu/~pkc/papers/tdm04.pdf) 

## Usage

For double precision results:

```csharp
double[] seriesA = new double[] {0, 1, 2, 3};
double[] seriesB = new double[] {1, 2, 3, 4, 5};

FastDtw.CSharp.Dtw.GetScore(seriesA, seriesB);
```

For single precision results:

```csharp
float[] seriesA = new float[] { 41.98f, 41.65f, 42.01f, 42.35f, 44.4f };
float[] seriesB = new float[] { 95.07f, 93.5f, 96.67f, 96.28f, 102.47f, 94.24f, 95.12f, 87.06f };

FastDtw.CSharp.Dtw.GetScoreF(seriesA, seriesB);
```

To get warp path:

```csharp
double[] seriesA = new double[] {0, 1, 2, 3};
double[] seriesB = new double[] {1, 2, 3, 4, 5};

FastDtw.CSharp.Dtw.GetPath(seriesA, seriesB)
```

## Performance results

The comparison was performed with the following packages: [FastDtw (v.1.1.4)](https://www.nuget.org/packages/FastDtw), [NDtw v.0.2.3](https://www.nuget.org/packages/NDtw), and [ADN.TimeSeries (v.1.3.0)](https://www.nuget.org/packages/ADN.TimeSeries). The proposed implementation is at least 3 times faster compared to the listed libraries. The complete results are summarized below on Apple M2 macOS Sonoma 14.6.1 (.NET 8.0):

| Method/Library             | Series length | Mean        | Ratio | Gen0    | Gen1    | Gen2    | Allocated  |
|--------------------------: |-------------- |-----------: |------ |-------- |-------- |-------- |-----------:|
| FastDtw.CSharp.GetScore()  | 8163x8089     |   151.6 ms  | 1.0   | -       | -       | -       | 131.3 KB   |
| FastDtw.CSharp.GetScoreF() | 8163x8089     |   130.9 ms  | 0.86  | -       | -       | -       | 66.8  KB   |
| FastDtw.CSharp.GetPath()   | 8163x8089     |   182.8 ms  | 1.21  | 333     | 333     | 333     | 528.8 MB   |
| FastDtw.Distance()         | 8163x8089     |     450 ms  | 2.97  | -       | -       | -       | 529.7 MB   |
| ADN.TimeSeries             | 8163x8089     |     2.8 s   | 18.3  | 1000    | 1000    | 1000    | 1.06  GB   |
| NDtw                       | 8163x8089     |   980.7 ms  | 6.47  | 343 K   | 178 K   | 7 K     | 2.11  GB   |
|                            |               |             |       |         |         |         |            |
| FastDtw.CSharp.GetScore()  | 500x500       |   0.331 ms  | 1.0   | 1.46    | -       | -       |  16.0 KB   |
| FastDtw.CSharp.GetScoreF() | 500x500       |   0.327 ms  | 0.99  | 0.49    | -       | -       |  8.07 KB   |
| FastDtw.CSharp.GetPath()   | 500x500       |   0.391 ms  | 1.18  | 339.4   | 335.4   | 332.5   |  2.03 MB   |
| FastDtw.Distance()         | 500x500       |   1.209 ms  | 3.65  | 347.7   | 339.8   | 330.1   |  2.09 MB   |
| ADN.TimeSeries             | 500x500       |   8.791 ms  | 26.5  | 562.5   | 562.5   | 484.4   |  4.02 MB   |
| NDtw                       | 500x500       |   2.931 ms  | 8.85  | 968.8   | 460.9   | 230.5   |  8.10 MB   |
|                            |               |             |       |         |         |         |            |
| FastDtw.CSharp.GetScore()  | 10x10         |   125.5 ns  | 1.0   | 0.047   | -       | -       |   392 B    |
| FastDtw.CSharp.GetScoreF() | 10x10         |   120.0 ns  | 0.96  | 0.028   | -       | -       |   232 B    |
| FastDtw.CSharp.GetPath()   | 10x10         |   258.6 ns  | 2.06  | 0.174   | -       | -       |  1.46 KB   |
| FastDtw.Distance()         | 10x10         |   758.9 ns  | 6.05  | 0.33    | -       | -       |  2.76 KB   |
| ADN.TimeSeries             | 10x10         | 2,648.8 ns  | 21.1  | 0.252   | -       | -       |  2.11 KB   |
| NDtw                       | 10x10         | 1,192.5 ns  | 9.5   | 0.696   | -       | -       |  5.83 KB   |

One must consider which method to use to get the best performance. Specifically, if you are interested in a DTW warp score only `.GetScore()` is sufficient since it has the smallest memory allocation and the fastest processing time. To get the warp path with `.GetPath()` method, the whole cost matrix is created and stored which requires larger allocations. Additionally, `.GetScoreF()` will decrease memory allocations since single precision variable is used instead of double. The implementation is slightly different when targeting .NET 6.0 and higher compared to .NET Standard. The results are summarized below on Intel Core i7-9750H CPU, Windows 11):

| Method     | Runtime  | Series length | Mean      | Gen0   | Allocated  |
|----------- |--------- |-------------- |---------- |------- |-----------:|
| GetScore() | .NET 8.0 | 8163x8089     | 181.5 ms  | -      | 130.8 KB   |
| GetScore() | .NET481  | 8163x8089     | 232.3 ms  | -      | 133.4 KB   |
|            |          |               |           |        |            |
| GetScore() | .NET 8.0 | 500x500       | 0.54 ms   | 1.95   |  16.2 KB   |
| GetScore() | .NET481  | 500x500       | 0.84 ms   | 4.88   |  33.1 KB   |
|            |          |               |           |        |            |
| GetScore() | .NET 8.0 | 10x10         | 292.9 ns  | 0.078  |   488 B    |
| GetScore() | .NET481  | 10x10         | 792.4 ns  | 0.187  |  1.18 KB   |


| Method    | Runtime  | Series length | Mean      | Gen0   | Gen1   | Gen2   | Allocated  |
|---------- |--------- |-------------- |---------- |------- |------- |------- |-----------:|
| GetPath() | .NET 8.0 | 8163x8089     |  213.8 ms | 500.0  | 500.0  | 500.0  | 516.4 MB   |
| GetPath() | .NET481  | 8163x8089     |  411.1 ms | 1000.0 | 1000.0 | 1000.0 | 516.4 MB   |
|           |          |               |           |        |        |        |            |
| GetPath() | .NET 8.0 | 500x500       |   1.45 ms | 498.05 | 498.05 | 498.05 |   2.0 MB   |
| GetPath() | .NET481  | 500x500       |   1.68 ms | 498.05 | 498.05 | 498.05 |   2.0 MB   |
|           |          |               |           |        |        |        |            |
| GetPath() | .NET 8.0 | 10x10         |  538.0 ns | 0.247  | -      | -      |   1.5 KB   |
| GetPath() | .NET481  | 10x10         | 1209.2 ns | 0.3557 | -      | -      |   2.2 KB   |

# License

FastDtw.CSharp is licensed under the [MIT license](https://github.com/kkartavenka/FastDtw.CSharp/blob/master/LICENSE.txt).
