using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public record User
{
    public required int Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public string? Role { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Username).IsRequired();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Role).IsRequired(false);
        builder.Property(u => u.DateOfBirth).IsRequired(false);
    }
}