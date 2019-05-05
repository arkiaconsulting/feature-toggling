using System;
using System.Collections.Generic;
using System.Text;

namespace FeatureToggling.Services
{
    public class WeatherDto
    {
        public string Source { get; set; }
        public string Temperature { get; set; }
        public int CloudCover { get; set; }
    }
}
