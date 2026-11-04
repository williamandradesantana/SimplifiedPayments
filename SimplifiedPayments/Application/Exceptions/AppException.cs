namespace SimplifiedPayments.Application.Exceptions;

public abstract class AppException(string message, Exception? innerException = null) : 
    Exception(message, innerException);
public sealed class ConflictException(string message) : AppException(message);
public sealed class InvalidRequestException(string message) : AppException(message);
public sealed class ResourceNotFoundException(string message) : AppException(message);
public sealed class TransferNotAuthorizedException() : AppException("Transfor was not authorized");
public sealed class ExternalServiceUnavailableException(string message, Exception? innerException = null) : 
    AppException(message, innerException);