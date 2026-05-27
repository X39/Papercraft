using X39.Solutions.PdfTemplate.Abstraction;

namespace X39.Solutions.PdfTemplate.Benchmark;

internal static class BenchmarkUtilities
{
    public static int CountControls(IEnumerable<IControl> controls)
    {
        var count = 0;
        foreach (var control in controls)
        {
            count++;
            if (control is IContentControl contentControl)
                count += CountControls(contentControl);
        }

        return count;
    }
}
