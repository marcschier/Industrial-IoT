/* 
 * Anomaly Detector Cognitive Service API
 *
 * The Anomaly Detector Service detects anomalies automatically in time series data. It supports two functionalities, one is for detecting the whole series with model trained by the time series, another is detecting last point with model trained by points before. By using this service, developers can discover incidents and establish a logic flow for root cause analysis.
 *
 * OpenAPI spec version: v1
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using SwaggerDateConverter = IO.Swagger.Client.SwaggerDateConverter;

namespace IO.Swagger.Model
{
    /// <summary>
    /// EntireDetectResponse
    /// </summary>
    [DataContract]
    public partial class EntireDetectResponse :  IEquatable<EntireDetectResponse>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntireDetectResponse" /> class.
        /// </summary>
        /// <param name="period">Gets or sets frequency extracted from the series, zero means no  recurrent pattern has been found..</param>
        /// <param name="expectedValues">Gets or sets expectedValues contain expected value for each input  point. The index of the array is consistent with the input series..</param>
        /// <param name="upperMargins">Gets or sets upperMargins contain upper margin of each input point.  UpperMargin is used to calculate upperBoundary, which equals to  expectedValue + (100 - marginScale)*upperMargin. Anomalies in  response can be filtered by upperBoundary and lowerBoundary. By  adjusting marginScale value, less significant anomalies can be  filtered in client side. The index of the array is consistent with  the input series..</param>
        /// <param name="lowerMargins">Gets or sets lowerMargins contain lower margin of each input point.  LowerMargin is used to calculate lowerBoundary, which equals to  expectedValue - (100 - marginScale)*lowerMargin. Points between the  boundary can be marked as normal ones in client side. The index of  the array is consistent with the input series..</param>
        /// <param name="isAnomaly">Gets or sets isAnomaly contains anomaly properties for each input  point. True means an anomaly either negative or positive has been  detected. The index of the array is consistent with the input  series..</param>
        /// <param name="isNegativeAnomaly">Gets or sets isNegativeAnomaly contains anomaly status in negative  direction for each input point. True means a negative anomaly has  been detected. A negative anomaly means the point is detected as an  anomaly and its real value is smaller than the expected one. The  index of the array is consistent with the input series..</param>
        /// <param name="isPositiveAnomaly">Gets or sets isPositiveAnomaly contain anomaly status in positive  direction for each input point. True means a positive anomaly has  been detected. A positive anomaly means the point is detected as an  anomaly and its real value is larger than the expected one. The  index of the array is consistent with the input series..</param>
        public EntireDetectResponse(int? period = default(int?), List<double?> expectedValues = default(List<double?>), List<double?> upperMargins = default(List<double?>), List<double?> lowerMargins = default(List<double?>), List<bool?> isAnomaly = default(List<bool?>), List<bool?> isNegativeAnomaly = default(List<bool?>), List<bool?> isPositiveAnomaly = default(List<bool?>))
        {
            this.Period = period;
            this.ExpectedValues = expectedValues;
            this.UpperMargins = upperMargins;
            this.LowerMargins = lowerMargins;
            this.IsAnomaly = isAnomaly;
            this.IsNegativeAnomaly = isNegativeAnomaly;
            this.IsPositiveAnomaly = isPositiveAnomaly;
        }
        
        /// <summary>
        /// Gets or sets frequency extracted from the series, zero means no  recurrent pattern has been found.
        /// </summary>
        /// <value>Gets or sets frequency extracted from the series, zero means no  recurrent pattern has been found.</value>
        [DataMember(Name="Period", EmitDefaultValue=false)]
        public int? Period { get; set; }

        /// <summary>
        /// Gets or sets expectedValues contain expected value for each input  point. The index of the array is consistent with the input series.
        /// </summary>
        /// <value>Gets or sets expectedValues contain expected value for each input  point. The index of the array is consistent with the input series.</value>
        [DataMember(Name="ExpectedValues", EmitDefaultValue=false)]
        public List<double?> ExpectedValues { get; set; }

        /// <summary>
        /// Gets or sets upperMargins contain upper margin of each input point.  UpperMargin is used to calculate upperBoundary, which equals to  expectedValue + (100 - marginScale)*upperMargin. Anomalies in  response can be filtered by upperBoundary and lowerBoundary. By  adjusting marginScale value, less significant anomalies can be  filtered in client side. The index of the array is consistent with  the input series.
        /// </summary>
        /// <value>Gets or sets upperMargins contain upper margin of each input point.  UpperMargin is used to calculate upperBoundary, which equals to  expectedValue + (100 - marginScale)*upperMargin. Anomalies in  response can be filtered by upperBoundary and lowerBoundary. By  adjusting marginScale value, less significant anomalies can be  filtered in client side. The index of the array is consistent with  the input series.</value>
        [DataMember(Name="UpperMargins", EmitDefaultValue=false)]
        public List<double?> UpperMargins { get; set; }

        /// <summary>
        /// Gets or sets lowerMargins contain lower margin of each input point.  LowerMargin is used to calculate lowerBoundary, which equals to  expectedValue - (100 - marginScale)*lowerMargin. Points between the  boundary can be marked as normal ones in client side. The index of  the array is consistent with the input series.
        /// </summary>
        /// <value>Gets or sets lowerMargins contain lower margin of each input point.  LowerMargin is used to calculate lowerBoundary, which equals to  expectedValue - (100 - marginScale)*lowerMargin. Points between the  boundary can be marked as normal ones in client side. The index of  the array is consistent with the input series.</value>
        [DataMember(Name="LowerMargins", EmitDefaultValue=false)]
        public List<double?> LowerMargins { get; set; }

        /// <summary>
        /// Gets or sets isAnomaly contains anomaly properties for each input  point. True means an anomaly either negative or positive has been  detected. The index of the array is consistent with the input  series.
        /// </summary>
        /// <value>Gets or sets isAnomaly contains anomaly properties for each input  point. True means an anomaly either negative or positive has been  detected. The index of the array is consistent with the input  series.</value>
        [DataMember(Name="IsAnomaly", EmitDefaultValue=false)]
        public List<bool?> IsAnomaly { get; set; }

        /// <summary>
        /// Gets or sets isNegativeAnomaly contains anomaly status in negative  direction for each input point. True means a negative anomaly has  been detected. A negative anomaly means the point is detected as an  anomaly and its real value is smaller than the expected one. The  index of the array is consistent with the input series.
        /// </summary>
        /// <value>Gets or sets isNegativeAnomaly contains anomaly status in negative  direction for each input point. True means a negative anomaly has  been detected. A negative anomaly means the point is detected as an  anomaly and its real value is smaller than the expected one. The  index of the array is consistent with the input series.</value>
        [DataMember(Name="IsNegativeAnomaly", EmitDefaultValue=false)]
        public List<bool?> IsNegativeAnomaly { get; set; }

        /// <summary>
        /// Gets or sets isPositiveAnomaly contain anomaly status in positive  direction for each input point. True means a positive anomaly has  been detected. A positive anomaly means the point is detected as an  anomaly and its real value is larger than the expected one. The  index of the array is consistent with the input series.
        /// </summary>
        /// <value>Gets or sets isPositiveAnomaly contain anomaly status in positive  direction for each input point. True means a positive anomaly has  been detected. A positive anomaly means the point is detected as an  anomaly and its real value is larger than the expected one. The  index of the array is consistent with the input series.</value>
        [DataMember(Name="IsPositiveAnomaly", EmitDefaultValue=false)]
        public List<bool?> IsPositiveAnomaly { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class EntireDetectResponse {\n");
            sb.Append("  Period: ").Append(Period).Append("\n");
            sb.Append("  ExpectedValues: ").Append(ExpectedValues).Append("\n");
            sb.Append("  UpperMargins: ").Append(UpperMargins).Append("\n");
            sb.Append("  LowerMargins: ").Append(LowerMargins).Append("\n");
            sb.Append("  IsAnomaly: ").Append(IsAnomaly).Append("\n");
            sb.Append("  IsNegativeAnomaly: ").Append(IsNegativeAnomaly).Append("\n");
            sb.Append("  IsPositiveAnomaly: ").Append(IsPositiveAnomaly).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as EntireDetectResponse);
        }

        /// <summary>
        /// Returns true if EntireDetectResponse instances are equal
        /// </summary>
        /// <param name="input">Instance of EntireDetectResponse to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(EntireDetectResponse input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Period == input.Period ||
                    (this.Period != null &&
                    this.Period.Equals(input.Period))
                ) && 
                (
                    this.ExpectedValues == input.ExpectedValues ||
                    this.ExpectedValues != null &&
                    this.ExpectedValues.SequenceEqual(input.ExpectedValues)
                ) && 
                (
                    this.UpperMargins == input.UpperMargins ||
                    this.UpperMargins != null &&
                    this.UpperMargins.SequenceEqual(input.UpperMargins)
                ) && 
                (
                    this.LowerMargins == input.LowerMargins ||
                    this.LowerMargins != null &&
                    this.LowerMargins.SequenceEqual(input.LowerMargins)
                ) && 
                (
                    this.IsAnomaly == input.IsAnomaly ||
                    this.IsAnomaly != null &&
                    this.IsAnomaly.SequenceEqual(input.IsAnomaly)
                ) && 
                (
                    this.IsNegativeAnomaly == input.IsNegativeAnomaly ||
                    this.IsNegativeAnomaly != null &&
                    this.IsNegativeAnomaly.SequenceEqual(input.IsNegativeAnomaly)
                ) && 
                (
                    this.IsPositiveAnomaly == input.IsPositiveAnomaly ||
                    this.IsPositiveAnomaly != null &&
                    this.IsPositiveAnomaly.SequenceEqual(input.IsPositiveAnomaly)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.Period != null)
                    hashCode = hashCode * 59 + this.Period.GetHashCode();
                if (this.ExpectedValues != null)
                    hashCode = hashCode * 59 + this.ExpectedValues.GetHashCode();
                if (this.UpperMargins != null)
                    hashCode = hashCode * 59 + this.UpperMargins.GetHashCode();
                if (this.LowerMargins != null)
                    hashCode = hashCode * 59 + this.LowerMargins.GetHashCode();
                if (this.IsAnomaly != null)
                    hashCode = hashCode * 59 + this.IsAnomaly.GetHashCode();
                if (this.IsNegativeAnomaly != null)
                    hashCode = hashCode * 59 + this.IsNegativeAnomaly.GetHashCode();
                if (this.IsPositiveAnomaly != null)
                    hashCode = hashCode * 59 + this.IsPositiveAnomaly.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}
