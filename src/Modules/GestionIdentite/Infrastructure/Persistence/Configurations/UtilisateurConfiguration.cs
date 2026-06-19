using CVTech.Modules.GestionIdentite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVTech.Modules.GestionIdentite.Infrastructure.Persistence.Configurations;

/// <summary>Mapping relationnel de l'entité de persistance <see cref="UtilisateurEntity"/>.</summary>
public sealed class UtilisateurConfiguration : IEntityTypeConfiguration<UtilisateurEntity>
{
    public void Configure(EntityTypeBuilder<UtilisateurEntity> builder)
    {
        builder.ToTable("Utilisateurs");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Email).IsRequired().HasMaxLength(320);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(u => u.EstBloque).IsRequired();
        builder.Property(u => u.MotDePasseHash).HasMaxLength(256);
    }
}
