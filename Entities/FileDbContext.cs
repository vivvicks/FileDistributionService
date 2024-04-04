using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FileDistributionService.Entities
{
    public class FileDbContext : DbContext
    {
        public DbSet<FileMetadata> FileMetadata { get; set; }

        public FileDbContext(DbContextOptions<FileDbContext> options) : base(options)
        {
        }
    }
}
