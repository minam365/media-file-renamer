using System;
using System.Collections.Generic;
using System.Text;

namespace Inamsoft.Libs.MetadataProviders.Abstractions
{
    public static class GpsLongitudeExtensions
    {
        /// <summary>
        /// Gets the first letter of ref. position of longitude point.
        /// </summary>
        /// <param name="longitude">The longitude value.</param>
        /// <returns>The first letter of longitude refrence position.</returns>
        public static string ToShortRefPosition(this GpsLongitude longitude)
        {
            if (longitude == null)
            {
                return string.Empty;
            }

            return longitude.RefPosition.ToString().Substring(0, 1);
        }
    }
}
