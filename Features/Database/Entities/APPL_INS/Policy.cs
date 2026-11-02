using TrustSystem.Backend.Models;

public abstract class Policy
{
    public abstract PolicyStatus PolicyStatus { get; }
    public abstract int PlanType { get; }
    public abstract TypeOfCoverage? TypeOfCoverageGetter { get; }
    public abstract string PolicyNumberGetter { get; }

    public abstract bool CovidFlag { get; }
    public abstract bool WinterFlag { get; }
    public abstract bool AccidentFlag { get; }
    public abstract string VoucherNumber { get; }
}