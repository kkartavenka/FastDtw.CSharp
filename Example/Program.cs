﻿var a = new double[] { 41.98, 41.65, 42.01, 42.35, 44.4, 43.08, 43.6, 42.84, 42.83, 44.01, 43.07, 44.3, 44.6, 46.54, 45.06, 44.96, 43.59, 46.84, 45.22, 45.52 };
var b = new double[] { 95.07, 93.5, 96.67, 96.28, 102.47, 94.24, 95.12, 87.06, 87.92, 88.73, 86.36, 95.34, 93.87, 99.42 };

// When the Warp path is not important
Console.WriteLine($"Score: {FastDtw.CSharp.Dtw.GetScore(a, b)}");

// When we are interest in the Warp path
var scoreWithPath = FastDtw.CSharp.Dtw.GetPath(a, b);
Console.WriteLine($"Score: {scoreWithPath.Score}");
var pathStringified = string.Join(", ", scoreWithPath.Path.Select(x => $"({x.Item1}, {x.Item2})"));
Console.WriteLine($"Path: {pathStringified}");