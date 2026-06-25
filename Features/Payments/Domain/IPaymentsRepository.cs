using System.Collections.Generic;
using System.Threading.Tasks;

namespace FuelTrack.Api.Features.Payments.Domain;

public interface IPaymentsRepository
{
    Task<IEnumerable<PaymentMethod>> GetPaymentMethodsAsync(string userId);
    Task<PaymentMethod> AddPaymentMethodAsync(string userId, NewPaymentMethodRequest request);
    Task<IEnumerable<PaymentHistory>> GetPaymentHistoryAsync(string userId);
}
