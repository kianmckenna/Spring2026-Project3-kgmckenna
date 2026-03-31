using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spring2026_Project3_kgmckenna.Data;
using Spring2026_Project3_kgmckenna.Models;
using Spring2026_Project3_kgmckenna.ViewModels;
using VaderSharp2;

namespace Spring2026_Project3_kgmckenna.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.ActorMovies)
                    .ThenInclude(am => am.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            var actorNames = movie.ActorMovies
                .Where(am => am.Actor != null)
                .Select(am => am.Actor!.Name)
                .ToList();

            var fakeReviews = new List<string>
    {
        "This movie was exciting and visually impressive.",
        "The performances were strong and the story was enjoyable.",
        "It had some slow parts, but overall it was entertaining.",
        "The action scenes were excellent and memorable.",
        "A fun movie with a lot of charm."
    };

            var analyzer = new SentimentIntensityAnalyzer();

            var reviewsWithSentiment = fakeReviews.Select(review =>
            {
                var score = analyzer.PolarityScores(review).Compound;

                string sentiment;
                if (score >= 0.05)
                    sentiment = "Positive";
                else if (score <= -0.05)
                    sentiment = "Negative";
                else
                    sentiment = "Neutral";

                return new ReviewSentimentViewModel
                {
                    Review = review,
                    Sentiment = sentiment
                };
            }).ToList();

            double average = fakeReviews
                .Select(r => analyzer.PolarityScores(r).Compound)
                .Average();

            string overallSentiment;
            if (average >= 0.05)
                overallSentiment = "Positive";
            else if (average <= -0.05)
                overallSentiment = "Negative";
            else
                overallSentiment = "Neutral";

            var vm = new MovieDetailsViewModel
            {
                Movie = movie,
                ActorNames = actorNames,
                ReviewsWithSentiment = reviewsWithSentiment,
                OverallSentiment = overallSentiment
            };

            return View(vm);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile? posterFile)
        {
            if (posterFile != null && posterFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await posterFile.CopyToAsync(memoryStream);
                movie.Poster = memoryStream.ToArray();
            }

            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie, IFormFile? posterFile)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (posterFile != null && posterFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await posterFile.CopyToAsync(memoryStream);
                movie.Poster = memoryStream.ToArray();
            }
            else
            {
                var existingMovie = await _context.Movies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (existingMovie != null)
                {
                    movie.Poster = existingMovie.Poster;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}
