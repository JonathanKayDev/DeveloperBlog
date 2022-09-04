#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogProject.Data;
using BlogProject.Models;
using BlogProject.Services;
using Microsoft.AspNetCore.Identity;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;

namespace BlogProject.Controllers
{
    public class PostsController : Controller
    {
        #region Properties
        private readonly ApplicationDbContext _context;
        private readonly ISlugService _slugService;
        private readonly IImageService _imageService;
        private readonly UserManager<BlogUser> _userManager;
        private readonly BlogSearchService _blogSearchService;
        #endregion

        #region Constructor
        public PostsController(ApplicationDbContext context, ISlugService slugService, IImageService imageService, UserManager<BlogUser> userManager, BlogSearchService blogSearchService)
        {
            _context = context;
            _slugService = slugService;
            _imageService = imageService;
            _userManager = userManager;
            _blogSearchService = blogSearchService;
        }
        #endregion

        #region Search Index
        public async Task<IActionResult> SearchIndex(int? page, string searchTerm)
        {
            ViewData["SearchTerm"] = searchTerm;

            var pageNumber = page ?? 1;
            var pageSize = 5;
            var posts = _blogSearchService.Search(searchTerm);

            return View(await posts.ToPagedListAsync(pageNumber, pageSize));
        }
        #endregion

        #region Index
        // GET: Posts
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posts.Include(p => p.Blog).Include(p => p.BlogUser);
            return View(await applicationDbContext.ToListAsync());
        }
        #endregion

        #region Blog Post Index
        // BlogPostIndex
        public async Task<IActionResult> BlogPostIndex(int? id, int? page)
        {
            if (id is null)
            {
                return NotFound();
            }

            var pageNumber = page ?? 1;
            var pageSize = 6;

            var blog = _context.Blogs.Where(b => b.Id == id).FirstOrDefault();

            var posts = _context.Posts
                .Where(p => p.BlogId == id && p.ReadyStatus == Enums.ReadyStatus.ProductionReady)
                .OrderByDescending(p => p.Created)
                .ToPagedListAsync(pageNumber, pageSize);

            ViewData["HeaderImage"] = _imageService.DecodeImage(blog.ImageData, blog.ImageType);
            ViewData["MainText"] = blog.Name;
            ViewData["SubText"] = blog.Description;

            return View(await posts);

        }
        #endregion

        #region All Post Index
        // AllPostIndex
        public async Task<IActionResult> AllPostIndex(int? page)
        {

            var pageNumber = page ?? 1;
            var pageSize = 6;

            var posts = _context.Posts
                .Where(p => p.ReadyStatus == Enums.ReadyStatus.ProductionReady)
                .OrderByDescending(p => p.Created)
                .ToPagedListAsync(pageNumber, pageSize);

            ViewData["MainText"] = "All Posts";
            ViewData["SubText"] = "Developer Blog";

            return View(await posts);

        }
        #endregion

        #region Draft Post Index
        // DraftPostIndex
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DraftPostIndex(int? page)
        {

            var pageNumber = page ?? 1;
            var pageSize = 6;

            var posts = _context.Posts
                .Where(p => p.ReadyStatus != Enums.ReadyStatus.ProductionReady)
                .OrderByDescending(p => p.Created)
                .ToPagedListAsync(pageNumber, pageSize);

            ViewData["MainText"] = "All Posts";
            ViewData["SubText"] = "Developer Blog";

            return View(await posts);

        }
        #endregion

