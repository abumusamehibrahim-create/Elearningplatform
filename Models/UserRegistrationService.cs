using ELearningPlatform.Data;

namespace ELearningPlatform.Models
{
    public class UserRegistrationService
    {
        private readonly ApplicationDbContext _db;

        public UserRegistrationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public (bool Success, string Message, string PaymentId) CompleteTransferPayment(
            string userId, int courseId, decimal amount, string transferNumber)
        {
            if (string.IsNullOrWhiteSpace(transferNumber))
                return (false, "رقم التحويل غير صالح", "");

            string paymentRef = "TRANSFER_" + transferNumber.Trim();

            var payment = new Payment
            {
                UserId = userId,
                CourseId = courseId,
                Amount = amount,
                StripePaymentId = paymentRef,
                Status = "Pending",
                PaymentDate = DateTime.UtcNow
            };

            _db.Payments.Add(payment);
            _db.SaveChanges();

            return (true, "تم تسجيل عملية التحويل", paymentRef);
        }
    }
}
