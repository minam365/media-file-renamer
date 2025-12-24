namespace Inamsoft.Libs.MetadataProviders.Abstractions
{
    public record GpsLatitude : GpsPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GpsLatitude"/> class.
        /// </summary>
        public GpsLatitude()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpsLatitude"/> class.
        /// </summary>
        /// <param name="degrees">The degrees value of the GPS coordinate.</param>
        /// <param name="minutes">The minutes value of the GPS coordinate.</param>
        /// <param name="seconds">The seconds value of the GPS coordinate.</param>
        public GpsLatitude(double degrees, double minutes, double seconds) : base(degrees, minutes, seconds)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpsLatitude"/> class.
        /// </summary>
        /// <param name="degrees">The degrees value of the GPS coordinate.</param>
        /// <param name="minutes">The minutes value of the GPS coordinate.</param>
        /// <param name="seconds">The seconds value of the GPS coordinate.</param>
        /// <param name="refPosition">The reference position of the point.</param>
        public GpsLatitude(double degrees, double minutes, double seconds, LatitudeRefPosition refPosition) : base(degrees, minutes, seconds)
        {
            RefPosition = refPosition;
        }

        /// <summary>
        /// Gets or sets the reference position of the point which is North or South.
        /// </summary>
        public LatitudeRefPosition RefPosition { get; set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var str = base.ToString();

            return $"{str} {this.ToShortRefPosition()}";
        }

        /// <summary>
        /// Determines whether the point
        /// lies on the South hemisphere or on the West side of the meridian.
        /// </summary>
        /// <returns><code>true</code> if the pointl ies on the South hemisphere or on the West side of the meridian;
        /// otherwise, <code>false</code>.</returns>
        internal override bool IsNegativePoint()
        {
            return RefPosition == LatitudeRefPosition.South;
        }


    }
}
