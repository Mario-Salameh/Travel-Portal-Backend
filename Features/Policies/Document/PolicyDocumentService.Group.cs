using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Endpoints;
using TrustSystem.Backend.Models;

namespace TrustSystem.Backend.Application.Reports;

public partial class PolicyDocumentService
{
    private async Task<byte[]> GenerateGroupReport(TV_POLICY_PACKAGE groupPolicy, bool isDraft = true)
    {
        var isCancelled = groupPolicy.PolicyStatus == PolicyStatus.Endorsed;

        if (UserGeneratedReport!.IsCustomer() && groupPolicy.PolicyStatus == PolicyStatus.Draft)
            throw new ApplicationException("Policy does not paid yet");

        if (!isDraft)
        {
            groupPolicy.TPP_PRINT_FLAG = (groupPolicy.TPP_PRINT_FLAG < 9) ? (groupPolicy.TPP_PRINT_FLAG + 1) : 9;
            _dbIns.Update(groupPolicy);
            await _dbIns.SaveChangesAsync();
        }

        var data = new CustomerViewGroupPolicy(groupPolicy);
        var totalPremium = (isCancelled ? -1 : 1) * Math.Abs(groupPolicy.TotalPremium);
        var insuredPersonsCount = groupPolicy.subPolicies?.Count ?? 0;

        var periods = await _dbCore.CR_DOMAIN_VALUES.Where(e => e.DomainId == PlanMapper.periods[3] &&
                                                               e.CompanyGroupId == groupPolicy.TPP_CGP_ID &&
                                                               e.Id == groupPolicy.TPP_PERIOD.ToString()).ToListAsync();
        var period = periods.FirstOrDefault();
        var destinationInfo = groupPolicy.DestinationCountry!;

        var typesOfCoverage = groupPolicy.TypeOfCoverage;

        QuestPDF.Settings.License = LicenseType.Community;
        FontManager.RegisterFont(File.OpenRead(AssetLocator.Font("Arabic.otf")));
        const string fontName = "Arabic UI Display";

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
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().AlignLeft().AlignMiddle().Text("Trust International\nInsurance Company - Palestine").FontSize(10).Bold();
                        row.RelativeItem().AlignCenter().Container().Width(60).Image(AssetLocator.Image("trust_logo.png"));
                        row.RelativeItem().AlignRight().AlignMiddle().Text("شركة ترست العالمية للتأمين - فلسطين").FontFamily(fontName).FontSize(12).Bold();
                    });

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
                            col.Item().Text("في حالة الطوارئ يرجى الاتصال").FontFamily(fontName).FontSize(8).Bold().AlignRight();
                            col.Item().Text("شركة سوان للمساعدة").FontFamily(fontName).FontSize(8).AlignRight();
                            col.Item().Text("001 514 448 4417 امريكا و كندا").FontFamily(fontName).FontSize(8).AlignRight();
                            col.Item().Text("0033 9 7073 2247 فرنسا و اوروبا").FontFamily(fontName).FontSize(8).AlignRight();
                            col.Item().Text("00961 9 211 662 دولي").FontFamily(fontName).FontSize(8).AlignRight();
                            col.Item().Text("00961 81 284 602 واتساب").FontFamily(fontName).FontSize(7).AlignRight();
                            col.Item().Text("request@swanassistance.com :بريد الكتروني").FontFamily(fontName).FontSize(7).AlignRight();
                        });
                    });
                });

                page.Content().PaddingTop(20).Column(column =>
                {
                    column.Item().Background(Colors.Grey.Lighten3).Padding(5).Row(row =>
                    {
                        row.RelativeItem().Text("Policy No:").FontSize(11).Bold();
                        row.RelativeItem().AlignCenter().Text(data.policyNumber).FontSize(11).Bold();
                        row.RelativeItem().AlignRight().Text(":رقم الوثيقة").FontFamily(fontName).FontSize(11).Bold();
                    });

                    column.Item().PaddingTop(10).Background(Colors.Grey.Lighten3).Padding(5).Row(row =>
                    {
                        row.RelativeItem().Text("Personal Information").FontSize(10).Bold();
                        row.RelativeItem().AlignRight().Text("المعلومات الشخصية").FontFamily(fontName).FontSize(10).Bold();
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

                        table.Cell().Border(0.5f).Padding(3).Text("Policy Holder/Insured").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(data.companyNameNative).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(data.companyNameForeign).FontFamily(fontName).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("حامل الوثيقة / المؤمن له").FontFamily(fontName).FontSize(9);
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
                        table.Cell().Border(0.5f).Padding(3).Text(period?.NameInEnglish).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(period?.NameInArabic).FontFamily(fontName).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("مدة التأمين").FontFamily(fontName).FontSize(9);
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
                        table.Cell().Border(0.5f).Padding(3).Text(data.startDate.ToString("dd/MM/yyyy")).AlignCenter().FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("تاريخ بدء التأمين").FontFamily(fontName).FontSize(9);

                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(6);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Border(0.5f).Padding(3).Text("Expiry Date").FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(groupPolicy.TPP_EXPIRY_DATE.ToString("dd/MM/yyyy")).AlignCenter().FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("تاريخ انتهاء التأمين").FontFamily(fontName).FontSize(9);

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
                        table.Cell().Border(0.5f).Padding(3).Text(destinationInfo.NameInEnglish).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).Text(destinationInfo.NameInArabic).AlignRight().FontFamily(fontName).FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("وجهة السفر").FontFamily(fontName).FontSize(9);
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
                        table.Cell().Border(0.5f).Padding(3).Text(typesOfCoverage!.NameInEnglish).AlignCenter().FontSize(9);
                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text("نوع التغطية").FontFamily(fontName).FontSize(9);

                    });


                    column.Item().PaddingTop(10).Background(Colors.Grey.Lighten3).Padding(5).Row(row =>
                    {
                        row.RelativeItem().Text("Gross premium as agreed").FontSize(10).Bold();
                        row.RelativeItem().AlignCenter().Text(totalPremium.ToString("F2")).FontSize(10).Bold();
                        row.RelativeItem().AlignRight().Text("القسط الاجمالي حسب الاتفاق").FontFamily(fontName).FontSize(10).Bold();
                    });

                    column.Item().PaddingTop(10).AlignCenter().Text("I HAVE READ, UNDERSTOOD & AGREED TO THE POLICY TERMS & CONDITIONS").FontSize(9).Bold();
                    column.Item().AlignCenter().Text("لقد قرأت وفهمت وأوافق على الشروط العامه للوثيقة").FontFamily(fontName).FontSize(9);

                    column.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text($"Issue date: {groupPolicy.TPP_ISSUE_DATE.ToString("dd/MM/yyyy")}").FontSize(9);
                            if (!groupPolicy.IsCreatedByCustomerPortal)
                                col.Item().Text($"Agent/Agency: {groupPolicy.CompanyOffice!.NameInEnglish}").FontSize(9);
                            col.Item().Text("For/Trust International Insurance co.").FontSize(8);

                            col.Item().PaddingTop(18).Text("Signature/Stamp").FontSize(8);
                            col.Item().Text("توقيع وختم الموظف/الوكيل").FontFamily(fontName).FontSize(8);
                        });

                        if (!isDraft) row.AddQRCode(groupPolicy);

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text($"تاريخ الاصدار: {groupPolicy.TPP_ISSUE_DATE.ToString("dd/MM/yyyy")}").FontFamily(fontName).FontSize(8);
                            if (!groupPolicy.IsCreatedByCustomerPortal)
                                col.Item().Text($"الوكاله/الوكيل: {groupPolicy.CompanyOffice!.NameInArabic}").FontFamily(fontName).FontSize(8);
                            col.Item().Text("").FontSize(8);

                            col.Item().PaddingTop(18).Text("Signature/Insured").FontSize(8);
                            col.Item().Text("توقيع المؤمن له").FontFamily(fontName).FontSize(8);
                        });
                    });

                    column.AddPolicyFooter(groupPolicy);
                });
            });

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                if (isCancelled)
                    page.AddCancelledWaterMark();
                else if (isDraft)
                    page.AddDraftWaterMark();

                page.Header().Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().AlignLeft().AlignMiddle().Text("Trust International\nInsurance Company - Palestine").FontSize(10).Bold();
                        row.RelativeItem().AlignCenter().Container().Width(60).Image(AssetLocator.Image("trust_logo.png"));
                        row.RelativeItem().AlignRight().AlignMiddle().Text("شركة ترست العالمية للتأمين - فلسطين").FontFamily(fontName).FontSize(12).Bold();
                    });

                    // column.Item().PaddingTop(20).AlignCenter().Container()
                    // .Border(0.5f).BorderLinearGradient(Colors.Black, ["#851819", "#e1af0bff"])
                    // .Padding(5)
                    // .AlignCenter()
                    // .Text($"INVOICE       {data.policyNumber}").FontSize(12);

                    column.AddInvoice(groupPolicy);
                });

                page.Content().PaddingTop(20).Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        if (!groupPolicy.IsCreatedByCustomerPortal)
                            row.RelativeItem(2).Border(1).BorderColor(Colors.Black).Padding(10).Row(col =>
                            {
                                col.RelativeItem().AlignLeft().Text(groupPolicy.CompanyOffice!.NameInEnglish).FontSize(10);
                                col.RelativeItem().AlignRight().Text(groupPolicy.CompanyOffice!.NameInArabic).FontFamily(fontName).FontSize(9);
                            });

                        row.RelativeItem(3).Border(1).BorderColor(Colors.Black).Column(dateCol =>
                        {
                            dateCol.Item().BorderBottom(1).BorderColor(Colors.Black).Row(dateRow =>
                            {
                                dateRow.RelativeItem().Padding(5).Text("Date:").FontSize(9);
                                dateRow.RelativeItem().Padding(5).Text(groupPolicy.TPP_ISSUE_DATE.ToString("dd/MM/yyyy")).FontSize(9);
                                dateRow.RelativeItem().Padding(5).AlignRight().Text(":التاريخ").FontFamily(fontName).FontSize(9);
                            });

                            dateCol.Item().BorderBottom(1).BorderColor(Colors.Black).Row(policyRow =>
                            {
                                policyRow.RelativeItem().Padding(5).Text("Policy No:").FontSize(9);
                                policyRow.RelativeItem().Padding(5).Text(data.policyNumber).FontSize(9);
                                policyRow.RelativeItem().Padding(5).AlignRight().Text(":رقم الوثيقة").FontFamily(fontName).FontSize(9);
                            });

                            dateCol.Item().BorderBottom(1).BorderColor(Colors.Black).Row(paymentRow =>
                            {
                                paymentRow.RelativeItem().Padding(5).Text("Payment Type:").FontSize(9);
                                paymentRow.RelativeItem().Padding(5).Text("On Account").FontSize(9);
                                paymentRow.RelativeItem().Padding(5).AlignRight().Text(":طريقة الدفع").FontFamily(fontName).FontSize(9);
                            });

                            dateCol.Item().Row(accountRow =>
                            {
                                accountRow.RelativeItem().Padding(5).Text("Account No:").FontSize(9);
                                accountRow.RelativeItem().Padding(5).Text(groupPolicy.TPP_TA_CODE).FontSize(9);
                                accountRow.RelativeItem().Padding(5).AlignRight().Text(":رقم الحساب").FontFamily(fontName).FontSize(9);
                            });
                        });
                    });

                    column.Item().PaddingTop(10).Background(Colors.Grey.Lighten3).Padding(5).Row(row =>
                    {
                        row.RelativeItem().Text("Insured:").FontSize(10).Bold();
                        row.RelativeItem(4).Text(data.companyNameNative).FontSize(10);
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
                                    detailRow.RelativeItem().AlignRight().Text(":التفاصيل").FontFamily(fontName).FontSize(9).Bold();
                                });
                            col.Item().BorderBottom(1).BorderColor(Colors.Black).PaddingTop(2).PaddingBottom(2);

                            col.Item().PaddingTop(5).PaddingBottom(80).Text("Travel Insurance").FontSize(9).Bold();
                        });
                        table.Cell().Border(0.5f).Padding(8);

                        table.Cell().Border(0.5f).Padding(8).Row(totalRow =>
                        {
                            totalRow.RelativeItem().AlignLeft().Text($"Total: {CurrencyToWordsService.ToWordsEnglish((double)totalPremium)}").FontSize(9).Bold();
                            totalRow.RelativeItem().AlignRight().Text($"المجموع: {CurrencyToWordsService.ToWordsArabic((double)totalPremium)}").FontFamily(fontName).FontSize(9).Bold();
                        });
                        table.Cell().Border(0.5f).Padding(8).AlignCenter().AlignMiddle().Text(totalPremium.ToString("F2")).FontSize(12).Bold();
                    });

                    column.Item().PaddingTop(40).Row(sigRow =>
                    {
                        sigRow.RelativeItem().Column(sigCol =>
                        {
                            sigCol.Item().AlignCenter().Text("Signature/Stamps").FontSize(12).Bold();
                            sigCol.Item().AlignCenter().Text("التوقيع/الختم").FontFamily(fontName).FontSize(12).Bold();
                        });

                        sigRow.RelativeItem();
                        sigRow.RelativeItem();
                    });

                    column.Item().AlignRight().PaddingRight(35).Container().Width(130).PaddingTop(170).LineHorizontal(1).LineColor(Colors.Black);
                    column.Item().PaddingTop(5).AlignRight().PaddingRight(40).Text("Authorized Signature").FontSize(12).Bold();
                });
            });


            // Page 3 Starts Here
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                if (isCancelled)
                    page.AddCancelledWaterMark();
                else if (isDraft)
                    page.AddDraftWaterMark();

                page.Header().Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().AlignLeft().AlignMiddle().Text("Endorsement of Assistance and Travel Insurance Policy \n 'Group Travel'").FontSize(10).Bold();
                        row.RelativeItem().AlignRight().AlignMiddle().Text("ملحق وثيقة العلاج الطبي والمساعدة أثناء السفر  \n 'السفر الجماعي'").FontFamily(fontName).FontSize(10).Bold();
                    });
                });

                page.Content().PaddingTop(10).Column(column =>
                {
                    column.Item().Table(table =>
                     {
                         table.ColumnsDefinition(columns =>
                         {
                             columns.RelativeColumn(2);
                             columns.RelativeColumn(6);
                             columns.RelativeColumn(2);
                         });

                         table.Cell().Border(0.5f).Padding(3).Text("Policy No").FontSize(9);
                         table.Cell().Border(0.5f).Padding(3).Text(data.policyNumber).AlignCenter().FontSize(9);
                         table.Cell().Border(0.5f).Padding(3).AlignRight().Text("رقم الوثيقة").FontFamily(fontName).FontSize(9);
                         table.Cell().Border(0.5f).Padding(3).Text("Issue Date").FontSize(9);
                         table.Cell().Border(0.5f).Padding(3).Text(groupPolicy.TPP_ISSUE_DATE.ToString("dd/MM/yyyy")).AlignCenter().FontSize(9);
                         table.Cell().Border(0.5f).Padding(3).AlignRight().Text("تاريخ الاصدار").FontFamily(fontName).FontSize(9);

                     });

                    column.Item().PaddingTop(10).Border(1).Background(Colors.Grey.Lighten3).Padding(10).Row(row =>
                  {
                      row.RelativeItem().Text($"Insured Persons ({insuredPersonsCount})").FontSize(13).Bold();
                      row.RelativeItem().AlignRight().Text($"المؤمن لهم ({insuredPersonsCount})").FontFamily(fontName).FontSize(13);
                  });

                    column.Item().Border(1).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(5);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            table.Cell().Background(Colors.Grey.Lighten3).AlignCenter().Padding(5).Text("Full Name \n الاسم الكامل").FontFamily(fontName);
                            table.Cell().Background(Colors.Grey.Lighten3).Border(1).Padding(5).AlignCenter().Text("Gender \n الجنس").FontFamily(fontName);
                            table.Cell().Background(Colors.Grey.Lighten3).Border(1).AlignCenter().Padding(5).Text("Birth Date \n تاريخ الميلاد").FontFamily(fontName);
                            table.Cell().Background(Colors.Grey.Lighten3).Border(1).AlignCenter().Padding(5).Text("Age \n العمر").FontFamily(fontName);
                            table.Cell().Background(Colors.Grey.Lighten3).Border(1).AlignCenter().Padding(5).Text("Passport No. \n رقم جواز السفر").FontFamily(fontName);
                            table.Cell().Background(Colors.Grey.Lighten3).Border(1).AlignCenter().Padding(5).Text("Nationality \n الجنسية").FontFamily(fontName);
                            table.Cell().Background(Colors.Grey.Lighten3).Border(1).AlignCenter().Padding(5).Text("Premium \n القسط").FontFamily(fontName);
                        });

                        foreach (var sub in groupPolicy.subPolicies!)
                        {
                            var fullNameNative = $"{sub.POL_PASSEGER_1ST_NAME_NTV} {sub.POL_PASSEGER_FATHER_NAME_NTV} {sub.POL_PASSEGER_LAST_NAME_NTV}";
                            var fullNameForeign = $"{sub.POL_PASSEGER_1ST_NAME_FRN} {sub.POL_PASSEGER_FATHER_NAME_FRN} {sub.POL_PASSEGER_LAST_NAME_FRN}";
                            var gender = sub.POL_PASSEGER_GENDER == 1 ? "Male" : "Female";
                            var birthDate = sub.POL_PASSEGER_BIRTH_DATE.ToString("dd/MM/yyyy");
                            var today = DateTime.Today;
                            var dob = sub.POL_PASSEGER_BIRTH_DATE;
                            var age = sub.POL_PASSEGER_AGE;
                            if (dob.Date > today.AddYears(-age))
                            {
                                age--;
                            }
                            var passport = sub.POL_PASSEGER_PASSPORT_NUMBER ?? "";
                            var nationality = sub.NationalityCountry?.NameInEnglish ?? "";
                            var premium = sub.POL_PREMIUM_AMOUNT.ToString("F2");

                            table.Cell().Border(1).AlignCenter().Padding(5).Text($"{fullNameNative}\n{fullNameForeign}").FontFamily(fontName).FontSize(8);
                            table.Cell().Border(1).AlignCenter().Padding(10).Text(gender).FontSize(8);
                            table.Cell().Border(1).AlignCenter().Padding(10).Text(birthDate).FontSize(8);
                            table.Cell().Border(1).AlignCenter().Padding(10).Text(age.ToString()).FontSize(8);
                            table.Cell().Border(1).AlignCenter().Padding(10).Text(passport).FontSize(8);
                            table.Cell().Border(1).AlignCenter().Padding(10).Text(nationality).FontSize(8);
                            table.Cell().Border(1).AlignCenter().Padding(10).Text(premium).FontSize(8);
                        }
                    });

                    // column.Item().PaddingTop(30).Column(col =>
                    // {
                    //     col.Item().Text("** لا تعد هذه الوثيقة عقدا تأمينيا وإنما لغرض المراجعة **").FontFamily(fontName).AlignRight().FontSize(7);
                    //     col.Item().Text("** الوثيقة غير قابلة للالغاء، ويتم التعديل فقط قبل بداية تاريخ التأمين **").FontFamily(fontName).AlignRight().FontSize(7);
                    //     col.Item().Text("** لا تكون هذه الوثيقة سارية الا اذا كان تاريخ اصدارها قبل تاريخ مغادرة فلسطين **").FontFamily(fontName).AlignRight().FontSize(7);
                    //     col.Item().Text($"** تشمل هذه الوثيقة التكاليف الطبية والجراحية والصيدلانية في الخارج بسبب المرض أو الحادث المفاجئ بحد أقصى {GetCoverLimit(groupPolicy.TypeOfCoverage!.Id)} دولار امريكي وفقا لشروط البوليصة **").FontFamily(fontName).AlignRight().FontSize(7);
                    //     col.Item().Text("** لا تكون هذه الوثيقة سارية الا اذا كانت تحمل ختم شركة الأراضي المقدسة للتأمين التكافلي أو احد وكلائه **").FontFamily(fontName).AlignRight().FontSize(7);
                    //     col.Item().PaddingTop(5).Text("* The document is Not applicable to cancellation, and the amendment is only before the beginning of the insurance date.").FontSize(7);
                    //     col.Item().Text("* This document shall not be valid unless it is stamped by the Al-Aradi Al-Muqadasa T.I Company or one of its agents.").FontSize(7);
                    //     col.Item().Text("* This document shall not be valid unless its date of issue is prior to the date of leaving Palestine.").FontSize(7);
                    //     col.Item().Text($"*This document includes medical hospitalization, surgical, and pharmaceutical expenses abroad, due to sudden illness or accident up to {GetCoverLimit(groupPolicy.TypeOfCoverage!.Id)} USD and subject to the term and condition of the policy").FontSize(7);
                    // });

                    column.AddPolicyFooter(groupPolicy);
                });
            });
        }).GeneratePdf();
    }
}

// 3. Use it:
// byte[] pdfBytes = PdfGenerator.GenerateSimplePdf();
// File.WriteAllBytes("output.pdf", pdfBytes);
// or return it from API: return File(pdfBytes, "application/pdf", "policy.pdf");