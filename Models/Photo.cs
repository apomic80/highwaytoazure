using System.ComponentModel.DataAnnotations;

namespace demo.Models
{
    public class Photo
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; }
    }
}