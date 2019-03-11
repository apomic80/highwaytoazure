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

namespace demo.Controllers
{
    public class PhotoController : Controller
    {
        private readonly ApplicationDbContext ctx = null;
        private readonly IHostingEnvironment env = null;
        private readonly IFileSystem fileSystem = null;

        public PhotoController(
            ApplicationDbContext ctx,
            IHostingEnvironment env,
            IFileSystem fileSystem)
        {
            this.ctx = ctx;
            this.env = env;
            this.fileSystem = fileSystem;
        }

        public async Task<IActionResult> Index() 
        {
            var model = await this.ctx.Photos.ToListAsync();
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
                var uploads = fileSystem.PathCombine(env.WebRootPath, "uploads");
                if(!Directory.Exists(uploads))
                {
                    fileSystem.CreateDirectory(uploads);
                }
                if (file.Length > 0) {
                    var filePath = fileSystem.PathCombine(uploads, file.FileName);
                    await fileSystem.SaveFile(file, filePath);
                    model.FileName = "/uploads/" + file.FileName;
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

    }
}