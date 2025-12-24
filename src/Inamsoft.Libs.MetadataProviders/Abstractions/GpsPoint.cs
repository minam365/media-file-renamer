namespace Inamsoft.Libs.MetadataProviders.Abstractions
{
    /// <summary>
    /// A container used to store the GPS coordinate point.
    /// </summary>
    public abstract record GpsPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GpsPoint"/> class.
        /// </summary>
        protected GpsPoint()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpsPoint"/> class.
        /// </summary>
        /// <param name="degrees">The degrees value of the GPS coordinate.</param>
        /// <param name="minutes">The minutes value of the GPS coordinate.</param>
        /// <param name="seconds">The seconds value of the GPS coordinate.</param>
        protected GpsPoint(double degrees, double minutes, double seconds)
        {
            Degrees = degrees;
            Minutes = minutes;
            Seconds = seconds;
        }

        /// <summary>
        /// Gets or sets the degrees value of the GPS coordinate
        /// </summary>
        public double Degrees { get; set; }

        /// <summary>
        /// Gets or sets the minutes value of the GPS coordinate
        /// </summary>

        public double Minutes { get; set; }

        /// <summary>
        /// Gets or sets the seconds value of the GPS coordinate
        /// </summary>
        public double Seconds { get; set; }


        /// <summary>
        /// Converts and returns the decimal representation of the coordinate.
        /// </summary>
        /// <returns>A <see cref="double"/> value that represents the point being in Degrees, Minutes, Seconds form.</returns>
        public double ToDecimal()
        {
            var doubleVal = Degrees;
            if (Minutes > 0)
                doubleVal += (Minutes / 60.0D);
            if(Seconds>0)
                doubleVal += (Seconds / 3600.0D);

            if (IsNegativePoint())
                doubleVal *= -1.0D;

            return doubleVal;
        }

        /// <summary>
        /// Determines whether the point 
        /// lies on the South hemisphere or on the West side of the meridian.
        /// </summary>
        /// <returns><code>true</code> if the point lies on the South hemisphere or on the West side of the meridian; 
        /// otherwise, <code>false</code>.</returns>
        internal abstract bool IsNegativePoint();

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Degrees}° {Minutes}' {Seconds}\"";
        }

    }
}
