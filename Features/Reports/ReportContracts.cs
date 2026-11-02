namespace TrustSystem.Backend.DTOs
{
    public record BiReport1Response(
        List<KpiCard>    KpiCards,
        LineChartDto     LineChartConfig,
        BarChartDto      BarChartConfig,
        DonutChartDto    DonutChartConfig,
        ScatterChartDto  ScatterChartConfig,
        HeatmapChartDto  HeatmapChartConfig
    );

    public record KpiCard(string Title, string Value, string Change, string Icon, bool Up);

    public record LineChartDto(string Title, string[] XAxisValues, List<ChartLine> Lines, bool Area);

    public record BarChartDto(string Title, string[] XAxisValues, List<ChartBar> Bars);

    public record DonutChartDto(string Title, List<ChartSlice> Data);

    public record ScatterChartDto(string Title, string XName, string YName, List<double[]> Points);

    public record HeatmapChartDto(string Title, string SeriesName, string[] XLabels, string[] YLabels, List<int[]> Data, int MaxValue);

    public record ChartLine(string Name, double[] Values);

    public record ChartBar(string Name, double[] Values);

    public record ChartSlice(double Value, string Name);
}
