using DocHub.Api.Data;
using DocHub.Api.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DocHub.Api.Controllers
{
    public class VersionController : Controller
    {
        private readonly IWebHostEnvironment env;
        public AppDbContext context { get; set; }
        private const string LAST_VERSION = "LAST_VERSION";

        public VersionController(IWebHostEnvironment env, AppDbContext context)
        {
            this.env = env;
            this.context = context;
        }

        private static string GetHighestFolder(string filePath)
        {
            return filePath.Split("/")[0];  
        }

        private void StoreTheFolder(IFormFileCollection folder)
        {
            string WebRootPath = env.WebRootPath;
            string highestFolder = GetHighestFolder(folder[0].FileName);
            string fakeHash = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string folderPath = Path.Combine(WebRootPath, highestFolder, fakeHash);
            Directory.CreateDirectory(folderPath);

            foreach (var file in folder)
            {
                // Combine folderPath and file.FileName to get the full file path
                string filePath = Path.Combine(folderPath, file.FileName);

                // Ensure that the directory containing the file exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // Open a file stream and copy the file content
                using var stream = new FileStream(filePath, FileMode.Create);
                file.CopyTo(stream);
            }
            addToPathsFolder(fakeHash, folderPath);
        }


        [HttpPost("add-version")]
        public IActionResult UploadFolderTest()
        {
            StoreTheFolder(Request.Form.Files);

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
            //string pathsFolder = Path.Join(env.WebRootPath, "paths");
            //if (!Directory.Exists(pathsFolder))
            //{
            //    return BadRequest(new
            //    {
            //        message = "paths folder doesn't exists!"
            //    });
            //}
            context.HashPathsPairs.Select(i => $"path : {i.Path} , hash : {i.Hash}").ToList().ForEach(i => Console.WriteLine(i));

            if (context.HashPathsPairs.FirstOrDefault(f => f.Hash == id) is not PathsPair pair)
            {
                return BadRequest(new
                {
                    message = "version not found!"
                });
            }

            string tokenPath = pair.Path;

            return Ok(new
            {
                tokenPath
            });
        }

        private string GetLastVersionPath(string folder)
        {
            var folderPath = Path.Join(env.WebRootPath, folder);
            if (!Directory.Exists(folderPath))
            {
                return string.Empty;
            }
            return Path.Join(Directory.GetDirectories(folderPath)[0],folder);
        }

        private string GetSpecificVersionPath(string folder , string version)
        {
            var folderPath = Path.Join(env.WebRootPath, folder);
            if (!Directory.Exists(folderPath))
            {
                return string.Empty;
            }

            try
            {
                var folderVersion = Directory.GetDirectories(folderPath).First(f => f.EndsWith(version));
                return Path.Join(folderVersion, folder);
            }
            catch
            {
                throw new Exception("version not found");
            }
        }



        [HttpGet("folder-content")]
        public IActionResult GetFolderContentEndpoint([FromQuery, Required] string FolderName, [FromQuery] string version = LAST_VERSION)
        {
            if (version == LAST_VERSION)
            {
                var folderPath = GetLastVersionPath(FolderName);

                if(folderPath == string.Empty)
                {
                    return BadRequest(new
                    {
                        message = "folder not found"
                    });
                }

                return Ok(GetFolderContent(folderPath));
            }

            try
            {
                return Ok(GetFolderContent(GetSpecificVersionPath(FolderName, version)));
            }
            catch
            {
                return BadRequest("version not found");
            }
        }

        private object GetFolderContent([Required] string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new Exception("folder not found");
            }

            return new
            {
                folderPath,
                files = Directory.GetFiles(folderPath),
                folders = Directory.GetDirectories(folderPath)
            };
        }

        [HttpGet("content-of-sub-folder")]
        public IActionResult ContentOfSubFolder([FromQuery, Required] string folderName,
                                                [FromQuery, Required] string subFolder,
                                                [FromQuery] string version = LAST_VERSION)
        {
            if (subFolder == folderName) return GetFolderContentEndpoint(folderName,version);

            var folderPath = Path.Join(GetLastVersionPath(folderName),subFolder);

            try
            {
                var content = GetFolderContent(folderPath);
                return Ok(content);
            }
            catch
            {
                return BadRequest(new
                {
                    message = "folder not found"
                });
            }
        }
    }
}
