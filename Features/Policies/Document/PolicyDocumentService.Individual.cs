using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Models;
using TrustSystem.Backend.Endpoints;

namespace TrustSystem.Backend.Application.Reports;

public partial class PolicyDocumentService
{
    private async Task<byte[]> GenerateSingleReport(TV_POLICIES policyInstance, bool isDraft = true)
    {
        var isCancelled = policyInstance.PolicyStatus == PolicyStatus.Endorsed;
        var policyAtrType = policyInstance.POL_ATR_TYPE;
        var periods = await _dbCore.CR_DOMAIN_VALUES.Where(e => e.DomainId == PlanMapper.periods[policyAtrType] &&
                                                               e.CompanyGroupId == policyInstance.POL_CGP_ID &&
                                                               e.Id == policyInstance.POL_PERIOD.ToString()).ToListAsync();

        if (!isDraft)
        {
            int newPrintFlag = ((policyInstance.POL_PRINT_FLAG ?? 0) < 9) ? ((policyInstance.POL_PRINT_FLAG ?? 0) + 1) : 9;
            policyInstance.POL_PRINT_FLAG = newPrintFlag;
            await _dbIns.TV_POLICIES
                .Where(p => p.POL_REF_KEY == policyInstance.POL_REF_KEY && p.POL_INT_SERIAL == policyInstance.POL_INT_SERIAL)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.POL_PRINT_FLAG, newPrintFlag));
        }

        if (periods.Count != 1)
            throw new ApplicationException("Invalid policy data");

        var policyTypes = await _dbIns.PolicyTypes.Where(policyType =>
            policyType.CompanyGroupId == 8 &&
            policyType.Id == policyInstance.POL_ATR_TYPE.ToString()
        ).ToListAsync();
        var policyType = policyTypes.Single();
        var today = DateTime.Today;
        var dob = policyInstance.POL_PASSEGER_BIRTH_DATE;

        var age = policyInstance.POL_PASSEGER_AGE;

        if (dob.Date > today.AddYears(-age))
        {
            age--;
        }
        var policy = new IndividualReportInfoDTO
        {
            dateOfBirth = DateOnly.FromDateTime(policyInstance.POL_PASSEGER_BIRTH_DATE),
            expiryDate = DateOnly.FromDateTime(policyInstance.POL_EXPIRY_DATE),
            fullNameEnglish = $"{policyInstance.POL_PASSEGER_1ST_NAME_NTV} {policyInstance.POL_PASSEGER_FATHER_NAME_NTV} {policyInstance.POL_PASSEGER_LAST_NAME_NTV}",
            fullNameArabic = $"{policyInstance.POL_PASSEGER_1ST_NAME_FRN} {policyInstance.POL_PASSEGER_FATHER_NAME_FRN} {policyInstance.POL_PASSEGER_LAST_NAME_FRN}",
            inceptionDate = DateOnly.FromDateTime(policyInstance.POL_INCEPTION_DATE),
            passportNumber = policyInstance.POL_PASSEGER_PASSPORT_NUMBER ?? "",
            policyNumber = policyInstance.POL_REF_KEY,

            phoneNumber = policyInstance.POL_PASSEGER_TELEPHONE ?? "",
            permanentAddressEnglish = policyInstance.POL_PASSEGER_ADDRESS_NTV ?? "",
            permanentAddressArabic = policyInstance.POL_PASSEGER_ADDRESS_FRN ?? "",
            issueDate = DateOnly.FromDateTime(policyInstance.POL_ISSUE_DATE),
            gender = int.Parse(policyInstance.POL_PASSEGER_GENDER?.ToString() ?? "1"),
            grossPremium = (isCancelled ? -1 : 1) * Math.Abs(Math.Round(policyInstance.POL_PREMIUM_AMOUNT, 2, MidpointRounding.AwayFromZero)),

            paymentAccount = policyInstance.POL_TA_CODE,
            paymentMethod = "On Account",

            destinationEnglish = policyInstance.DestinationCountry?.NameInEnglish ?? "",
            destinationArabic = policyInstance.DestinationCountry?.NameInArabic ?? "",

            nationalityEnglish = policyInstance.NationalityCountry?.NameInEnglish ?? "",
            nationalityArabic = policyInstance.NationalityCountry?.NameInArabic ?? "",

            periodEnglish = periods[0].NameInEnglish ?? "",
            periodArabic = periods[0].NameInArabic ?? "",

            typeOfCoverageEnglish = policyInstance.TypeOfCoverage?.NameInEnglish ?? "",
            typeOfCoverageArabic = policyInstance.TypeOfCoverage?.NameInArabic ?? "",

            policyTypeNameEnglish = policyType.NameInEnglish
                ?? throw new ApplicationException("Policy doesn't have a valid type")
        };

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                if (isCancelled) page.AddCancelledWaterMark();
                else if (isDraft) page.AddDraftWaterMark();

                page.Header().Column(column =>
                {
                    column.AddTrustPalestineHeader();
                    column.AddSwanSupportInfo();
                });

                page.Content().PaddingTop(20).Column(column =>
                {
                    column.AddPolicyNumber(policy!.policyNumber);

                    column.Item().PaddingTop(10).Background(Colors.Grey.Lighten3).Padding(5).Row(row =>
                    {
                        row.RelativeItem().Text("Personal Information").FontSize(10).Bold();
                        row.RelativeItem().AlignRight().Text("المعلومات الشخصية").FontFamily(FontName).FontSize(10).Bold();
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Border(0.5f).Padding(3).Text("Full Name").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(policy?.fullNameEnglish).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(policy?.fullNameArabic).FontFamily(FontName).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("الاسم الكامل").FontFamily(FontName).FontSize(9);
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(6);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Border(0.5f).Padding(3).Text("Date of Birth").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(policy?.dateOfBirth.ToString("dd/MM/yyyy")).AlignCenter().FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("تاريخ الميلاد").FontFamily(FontName).FontSize(9);
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(6);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Border(0.5f).Padding(3).Text("Age").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(age.ToString()).AlignCenter().FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("العمر ").FontFamily(FontName).FontSize(9);
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Border(0.5f).Padding(3).Text("Gender").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(policy?.gender == 1 ? "Male" : "Female").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(policy?.gender == 1 ? "ذكر" : "أنثى").FontFamily(FontName).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("الجنس").FontFamily(FontName).FontSize(9);

                        table.Cell().Border(0.5f).Padding(3).Text("Nationality").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(policy?.nationalityEnglish).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(policy?.nationalityArabic).FontFamily(FontName).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("الجنسيه").FontFamily(FontName).FontSize(9);
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(6);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Border(0.5f).Padding(3).Text("Passport/ID").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(policy?.passportNumber).AlignCenter().FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("رقم جواز السفر").FontFamily(FontName).FontSize(9);

                        table.Cell().Border(0.5f).Padding(3).Text("Phone Number").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(policy?.phoneNumber).AlignCenter().FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("الهاتف").FontFamily(FontName).FontSize(9);
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Border(0.5f).Padding(3).Text("Permanent Address").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(policy?.permanentAddressEnglish).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(policy?.permanentAddressArabic).AlignRight().FontFamily(FontName).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("العنوان الدائم").FontFamily(FontName).FontSize(9);
                    });

                    column.Item().PaddingTop(10).Background(Colors.Grey.Lighten3).Padding(5).Row(row =>
                    {
                        row.RelativeItem().Text("Travel Policy Details").FontSize(10).Bold();
                        row.RelativeItem().AlignRight().Text("بيانات وثيقة السفر").FontFamily(FontName).FontSize(10).Bold();
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(6);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Border(0.5f).Padding(3).Text("Program Type").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(policy?.policyTypeNameEnglish).AlignCenter().FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("نوع البرنامج").FontFamily(FontName).FontSize(9);
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                        });


                        table.Cell().Border(0.5f).Padding(3).Text("Period of Insurance").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(policy?.periodEnglish).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(policy?.periodArabic).FontFamily(FontName).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("مدة التأمين").FontFamily(FontName).FontSize(9);
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(6);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Border(0.5f).Padding(3).Text("Inception Date").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(policy?.inceptionDate.ToString("dd/MM/yyyy")).AlignCenter().FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("تاريخ بدء التأمين").FontFamily(FontName).FontSize(9);

                        table.Cell().Border(0.5f).Padding(3).Text("Expiry Date").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(policy?.expiryDate.ToString("dd/MM/yyyy")).AlignCenter().FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("تاريخ انتهاء التأمين").FontFamily(FontName).FontSize(9);
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Border(0.5f).Padding(3).Text("Destination").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(policy?.destinationEnglish).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(policy?.destinationArabic).FontFamily(FontName).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("وجهة السفر").FontFamily(FontName).FontSize(9);
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(6);
                            columns.RelativeColumn(2);
                        });


                        table.Cell().Border(0.5f).Padding(3).Text("Type of Coverage").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(policy?.typeOfCoverageEnglish).AlignCenter().FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("نوع التغطيه").FontFamily(FontName).FontSize(9);
                    });

                    column.Item().PaddingTop(10).Background(Colors.Grey.Lighten3).Padding(5).Row(row =>
                    {
                        row.RelativeItem().Text("Gross Premium as Agreed").FontSize(10).Bold();
                        row.RelativeItem().AlignCenter().Text(policy?.grossPremium.ToString("F2")).FontSize(10).Bold();
                        row.RelativeItem().AlignRight().Text("القسط الاجمالي حسب الاتفاق").FontFamily(FontName).FontSize(10).Bold();
                    });

                    column.Item().PaddingTop(10).AlignCenter().Text("I HAVE READ, UNDERSTOOD & AGREED TO THE POLICY TERMS & CONDITIONS").FontSize(9).Bold();
                    column.Item().AlignCenter().Text("لقد قرأت وفهمت وأوافق على الشروط العامه للوثيقة").FontFamily(FontName).FontSize(9);

                    column.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text($"Issue Date: {policy?.issueDate.ToString("dd/MM/yyyy")}").FontSize(9);
                            if (!policyInstance.IsCreatedByCustomerPortal)
                                col.Item().Text($"Agent/Agency: {policyInstance.CompanyOffice!.NameInEnglish}").FontSize(9);
                            col.Item().Text("For/Trust International Insurance co.").FontSize(8);

                            col.Item().PaddingTop(18).Text("Signature/Stamp").FontSize(8);
                            col.Item().Text("توقيع وختم الموظف/الوكيل").FontFamily(FontName).FontSize(8);
                        });

                        if (!isDraft) row.AddQRCode(policyInstance);

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text($"تاريخ الاصدار: {policy?.issueDate.ToString("dd/MM/yyyy")}").FontFamily(FontName).FontSize(8);
                            if (!policyInstance.IsCreatedByCustomerPortal)
                                col.Item().Text($"الوكاله/الوكيل: {policyInstance.CompanyOffice!.NameInArabic}").FontFamily(FontName).FontSize(8);
                            col.Item().Text("").FontSize(8);

                            col.Item().PaddingTop(18).Text("Signature/Insured").FontSize(8);
                            col.Item().Text("توقيع المؤمن له").FontFamily(FontName).FontSize(8);
                        });
                    });

                    column.AddPolicyFooter(policyInstance);
                });
            });

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                if (isDraft)
                {
                    page.Foreground()
                    .AlignCenter()
                    .AlignMiddle()
                    .Text("DRAFT")
                    .FontSize(100)
                    .FontColor("#80808028");
                }

                page.Header().Column(column =>
                {
                    column.AddTrustPalestineHeader();

                    // column.Item().PaddingTop(20).AlignCenter().Container()
                    // .Border(0.5f).BorderLinearGradient(Colors.Black, ["#851819", "#e1af0bff"])
                    // .Padding(5)
                    // .AlignCenter()
                    // .Text($"INVOICE       {policy?.policyNumber}").FontSize(12);

                    column.AddInvoice(policyInstance);
                });

                page.Content().PaddingTop(20).Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        if (!policyInstance.IsCreatedByCustomerPortal)
                            row.RelativeItem(2).Border(1).BorderColor(Colors.Black).Padding(10).Row(col =>
                            {
                                col.RelativeItem().AlignLeft().Text(policyInstance.CompanyOffice!.NameInEnglish).FontSize(10);
                                col.RelativeItem().AlignRight().Text(policyInstance.CompanyOffice!.NameInArabic).FontFamily(FontName).FontSize(9);
                            });

                        row.RelativeItem(3).Border(1).BorderColor(Colors.Black).Column(dateCol =>
                        {
                            dateCol.Item().BorderBottom(1).BorderColor(Colors.Black).Row(dateRow =>
                            {
                                dateRow.RelativeItem().Padding(5).Text("Date:").FontSize(9);
                                dateRow.RelativeItem().Padding(5).Text(policy?.issueDate.ToString("dd/MM/yyyy")).FontSize(9);
                                dateRow.RelativeItem().Padding(5).AlignRight().Text(":التاريخ").FontFamily(FontName).FontSize(9);
                            });

                            dateCol.Item().BorderBottom(1).BorderColor(Colors.Black).Row(policyRow =>
                            {
                                policyRow.RelativeItem().Padding(5).Text("Policy No:").FontSize(9);
                                policyRow.RelativeItem().Padding(5).Text(policy?.policyNumber).FontSize(9);
                                policyRow.RelativeItem().Padding(5).AlignRight().Text(":رقم الوثيقة").FontFamily(FontName).FontSize(9);
                            });

                            dateCol.Item().BorderBottom(1).BorderColor(Colors.Black).Row(paymentRow =>
                            {
                                paymentRow.RelativeItem().Padding(5).Text("Payment Type:").FontSize(9);
                                paymentRow.RelativeItem().Padding(5).Text(policy?.paymentMethod).FontSize(9);
                                paymentRow.RelativeItem().Padding(5).AlignRight().Text(":طريقة الدفع").FontFamily(FontName).FontSize(9);
                            });

                            dateCol.Item().Row(accountRow =>
                            {
                                accountRow.RelativeItem().Padding(5).Text("Account No:").FontSize(9);
                                accountRow.RelativeItem().Padding(5).Text(policy?.paymentAccount).FontSize(9);
                                accountRow.RelativeItem().Padding(5).AlignRight().Text(":رقم الحساب").FontFamily(FontName).FontSize(9);
                            });
                        });
                    });

                    column.Item().PaddingTop(10).Background(Colors.Grey.Lighten3).Padding(5).Row(row =>
                    {
                        row.RelativeItem().Text("Insured:").FontSize(10).Bold();
                        row.RelativeItem(4).Text(policy?.fullNameEnglish).FontSize(10);
                    });

                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                        });


                        table.Cell().Border(0.5f).Background(Colors.Grey.Lighten3).Padding(8).Text("DESCRIPTION").FontSize(10).Bold();
                        table.Cell().Border(0.5f).Background(Colors.Grey.Lighten3).Padding(8).AlignCenter().AlignMiddle().Text("Currency U.S $").FontSize(9).Bold();
                        table.Cell().Border(0.5f).Padding(8).Column(col =>
                        {
                            col.Item().Text("WE HAVE DEBITED YOUR ACCOUNT WITH THE FOLLOWING AMOUNT DUE IN RESPECT OF:").FontSize(9);

                            col.Item().PaddingTop(20).Row(detailRow =>
                                {
                                    detailRow.RelativeItem().Text("Details").FontSize(9).Bold();
                                    detailRow.RelativeItem().AlignRight().Text(":التفاصيل").FontFamily(FontName).FontSize(9).Bold();
                                });
                            col.Item().BorderBottom(1).BorderColor(Colors.Black).PaddingTop(2).PaddingBottom(2);

                            col.Item().PaddingTop(5).PaddingBottom(80).Text("Travel Insurance").FontSize(9).Bold();
                        });
                        table.Cell().Border(0.5f).Padding(8);

                        table.Cell().Border(0.5f).Padding(8).Row(totalRow =>
                        {
                            totalRow.RelativeItem().AlignLeft().Text($"Total: {CurrencyToWordsService.ToWordsEnglish(policy?.grossPremium ?? 0)}").FontSize(9).Bold();
                            totalRow.RelativeItem().AlignRight().Text($"المجموع: {CurrencyToWordsService.ToWordsArabic(policy?.grossPremium ?? 0)}").FontFamily(FontName).FontSize(9).Bold();
                        });
                        table.Cell().Border(0.5f).Padding(8).AlignCenter().AlignMiddle().Text(policy?.grossPremium.ToString("F2")).FontSize(12).Bold();
                    });

                    // column.Item().PaddingTop(40).Row(sigRow =>
                    // {
                    //     sigRow.RelativeItem().Column(sigCol =>
                    //     {
                    //         sigCol.Item().AlignCenter().Text("Signature/Stamps").FontSize(12).Bold();
                    //         sigCol.Item().AlignCenter().Text("التوقيع/الختم").FontFamily(FontName).FontSize(12).Bold();
                    //     });

                    //     sigRow.RelativeItem();
                    //     sigRow.RelativeItem();
                    // });

                    column.Item().PaddingTop(60);

                    column.Item().AlignRight().PaddingRight(35).Container().Width(130).PaddingTop(170).LineHorizontal(1).LineColor(Colors.Black);
                    column.Item().PaddingTop(5).AlignRight().PaddingRight(40).Text("Authorized Signature").FontSize(12).Bold();
                });
            });

        }).GeneratePdf();
    }
}

public class IndividualReportInfoDTO
{
    public required string policyTypeNameEnglish { get; set; }
    public required string policyNumber { get; set; }
    public required string fullNameEnglish { get; set; }
    public required string fullNameArabic { get; set; }
    public required DateOnly dateOfBirth { get; set; }
    public required int gender { get; set; }
    public required string nationalityEnglish { get; set; }
    public required string nationalityArabic { get; set; }
    public required string passportNumber { get; set; }
    public required string phoneNumber { get; set; }
    public required string permanentAddressEnglish { get; set; }
    public required string permanentAddressArabic { get; set; }
    public required string periodEnglish { get; set; }
    public required string periodArabic { get; set; }
    public required DateOnly inceptionDate { get; set; }
    public required DateOnly expiryDate { get; set; }
    public required string destinationEnglish { get; set; }
    public required string destinationArabic { get; set; }
    public required string typeOfCoverageEnglish { get; set; }
    public required string typeOfCoverageArabic { get; set; }
    public required DateOnly issueDate { get; set; }
    public required string paymentMethod { get; set; }
    public required string paymentAccount { get; set; }
    public required double grossPremium { get; set; }
}