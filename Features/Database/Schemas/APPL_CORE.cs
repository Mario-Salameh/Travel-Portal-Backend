using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Models;

namespace TrustSystem.Backend.Data;

public class APPL_CORE(DbContextOptions<APPL_CORE> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<DomainValue> CR_DOMAIN_VALUES => Set<DomainValue>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<Country> CR_COUNTRIES => Set<Country>();
    public DbSet<CurrencyRate> CR_CURRENCY_RATES => Set<CurrencyRate>();
    public DbSet<CompanyOffice> CR_OFFICES => Set<CompanyOffice>();
    public DbSet<CompanyGroup> CR_COMP_GROUPS => Set<CompanyGroup>();
    public DbSet<CompanyBranch> CR_COMPANY_BRANCHES => Set<CompanyBranch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompanyGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<CompanyBranch>(entity =>
        {
            entity.HasKey(e => new
            {
                e.CompanyGroupId,
                e.Id
            });
        });

        modelBuilder.Entity<CurrencyRate>(entity =>
        {
            entity.HasKey(e => new
            {
                e.Id,
                e.CompanyGroupId,

                e.BaseCurrencyCode,
                e.CurrencyCode
            });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => new { user.GroupId, user.Code, user.CompanyGroupId });

            entity.HasOne(user => user.Office)
                  .WithMany(office => office.Users)
                  .HasForeignKey(user => new
                  {
                      // user.USR_CBR_ID,
                      user.CompanyId,
                      user.CompanyGroupId,
                      user.USR_OFC_ID
                  })
                  .HasPrincipalKey(office => new
                  {
                      // office.CompanyBranchId,
                      office.CompanyId,
                      office.CompanyGroupId,
                      office.Id
                  });
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasNoKey();
        });

        modelBuilder.Entity<CompanyOffice>(entity =>
        {
            entity.HasKey(e => new
            {
                e.Id,
                e.CompanyBranchId,
                e.CompanyId,
                e.CompanyGroupId
            });
        });

        modelBuilder.Entity<DomainValue>(domainValue =>
        {
            domainValue.HasKey(v => new
            {
                v.DomainId,
                v.Id,
                v.SystemId,
                v.CompanyGroupId
            });
        });

        base.OnModelCreating(modelBuilder);
    }
}
