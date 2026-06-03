namespace SoberNetwork.Domain.Enums;

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
    GroupProbationCleared,

    // Member profile
    ProfileUpdated,
    EmailChangeRequested,
    EmailChanged,
    PasswordChanged,
    AccountDeactivated,
    SobrietyDateSet,
    SobrietyDateRemoved,
    SobrietyVisibilityChanged,
    PhoneSet,
    PhoneRemoved,
    PhoneVisibilityChanged
}
