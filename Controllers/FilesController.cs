using FileDistributionService.Entities;
using FileDistributionService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FileDistributionService.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private readonly ILogger<FilesController> _logger;
        private readonly FileDbContext _dbContext;
        private readonly FileSettings _fileSettings;

        public FilesController(ILogger<FilesController> logger, FileDbContext dbContext, IOptions<FileSettings> fileSettings)
        {
            _logger = logger;
            _dbContext = dbContext;
            _fileSettings = fileSettings.Value;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string uploadedBy)
        {
            try
            {
                // Check if file upload is enabled
                if (!_fileSettings.EnableFileUpload)
                {
                    _logger.LogWarning("File upload is disabled.");
                    return BadRequest("File upload is disabled.");
                }

                // Check file size limit
                var maxFileSizeMB = _fileSettings.MaxFileSizeMB;
                if (file.Length > maxFileSizeMB * 1024L * 1024L)
                {
                    _logger.LogWarning("File size exceeds the limit of {MaxFileSize} MB.", maxFileSizeMB);
                    return BadRequest($"File size exceeds the limit of {maxFileSizeMB} MB.");
                }

                // Check file type
                var allowedFileTypes = _fileSettings.AllowedFileTypes;
                var fileExtension = Path.GetExtension(file.FileName);
                if (!allowedFileTypes.Contains(fileExtension))
                {
                    _logger.LogWarning("File type {FileType} is not allowed.", fileExtension);
                    return BadRequest($"File type {fileExtension} is not allowed.");
                }

                // Save uploaded file to designated folder
                var uploadFolderPath = _fileSettings.UploadFolderPath;
                if (!Directory.Exists(uploadFolderPath))
                {
                    Directory.CreateDirectory(uploadFolderPath);
                }

                var fileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadFolderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Save file metadata to database
                var fileMetadata = new FileMetadata
                {
                    FileName = file.FileName,
                    FileSize = file.Length,
                    UploadTimestamp = DateTime.UtcNow,
                    UploadedBy = uploadedBy,
                    FilePath = filePath
                };

                _dbContext.FileMetadata.Add(fileMetadata);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("File uploaded successfully: {FileName}", file.FileName);
                return Ok(new { fileName = fileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading file.");
                return StatusCode(500, "An error occurred while uploading file.");
            }
        }

        [HttpGet("download/{fileName}")]
        //[Consumes("multipart/form-data")]
        public async Task<IActionResult> Download(string fileName)
        {
            try
            {
                // Check if file download is enabled
                if (!_fileSettings.EnableFileDownload)
                {
                    _logger.LogWarning("File download is disabled.");
                    return BadRequest("File download is disabled.");
                }

                // Check if current time falls within the allowed time period for downloading
                var currentTime = DateTime.Now.TimeOfDay;
                var downloadStartTime = TimeSpan.Parse(_fileSettings.DownloadStartTime);
                var downloadEndTime = TimeSpan.Parse(_fileSettings.DownloadEndTime);
                if (currentTime < downloadStartTime || currentTime > downloadEndTime)
                {
                    _logger.LogWarning("File download is allowed only during a specific time period.");
                    return BadRequest("File download is allowed only during a specific time period.");
                }

                // Retrieve file path from database based on file name
                var fileMetadata = await _dbContext.FileMetadata.FirstOrDefaultAsync(f => f.FileName == fileName);
                if (fileMetadata == null)
                {
                    _logger.LogWarning("File {FileName} not found.", fileName);
                    return NotFound("File not found.");
                }

                // Serve the file
                var filePath = fileMetadata.FilePath;
                var fileStream = new FileStream(filePath, FileMode.Open);
                return File(fileStream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while downloading file {FileName}.", fileName);
                return StatusCode(500, "An error occurred while downloading file.");
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                // Get file metadata from database
                var files = await _dbContext.FileMetadata.ToListAsync();

                // Return data for dashboard API
                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving dashboard data.");
                return StatusCode(500, "An error occurred while retrieving dashboard data.");
            }
        }
    }
}
