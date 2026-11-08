using QRCoder;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.Models;
using TrustSystem.Backend.Services;
namespace TrustSystem.Backend.Application.Reports;

public partial class PolicyDocumentService(APPL_INS dbIns, APPL_CORE dbCore, CurrentUserService currentUserService)
{
    private readonly APPL_INS _dbIns = dbIns;
    private readonly APPL_CORE _dbCore = dbCore;
    private readonly CurrentUserService _currentUserService = currentUserService;
    public static readonly string FontName = "Arabic UI Display";

    // This configuration will run once that class is first used, regardless of how it's instantiated (currently instantiated though DI)
    static PolicyDocumentService()
    {
        // Configuring QuestPDF and the right arabic font to use
        QuestPDF.Settings.License = LicenseType.Community;
        FontManager.RegisterFont(File.OpenRead(AssetLocator.Font("Arabic.otf")));
    }

    private User? UserGeneratedReport { get; set; }
    public async Task<byte[]?> GenerateReport(Policy Policy, bool PrintAsDraft = true)
    {
        // Getting UserInstance which requested to print the policy
        await _currentUserService.RetrieveUser();
        UserGeneratedReport = _currentUserService.UserInstance();

        // Checking if it was customer or not, customers can't print the policy if it wasn't paid!
        if (UserGeneratedReport.IsCustomer() && Policy.PolicyStatus == PolicyStatus.Draft)
            throw new ApplicationException("Policy doesn't paid yet");

        // You can't print the policy as draft if you have already generated the original version of it
        if (PrintAsDraft && Policy.PolicyStatus == PolicyStatus.Generated)
            throw new ApplicationException("Cannot print as draft, original version already generated");

        if (Policy is TV_POLICIES SinglePolicy)
            return await GenerateSingleReport(SinglePolicy, PrintAsDraft);

        else if (Policy is TV_POLICY_PACKAGE GroupPolicy)
            return await GenerateGroupReport(GroupPolicy, PrintAsDraft);

        else throw new ApplicationException("Unsupported policy type");
    }
}

public static class PolicyDocumentServiceExtensions
{
    public static string GetCoverLimit(string typeOfCoverage)
    {
        switch (typeOfCoverage)
        {
            case "1":
                return "75000";

            case "11":
            case "12":
                return "50000";

            default:
                return "100000";
        }
    }

    // public static void AddDraftWaterMark(this PageDescriptor page) =>
    //     page.Background()
    //         .AlignCenter()
    //         .AlignMiddle()
    //         .Rotate(-45)
    //         .Text("DRAFT")
    //         .FontSize(150)
    //         .FontColor("#80808028");

    // public static void AddCancelledWaterMark(this PageDescriptor page) =>
    //     page.Background()
    //         .AlignCenter()
    //         .AlignMiddle()
    //         .Rotate(-45)
    //         .Text("CANCELLED")
    //         .FontSize(150)
    //         .FontColor("#80808028");      

    public static void AddDraftWaterMark(this PageDescriptor page) =>
    page.Background()
        .Element(container =>
            container
                .AlignCenter()
                .AlignMiddle()
                .PaddingTop(170)
                .PaddingLeft(20)
                .Rotate(-45)
                .Text("DRAFT")
                .FontSize(150)
                .FontColor("#80808028")
        );

    public static void AddCancelledWaterMark(this PageDescriptor page) =>
        page.Background()
            .Element(container =>
                container
                    .AlignCenter()
                    .AlignMiddle()
                    .PaddingTop(170)
                    .PaddingLeft(50)
                    .Rotate(-45)
                    .Text("CANCELLED")
                    .FontSize(95)
                    .FontColor("#80808028")
            );

    public static void AddTrustPalestineHeader(this ColumnDescriptor column) =>
        column.Item().Row(row =>
        {
            row.RelativeItem().AlignLeft().AlignMiddle().Text("Trust International\nInsurance Company - Palestine").FontSize(10).Bold();
            row.RelativeItem().AlignCenter().Container().Width(60).Image(AssetLocator.Image("trust_logo.png"));
            row.RelativeItem().AlignRight().AlignMiddle().Text("شركة ترست العالمية للتأمين - فلسطين").FontFamily(PolicyDocumentService.FontName).FontSize(12).Bold();
        });

