using DocHub.Api.Data;
using DocHub.Api.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace DocHub.Api.Controllers
{
    public class VersionController : Controller
    {
        private readonly IWebHostEnvironment env;
        public AppDbContext context { get; set; }

        public VersionController(IWebHostEnvironment env, AppDbContext context)
        {
            this.env = env;
            this.context = context;
        }

        private static string GetHighestFolder(string filePath)
        {
            return filePath.Split("/")[0];
        }

        private void storeTheFolder(IFormFileCollection folder)
        {
            string WebRootPath = env.WebRootPath;
            string highestFolder = GetHighestFolder(folder.First().FileName);
            //Console.WriteLine($"highest folder name is : {highestFolder}\n\n\n");
            string fakeHash = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string folderPath = Path.Combine(WebRootPath, highestFolder, fakeHash);
            //Console.WriteLine($"folder path is : {folderPath}\n\n\n");
            Directory.CreateDirectory(folderPath);

            foreach (var file in folder)
            {
                // Combine folderPath and file.FileName to get the full file path
                string filePath = Path.Combine(folderPath, file.FileName);
                //Console.WriteLine($"filepath is : {filePath}\n\n\n");

                // Ensure that the directory containing the file exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // Open a file stream and copy the file content
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }
            addToPathsFolder(fakeHash, folderPath);
        }


        [HttpPost("add-version")]
        public IActionResult UploadFolderTest()
        {
            storeTheFolder(Request.Form.Files);

            return Ok(new
            {
                message = "done"
            });
        }

        private static string[] GetDirectoryFolders(string directoryPath)
        {
            string[] filePaths = Directory.GetDirectories(directoryPath);
            string[] fileNames = new string[filePaths.Length];

            // Extract only filenames from full file paths
            for (int i = 0; i < filePaths.Length; i++)
            {
                fileNames[i] = Path.GetFileName(filePaths[i]);
            }

            return fileNames;
        }

        private void addToPathsFolder(string filename, string path)
        {
            context.HashPathsPairs.Add(new PathsPair()
            {
                Hash = filename,
                Path = path
            });
            context.SaveChanges();
        }

        [HttpGet("get-folders")]
        public IActionResult GetFolders()
        {
            var folders = Directory.GetDirectories(env.WebRootPath);

            Dictionary<string, string[]> folderVersionsPairs = new();

            foreach (var folder in folders)
            {
                folderVersionsPairs.Add(Path.GetFileName(folder), GetDirectoryFolders(folder));
            }

            return Ok(folderVersionsPairs);
        }

        [HttpGet("get-folder/{id}")]
        public IActionResult GetFolder(string id)
        {
            string pathsFolder = Path.Join(env.WebRootPath, "paths");
            if (!Directory.Exists(pathsFolder))
            //if (context.HashPathsPairs.FirstOrDefault(f => f.Hash == id) is not PathsPair pair)
            {
                return BadRequest(new
                {
                    message = "paths folder doesn't exists!"
                });
            }
            //Directory.GetFiles(pathsFolder).ToList().ForEach(i => Console.WriteLine(i));
            context.HashPathsPairs.Select(i => $"path : {i.Path} , hash : {i.Hash}").ToList().ForEach(i => Console.WriteLine(i));

            if (/*!Directory.GetFiles(pathsFolder).Select(f => Path.GetFileName(f)).Contains(id)*/ context.HashPathsPairs.FirstOrDefault(f => f.Hash == id) is not PathsPair pair)
            {
                return BadRequest(new
                {
                    message = "version not found!"
                });
            }

            //string tokenPath = System.IO.File.ReadLines(pathsFolder).First();
            string tokenPath = pair.Path;

            return Ok(new
            {
                tokenPath
            });
        }
    }
}
