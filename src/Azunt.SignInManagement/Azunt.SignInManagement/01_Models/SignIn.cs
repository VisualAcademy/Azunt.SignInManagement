using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Azunt.SignInManagement;

/// <summary>
/// 사용자 로그인 이력(SignIns) 엔티티
/// </summary>
[Table("SignIns")]
public class SignIn
{
    /// <summary>기본 키(자동 증가) → [Id] BIGINT IDENTITY</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>로그인 시각 → [DateTimeSignedIn] DATETIMEOFFSET(0) NOT NULL</summary>
    public DateTimeOffset DateTimeSignedIn { get; set; }

    /// <summary>사용자 ID → [UserId] NVARCHAR(450) NULL</summary>
    [MaxLength(450)]
    public string? UserId { get; set; }

    /// <summary>로그인에 사용된 이메일 → [Email] NVARCHAR(MAX) NOT NULL</summary>
    [Required]
    public string Email { get; set; } = default!;

    /// <summary>이름 → [FirstName] NVARCHAR(MAX) NULL</summary>
    public string? FirstName { get; set; }

    /// <summary>성 → [LastName] NVARCHAR(MAX) NULL</summary>
    public string? LastName { get; set; }

    /// <summary>로그인 결과(성공/실패 등) → [Result] NVARCHAR(MAX) NOT NULL</summary>
    [Required]
    public string Result { get; set; } = default!;

    /// <summary>클라이언트 IP 주소 → [IpAddress] NVARCHAR(MAX) NULL</summary>
    public string? IpAddress { get; set; }

    /// <summary>비고/메모 → [Note] NVARCHAR(MAX) NULL</summary>
    public string? Note { get; set; }

    /// <summary>테넌트 ID → [TenantId] BIGINT NULL</summary>
    public long? TenantId { get; set; }

    /// <summary>테넌트 이름 → [TenantName] NVARCHAR(255) NULL</summary>
    [MaxLength(255)]
    public string? TenantName { get; set; }
}

