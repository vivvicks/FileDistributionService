namespace FileDistributionService.Models
{
    public class FileSettings
    {
        public string UploadFolderPath { get; set; }
        public List<string> AllowedFileTypes { get; set; }
        public int MaxFileSizeMB { get; set; }
        public bool EnableFileUpload { get; set; }
        public bool EnableFileDownload { get; set; }
        public string DownloadStartTime { get; set; }
        public string DownloadEndTime { get; set; }

    }
}
