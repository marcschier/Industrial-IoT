// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Swagger.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary/>
    public partial class TimeSeriesPoint
    {
        /// <summary>
        /// Initializes a new instance of the TimeSeriesPoint class.
        /// </summary>
        public TimeSeriesPoint()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the TimeSeriesPoint class.
        /// </summary>
        public TimeSeriesPoint(System.DateTime? timestamp = default(System.DateTime?), double? value = default(double?))
        {
            Timestamp = timestamp;
            Value = value;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "Timestamp")]
        public System.DateTime? Timestamp { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "Value")]
        public double? Value { get; set; }

    }
}