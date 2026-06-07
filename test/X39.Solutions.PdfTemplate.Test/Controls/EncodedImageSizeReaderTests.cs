using System.Buffers.Binary;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class EncodedImageSizeReaderTests
{
    [Fact]
    public void GetSizeReadsPngHeader()
    {
        var bytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAQAAAACCAIAAADwyuo0AAAAJUlEQVR4AQEaAOX/AAAA/wAA/wAA/wAA/wAAAP8AAP8AAP8AAP9fugf5AHw6kwAAAABJRU5ErkJggg==");

        Assert.Equal(new Size(4, 2), EncodedImageSizeReader.GetSize(bytes));
    }

    [Fact]
    public void GetSizeReadsJpegStartOfFrameHeader()
    {
        byte[] bytes =
        [
            0xFF, 0xD8,
            0xFF, 0xC0,
            0x00, 0x0B,
            0x08,
            0x00, 0x0D,
            0x00, 0x0B,
            0x01,
            0x01, 0x11, 0x00,
        ];

        Assert.Equal(new Size(11, 13), EncodedImageSizeReader.GetSize(bytes));
    }

    [Fact]
    public void GetSizeReadsGifLogicalScreenDescriptor()
    {
        byte[] bytes = [0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0, 0, 0, 0];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(6, 2), 7);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(8, 2), 5);

        Assert.Equal(new Size(7, 5), EncodedImageSizeReader.GetSize(bytes));
    }

    [Fact]
    public void GetSizeReadsBmpInfoHeader()
    {
        var bytes = new byte[26];
        bytes[0] = 0x42;
        bytes[1] = 0x4D;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(14, 4), 40);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(18, 4), 17);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(22, 4), -19);

        Assert.Equal(new Size(17, 19), EncodedImageSizeReader.GetSize(bytes));
    }

    [Fact]
    public void GetSizeRejectsUnsupportedImageFormat()
    {
        var exception = Assert.Throws<NotSupportedException>(() => EncodedImageSizeReader.GetSize([1, 2, 3, 4]));

        Assert.Contains("Unsupported image format", exception.Message, StringComparison.Ordinal);
    }
}
