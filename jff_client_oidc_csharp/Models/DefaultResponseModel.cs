using System;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public class DefaultResponseModel<T>
    {
        public string Message { get; set; } = string.Empty;
        public List<string> ListErrors { get; set; } = new List<string>();
        public T Result { get; set; }
        public bool Success
        {
            get
            {
                var messageVerify = string.IsNullOrEmpty(Error) && ListErrors?.Any() != true;
                return messageVerify;
            }
        }
        public string StackTrace { get; set; } = string.Empty;
        public string BaseException { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;

        public void Extract(Exception ex = null)
        {
            if (ex != null)
            {
                Error = ex.ToString();
                StackTrace = ex.StackTrace;
                BaseException = ex.GetBaseException().Message;
                ListErrors.Add(ex.Message);
            }
        }

        public void Extract<TExtract>(DefaultResponseModel<TExtract> ex = null)
        {
            if (ex != null && !ex.Success)
            {
                Error = ex.Error;
                StackTrace = ex.StackTrace;
                BaseException = ex.BaseException;
                ListErrors.AddRange(ex.ListErrors);
            }
        }
    }
}
