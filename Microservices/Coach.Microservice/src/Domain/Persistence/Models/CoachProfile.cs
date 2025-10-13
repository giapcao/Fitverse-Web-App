using Domain.Persistence.Enums;

namespace Domain.Persistence.Models;

public partial class CoachProfile
{
    public Guid UserId { get; set; }

    public string? Bio { get; set; }

    public int? YearsExperience { get; set; }

    public long? BasePriceVnd { get; set; }

    public decimal? ServiceRadiusKm { get; set; }

    public KycStatus KycStatus { get; set; }    

    public string? KycNote { get; set; }

    public decimal? RatingAvg { get; set; }

    public int RatingCount { get; set; }

    public bool IsPublic { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// URL ảnh đại diện của huấn luyện viên
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Ngày sinh (DATE)
    /// </summary>
    public DateOnly? BirthDate { get; set; }

    /// <summary>
    /// Cân nặng (kg), 0–500
    /// </summary>
    public decimal? WeightKg { get; set; }

    /// <summary>
    /// Chiều cao (cm), 0–300
    /// </summary>
    public decimal? HeightCm { get; set; }

    /// <summary>
    /// Giới tính: male/female/other/unspecified
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Khu vực/Nơi hoạt động chính
    /// </summary>
    public string? OperatingLocation { get; set; }

    /// <summary>
    /// Mã số thuế (10 hoặc 13 chữ số)
    /// </summary>
    public string? TaxCode { get; set; }

    /// <summary>
    /// CCCD/CMND (9 hoặc 12 chữ số)
    /// </summary>
    public string? CitizenId { get; set; }

    /// <summary>
    /// Ngày cấp CCCD/CMND
    /// </summary>
    public DateOnly? CitizenIssueDate { get; set; }

    /// <summary>
    /// Nơi cấp CCCD/CMND
    /// </summary>
    public string? CitizenIssuePlace { get; set; }

    /// <summary>
    /// Họ và tên
    /// </summary>
    public string? Fullname { get; set; }

    public virtual ICollection<CoachCertification> CoachCertifications { get; set; } = new List<CoachCertification>();

    public virtual ICollection<CoachMedium> CoachMedia { get; set; } = new List<CoachMedium>();

    public virtual ICollection<CoachService> CoachServices { get; set; } = new List<CoachService>();

    public virtual ICollection<KycRecord> KycRecords { get; set; } = new List<KycRecord>();

    public virtual ICollection<Sport> Sports { get; set; } = new List<Sport>();
}
