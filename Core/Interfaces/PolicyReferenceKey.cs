public record PolicyReferenceKey
{
    public string OfficeCode { get; set; }
    public string PolicyType { get; set; }
    public int Year { get; set; }
    public string PolicyNumber { get; set; }

    public PolicyReferenceKey(string referenceKey)
    {
        var parts = referenceKey.Split(' ');
        if (parts.Length != 4)
            throw new ApplicationException("Invalid reference key format.");

        OfficeCode = parts[0];
        PolicyType = parts[1];
        Year = int.Parse(parts[2]);
        PolicyNumber = parts[3];
    }
}