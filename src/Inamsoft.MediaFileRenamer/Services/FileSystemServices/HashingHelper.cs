using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Inamsoft.MediaFileRenamer.Services.FileSystemServices;

internal static class HashingHelper
{
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