    public static void AddSwanSupportInfo(this ColumnDescriptor column) =>
        column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("In case of emergency please call").FontSize(9).Bold();
                    col.Item().Text("Swan International Assistance").FontSize(9);
                    col.Item().Text("USA/Canada: 001 514 448 4417").FontSize(8);
                    col.Item().Text("France/Europe: 0033 9 7073 2247").FontSize(8);
                    col.Item().Text("International: 00961 9 211 662").FontSize(8);
                    col.Item().Text("WhatsApp 00961 81 284 602").FontSize(8);
                    col.Item().Text("Email: request@swanassistance.com").FontSize(8);
                });

                row.RelativeItem().AlignCenter().Container().Width(60).AlignMiddle().Image(AssetLocator.Image("swan_logo.png"));
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text("في حالة الطوارئ يرجى الاتصال").FontFamily(PolicyDocumentService.FontName).FontSize(8).Bold().AlignRight();
                    col.Item().Text("شركة سوان للمساعدة").FontFamily(PolicyDocumentService.FontName).FontSize(8).AlignRight();
                    col.Item().Text("001 514 448 4417 امريكا و كندا").FontFamily(PolicyDocumentService.FontName).FontSize(8).AlignRight();
                    col.Item().Text("0033 9 7073 2247 فرنسا و اوروبا").FontFamily(PolicyDocumentService.FontName).FontSize(8).AlignRight();
                    col.Item().Text("00961 9 211 662 دولي").FontFamily(PolicyDocumentService.FontName).FontSize(8).AlignRight();
                    col.Item().Text("00961 81 284 602 واتساب").FontFamily(PolicyDocumentService.FontName).FontSize(7).AlignRight();
                    col.Item().Text("request@swanassistance.com :بريد الكتروني").FontFamily(PolicyDocumentService.FontName).FontSize(7).AlignRight();
                });
            });

    public static void AddPolicyNumber(this ColumnDescriptor column, string policyNumber) =>
        column.Item().Background(Colors.Grey.Lighten3).Padding(5).Row(row =>
            {
                row.RelativeItem().Text("Policy No:").FontSize(11).Bold();
                row.RelativeItem().AlignCenter().Text(policyNumber).FontSize(11).Bold();
                row.RelativeItem().AlignRight().Text(":رقم الوثيقة").FontFamily(PolicyDocumentService.FontName).FontSize(11).Bold();
            });

    public static void AddPolicyFooter(this ColumnDescriptor column, Policy policyInstance) =>
        column.Item().PaddingTop(20).Column(col =>
            {
                col.Item().Text("** الوثيقة غير قابلة للالغاء، ويتم التعديل فقط قبل بداية تاريخ التأمين **").FontFamily(PolicyDocumentService.FontName).FontSize(7).AlignRight();

                if (policyInstance.TypeOfCoverageGetter!.Id == "2")
                    col.Item().Text("** تشمل هذه الوثيقة التكاليف الطبية والجراحية والصيدلانية في الخارج بسبب المرض أو الحادث المفاجئ بحد أقصى 100,000 دولار أمريكي وفقاً لشروط البوليصة. **").FontFamily(PolicyDocumentService.FontName).FontSize(7).AlignRight();

                col.Item().Text("** لا تكون هذه الوثيقة سارية إلا إذا كانت تحمل ختم شركة ترست العالمية للتأمين أو أحد وكلائها. **").FontFamily(PolicyDocumentService.FontName).FontSize(7).AlignRight();

                if (policyInstance.CovidFlag)
                    if (new[] { 8, 9 }.Contains(policyInstance.PlanType) || policyInstance.TypeOfCoverageGetter!.Id == "4")
                        col.Item().Text("** تشمل هذه الوثيقة تكاليف الاستقرار الطبي والتكاليف الطبية لفيروس كورونا داخل المستشفى بحد أقصى 10,000 دولار أمريكي. **").FontFamily(PolicyDocumentService.FontName).FontSize(7).AlignRight();

                    else
                        col.Item().Text("** تشمل هذه الوثيقة تكاليف الاستقرار الطبي والتكاليف الطبية لفيروس كورونا داخل المستشفى بحد أقصى 30,000 يورو. **").FontFamily(PolicyDocumentService.FontName).FontSize(7).AlignRight();

                if (!new[] { 8, 9 }.Contains(policyInstance.PlanType))
                    col.Item().Text("** لا تكون هذه الوثيقة سارية إلا إذا كان تاريخ إصدارها قبل تاريخ مغادرة فلسطين. **").FontFamily(PolicyDocumentService.FontName).FontSize(7).AlignRight();
                else
                    col.Item().Text("** لا تكون هذه الوثيقة سارية إلا إذا كان تاريخ إصدارها قبل تاريخ مغادرة البلد المقصد أي البلد خارج فلسطين. **").FontFamily(PolicyDocumentService.FontName).FontSize(7).AlignRight();

                if (policyInstance.WinterFlag)
                    col.Item().Text("** تشمل هذه الوثيقة تغطية الرياضات الشتوية (التزلج) حتى 10,000 دولار أمريكي. **").FontFamily(PolicyDocumentService.FontName).FontSize(7).AlignRight();

                if (policyInstance.AccidentFlag)
                    col.Item().Text("** تشمل هذه الوثيقة إلغاء الرحلة حتى 3,000 دولار أمريكي. **").FontFamily(PolicyDocumentService.FontName).FontSize(7).AlignRight();

                if (policyInstance.TypeOfCoverageGetter.Id == "11")
                    col.Item().Text("** تشمل هذه الوثيقة التكاليف الطبية والجراحية والصيدلانية في الخارج بسبب المرض أو الحادث المفاجئ بحد أقصى 50,000 دولار أمريكي وفقاً لشروط البوليصة. **").FontFamily(PolicyDocumentService.FontName).FontSize(7).AlignRight();

                if (/* !new[] { "8", "9", "11" }.Contains(policyInstance.TypeOfCoverageGetter.Id) */ policyInstance.TypeOfCoverageGetter!.Id != "2")
                    col.Item().Text("** تشمل هذه الوثيقة التكاليف الطبية والجراحية والصيدلانية في الخارج بسبب المرض أو الحادث المفاجئ بحد أقصى 75,000 دولار أمريكي وفقاً لشروط البوليصة. **").FontFamily(PolicyDocumentService.FontName).FontSize(7).AlignRight();

                // OLD CONFIGURATION
                // col.Item().Text("** لا تكون هذه الوثيقة سارية الا اذا كانت تحمل ختم شركة ترست العالمية للتأمين أو احد وكلائها **").FontFamily(PolicyDocumentService.FontName).FontSize(7).AlignRight();
                // col.Item().Text("** لا تكون هذه الوثيقة سارية الا اذا كان تاريخ اصدارها قبل تاريخ مغادرة فلسطين **").FontFamily(PolicyDocumentService.FontName).FontSize(7).AlignRight();
                // col.Item().Text($"** تشمل هذه الوثيقة التكاليف الطبية والجراحية والصيدلانية في الخارج بسبب المرض أو الحادث المفاجئ بحد أقصى {GetCoverLimit(policyInstance.TypeOfCoverageGetter!.Id)} دولار امريكي وفقا لشروط البوليصة **").FontFamily(PolicyDocumentService.FontName).AlignRight().Bold().FontSize(7);

                // col.Item().PaddingTop(5).Text("* The document is Not applicable to cancellation, and the amendment is only before the beginning of the insurance date.").FontFamily(PolicyDocumentService.FontName).FontSize(7);
                // col.Item().Text("* This document shall not be valid unless its date of issue is prior to the date of leaving Palestine.").FontFamily(PolicyDocumentService.FontName).FontSize(7);
                // col.Item().Text("* This document shall not be valid unless it is stamped by Trust International Insurance Company or one of its agents.").FontFamily(PolicyDocumentService.FontName).FontSize(7);
                // col.Item().Text($"*This document includes medical hospitalization, surgical, and pharmaceutical expenses abroad, due to sudden illness or accident up to {GetCoverLimit(policyInstance.TypeOfCoverageGetter!.Id)} USD and subject to the term and condition of the policy").FontFamily(PolicyDocumentService.FontName).FontSize(7).Bold();

                col.Item().Text("The document is Not applicable to cancellation, and the amendment is only before the beginning of the insurance.").FontFamily(PolicyDocumentService.FontName).FontSize(7).Bold();

                if (!new[] { 8, 9 }.Contains(policyInstance.PlanType))
                    col.Item().Text("This document shall not be valid unless its date of issue is prior to the date of leaving Palestine.").FontFamily(PolicyDocumentService.FontName).FontSize(7).Bold();
                else
                    col.Item().Text("This document shall not be valid unless its date of issue is prior to the date of leaving the overseas Countries.").FontFamily(PolicyDocumentService.FontName).FontSize(7).Bold();

                col.Item().Text("This document shall not be valid unless it is stamped by the Trust International Insurance Company or one of its agents.").FontFamily(PolicyDocumentService.FontName).FontSize(7).Bold();

                if (policyInstance.CovidFlag)
                    if (new[] { 8, 9 }.Contains(policyInstance.PlanType) || policyInstance.TypeOfCoverageGetter!.Id == "4")
                        col.Item().Text("This document includes Covid-19 medical stabilization &medical expenses through hospital admission only and up to 10,000 USD.").FontFamily(PolicyDocumentService.FontName).FontSize(7).Bold();
                    else
                        col.Item().Text("This document includes Covid-19 medical stabilization &medical expenses through hospital admission only and up to 30,000 Euros.").FontFamily(PolicyDocumentService.FontName).FontSize(7).Bold();

                if (policyInstance.WinterFlag)
                    col.Item().Text("This document includes Cover of Winter Sports(Ski) up to 10,000 USD.").FontFamily(PolicyDocumentService.FontName).FontSize(7).Bold();

                if (policyInstance.AccidentFlag)
                    col.Item().Text("This document includes trip cancellation up to 3,000 USD.").FontFamily(PolicyDocumentService.FontName).FontSize(7).Bold();

                if (policyInstance.TypeOfCoverageGetter!.Id == "2")
                    col.Item().Text("This document includes medical hospitalization, surgical, and pharmaceutical expenses aboard, due to sudden illness or accident up to 100,000 USD and is subject to the terms and condition of the policy.").FontFamily(PolicyDocumentService.FontName).FontSize(7).Bold();

                if (policyInstance.TypeOfCoverageGetter.Id == "11")
                    col.Item().Text("This document includes medical hospitalization, surgical, and pharmaceutical expenses aboard, due to sudden illness or accident up to 50,000 USD and is subject to the terms and condition of the policy.").FontFamily(PolicyDocumentService.FontName).FontSize(7).Bold();

                if (!new[] { "8", "9", "11" }.Contains(policyInstance.TypeOfCoverageGetter.Id))
                    col.Item().Text("This document includes medical hospitalization, surgical, and pharmaceutical expenses aboard, due to sudden illness or accident up to 75,000 USD and is subject to the terms and condition of the policy.").FontFamily(PolicyDocumentService.FontName).FontSize(7).Bold();
            });

    public static void AddQRCode(this RowDescriptor row, Policy policy)
    {
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode($"Policy: {policy.PolicyNumberGetter}", QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeBytes = qrCode.GetGraphic(20);

        row.RelativeItem().ScaleToFit().AlignCenter()
        .Width(65)
        .Height(65)
        .Padding(1)
        .Image(qrCodeBytes).FitArea();
    }

    public static void AddInvoice(this ColumnDescriptor column, Policy policy)
    {
        column.Item().PaddingTop(20).AlignCenter().Container()
                    .Border(0.5f).BorderLinearGradient(Colors.Black, ["#851819", "#e1af0bff"])
                    .Padding(5)
                    .AlignCenter()
                    .Text($"INVOICE       {policy.VoucherNumber}").FontSize(12);
    }
}