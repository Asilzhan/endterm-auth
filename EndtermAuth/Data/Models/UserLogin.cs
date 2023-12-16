using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EndtermAuth.Data.Models;

public record UserLogin
{
    public required int UserId { get; set; }
    public required User User { get; set; }
    public DateTime LastLogin { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}

public class UserLoginConfiguration : IEntityTypeConfiguration<UserLogin>
{
    public void Configure(EntityTypeBuilder<UserLogin> builder)
    {
        builder.HasKey(ul => ul.UserId);
        builder.Property(ul => ul.LastLogin).IsRequired();
        builder.Property(ul => ul.RefreshToken).IsRequired(false); // Allow null
        builder.Property(ul => ul.RefreshTokenExpiryTime).IsRequired(false);

        builder.HasOne(ul => ul.User)
            .WithMany()
            .HasForeignKey(ul => ul.UserId);
    }
}
