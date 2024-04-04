namespace FileDistributionService.Entities
{
    public class FileMetadata
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadTimestamp { get; set; }
        public string UploadedBy { get; set; }
        public string FilePath { get; set; }
    }
}
