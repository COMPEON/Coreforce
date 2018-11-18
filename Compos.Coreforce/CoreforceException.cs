using Compos.Coreforce.Models;
using System;
using System.Collections.Generic;

namespace Compos.Coreforce
{
    public class CoreforceException : Exception
    {
        public new Exception InnerException { get; private set; }
        public new string Message { get; private set; }
        public CoreforceError CoreforceError { get; private set; }
        public List<ApiError> ApiErrors { get; set; }

        public CoreforceException(
            CoreforceError coreforceError,
            string message,
            Exception innerException,
            List<ApiError> apiErrors
            )
        {
            CoreforceError = coreforceError;
            Message = message;
            InnerException = innerException;
            ApiErrors = apiErrors;
        }

        public CoreforceException(
            CoreforceError coreforceError,
            string message,
            Exception innerException
            ) : this(coreforceError, message, innerException, null)
        { }

        public CoreforceException(
            CoreforceError coreforceError,
            Exception innerException
            ) : this(coreforceError, coreforceError.ToString(), innerException, null)
        { }

        public CoreforceException(
            CoreforceError coreforceError,
            List<ApiError> apiErrors
            ) : this(coreforceError, coreforceError.ToString(), null, apiErrors)
        { }

        public CoreforceException(
            CoreforceError coreforceError,
            string message
            ) : this(coreforceError, message, null, null)
        { }

        public CoreforceException(
            CoreforceError coreforceError
            ) : this(coreforceError, coreforceError.ToString(), null, null)
        { }
    }

    public enum CoreforceError
    {
        /// <summary>
        /// Thrown when coreforce settings are missing (call AddCoreforce method).
        /// </summary>
        SettingsException,

        /// <summary>
        /// Thrown when salesforce response is BadRequest.
        /// </summary>
        AuthorizationBadRequest,

        /// <summary>
        /// Thrown when authorization throws not known error.
        /// </summary>
        AuthorizationError,
        CommandIsEmpty,
        SalesforceObjectNotFound,
        ProcessingError,
        NoInsertResponse,
        InsertError,
        DeleteError,
        NoDeleteResponse,
        UpdateError,
        NoUpdateResponse,
        SalesforceObjectWithoutId,
        SalesforceObjectIdIsNull
    }
}
