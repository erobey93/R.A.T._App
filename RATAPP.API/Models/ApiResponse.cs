using System.Text.Json.Serialization;

namespace RATAPP.API.Models
{
    /// <summary>
    /// Standardized wrapper for all API responses.
    /// Provides consistent structure for both successful and error responses.
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the request was successful
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// The actual data being returned (null if request failed)
        /// </summary>
        [JsonPropertyName("data")]
        public T? Data { get; set; }

        /// <summary>
        /// A human-readable message describing the result
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// Array of error messages (null if request was successful)
        /// </summary>
        [JsonPropertyName("errors")]
        public string[]? Errors { get; set; }

        /// <summary>
        /// Creates a successful response with data
        /// </summary>
        public static ApiResponse<T> Success_(T data, string message = "Request successful") //FIXME not sure what's up here with the repeated success error 
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        /// <summary>
        /// Creates an error response
        /// </summary>
        public static ApiResponse<T> Error(string message, params string[] errors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors.Length > 0 ? errors : null
            };
        }
    }
}
