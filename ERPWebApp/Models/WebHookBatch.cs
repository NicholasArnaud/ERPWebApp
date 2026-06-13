using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models
{
    /// <summary>  
    /// Represents a batch of webhook requests and responses, including error information and retry counts.  
    /// </summary>  
    public class WebHookBatch
    {
        /// <summary>  
        /// Gets or sets the primary key for the WebHookBatch model.  
        /// </summary>  
        [Key]
        public int WebHookBatchId { get; set; }

        /// <summary>  
        /// Gets or sets the creation date of the webhook batch.  
        /// </summary>  
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreateDate { get; set; }

        /// <summary>  
        /// Gets or sets the URL of the webhook.  
        /// </summary>  
        [Required]
        [DataType(DataType.Url)]
        [MaxLength(2048)]
        public string WebhookURL { get; set; }

        /// <summary>  
        /// Gets or sets the request headers for the webhook batch.  
        /// </summary>  
        [MaxLength(4000)]
        public string RequestHeaders { get; set; }

        /// <summary>  
        /// Gets or sets the request body for the webhook batch.  
        /// </summary> 
        public string RequestBody { get; set; }

        /// <summary>  
        /// Gets or sets the HTTP response status code for the webhook batch.  
        /// </summary>  
        [Range(100, 599)]
        public int ResponseStatus { get; set; }

        /// <summary>  
        /// Gets or sets the response headers for the webhook batch.  
        /// </summary>  
        [MaxLength(4000)]
        public string ResponseHeaders { get; set; }

        /// <summary>  
        /// Gets or sets the response body for the webhook batch.  
        /// </summary>
        public string ResponseBody { get; set; }

        /// <summary>  
        /// Gets or sets the error message for the webhook batch, if any.  
        /// </summary>  
        [MaxLength(4000)]
        public string ErrorMessage { get; set; }

        /// <summary>  
        /// Gets or sets the error stack trace for the webhook batch, if any.  
        /// </summary>  
        [MaxLength(4000)]
        public string ErrorStackTrace { get; set; }

        /// <summary>  
        /// Gets or sets the retry count for the webhook batch.  
        /// </summary>  
        [Range(0, int.MaxValue)]
        public int RetryCount { get; set; }
    }
}
