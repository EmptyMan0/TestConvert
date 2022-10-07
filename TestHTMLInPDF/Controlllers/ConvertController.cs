using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TestHTMLInPDF.Controlllers
{
    public class ConvertController : Controller
    {
        public class DocumentUpload
        {
            public string Description { get; set; }
            public IFormFile File { get; set; }
            public string ClientDate { get; set; }
        }
            public IActionResult Index()
            {
                return View();
            }

        
        public async Task<IActionResult> Download(DocumentUpload batchUsers)
        {
            if (batchUsers == null)
                return Content("filename not present");

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "reg.html");

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(path), Path.GetFileName(path));
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".html", "application/html"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {".mp4", "video/mp4" }
            };
        }
        
        public async Task <dynamic> ConvertHTMLInPDF([FromForm] DocumentUpload batchUsers)
        {
            if (batchUsers.File !=null)
            {
                string file = "";
                //полученный фаил
                IFormFile GetFale = batchUsers.File;
                string fn = GetFale.FileName;
             
                var tempFilename = $@"{fn}";

                using (var fileStream = new FileStream(tempFilename, FileMode.Create))
                {
                    await GetFale.CopyToAsync(fileStream);
                    file = fileStream.Name;
                    fileStream.Close();
                }
                var HourMinuteSecond = DateTime.UtcNow;
              
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                using (var page = await browser.NewPageAsync())
                {
                   
                    await page.GoToAsync(file);

                    var html = await page.GetContentAsync();
                    await page.SetContentAsync(html);

                    var result = await page.GetContentAsync();
     
                    await page.PdfAsync($"{DateTime.Today.ToShortDateString().Replace("/", "-")}+{HourMinuteSecond.Hour}-{HourMinuteSecond.Minute}-{HourMinuteSecond.Second}.pdf");
                    var  pars =  page.Url.Remove(0, 8);
                   
                    var words01 = pars.Split('/', StringSplitOptions.RemoveEmptyEntries);
                   
                    string namefilepdf= $"{DateTime.Today.ToShortDateString().Replace(" / ", " - ")}+{HourMinuteSecond.Hour}-{HourMinuteSecond.Minute}-{HourMinuteSecond.Second}.pdf";
                    
                    string getwave = "";
                    for (int i = 0; i < words01.Length-1; i++)
                    {
                        getwave += words01[i];
                        getwave += "/";
                    }
                    getwave += $"{namefilepdf}";
                    byte[] abc = System.IO.File.ReadAllBytes(getwave);
                    //удаление HTML файла
                    System.IO.File.Delete(page.Url.Remove(0,8));

                    System.IO.File.WriteAllBytes(file, abc);
                    MemoryStream ms = new MemoryStream(abc);
                    return new FileStreamResult(ms, "application/pdf");
                }
            
            }
            return StatusCode(404, new JsonResult(new { NoFile = "Аile not added" }).Value);
        }
    }
}
