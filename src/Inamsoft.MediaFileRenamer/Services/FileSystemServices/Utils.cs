using System.Security.Cryptography;

namespace Inamsoft.MediaFileRenamer.Services.FileSystemServices;

internal static class Utils
{
    private static readonly Dictionary<string, string> _mimeByExtension =
    new(StringComparer.OrdinalIgnoreCase)
    {
        [".txt"] = "text/plain",
        [".log"] = "text/plain",
        [".md"] = "text/markdown",
        [".json"] = "application/json",
        [".xml"] = "application/xml",
        [".csv"] = "text/csv",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".gif"] = "image/gif",
        [".bmp"] = "image/bmp",
        [".webp"] = "image/webp",
        [".mp3"] = "audio/mpeg",
        [".wav"] = "audio/wav",
        [".mp4"] = "video/mp4",
        [".mkv"] = "video/x-matroska",
        [".zip"] = "application/zip",
        [".7z"] = "application/x-7z-compressed",
        [".rar"] = "application/x-rar-compressed",
        [".pdf"] = "application/pdf",
        [".doc"] = "application/msword",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".xls"] = "application/vnd.ms-excel",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".ppt"] = "application/vnd.ms-powerpoint",
        [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        [".dng"] = "image/x-adobe-dng",
        [".cr2"] = "image/x-canon-cr2",
        [".nef"] = "image/x-nikon-nef",
        [".arw"] = "image/x-sony-arw",
        [".rw2"] = "image/x-panasonic-rw2",
        [".orf"] = "image/x-olympus-orf",
        [".sr2"] = "image/x-sony-sr2",
        [".heic"] = "image/heic",
        [".mpv"] = "video/x-matroska",
        [".flac"] = "audio/flac",
        [".aac"] = "audio/aac",
        [".mov"] = "video/quicktime",
        [".avi"] = "video/x-msvideo",
        [".wmv"] = "video/x-ms-wmv",
        [".m4v"] = "video/x-m4v",
        [".m2ts"] = "video/vnd.dlna.mpeg-tts",
        [".m4a"] = "audio/mp4",
        [".flv"] = "video/x-flv",
        [".svg"] = "image/svg+xml",
        [".ico"] = "image/vnd.microsoft.icon",
        [".tiff"] = "image/tiff",
        [".tif"] = "image/tiff",
        [".mpg"] = "video/mpeg",
        [".mpeg"] = "video/mpeg",
        [".ogg"] = "audio/ogg",
        [".ogv"] = "video/ogg"
        
        // add more as needed
    };

    public static string GetMimeType(FileInfo file)
    {
        var ext = file.Extension;
        if (string.IsNullOrEmpty(ext))
            return "application/octet-stream";

        return _mimeByExtension.TryGetValue(ext, out var mime)
            ? mime
            : "application/octet-stream";
    }

    public static string GetFileIcon(string mimeType)
    {
        if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return "🖼️";
        if (mimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            return "🎵";
        if (mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            return "🎬";
        if (mimeType == "application/pdf")
            return "📕";
        if (mimeType is "application/zip" or "application/x-7z-compressed" or "application/x-rar-compressed")
            return "🗜️";
        if (mimeType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
            return "📄";
        if (mimeType.StartsWith("application/vnd.", StringComparison.OrdinalIgnoreCase))
            return "📊";

        return "📁"; // fallback
    }


    public static async Task<string> ComputeSha256Async(string path,
                                                     CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash);
    }
}
