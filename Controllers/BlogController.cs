using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.Models;
using EntityFrameworkCore.Data;
using System.Linq;
using System.Threading.Tasks;
namespace EntityFrameworkCore.Controllers
{
    public class BlogController : Controller
    {
        private readonly BlogContext _context;
        public BlogController(BlogContext context)
        {
            _context = context;
        }
        // (C)reate
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Blog blog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }


        // (R)ead -
        public IActionResult Index()
        {
            return View(_context.Blogs.ToList());
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();
            var blog = _context.Blogs.FirstOrDefault(d => d.BlogId == id);
            if (blog == null)
                return NotFound();
            return View(blog);
        }

        // (U)pdate
        [HttpPost]
        public IActionResult Edit(int? id, Blog blog)
        {
            if (id == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(blog);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.BlogId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
                return NotFound();

            return View(blog);
        }

        // (D)elete
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();
            var blog = _context.Blogs.FirstOrDefault(b => b.BlogId == id);
            if (blog == null)
                return NotFound();
            return View(blog);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var blog = _context.Blogs.Find(id);
            _context.Blogs.Remove(blog);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.BlogId == id);
        }


    }
}