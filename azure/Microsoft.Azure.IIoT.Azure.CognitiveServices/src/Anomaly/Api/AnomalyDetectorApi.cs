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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RestSharp;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace IO.Swagger.Api
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IAnomalyDetectorApi : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Detects trend change point in the time series given the provided model.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>ChangePointDetectResponse</returns>
        ChangePointDetectResponse AnomalydetectorV10TimeseriesChangepointDetectPost (ChangePointDetectRequest body = null);

        /// <summary>
        /// Detects trend change point in the time series given the provided model.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>ApiResponse of ChangePointDetectResponse</returns>
        ApiResponse<ChangePointDetectResponse> AnomalydetectorV10TimeseriesChangepointDetectPostWithHttpInfo (ChangePointDetectRequest body = null);
        /// <summary>
        /// Detects anomaly in the time series given the provided model.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>EntireDetectResponse</returns>
        EntireDetectResponse AnomalydetectorV10TimeseriesEntireDetectPost (AnomalyDetectRequest body = null);

        /// <summary>
        /// Detects anomaly in the time series given the provided model.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>ApiResponse of EntireDetectResponse</returns>
        ApiResponse<EntireDetectResponse> AnomalydetectorV10TimeseriesEntireDetectPostWithHttpInfo (AnomalyDetectRequest body = null);
        /// <summary>
        /// Detects anomaly in the time series given the provided model.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>LastDetectResponse</returns>
        LastDetectResponse AnomalydetectorV10TimeseriesLastDetectPost (AnomalyDetectRequest body = null);

        /// <summary>
        /// Detects anomaly in the time series given the provided model.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>ApiResponse of LastDetectResponse</returns>
        ApiResponse<LastDetectResponse> AnomalydetectorV10TimeseriesLastDetectPostWithHttpInfo (AnomalyDetectRequest body = null);
        #endregion Synchronous Operations
        #region Asynchronous Operations
        /// <summary>
        /// Detects trend change point in the time series given the provided model.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>Task of ChangePointDetectResponse</returns>
        System.Threading.Tasks.Task<ChangePointDetectResponse> AnomalydetectorV10TimeseriesChangepointDetectPostAsync (ChangePointDetectRequest body = null);

        /// <summary>
        /// Detects trend change point in the time series given the provided model.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>Task of ApiResponse (ChangePointDetectResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<ChangePointDetectResponse>> AnomalydetectorV10TimeseriesChangepointDetectPostAsyncWithHttpInfo (ChangePointDetectRequest body = null);
        /// <summary>
        /// Detects anomaly in the time series given the provided model.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>Task of EntireDetectResponse</returns>
        System.Threading.Tasks.Task<EntireDetectResponse> AnomalydetectorV10TimeseriesEntireDetectPostAsync (AnomalyDetectRequest body = null);

        /// <summary>
        /// Detects anomaly in the time series given the provided model.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>Task of ApiResponse (EntireDetectResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<EntireDetectResponse>> AnomalydetectorV10TimeseriesEntireDetectPostAsyncWithHttpInfo (AnomalyDetectRequest body = null);
        /// <summary>
        /// Detects anomaly in the time series given the provided model.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>Task of LastDetectResponse</returns>
        System.Threading.Tasks.Task<LastDetectResponse> AnomalydetectorV10TimeseriesLastDetectPostAsync (AnomalyDetectRequest body = null);

        /// <summary>
        /// Detects anomaly in the time series given the provided model.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>Task of ApiResponse (LastDetectResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<LastDetectResponse>> AnomalydetectorV10TimeseriesLastDetectPostAsyncWithHttpInfo (AnomalyDetectRequest body = null);
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class AnomalyDetectorApi : IAnomalyDetectorApi
    {
        private IO.Swagger.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnomalyDetectorApi"/> class.
        /// </summary>
        /// <returns></returns>
        public AnomalyDetectorApi(String basePath)
        {
            this.Configuration = new IO.Swagger.Client.Configuration { BasePath = basePath };

            ExceptionFactory = IO.Swagger.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnomalyDetectorApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public AnomalyDetectorApi(IO.Swagger.Client.Configuration configuration = null)
        {
            if (configuration == null) // use the default one in Configuration
                this.Configuration = IO.Swagger.Client.Configuration.Default;
            else
                this.Configuration = configuration;

            ExceptionFactory = IO.Swagger.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public String GetBasePath()
        {
            return this.Configuration.ApiClient.RestClient.BaseUrl.ToString();
        }

        /// <summary>
        /// Sets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        [Obsolete("SetBasePath is deprecated, please do 'Configuration.ApiClient = new ApiClient(\"http://new-path\")' instead.")]
        public void SetBasePath(String basePath)
        {
            // do nothing
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public IO.Swagger.Client.Configuration Configuration {get; set;}

        /// <summary>
        /// Provides a factory method hook for the creation of exceptions.
        /// </summary>
        public IO.Swagger.Client.ExceptionFactory ExceptionFactory
        {
            get
            {
                if (_exceptionFactory != null && _exceptionFactory.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("Multicast delegate for ExceptionFactory is unsupported.");
                }
                return _exceptionFactory;
            }
            set { _exceptionFactory = value; }
        }

        /// <summary>
        /// Gets the default header.
        /// </summary>
        /// <returns>Dictionary of HTTP header</returns>
        [Obsolete("DefaultHeader is deprecated, please use Configuration.DefaultHeader instead.")]
        public IDictionary<String, String> DefaultHeader()
        {
            return new ReadOnlyDictionary<string, string>(this.Configuration.DefaultHeader);
        }

        /// <summary>
        /// Add default header.
        /// </summary>
        /// <param name="key">Header field name.</param>
        /// <param name="value">Header field value.</param>
        /// <returns></returns>
        [Obsolete("AddDefaultHeader is deprecated, please use Configuration.AddDefaultHeader instead.")]
        public void AddDefaultHeader(string key, string value)
        {
            this.Configuration.AddDefaultHeader(key, value);
        }

        /// <summary>
        /// Detects trend change point in the time series given the provided model. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>ChangePointDetectResponse</returns>
        public ChangePointDetectResponse AnomalydetectorV10TimeseriesChangepointDetectPost (ChangePointDetectRequest body = null)
        {
             ApiResponse<ChangePointDetectResponse> localVarResponse = AnomalydetectorV10TimeseriesChangepointDetectPostWithHttpInfo(body);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Detects trend change point in the time series given the provided model. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>ApiResponse of ChangePointDetectResponse</returns>
        public ApiResponse< ChangePointDetectResponse > AnomalydetectorV10TimeseriesChangepointDetectPostWithHttpInfo (ChangePointDetectRequest body = null)
        {

            var localVarPath = "/anomalydetector/v1.0/timeseries/changepoint/detect";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
                "application/json-patch+json", 
                "application/json", 
                "text/json", 
                "application/_*+json"
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json",
                "text/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) this.Configuration.ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("AnomalydetectorV10TimeseriesChangepointDetectPost", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<ChangePointDetectResponse>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (ChangePointDetectResponse) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(ChangePointDetectResponse)));
        }

        /// <summary>
        /// Detects trend change point in the time series given the provided model. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>Task of ChangePointDetectResponse</returns>
        public async System.Threading.Tasks.Task<ChangePointDetectResponse> AnomalydetectorV10TimeseriesChangepointDetectPostAsync (ChangePointDetectRequest body = null)
        {
             ApiResponse<ChangePointDetectResponse> localVarResponse = await AnomalydetectorV10TimeseriesChangepointDetectPostAsyncWithHttpInfo(body);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Detects trend change point in the time series given the provided model. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>Task of ApiResponse (ChangePointDetectResponse)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<ChangePointDetectResponse>> AnomalydetectorV10TimeseriesChangepointDetectPostAsyncWithHttpInfo (ChangePointDetectRequest body = null)
        {

            var localVarPath = "/anomalydetector/v1.0/timeseries/changepoint/detect";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
                "application/json-patch+json", 
                "application/json", 
                "text/json", 
                "application/_*+json"
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json",
                "text/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) await this.Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("AnomalydetectorV10TimeseriesChangepointDetectPost", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<ChangePointDetectResponse>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (ChangePointDetectResponse) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(ChangePointDetectResponse)));
        }

        /// <summary>
        /// Detects anomaly in the time series given the provided model. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>EntireDetectResponse</returns>
        public EntireDetectResponse AnomalydetectorV10TimeseriesEntireDetectPost (AnomalyDetectRequest body = null)
        {
             ApiResponse<EntireDetectResponse> localVarResponse = AnomalydetectorV10TimeseriesEntireDetectPostWithHttpInfo(body);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Detects anomaly in the time series given the provided model. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>ApiResponse of EntireDetectResponse</returns>
        public ApiResponse< EntireDetectResponse > AnomalydetectorV10TimeseriesEntireDetectPostWithHttpInfo (AnomalyDetectRequest body = null)
        {

            var localVarPath = "/anomalydetector/v1.0/timeseries/entire/detect";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
                "application/json-patch+json", 
                "application/json", 
                "text/json", 
                "application/_*+json"
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json",
                "text/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) this.Configuration.ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("AnomalydetectorV10TimeseriesEntireDetectPost", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<EntireDetectResponse>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (EntireDetectResponse) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(EntireDetectResponse)));
        }

        /// <summary>
        /// Detects anomaly in the time series given the provided model. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>Task of EntireDetectResponse</returns>
        public async System.Threading.Tasks.Task<EntireDetectResponse> AnomalydetectorV10TimeseriesEntireDetectPostAsync (AnomalyDetectRequest body = null)
        {
             ApiResponse<EntireDetectResponse> localVarResponse = await AnomalydetectorV10TimeseriesEntireDetectPostAsyncWithHttpInfo(body);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Detects anomaly in the time series given the provided model. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>Task of ApiResponse (EntireDetectResponse)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<EntireDetectResponse>> AnomalydetectorV10TimeseriesEntireDetectPostAsyncWithHttpInfo (AnomalyDetectRequest body = null)
        {

            var localVarPath = "/anomalydetector/v1.0/timeseries/entire/detect";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
                "application/json-patch+json", 
                "application/json", 
                "text/json", 
                "application/_*+json"
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json",
                "text/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) await this.Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("AnomalydetectorV10TimeseriesEntireDetectPost", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<EntireDetectResponse>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (EntireDetectResponse) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(EntireDetectResponse)));
        }

        /// <summary>
        /// Detects anomaly in the time series given the provided model. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>LastDetectResponse</returns>
        public LastDetectResponse AnomalydetectorV10TimeseriesLastDetectPost (AnomalyDetectRequest body = null)
        {
             ApiResponse<LastDetectResponse> localVarResponse = AnomalydetectorV10TimeseriesLastDetectPostWithHttpInfo(body);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Detects anomaly in the time series given the provided model. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>ApiResponse of LastDetectResponse</returns>
        public ApiResponse< LastDetectResponse > AnomalydetectorV10TimeseriesLastDetectPostWithHttpInfo (AnomalyDetectRequest body = null)
        {

            var localVarPath = "/anomalydetector/v1.0/timeseries/last/detect";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
                "application/json-patch+json", 
                "application/json", 
                "text/json", 
                "application/_*+json"
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json",
                "text/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) this.Configuration.ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("AnomalydetectorV10TimeseriesLastDetectPost", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<LastDetectResponse>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (LastDetectResponse) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(LastDetectResponse)));
        }

        /// <summary>
        /// Detects anomaly in the time series given the provided model. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>Task of LastDetectResponse</returns>
        public async System.Threading.Tasks.Task<LastDetectResponse> AnomalydetectorV10TimeseriesLastDetectPostAsync (AnomalyDetectRequest body = null)
        {
             ApiResponse<LastDetectResponse> localVarResponse = await AnomalydetectorV10TimeseriesLastDetectPostAsyncWithHttpInfo(body);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Detects anomaly in the time series given the provided model. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body">The time series to analyze. (optional)</param>
        /// <returns>Task of ApiResponse (LastDetectResponse)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<LastDetectResponse>> AnomalydetectorV10TimeseriesLastDetectPostAsyncWithHttpInfo (AnomalyDetectRequest body = null)
        {

            var localVarPath = "/anomalydetector/v1.0/timeseries/last/detect";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
                "application/json-patch+json", 
                "application/json", 
                "text/json", 
                "application/_*+json"
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json",
                "text/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) await this.Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("AnomalydetectorV10TimeseriesLastDetectPost", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<LastDetectResponse>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (LastDetectResponse) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(LastDetectResponse)));
        }

    }
}