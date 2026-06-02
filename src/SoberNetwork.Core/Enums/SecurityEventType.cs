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
    Logout,

    // Group lifecycle
    GroupCreated,
    GroupUpdated,
    GroupDeleted,

    // Membership lifecycle
    GroupJoinRequested,
    GroupMemberApproved,
    GroupMemberRejected,
    GroupMemberRemoved,
    GroupMemberLeft,
    GroupRoleChanged,
    GroupMemberStatusChanged,
    GroupProbationCleared
}
