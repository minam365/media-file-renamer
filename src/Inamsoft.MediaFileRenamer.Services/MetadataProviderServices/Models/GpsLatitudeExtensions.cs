namespace Inamsoft.MediaFileRenamer.Services.MetadataProviderServices.Models
{
    public static class GpsLatitudeExtensions
    {
        

        /// <summary>
        /// Gets the first letter of ref. position of latitude point.
        /// </summary>
        /// <param name="latitude">The latitude value.</param>
        /// <returns>The first letter of latitude reference position.</returns>
        public static string ToShortRefPosition(this GpsLatitude latitude)
        {
            return latitude.RefPosition.ToString()[..1];
        }
    }
}
