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
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using OpenAI;

namespace Spring2026_Project3_kgmckenna.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

            var reviews = await GenerateMovieReviewsAsync(movie);

            var analyzer = new SentimentIntensityAnalyzer();

            var reviewsWithSentiment = reviews.Select(review =>
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

            double average = reviews
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
        private readonly IConfiguration _configuration;
        private async Task<List<string>> GenerateMovieReviewsAsync(Movie movie)
        {
            var endpoint = _configuration["AzureOpenAI:Endpoint"];
            var apiKey = _configuration["AzureOpenAI:Key"];
            var deploymentName = _configuration["AzureOpenAI:DeploymentName"];

            if (string.IsNullOrWhiteSpace(endpoint) ||
                string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(deploymentName))
            {
                return new List<string>
        {
            "AI configuration is missing.",
            "AI configuration is missing.",
            "AI configuration is missing.",
            "AI configuration is missing.",
            "AI configuration is missing."
        };
            }

            var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            var chatClient = client.GetChatClient(deploymentName);

            var prompt = $"""
        Generate exactly 5 short reviews for the movie "{movie.Title}".
        Genre: {movie.Genre}
        Year: {movie.Year}

        Requirements:
        - Make each review 1 to 2 sentences
        - Put each review on its own line
        - Number them 1 through 5
        - Do not include any extra intro or conclusion text
        """;

            var response = await chatClient.CompleteChatAsync(
                new List<ChatMessage>
                {
            new SystemChatMessage("You write short movie reviews."),
            new UserChatMessage(prompt)
                });

            var content = response.Value.Content[0].Text;

            var lines = content
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            var cleaned = lines
                .Select(line =>
                {
                    if (line.Length > 2 && char.IsDigit(line[0]) && line[1] == '.')
                        return line.Substring(2).Trim();
                    if (line.Length > 1 && char.IsDigit(line[0]) && line[1] == ')')
                        return line.Substring(2).Trim();
                    return line;
                })
                .ToList();

            while (cleaned.Count < 5)
            {
                cleaned.Add("Review unavailable.");
            }

            return cleaned.Take(5).ToList();
        }
    }
    
}
