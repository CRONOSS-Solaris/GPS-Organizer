using System;
using System.Windows.Media;
using System.Globalization;

namespace GPS_Organizer.Utils
{
    public static class OnParseGpsString
    {
        public static ParsedGpsData ParseGpsString(string gpsString)
        {
            // Ensure the GPS string starts with "GPS:"
            if (!gpsString.StartsWith("GPS:"))
            {
                throw new FormatException("Invalid GPS string format.");
            }

            // Remove the "GPS:" prefix
            gpsString = gpsString.Substring(4);

            // Split the remaining string into its components
            var components = gpsString.Split(':');

            // Ensure we have the right number of components
            if (components.Length != 6)
            {
                throw new FormatException("Invalid GPS string format.");
            }

            // Parse and assign each component to a new ParsedGpsData object
            return new ParsedGpsData
            {
                Name = components[0],
                XCoord = double.Parse(components[1], CultureInfo.InvariantCulture),
                YCoord = double.Parse(components[2], CultureInfo.InvariantCulture),
                ZCoord = double.Parse(components[3], CultureInfo.InvariantCulture),
                Color = (Color)ColorConverter.ConvertFromString(components[4].TrimEnd(':'))
            };
        }

        public class ParsedGpsData
        {
            public string Name { get; set; }
            public double XCoord { get; set; }
            public double YCoord { get; set; }
            public double ZCoord { get; set; }
            public Color Color { get; set; }
        }
    }
}
