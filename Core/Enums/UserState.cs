namespace TrustSystem.Backend.Application.Enums;

public enum UserState
{
    NOTFOUND,
    VALID,
    INVALID,

    REQUIREPASSWORDCHANGE,
    LOCKED,
    EXPIRED
}