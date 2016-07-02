namespace GF.UCenter.Common.Portable.Exceptions
{
    using System;
    using Contracts;
    using Resource;

    public class UCenterException : ApplicationException
    {
        public UCenterException(UCenterErrorCode errorCode, string message, Exception innerException = null)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }

        public UCenterException(UCenterErrorCode errorCode, Exception innerException = null)
            : this(errorCode, UCenterResourceManager.GetErrorMessage(errorCode), innerException)
        {
        }

        public UCenterErrorCode ErrorCode { get; private set; }
    }
}