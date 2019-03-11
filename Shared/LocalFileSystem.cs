using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace demo.Shared
{
    public class LocalFileSystem
        : IFileSystem
    {
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public string PathCombine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public async Task SaveFile(IFormFile file, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create)) {
                await file.CopyToAsync(fileStream);
            }
        }
    }
}