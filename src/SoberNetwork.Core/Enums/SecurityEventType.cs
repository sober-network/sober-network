namespace SoberNetwork.Core.Enums;

public enum SecurityEventType
{
    // Registration & email
    Register,
    EmailConfirmed,
    ResendConfirmation,

    // Login
    LoginSuccess,
    LoginFailed,
    Lockout,

    // Password
    ForgotPassword,
    PasswordReset,

    // Token lifecycle
    TokenRefreshed,
    TokenRevoked,
    Logout
}
