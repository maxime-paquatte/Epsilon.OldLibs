namespace Epsilon.Model.Repository.Models;

public class RepoValue
{
    public int CultureId { get; set; }
    public string CultureKey { get; set; }
    
    public int RepoValueId { get; set; }
    public int RepoId { get; set; }
    
    public string Name { get; set; }
    public string SystemKey { get; set; }
    public string Value { get; set; }
    public string Path { get; set; }
    public string Content { get; set; }
    
    public int? AlphaId { get; set; }
    public int? OldPlatformId { get; set; }
    
    public int Depth { get; set; }
    public int Descendants { get; set; }
}