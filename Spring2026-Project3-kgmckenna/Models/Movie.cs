using System.ComponentModel.DataAnnotations;

namespace Spring2026_Project3_kgmckenna.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = "";

        [Display(Name = "IMDb Link")]
        [Required]
        [Url]
        public string ImdbLink { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string Genre { get; set; } = "";

        [Display(Name = "Year of Release")]
        [Range(1888, 2100)]
        public int Year { get; set; }

        [Display(Name = "Poster")]
        public byte[]? Poster { get; set; }

        // Relationship table navigation
        public ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();
    }
}