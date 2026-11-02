using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Models;

namespace TrustSystem.Backend.Data;

public class APPL_INS(DbContextOptions<APPL_INS> options) : DbContext(options)
{
    public DbSet<TV_POLICIES> TV_POLICIES => Set<TV_POLICIES>();
    public DbSet<BrokerCommission> BrokerCommission => Set<BrokerCommission>();
    public DbSet<TV_POLICIES_ADDITIONAL> TV_POLICIES_ADDITIONAL => Set<TV_POLICIES_ADDITIONAL>();
    public DbSet<TV_POLICY_PACKAGE> TV_POLICY_PACKAGE => Set<TV_POLICY_PACKAGE>();
    public DbSet<TravelPrice> TravelPrices => Set<TravelPrice>();
    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<TypeOfCoverage> TypesOfCoverage => Set<TypeOfCoverage>();
    public DbSet<PolicyType> PolicyTypes => Set<PolicyType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => new { user.GroupId, user.Code, user.CompanyGroupId });
            entity.Ignore(u => u.Office);
        });

        modelBuilder.Entity<CompanyOffice>(entity =>
        {
            entity.Ignore(o => o.Users);
        });

        modelBuilder.Entity<DomainValue>(entity =>
        {
            entity.HasKey(e => new
            {
                e.Id,
                e.DomainId,
                e.CompanyGroupId,
                e.SystemId
            });

            entity.HasQueryFilter(domainValue =>
            domainValue.SystemId == 3);

            entity.HasDiscriminator(e => e.DomainId)
                  .HasValue<DomainValue>(0)
                  .HasValue<TypeOfCoverage>(104)
                  .HasValue<PolicyType>(550)
                  .IsComplete(false);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => new
            {
                e.CompanyGroupId,
                e.Code,
            });
        });

        modelBuilder.Entity<TravelPrice>(entity =>
        {
            entity.HasKey(e => new
            {
                e.TypeOfCoverageId,
                e.Duration,
                e.AgeFrom,
                e.AgeTo,
                e.CompanyGroupId,
                e.PlanType
            });
        });

        modelBuilder.Entity<TV_POLICY_PACKAGE>(entity =>
        {
            entity.HasKey(groupPolicy => new
            {
                groupPolicy.TPP_OFC_ID,
                groupPolicy.TPP_PLT_ID,
                groupPolicy.TPP_YEAR,
                groupPolicy.TPP_POL_NO,
            });

            entity.HasOne(groupPolicy => groupPolicy.DestinationCountry)
                  .WithMany()
                  .HasForeignKey(groupPolicy => groupPolicy.TPP_DESTINATION)
                  .HasPrincipalKey(country => country.Id);

            entity.HasOne(groupPolicy => groupPolicy.CreatedBy)
                .WithMany()
                .HasForeignKey(p => new { p.TPP_CREATED_BY, p.TPP_CGP_ID })
                .HasPrincipalKey(u => new { u.Code, u.CompanyGroupId });

            entity.HasOne(policy => policy.TypeOfCoverage)
                .WithMany()
                .HasForeignKey(policy => new
                {
                    policy.TPP_AREA_CODE,
                    policy.TPP_CGP_ID
                })
                .HasPrincipalKey(typeOfCoverage => new
                {
                    typeOfCoverage.Id,
                    typeOfCoverage.CompanyGroupId
                });

            entity.HasOne(policy => policy.CompanyOffice)
                  .WithMany()
                  .HasForeignKey(policy => new
                  {
                      policy.TPP_OFC_ID,
                      policy.TPP_CBR_ID,
                      policy.TPP_CMP_ID,
                      policy.TPP_CGP_ID
                  })
                  .HasPrincipalKey(companyOffice => new
                  {
                      companyOffice.Id,
                      companyOffice.CompanyBranchId,
                      companyOffice.CompanyId,
                      companyOffice.CompanyGroupId
                  });
        });

        modelBuilder.Entity<TV_POLICIES>(entity =>
        {
            entity.HasKey(e => new
            {
                e.POL_NO,
                e.POL_YEAR,
                e.POL_ENDT_NO,
                e.POL_ENDT_YEAR,
                e.POL_CLS_ID,
                e.POL_PLT_ID,
                e.POL_CMP_ID,
                e.POL_CGP_ID,
                e.POL_DCT_ID,
                e.POL_OFC_ID,
                e.POL_CBR_ID,
                e.POL_INT_SERIAL
            });

            entity.HasOne(policy => policy.groupPolicy)
                  .WithMany(groupPolicy => groupPolicy.subPolicies)
                  .HasForeignKey(policy => new
                  {
                      policy.POL_OFC_ID,
                      policy.POL_PLT_ID,
                      policy.POL_YEAR,
                      policy.POL_NO,
                      policy.POL_STATUS
                  })
                  .HasPrincipalKey(groupPolicy => new
                  {
                      groupPolicy.TPP_OFC_ID,
                      groupPolicy.TPP_PLT_ID,
                      groupPolicy.TPP_YEAR,
                      groupPolicy.TPP_POL_NO,
                      groupPolicy.TPP_STATUS
                  });

            entity.HasOne(policy => policy.TypeOfCoverage)
                  .WithMany()
                  .HasForeignKey(policy => new
                  {
                      policy.POL_AREA_CODE,
                      policy.POL_CGP_ID
                  })
                  .HasPrincipalKey(typeOfCoverage => new
                  {
                      typeOfCoverage.Id,
                      typeOfCoverage.CompanyGroupId
                  });

            entity.HasOne(policy => policy.OriginCountry)
                  .WithMany()
                  .HasForeignKey(policy => new
                  {
                      policy.POL_SOURCE
                  })
                  .HasPrincipalKey(country => new
                  {
                      country.Id
                  });

            entity.HasOne(policy => policy.DestinationCountry)
                  .WithMany()
                  .HasForeignKey(policy => new
                  {
                      policy.POL_DESTINATION
                  })
                  .HasPrincipalKey(country => new
                  {
                      country.Id
                  });

            entity.HasOne(policy => policy.NationalityCountry)
                  .WithMany()
                  .HasForeignKey(policy => new
                  {
                      policy.POL_PASSEGER_NATIONALITY_ID
                  })
                  .HasPrincipalKey(country => new
                  {
                      country.Id
                  });

            entity.HasOne(policy => policy.CompanyOffice)
                  .WithMany()
                  .HasForeignKey(policy => new
                  {
                      policy.POL_OFC_ID,
                      // policy.POL_CBR_ID,
                      policy.POL_CMP_ID,
                      policy.POL_CGP_ID
                  })
                  .HasPrincipalKey(companyOffice => new
                  {
                      companyOffice.Id,
                      // companyOffice.CompanyBranchId,
                      companyOffice.CompanyId,
                      companyOffice.CompanyGroupId
                  });

            entity.HasOne(policy => policy.PaymentAccount)
                  .WithMany()
                  .HasForeignKey(policy => new
                  {
                      policy.POL_TA_CODE,
                      policy.POL_CGP_ID
                  })
                  .HasPrincipalKey(account => new
                  {
                      account.Code,
                      account.CompanyGroupId
                  })
                  .IsRequired(false);

            entity.HasOne(policy => policy.additionalInfo)
                  .WithOne()
                  .HasForeignKey<TV_POLICIES_ADDITIONAL>(additional => new
                  {
                      additional.ADD_POL_NO,
                      additional.ADD_POL_CMP_ID,
                      additional.ADD_POL_CGP_ID,
                      additional.ADD_POL_DCT_ID,
                      additional.ADD_POL_OFC_ID,
                      additional.ADD_POL_CBR_ID,
                      additional.ADD_POL_INT_SERIAL
                  })
                  .HasPrincipalKey<TV_POLICIES>(policy => new
                  {
                      policy.POL_NO,
                      policy.POL_CMP_ID,
                      policy.POL_CGP_ID,
                      policy.POL_DCT_ID,
                      policy.POL_OFC_ID,
                      policy.POL_CBR_ID,
                      policy.POL_INT_SERIAL
                  });

            entity.HasOne(p => p.CreatedBy)
                  .WithMany()
                  .HasForeignKey(p => new { p.POL_CREATED_BY, p.POL_CGP_ID })
                  .HasPrincipalKey(u => new { u.Code, u.CompanyGroupId });
        });

        modelBuilder.Entity<TV_POLICIES_ADDITIONAL>(entity =>
        {
            entity.HasKey(e => new
            {
                e.ADD_POL_NO,
                e.ADD_POL_YEAR,
                e.ADD_POL_ENDT_NO,
                e.ADD_POL_ENDT_YEAR,
                e.ADD_POL_CLS_ID,
                e.ADD_POL_PLT_ID,
                e.ADD_POL_CMP_ID,
                e.ADD_POL_CGP_ID,
                e.ADD_POL_DCT_ID,
                e.ADD_POL_OFC_ID,
                e.ADD_POL_CBR_ID,
                e.ADD_POL_INT_SERIAL,
            });
        });

        modelBuilder.Entity<BrokerCommission>(entity =>
        {
            entity.HasKey(e => new
            {
                e.CompanyGroupId,
                e.BrokerCode,

                e.STC_BROKER_SLT_ID,
                e.STC_CMP_ID,
                e.STC_CLS_ID,
                e.STC_PLT_ID,
                e.STC_BUT_ID,
                e.STC_IRS_TYPE,
                e.STC_IRS_ID,
                e.STC_PREM_IRS_TYPE
            });
        });

        base.OnModelCreating(modelBuilder);
    }
}
