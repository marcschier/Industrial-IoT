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
    public partial class InnerError
    {
        /// <summary>
        /// Initializes a new instance of the InnerError class.
        /// </summary>
        public InnerError()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the InnerError class.
        /// </summary>
        public InnerError(string requestId = default(string))
        {
            RequestId = requestId;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "RequestId")]
        public string RequestId { get; set; }

    }
}