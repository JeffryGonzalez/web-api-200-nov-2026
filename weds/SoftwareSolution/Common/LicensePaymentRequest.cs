namespace Common;

public record LicensePaymentRequest(string Product, int Seats);

public record Receipt(string Product, int Seats, decimal Amount);