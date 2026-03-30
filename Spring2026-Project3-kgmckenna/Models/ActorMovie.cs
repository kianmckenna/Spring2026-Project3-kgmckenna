using System.ComponentModel.DataAnnotations;

namespace Spring2026_Project3_kgmckenna.Models
{
    public class ActorMovie
    {
        public int Id { get; set; }

        [Display(Name = "Actor")]
        [Required]
        public int ActorId { get; set; }

        [Display(Name = "Movie")]
        [Required]
        public int MovieId { get; set; }

        // Navigation properties
        public Actor? Actor { get; set; }
        public Movie? Movie { get; set; }
    }
}