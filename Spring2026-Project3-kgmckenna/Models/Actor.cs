using System.ComponentModel.DataAnnotations;

namespace Spring2026_Project3_kgmckenna.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [Required]
        [StringLength(30)]
        public string Gender { get; set; } = "";

        [Range(0, 120)]
        public int Age { get; set; }

        [Display(Name = "IMDb Link")]
        [Required]
        [Url]
        public string ImdbLink { get; set; } = "";

        [Display(Name = "Photo")]
        public byte[]? Photo { get; set; }

        // Relationship table navigation
        public ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();
    }
}