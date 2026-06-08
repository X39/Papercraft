namespace X39.Solutions.PdfTemplate.Test.ExpressionTests;

internal static class TransformerAssert
{
    public static async Task<TException> ThrowsDirectOrWrappedAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        var exception = await Assert.ThrowsAnyAsync<Exception>(action);
        var expectedException = FindException<TException>(exception);
        Assert.NotNull(expectedException);
        return expectedException;
    }

    private static TException? FindException<TException>(Exception exception)
        where TException : Exception
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is TException expectedException)
                return expectedException;
        }

        return null;
    }
}
