namespace Inamsoft.Libs.MetadataProviders;

/// <summary>
/// Provides services used to get metadata information from files.
/// </summary>
/// <typeparam name="TMetadata">The type of the metadata container object.</typeparam>
public interface IMetadataProvider<TMetadata>
{
    /// <summary>
    /// Obtains and returns the metadata information from the specified file.
    /// </summary>
    /// <param name="filePath">The full path of the file from which metadata to obtain.</param>
    /// <returns>A <typeparamref name="TMetadata"/> object that contains the obtained metadata information.</returns>
    TMetadata ReadMetadata(string filePath);

    /// <summary>
    /// Reads metadata of type TMetadata from the specified file.
    /// </summary>
    /// <param name="fileInfo">The FileInfo that identifies the file containing the metadata.</param>
    /// <returns>The metadata read from the file as an instance of TMetadata.</returns>
    TMetadata ReadMetadata(FileInfo fileInfo);
}
