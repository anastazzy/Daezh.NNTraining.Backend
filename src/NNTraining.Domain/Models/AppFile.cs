namespace NNTraining.Domain;

public class AppFile
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime TimeOfCreation { get; set; } = DateTime.UtcNow;
    public byte[]? Content { get; set; }
}