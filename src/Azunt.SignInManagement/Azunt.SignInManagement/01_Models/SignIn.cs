using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Azunt.SignInManagement
{
    /// <summary>
    /// 사용자 로그인 이력 엔티티 (매핑 대상: [dbo].[SignIns]).
    /// </summary>
    [Table("SignIns")]
    public class SignIn
    {
        /// <summary>기본 키 (IDENTITY).</summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>로그인 시각 (DATETIMEOFFSET(0)).</summary>
        public DateTimeOffset DateTimeSignedIn { get; set; }

        /// <summary>사용자 ID (최대 450자).</summary>
        [MaxLength(450)]
        public string? UserId { get; set; }

        /// <summary>로그인에 사용된 이메일 (필수).</summary>
        [Required]
        public string Email { get; set; } = default!;

        /// <summary>이름.</summary>
        public string? FirstName { get; set; }

        /// <summary>성.</summary>
        public string? LastName { get; set; }

        /// <summary>로그인 결과(예: Success/Failure) (필수).</summary>
        [Required]
        public string Result { get; set; } = default!;

        /// <summary>클라이언트 IP 주소.</summary>
        public string? IpAddress { get; set; }

        /// <summary>비고/메모.</summary>
        public string? Note { get; set; }

        /// <summary>
        /// 테넌트 ID (레거시 미사용 → EF 매핑 제외).
        /// </summary>
        [NotMapped]
        public long? TenantId { get; set; }

        /// <summary>
        /// 테넌트 이름 (레거시 미사용 → EF 매핑 제외, 최대 255자).
        /// </summary>
        [MaxLength(255)]
        public string? TenantName { get; set; }

        // --------------------------------------------------------------------
        // Legacy-friendly shims (EF 미매핑)
        // 기존 레거시 코드의 대문자 명명(ID/UserID/IPAddress)을 그대로 지원합니다.
        // --------------------------------------------------------------------

        /// <summary>레거시 호환용: ID ↔ Id.</summary>
        [NotMapped]
        public long ID
        {
            get => Id;
            set => Id = value;
        }

        /// <summary>레거시 호환용: UserID ↔ UserId.</summary>
        [NotMapped]
        public string? UserID
        {
            get => UserId;
            set => UserId = value;
        }

        /// <summary>레거시 호환용: IPAddress ↔ IpAddress.</summary>
        [NotMapped]
        public string? IPAddress
        {
            get => IpAddress;
            set => IpAddress = value;
        }

        /// <summary>로컬 시간대 기준 로그인 시각(뷰 전용).</summary>
        [NotMapped]
        public DateTime DateTimeSignedInLocal => DateTimeSignedIn.LocalDateTime;

        /// <summary>UTC 기준 로그인 시각(뷰 전용).</summary>
        [NotMapped]
        public DateTime DateTimeSignedInUtc => DateTimeSignedIn.UtcDateTime;

        /// <summary>사용자 타임존 변환 결과 로그인 시각(뷰 전용).</summary>
        [NotMapped]
        public DateTime DateTimeSignedInDisplay { get; set; }
    }
}
