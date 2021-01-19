// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Swagger.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary/>
    public partial class ChangePointDetectResponse
    {
        /// <summary>
        /// Initializes a new instance of the ChangePointDetectResponse class.
        /// </summary>
        public ChangePointDetectResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ChangePointDetectResponse class.
        /// </summary>
        /// <param name="period">Frequency extracted from the series, zero
        /// means no recurrent pattern has been found.</param>
        /// <param name="isChangePoint">isChangePoint contains change point
        /// properties for each input point.
        /// True means an anomaly either negative or positive has been
        /// detected.
        /// The index of the array is consistent with the input series.</param>
        /// <param name="confidenceScores">the change point confidence of each
        /// point.</param>
        public ChangePointDetectResponse(int? period = default(int?), IList<bool?> isChangePoint = default(IList<bool?>), IList<double?> confidenceScores = default(IList<double?>))
        {
            Period = period;
            IsChangePoint = isChangePoint;
            ConfidenceScores = confidenceScores;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets frequency extracted from the series, zero means no
        /// recurrent pattern has been found.
        /// </summary>
        [JsonProperty(PropertyName = "Period")]
        public int? Period { get; set; }

        /// <summary>
        /// Gets or sets isChangePoint contains change point properties for
        /// each input point.
        /// True means an anomaly either negative or positive has been
        /// detected.
        /// The index of the array is consistent with the input series.
        /// </summary>
        [JsonProperty(PropertyName = "IsChangePoint")]
        public IList<bool?> IsChangePoint { get; set; }

        /// <summary>
        /// Gets or sets the change point confidence of each point.
        /// </summary>
        [JsonProperty(PropertyName = "ConfidenceScores")]
        public IList<double?> ConfidenceScores { get; set; }

    }
}