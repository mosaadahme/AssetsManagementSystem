using AssetsManagementSystem.Models.DbSets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetsManagementSystem.Data.Configurations
{
    public class AssetConfiguration : IEntityTypeConfiguration<Asset>
    {
        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            builder.HasIndex(a => a.Barcode)
                   .IsUnique();

            builder.HasOne(m => m.Manufacturer)
               .WithMany(a => a.Assets)
               .HasForeignKey(fk => fk.ManufacturerId)
               .IsRequired(false);

           
        }
    }
}
