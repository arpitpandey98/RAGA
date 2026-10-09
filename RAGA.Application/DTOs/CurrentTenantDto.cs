namespace RAGA.Application.DTOs;

public class CurrentTenantDto
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? DisplayName { get; set; }
}
