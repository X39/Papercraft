using System.Buffers.Binary;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Services;

internal static class EncodedImageSizeReader
{
    private static ReadOnlySpan<byte> PngSignature => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static ReadOnlySpan<byte> PngHeaderChunkType => [0x49, 0x48, 0x44, 0x52];
    private static ReadOnlySpan<byte> Gif87Signature => [0x47, 0x49, 0x46, 0x38, 0x37, 0x61];
    private static ReadOnlySpan<byte> Gif89Signature => [0x47, 0x49, 0x46, 0x38, 0x39, 0x61];

    internal static Size GetSize(ReadOnlySpan<byte> bytes)
    {
        if (HasSignature(bytes, PngSignature))
            return ReadPngSize(bytes);
        if (IsJpeg(bytes))
            return ReadJpegSize(bytes);
        if (HasSignature(bytes, Gif87Signature) || HasSignature(bytes, Gif89Signature))
            return ReadGifSize(bytes);
        if (IsBmp(bytes))
            return ReadBmpSize(bytes);

        throw new NotSupportedException("Unsupported image format. Supported image formats are PNG, JPEG, GIF, and BMP.");
    }

    private static Size ReadPngSize(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 24)
            throw new InvalidDataException("Invalid PNG image data: the IHDR chunk is incomplete.");
        if (BinaryPrimitives.ReadUInt32BigEndian(bytes[8..12]) is not 13
            || !bytes[12..16].SequenceEqual(PngHeaderChunkType))
            throw new InvalidDataException("Invalid PNG image data: the first chunk must be IHDR.");

        var width = BinaryPrimitives.ReadUInt32BigEndian(bytes[16..20]);
        var height = BinaryPrimitives.ReadUInt32BigEndian(bytes[20..24]);
        return CreateSize(width, height, "PNG");
    }

    private static Size ReadJpegSize(ReadOnlySpan<byte> bytes)
    {
        var offset = 2;
        while (offset < bytes.Length)
        {
            if (bytes[offset] is not 0xFF)
                throw new InvalidDataException("Invalid JPEG image data: expected a marker.");

            while (offset < bytes.Length && bytes[offset] is 0xFF)
                offset++;
            if (offset >= bytes.Length)
                break;

            var marker = bytes[offset++];
            if (marker is 0xD9 or 0xDA)
                break;
            if (marker is 0x01 || marker is >= 0xD0 and <= 0xD7)
                continue;

            if (offset + 2 > bytes.Length)
                throw new InvalidDataException("Invalid JPEG image data: segment length is incomplete.");

            var segmentLength = BinaryPrimitives.ReadUInt16BigEndian(bytes[offset..(offset + 2)]);
            offset += 2;
            if (segmentLength < 2)
                throw new InvalidDataException("Invalid JPEG image data: segment length is invalid.");

            var payloadLength = segmentLength - 2;
            if (offset + payloadLength > bytes.Length)
                throw new InvalidDataException("Invalid JPEG image data: segment payload is incomplete.");

            if (IsJpegStartOfFrameMarker(marker))
            {
                if (payloadLength < 5)
                    throw new InvalidDataException("Invalid JPEG image data: size header is incomplete.");

                var height = BinaryPrimitives.ReadUInt16BigEndian(bytes[(offset + 1)..(offset + 3)]);
                var width = BinaryPrimitives.ReadUInt16BigEndian(bytes[(offset + 3)..(offset + 5)]);
                return CreateSize(width, height, "JPEG");
            }

            offset += payloadLength;
        }

        throw new InvalidDataException("Invalid JPEG image data: no size header was found.");
    }

    private static Size ReadGifSize(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 10)
            throw new InvalidDataException("Invalid GIF image data: the logical screen descriptor is incomplete.");

        var width = BinaryPrimitives.ReadUInt16LittleEndian(bytes[6..8]);
        var height = BinaryPrimitives.ReadUInt16LittleEndian(bytes[8..10]);
        return CreateSize(width, height, "GIF");
    }

    private static Size ReadBmpSize(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 18)
            throw new InvalidDataException("Invalid BMP image data: the DIB header is incomplete.");

        var dibHeaderSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes[14..18]);
        if (dibHeaderSize is 12)
        {
            if (bytes.Length < 22)
                throw new InvalidDataException("Invalid BMP image data: the BITMAPCOREHEADER dimensions are incomplete.");

            var width = BinaryPrimitives.ReadUInt16LittleEndian(bytes[18..20]);
            var height = BinaryPrimitives.ReadUInt16LittleEndian(bytes[20..22]);
            return CreateSize(width, height, "BMP");
        }

        if (dibHeaderSize < 40)
            throw new NotSupportedException("Unsupported BMP DIB header. Supported BMP headers are BITMAPCOREHEADER and BITMAPINFOHEADER-compatible headers.");
        if (bytes.Length < 26)
            throw new InvalidDataException("Invalid BMP image data: the BITMAPINFOHEADER dimensions are incomplete.");

        var infoWidth = BinaryPrimitives.ReadInt32LittleEndian(bytes[18..22]);
        var infoHeight = BinaryPrimitives.ReadInt32LittleEndian(bytes[22..26]);
        if (infoHeight is int.MinValue)
            throw new InvalidDataException("Invalid BMP image data: height is outside the supported range.");

        return CreateSize(infoWidth, Math.Abs(infoHeight), "BMP");
    }

    private static bool HasSignature(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> signature)
        => bytes.Length >= signature.Length && bytes[..signature.Length].SequenceEqual(signature);

    private static bool IsJpeg(ReadOnlySpan<byte> bytes)
        => bytes.Length >= 2 && bytes[0] is 0xFF && bytes[1] is 0xD8;

    private static bool IsBmp(ReadOnlySpan<byte> bytes)
        => bytes.Length >= 2 && bytes[0] is 0x42 && bytes[1] is 0x4D;

    private static bool IsJpegStartOfFrameMarker(byte marker)
        => marker is >= 0xC0 and <= 0xCF
           && marker is not 0xC4
           && marker is not 0xC8
           && marker is not 0xCC;

    private static Size CreateSize(long width, long height, string format)
    {
        if (width <= 0 || height <= 0)
            throw new InvalidDataException($"Invalid {format} image data: dimensions must be positive.");
        if (width > int.MaxValue || height > int.MaxValue)
            throw new NotSupportedException($"{format} image dimensions exceed the supported range.");

        return new Size((float) width, (float) height);
    }
}
