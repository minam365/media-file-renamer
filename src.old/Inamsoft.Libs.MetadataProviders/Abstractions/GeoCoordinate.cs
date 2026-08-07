using CommunityToolkit.Diagnostics;
using System.Globalization;

namespace Inamsoft.Libs.MetadataProviders.Abstractions
{
    /// <summary>
    /// A class that defines location GeoCoordinate value.
    /// </summary>
    public record GeoCoordinate
    {
        #region Private Properties

        private double _latitude, _longitude;
        private GpsPoint _latitudePoint, _longitudePoint;

        #endregion

        #region Constructor

        /// <summary>
        /// A location GeoCoordinate.
        /// </summary>
        public GeoCoordinate()
        {
        }

        /// <summary>
        /// A location GeoCoordinate.
        /// </summary>
        /// <param name="latitude">Latitude coordinate vlaue.</param>
        /// <param name="longitude">Longitude coordinate value.</param>
        public GeoCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoCoordinate"/> class.
        /// </summary>
        /// <param name="latitude">Latitude coordinate vlaue.</param>
        /// <param name="longitude">Longitude coordinate value.</param>
        public GeoCoordinate(GpsPoint latitude, GpsPoint longitude)
        {
            Guard.IsNotNull(latitude, nameof(latitude));
            Guard.IsNotNull(longitude, nameof(longitude));

            _latitudePoint = latitude;
            _longitudePoint = longitude;

            Latitude = latitude.ToDecimal();
            Longitude = longitude.ToDecimal();

        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Latitude GeoCoordinate.
        /// </summary>
        public double Latitude
        {
            get
            {
                return _latitude;
            }
            set
            {
                if (!double.IsNaN(value) && value <= 90 && value >= -90)
                {
                    //Only need to keep the first 5 decimal places. Any more just adds more data being passed around.
                    _latitude = Math.Round(value, 5, MidpointRounding.AwayFromZero);
                }
            }
        }

        /// <summary>
        /// Longitude GeoCoordinate.
        /// </summary>
        public double Longitude
        {
            get
            {
                return _longitude;
            }
            set
            {
                if (!double.IsNaN(value) && value <= 180 && value >= -180)
                {
                    //Only need to keep the first 5 decimal places. Any more just adds more data being passed around.
                    _longitude = Math.Round(value, 5, MidpointRounding.AwayFromZero);
                }
            }
        }

        #endregion


        /// <summary>
        /// Returns a formatted string of the GeoCoordinate in the format "latitude,longitude", with the values rounded off to 5 decimal places.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.#####},{1:0.#####}", Latitude, Longitude);
        }

    }
}
