namespace SimplifiedPayments.Domain.Exceptions;

public class DomainException(string message) : Exception(message);

public class InvalidDocumentException(string message) : DomainException(message);

public class InsufficientBalanceException() : DomainException("Insufficient balance.");

public class ShopkeeperCannotTransferException() : DomainException("Shopkeepers cannot send transfers.");
