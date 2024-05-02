using System.ComponentModel.DataAnnotations;

namespace DocHub.Api.Data.Models
{
    public class PathsPair
    {
        [Key]
        public string Hash { get; set; }
        public string Path { get; set; }
    }
}
