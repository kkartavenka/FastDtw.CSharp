namespace FastDtw.CSharp
{
    public enum NormalizationType
    {
        /// <summary>
        /// Divides the score by the path length
        /// </summary>
        PathLength,
        /// <summary>
        /// Divides the score by the max length of two series
        /// </summary>
        MaxSeriesLength,
        /// <summary>
        /// Divide the sum of series length
        /// </summary>
        SumSeriesLength
    }
}