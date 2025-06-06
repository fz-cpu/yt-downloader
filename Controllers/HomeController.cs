using DownloaderR2.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DownloaderR2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost("download")]
        public IActionResult DownloadVid(string url, Format format)
        {
			var regex = new Regex("@^(https?://)?(www.)?(youtube.com/watch?v=|youtu.be/)[w-]{11}(&.*)?$", RegexOptions.IgnoreCase);
            if(!regex.IsMatch(url))
            {
                return BadRequest("Invalid URL");
            }
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            var outputTemplate = Path.Combine(tempDir, "%(title)s.%(ext)s");
            var ext = format.ToString();
			
			var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = $"--extract-audio --audio-format {ext} -o \"{outputTemplate}\" {url}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            var file = Directory.GetFiles(tempDir).FirstOrDefault(f => f.EndsWith(ext));
            if ( file is null)
            {
                Directory.Delete(tempDir, true);
                return BadRequest("Download failed.");
            }
            var fileBytes = System.IO.File.ReadAllBytes(file);
            Directory.Delete(tempDir, true);
            return File(fileBytes, "application/octet-stream", file);
        }

    }
}
