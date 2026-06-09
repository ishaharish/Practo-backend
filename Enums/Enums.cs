namespace PractoBackend.Enums;

public enum Role
{
    Admin,
    Doctor,
    Patient
}

public enum OtpPurpose
{
    Registration,
    PasswordReset,
    Login
}

public enum Gender
{
    Male,
    Female,
    Other
}

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed,
    WaitingInQueue
}

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    UPI,
    NetBanking,
    Cash
}

public enum PaymentStatus
{
    Pending,
    Successful,
    Failed
}
