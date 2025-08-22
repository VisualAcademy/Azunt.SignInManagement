using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Azunt.SignInManagement;

/// <summary>
/// EF Core Fluent API 매핑: 테이블/컬럼 스키마 정밀도 등을 보장
/// </summary>
public class SignInConfiguration : IEntityTypeConfiguration<SignIn>
{
    public void Configure(EntityTypeBuilder<SignIn> b)
    {
        b.ToTable("SignIns");
        b.HasKey(x => x.Id);

        // 컬럼 매핑(정밀도/길이/필수)
        b.Property(x => x.DateTimeSignedIn)
            .HasPrecision(0)                 // DATETIMEOFFSET(0)
            .IsRequired();

        b.Property(x => x.UserId)
            .HasMaxLength(450);

        b.Property(x => x.Email)
            .IsRequired();                   // NVARCHAR(MAX) NOT NULL

        b.Property(x => x.Result)
            .IsRequired();                   // NVARCHAR(MAX) NOT NULL

        b.Property(x => x.TenantName)
            .HasMaxLength(255);
    }
}
