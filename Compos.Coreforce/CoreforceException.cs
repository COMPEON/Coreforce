using Compos.Coreforce.Models;
using System;
using System.Collections.Generic;

namespace Compos.Coreforce
{
    class CoreforceException : Exception
    {
        public new Exception InnerException { get; private set; }
        public new string Message { get; private set; }
        public List<ApiError> ApiErrors { get; set; }

        public CoreforceException(
            string error,
            List<ApiError> apiErrors,
            Exception innerException
            )
        {
            ApiErrors = apiErrors;
            InnerException = innerException;
            Message = error;
        }

        public CoreforceException(
            CoreforceError error,
            Exception innerException
            ) : this(error.ToString(), null, innerException)
        { }

        public CoreforceException(
            string error
            )
        {
            ApiErrors = null;
            InnerException = null;
            Message = error;
        }

        public CoreforceException(
            CoreforceError error
            ) : this(error.ToString())
        { }

        public CoreforceException(
            CoreforceError error,
            List<ApiError> apiErrors
            ) : this(error.ToString(), apiErrors, null)
        { }

        public CoreforceException(
            string error,
            Exception innerException
            ) : this(error, null, innerException)
        { }
    }

    public enum CoreforceError
    {
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
