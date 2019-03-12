using System.IO;
using System.Linq;
using System.Threading.Tasks;
using demo.Data;
using demo.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using demo.Shared;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;

namespace demo.Controllers
{
    public class PhotoController : Controller
    {
        private readonly ApplicationDbContext ctx = null;
        private readonly IFileSystem fileSystem = null;

        public PhotoController(
            ApplicationDbContext ctx,
            IFileSystem fileSystem)
        {
            this.ctx = ctx;
            this.fileSystem = fileSystem;
        }

        public async Task<IActionResult> Index() 
        {
            var model = await this.ctx.Photos
                .ToListAsync();
            return View(model);
        }

        public IActionResult Create() 
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Photo model, IFormFile file) 
        {
            try
            {
                // var result = await checkImage(file);

                var result = checkImageFaces(file);

                var result2 = checkText(model.Description);

                var uploads = "uploads";
                if(!Directory.Exists(uploads))
                {
                    fileSystem.CreateDirectory(uploads);
                }
                if (file.Length > 0) {
                    var filePath = fileSystem.PathCombine(uploads, file.FileName);
                    await fileSystem.SaveFile(file, filePath);
                    model.FileName = filePath;
                }
                                
                ctx.Add(model);
                ctx.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var student = await ctx.Photos
                .SingleOrDefaultAsync(m => m.Id == id);

            if (student == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                ctx.Photos.Remove(student);
                await ctx.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (DbUpdateException /* ex */)
            {
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }

        private async Task<string> checkImage(IFormFile file) 
        {
            var uri = "https://westeurope.api.cognitive.microsoft.com/contentmoderator/moderate/v1.0/ProcessImage/Evaluate";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add(
                "Ocp-Apicription-Keym-Subs",
                "292d6f7673d54abeb7ae1c4bdf1ef410"
            );

            HttpResponseMessage response = null;
            using(var byteArray = new MemoryStream())
            {
                file.CopyTo(byteArray);
                using(var content = new ByteArrayContent(byteArray.ToArray())) 
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    response = await client.PostAsync(uri, content);
                    return await response.Content.ReadAsStringAsync(); 
                }
            }
        }

        private Evaluate checkImage2(IFormFile file)
        {
            var client = new ContentModeratorClient(
                new ApiKeyServiceClientCredentials("292d6f7673d54abeb7ae1c4bdf1ef410"));
            client.Endpoint = "https://westeurope.api.cognitive.microsoft.com";
        
            Evaluate response = client.ImageModeration.EvaluateFileInput(file.OpenReadStream());
            return response;
        }

        private OCR checkImageOCR(IFormFile file)
        {
            var client = new ContentModeratorClient(
                new ApiKeyServiceClientCredentials("292d6f7673d54abeb7ae1c4bdf1ef410"));
            client.Endpoint = "https://westeurope.api.cognitive.microsoft.com";
        
            var response = client.ImageModeration
                .OCRFileInput("ita", file.OpenReadStream());
            return response;
        }

        private FoundFaces checkImageFaces(IFormFile file)
        {
            var client = new ContentModeratorClient(
                new ApiKeyServiceClientCredentials("292d6f7673d54abeb7ae1c4bdf1ef410"));
            client.Endpoint = "https://westeurope.api.cognitive.microsoft.com";
        
            var response = client.ImageModeration
                .FindFacesFileInput(file.OpenReadStream());
            return response;
        }

         private Screen checkText(string text)
        {
            var client = new ContentModeratorClient(
                new ApiKeyServiceClientCredentials("292d6f7673d54abeb7ae1c4bdf1ef410"));
            client.Endpoint = "https://westeurope.api.cognitive.microsoft.com";
        
            var response = client.TextModeration
                .ScreenText("text/plain", this.generateStreamFromString(text), "ita", true, true, null, true);
            return response;
        }

        private Stream generateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

    }
}