using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Mamlaka.API.DAL.Enums;

namespace Mamlaka.API.DAL.Entities.DataConfigurations;
public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.ToTable("AspNetRoles").HasIndex(s => s.NormalizedName).IsUnique();
        builder.Property(s => s.Id);
        builder.Property(s => s.Name);
        builder.Property(s => s.NormalizedName);
        builder.HasData
        (
            new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = nameof(Roles.SuperAdmin),
                NormalizedName = nameof(Roles.SuperAdmin).ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
             new IdentityRole
             {
                 Id = Guid.NewGuid().ToString(),
                 Name = nameof(Roles.Admin),
                 NormalizedName = nameof(Roles.Admin).ToUpper(),
                 ConcurrencyStamp = Guid.NewGuid().ToString()
             },
             new IdentityRole
             {
                 Id = Guid.NewGuid().ToString(),
                 Name = nameof(Roles.Customer),
                 NormalizedName = nameof(Roles.Customer).ToUpper(),
                 ConcurrencyStamp = Guid.NewGuid().ToString()
             }            
        );
    }
}
