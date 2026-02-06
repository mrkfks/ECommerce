namespace Dashboard.Web.Models;

/// <summary>
/// Özellik değeri
/// </summary>
public class FeatureValueViewModel
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Özellik görünüm modeli
/// </summary>
public class FeatureViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<FeatureValueViewModel> Values { get; set; } = new();
}

/// <summary>
/// Özellik oluşturma modeli
/// </summary>
public class FeatureCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Values { get; set; } = new();
}

/// <summary>
/// Özellik güncelleme modeli
/// </summary>
public class FeatureUpdateViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public List<FeatureValueViewModel> Values { get; set; } = new();
    public List<string> NewValues { get; set; } = new();
}
