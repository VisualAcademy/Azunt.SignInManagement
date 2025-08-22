using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Azunt.SignInManagement
{
    /// <summary>
    /// EF Core Fluent API 매핑.
    /// - SignIns.sql 스키마와 정밀도/길이/필수 제약을 일치시킵니다.
    /// - v1.0.1: TenantId/TenantName은 레거시 미사용으로 매핑하지 않습니다.
    /// </summary>
    public class SignInConfiguration : IEntityTypeConfiguration<SignIn>
    {
        public void Configure(EntityTypeBuilder<SignIn> b)
        {
            b.ToTable("SignIns");
            b.HasKey(x => x.Id);

            b.Property(x => x.DateTimeSignedIn)
             .HasPrecision(0)          // DATETIMEOFFSET(0)
             .IsRequired();

            b.Property(x => x.UserId)
             .HasMaxLength(450);

            b.Property(x => x.Email)
             .IsRequired();

            b.Property(x => x.Result)
             .IsRequired();

            // v1.0.2에서 테넌트 필드 사용 시작 시 아래 매핑을 활성화:
            // b.Property(x => x.TenantId);
            // b.Property(x => x.TenantName).HasMaxLength(255);
        }
    }
}
