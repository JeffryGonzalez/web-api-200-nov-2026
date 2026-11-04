namespace Common;


// From the SoftwareApi POST /licenses to License.Api POST /requests
public record LicensePaymentRequest(string Product, int Seats);

// This is what you get back from the license Api.
public record Receipt(string Product, int Seats, decimal Amount);