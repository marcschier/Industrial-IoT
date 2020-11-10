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
    /// LastDetectResponse
    /// </summary>
    [DataContract]
    public partial class LastDetectResponse :  IEquatable<LastDetectResponse>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LastDetectResponse" /> class.
        /// </summary>
        /// <param name="period">Gets or sets frequency extracted from the series, zero means no  recurrent pattern has been found..</param>
        /// <param name="suggestedWindow">Gets or sets suggested input series points needed for detecting the  latest point..</param>
        /// <param name="expectedValue">Gets or sets expected value of the latest point..</param>
        /// <param name="upperMargin">Gets or sets upper margin of the latest point. UpperMargin is used  to calculate upperBoundary, which equals to expectedValue + (100 -  marginScale)*upperMargin. If the value of latest point is between  upperBoundary and lowerBoundary, it should be treated as normal  value. By adjusting marginScale value, anomaly status of latest  point can be changed..</param>
        /// <param name="lowerMargin">Gets or sets lower margin of the latest point. LowerMargin is used  to calculate lowerBoundary, which equals to expectedValue - (100 -  marginScale)*lowerMargin..</param>
        /// <param name="isAnomaly">Gets or sets anomaly status of the latest point, true means the  latest point is an anomaly either in negative direction or positive  direction..</param>
        /// <param name="isNegativeAnomaly">Gets or sets anomaly status in negative direction of the latest  point. True means the latest point is an anomaly and its real value  is smaller than the expected one..</param>
        /// <param name="isPositiveAnomaly">Gets or sets anomaly status in positive direction of the latest  point. True means the latest point is an anomaly and its real value  is larger than the expected one..</param>
        public LastDetectResponse(int? period = default(int?), int? suggestedWindow = default(int?), double? expectedValue = default(double?), double? upperMargin = default(double?), double? lowerMargin = default(double?), bool? isAnomaly = default(bool?), bool? isNegativeAnomaly = default(bool?), bool? isPositiveAnomaly = default(bool?))
        {
            this.Period = period;
            this.SuggestedWindow = suggestedWindow;
            this.ExpectedValue = expectedValue;
            this.UpperMargin = upperMargin;
            this.LowerMargin = lowerMargin;
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
        /// Gets or sets suggested input series points needed for detecting the  latest point.
        /// </summary>
        /// <value>Gets or sets suggested input series points needed for detecting the  latest point.</value>
        [DataMember(Name="SuggestedWindow", EmitDefaultValue=false)]
        public int? SuggestedWindow { get; set; }

        /// <summary>
        /// Gets or sets expected value of the latest point.
        /// </summary>
        /// <value>Gets or sets expected value of the latest point.</value>
        [DataMember(Name="ExpectedValue", EmitDefaultValue=false)]
        public double? ExpectedValue { get; set; }

        /// <summary>
        /// Gets or sets upper margin of the latest point. UpperMargin is used  to calculate upperBoundary, which equals to expectedValue + (100 -  marginScale)*upperMargin. If the value of latest point is between  upperBoundary and lowerBoundary, it should be treated as normal  value. By adjusting marginScale value, anomaly status of latest  point can be changed.
        /// </summary>
        /// <value>Gets or sets upper margin of the latest point. UpperMargin is used  to calculate upperBoundary, which equals to expectedValue + (100 -  marginScale)*upperMargin. If the value of latest point is between  upperBoundary and lowerBoundary, it should be treated as normal  value. By adjusting marginScale value, anomaly status of latest  point can be changed.</value>
        [DataMember(Name="UpperMargin", EmitDefaultValue=false)]
        public double? UpperMargin { get; set; }

        /// <summary>
        /// Gets or sets lower margin of the latest point. LowerMargin is used  to calculate lowerBoundary, which equals to expectedValue - (100 -  marginScale)*lowerMargin.
        /// </summary>
        /// <value>Gets or sets lower margin of the latest point. LowerMargin is used  to calculate lowerBoundary, which equals to expectedValue - (100 -  marginScale)*lowerMargin.</value>
        [DataMember(Name="LowerMargin", EmitDefaultValue=false)]
        public double? LowerMargin { get; set; }

        /// <summary>
        /// Gets or sets anomaly status of the latest point, true means the  latest point is an anomaly either in negative direction or positive  direction.
        /// </summary>
        /// <value>Gets or sets anomaly status of the latest point, true means the  latest point is an anomaly either in negative direction or positive  direction.</value>
        [DataMember(Name="IsAnomaly", EmitDefaultValue=false)]
        public bool? IsAnomaly { get; set; }

        /// <summary>
        /// Gets or sets anomaly status in negative direction of the latest  point. True means the latest point is an anomaly and its real value  is smaller than the expected one.
        /// </summary>
        /// <value>Gets or sets anomaly status in negative direction of the latest  point. True means the latest point is an anomaly and its real value  is smaller than the expected one.</value>
        [DataMember(Name="IsNegativeAnomaly", EmitDefaultValue=false)]
        public bool? IsNegativeAnomaly { get; set; }

        /// <summary>
        /// Gets or sets anomaly status in positive direction of the latest  point. True means the latest point is an anomaly and its real value  is larger than the expected one.
        /// </summary>
        /// <value>Gets or sets anomaly status in positive direction of the latest  point. True means the latest point is an anomaly and its real value  is larger than the expected one.</value>
        [DataMember(Name="IsPositiveAnomaly", EmitDefaultValue=false)]
        public bool? IsPositiveAnomaly { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class LastDetectResponse {\n");
            sb.Append("  Period: ").Append(Period).Append("\n");
            sb.Append("  SuggestedWindow: ").Append(SuggestedWindow).Append("\n");
            sb.Append("  ExpectedValue: ").Append(ExpectedValue).Append("\n");
            sb.Append("  UpperMargin: ").Append(UpperMargin).Append("\n");
            sb.Append("  LowerMargin: ").Append(LowerMargin).Append("\n");
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
            return this.Equals(input as LastDetectResponse);
        }

        /// <summary>
        /// Returns true if LastDetectResponse instances are equal
        /// </summary>
        /// <param name="input">Instance of LastDetectResponse to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(LastDetectResponse input)
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
                    this.SuggestedWindow == input.SuggestedWindow ||
                    (this.SuggestedWindow != null &&
                    this.SuggestedWindow.Equals(input.SuggestedWindow))
                ) && 
                (
                    this.ExpectedValue == input.ExpectedValue ||
                    (this.ExpectedValue != null &&
                    this.ExpectedValue.Equals(input.ExpectedValue))
                ) && 
                (
                    this.UpperMargin == input.UpperMargin ||
                    (this.UpperMargin != null &&
                    this.UpperMargin.Equals(input.UpperMargin))
                ) && 
                (
                    this.LowerMargin == input.LowerMargin ||
                    (this.LowerMargin != null &&
                    this.LowerMargin.Equals(input.LowerMargin))
                ) && 
                (
                    this.IsAnomaly == input.IsAnomaly ||
                    (this.IsAnomaly != null &&
                    this.IsAnomaly.Equals(input.IsAnomaly))
                ) && 
                (
                    this.IsNegativeAnomaly == input.IsNegativeAnomaly ||
                    (this.IsNegativeAnomaly != null &&
                    this.IsNegativeAnomaly.Equals(input.IsNegativeAnomaly))
                ) && 
                (
                    this.IsPositiveAnomaly == input.IsPositiveAnomaly ||
                    (this.IsPositiveAnomaly != null &&
                    this.IsPositiveAnomaly.Equals(input.IsPositiveAnomaly))
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
                if (this.SuggestedWindow != null)
                    hashCode = hashCode * 59 + this.SuggestedWindow.GetHashCode();
                if (this.ExpectedValue != null)
                    hashCode = hashCode * 59 + this.ExpectedValue.GetHashCode();
                if (this.UpperMargin != null)
                    hashCode = hashCode * 59 + this.UpperMargin.GetHashCode();
                if (this.LowerMargin != null)
                    hashCode = hashCode * 59 + this.LowerMargin.GetHashCode();
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