        #region Details
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Blog)
                .Include(p => p.BlogUser)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .ThenInclude(c => c.BlogUser)
                .FirstOrDefaultAsync(m => m.Slug == slug);
            if (post == null)
            {
                return NotFound();
            }

            ViewData["HeaderImage"] = _imageService.DecodeImage(post.ImageData, post.ImageType);
            ViewData["MainText"] = post.Title;
            ViewData["SubText"] = post.Abstract;

            return View(post);
        }
        #endregion

        #region Create
        // GET: Posts/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create(int? id)
        {
            var blog = _context.Blogs.Where(b => b.Id == id).FirstOrDefault();

            ViewData["HeaderImage"] = _imageService.DecodeImage(blog.ImageData, blog.ImageType);
            ViewData["MainText"] = blog.Name;
            ViewData["SubText"] = blog.Description;


            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name");
            //ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BlogId,Title,Abstract,Content,ReadyStatus,Image")] Post post, List<string> tagValues)
        {
            if (ModelState.IsValid)
            {
                post.Created = DateTime.UtcNow;

                var authorId = _userManager.GetUserId(User);
                post.BlogUserId = authorId;

                // Use the _imageService to store user image
                post.ImageData = await _imageService.EncodeImageAsync(post.Image);
                post.ImageType = _imageService.ContentType(post.Image);

                // Create the slug and determine if it's unique
                var slug = _slugService.UrlFriendly(post.Title);

                // Create variable to store whether an error has occurred
                var validationError = false;

                // Using else if to avoid collisions, so 2 different errors don't show up for the same property
                if (string.IsNullOrEmpty(slug))
                {
                    ModelState.AddModelError("", "The Title you provided cannot be used as it results in an empty slug.");
                    validationError = true;
                }
                else if (!_slugService.IsUnique(slug))
                {
                    ModelState.AddModelError("Title", "The Title you provided cannot be used as it must be unique.");
                    validationError = true;
                }

                if (validationError)
                {
                    // return the user back to the Create view
                    ViewData["TagValues"] = string.Join(",", tagValues);
                    return View(post);
                }

                post.Slug = slug;

                _context.Add(post);
                await _context.SaveChangesAsync();

                // Loop over incoming list of string
                foreach (var tagText in tagValues)
                {
                    _context.Add(new Tag()
                    {
                        PostId = post.Id,
                        BlogUserId = authorId,
                        Text = tagText
                    });
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", post.BlogUserId);
            return View(post);
        }
        #endregion

        #region Edit
        // GET: Posts/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            // find post and the tags that come along with it
            var post = await _context.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Slug == slug);

            if (post == null)
            {
                return NotFound();
            }
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
            ViewData["TagValues"] = string.Join(",", post.Tags.Select(t => t.Text));

            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,BlogId,Title,Abstract,Content,ReadyStatus")] Post post, IFormFile newImage, List<string> tagValues) // name of selectlist tagValues has to match, important!
        {
            if (id == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get a copy of the original post before we apply the edits
                    var newPost = await _context.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == post.Id);

                    newPost.Updated = DateTime.UtcNow;
                    newPost.BlogId = post.BlogId;
                    newPost.Title = post.Title;
                    newPost.Abstract = post.Abstract;
                    newPost.Content = post.Content;
                    newPost.ReadyStatus = post.ReadyStatus;

                    var newSlug = _slugService.UrlFriendly(post.Title);
                    // newPost.Slug is really the old slug
                    if (newSlug != newPost.Slug)
                    {
                        if (_slugService.IsUnique(newSlug))
                        {
                            newPost.Title = post.Title;
                            newPost.Slug = newSlug;
                        }
                        else
                        {
                            ModelState.AddModelError("Title", "This Title cannot be used as it results in a duplicate slug");
                            ViewData["TagValues"] = string.Join(",", post.Tags.Select(t => t.Text));
                            return View(post);
                        }
                    }

                    if (newImage is not null)
                    {
                        newPost.ImageData = await _imageService.EncodeImageAsync(newImage);
                        newPost.ImageType = _imageService.ContentType(newImage);
                    }

                    // Remove all Tags previously associated with this post
                    _context.Tags.RemoveRange(newPost.Tags);

                    // Add new tags from the form
                    foreach (var tagText in tagValues)
                    {
                        _context.Add(new Tag()
                        {
                            PostId = post.Id,
                            BlogUserId = newPost.BlogUserId,
                            Text = tagText
                        });
                    }

                    // save specific changes
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
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
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", post.BlogUserId);
            return View(post);
        }
        #endregion

        #region Delete
        // GET: Posts/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Blog)
                .Include(p => p.BlogUser)
                .FirstOrDefaultAsync(m => m.Slug == slug);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Post Exists
        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        } 
        #endregion
    }
}
