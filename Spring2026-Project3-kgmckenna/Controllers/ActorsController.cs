using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spring2026_Project3_kgmckenna.Data;
using Spring2026_Project3_kgmckenna.Models;
using Spring2026_Project3_kgmckenna.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VaderSharp2;

namespace Spring2026_Project3_kgmckenna.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .Include(a => a.ActorMovies)
                    .ThenInclude(am => am.Movie)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            var movieTitles = actor.ActorMovies
                .Where(am => am.Movie != null)
                .Select(am => am.Movie!.Title)
                .ToList();

            var fakeTweets = new List<string>
    {
        $"{actor.Name} absolutely stole the show.",
        $"{actor.Name} gave such a fun performance in this movie.",
        $"I’d watch anything with {actor.Name} in it.",
        $"{actor.Name} brought a lot of energy to the role.",
        $"{actor.Name} was one of the best parts of the cast.",
        $"Really liked what {actor.Name} added to the movie.",
        $"{actor.Name} made the character feel memorable.",
        $"{actor.Name}'s scenes were some of my favorites.",
        $"{actor.Name} was entertaining from start to finish.",
        $"{actor.Name} deserves more recognition for this performance."
    };

            var analyzer = new SentimentIntensityAnalyzer();

            var tweetsWithSentiment = fakeTweets.Select(tweet =>
            {
                var score = analyzer.PolarityScores(tweet).Compound;

                string sentiment;
                if (score >= 0.05)
                    sentiment = "Positive";
                else if (score <= -0.05)
                    sentiment = "Negative";
                else
                    sentiment = "Neutral";

                return new ReviewSentimentViewModel
                {
                    Review = tweet,
                    Sentiment = sentiment
                };
            }).ToList();

            double average = fakeTweets
                .Select(t => analyzer.PolarityScores(t).Compound)
                .Average();

            string overallSentiment;
            if (average >= 0.05)
                overallSentiment = "Positive";
            else if (average <= -0.05)
                overallSentiment = "Negative";
            else
                overallSentiment = "Neutral";

            var vm = new ActorDetailsViewModel
            {
                Actor = actor,
                MovieTitles = movieTitles,
                TweetsWithSentiment = tweetsWithSentiment,
                OverallSentiment = overallSentiment
            };

            return View(vm);
        }

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Actor actor, IFormFile? photoFile)
        {
            if (photoFile != null && photoFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await photoFile.CopyToAsync(memoryStream);
                actor.Photo = memoryStream.ToArray();
            }

            if (ModelState.IsValid)
            {
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Actor actor, IFormFile? photoFile)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            if (photoFile != null && photoFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await photoFile.CopyToAsync(memoryStream);
                actor.Photo = memoryStream.ToArray();
            }
            else
            {
                var existingActor = await _context.Actors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (existingActor != null)
                {
                    actor.Photo = existingActor.Photo;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.Id))
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

            return View(actor);
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actors.Any(e => e.Id == id);
        }
    }
}
