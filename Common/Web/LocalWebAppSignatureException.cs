using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Web;

/// <summary>
/// The error types for local web app signature.
/// </summary>
public enum LocalWebAppSignatureErrorTypes : byte
{
    /// <summary>
    /// Unknown error.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The signature is incorrect.
    /// </summary>
    Incorrect = 1,

    /// <summary>
    /// The signature file is not found.
    /// </summary>
    MissSignatureFile = 2,

    /// <summary>
    /// The signature information is missed.
    /// </summary>
    MissSignatureInfo = 3,

    /// <summary>
    /// Only a part of files pass but no signature for rest.
    /// </summary>
    Partial = 4,

    /// <summary>
    /// Invalid operation during verification.
    /// </summary>
    InvalidOperation = 5,

    /// <summary>
    /// Some operation is not supported.
    /// </summary>
    NotSupported = 6,

    /// <summary>
    /// IO exception.
    /// </summary>
    IO = 7,

    /// <summary>
    /// External exception.
    /// </summary>
    External = 8
}

/// <summary>
/// The exception for local web app signature.
/// </summary>
public class LocalWebAppSignatureException : Exception
{
    /// <summary>
    /// Initializes a new instance of the LocalWebAppSignatureException class.
    /// </summary>
    /// <param name="type">The error type.</param>
    /// <param name="message">The message that describes the error.</param>
    public LocalWebAppSignatureException(LocalWebAppSignatureErrorTypes type, string message)
        : base(message)
    {
        ErrorType = type;
    }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppSignatureException class.
    /// </summary>
    /// <param name="type">The error type.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public LocalWebAppSignatureException(LocalWebAppSignatureErrorTypes type, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorType = type;
    }

    /// <summary>
    /// Gets the error type.
    /// </summary>
    public LocalWebAppSignatureErrorTypes ErrorType { get; }
}
