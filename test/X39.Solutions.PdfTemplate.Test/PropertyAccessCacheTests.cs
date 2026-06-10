using X39.Solutions.Papercraft.Services.PropertyAccessCache;

namespace X39.Solutions.PdfTemplate.Test;

public sealed class PropertyAccessCacheTests
{
    [Fact]
    public void Get_MapsTypeOnFirstUse()
    {
        var cache = new PropertyAccessCache();
        var target = new Sample { Name = "before" };

        var found = cache.Get(target, nameof(Sample.Name), out var value);

        Assert.True(found);
        Assert.Equal("before", value);
    }

    [Fact]
    public void Set_MapsTypeOnFirstUse()
    {
        var cache = new PropertyAccessCache();
        var target = new Sample();

        var found = cache.Set(target, nameof(Sample.Count), 42);

        Assert.True(found);
        Assert.Equal(42, target.Count);
    }

    [Fact]
    public void Get_ReturnsFalseForMissingProperty()
    {
        var cache = new PropertyAccessCache();
        var target = new Sample();

        var found = cache.Get(target, "Missing", out var value);

        Assert.False(found);
        Assert.Null(value);
    }

    [Fact]
    public void Clear_AllowsTypeToBeMappedAgain()
    {
        var cache = new PropertyAccessCache();
        var target = new Sample { Name = "before" };
        Assert.True(cache.Get(target, nameof(Sample.Name), out _));

        cache.Clear();
        target.Name = "after";
        var found = cache.Get(target, nameof(Sample.Name), out var value);

        Assert.True(found);
        Assert.Equal("after", value);
    }

    private sealed class Sample
    {
        public string Name { get; set; } = string.Empty;

        public int Count { get; set; }
    }
}
