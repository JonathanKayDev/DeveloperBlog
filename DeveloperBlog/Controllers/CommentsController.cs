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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace BlogProject.Controllers
{
    public class CommentsController : Controller
    {
        #region Properties
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;
        #endregion

        #region Constructor
        public CommentsController(ApplicationDbContext context, UserManager<BlogUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        #endregion

        #region Moderated Index
        [Authorize(Roles = "Administrator, Moderator")]
        public async Task<IActionResult> ModeratedIndex()
        {
            var moderatedComments = await _context.Comments.Where(c => c.Moderated != null).ToListAsync();
            return View("Index", moderatedComments);
        }
        #endregion

        #region Index
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index()
        {
            var allComments = await _context.Comments.ToListAsync();
            return View("Index", allComments);
        }
        #endregion

        #region Details
        // GET: Comments/Details/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.BlogUser)
                .Include(c => c.Moderator)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }
        #endregion

        #region Create
        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,Body")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.BlogUserId = _userManager.GetUserId(User);
                comment.Created = DateTime.UtcNow;

                _context.Add(comment);
                await _context.SaveChangesAsync();
                //return Redirect($"BlogPosts/UrlFriendly/{slug}");
                return RedirectToAction("Index", "Home");
            }

            return View(comment);
        }
        #endregion

        #region Edit
        // GET: Comments/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", comment.BlogUserId);
            ViewData["ModeratorId"] = new SelectList(_context.Users, "Id", "Id", comment.ModeratorId);
            ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract", comment.PostId);
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Body")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var newComment = await _context.Comments.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == comment.Id);
                try
                {
                    newComment.Body = comment.Body;
                    newComment.Updated = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //return RedirectToAction(nameof(Index));

                // (Action, Controller, Route, Fragment to scroll to)
                return RedirectToAction("Details", "Posts", new { slug = newComment.Post.Slug }, "commentSection");
            }

            //ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", comment.BlogUserId);
            //ViewData["ModeratorId"] = new SelectList(_context.Users, "Id", "Id", comment.ModeratorId);
            //ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract", comment.PostId);

            return View(comment);
        }
        #endregion

        #region Moderate
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Moderate(int id, [Bind("Id,Body,ModeratedBody,ModerationType")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var newComment = await _context.Comments.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == comment.Id);
                try
                {
                    newComment.ModeratedBody = comment.ModeratedBody;
                    newComment.ModerationType = comment.ModerationType;

                    newComment.Moderated = DateTime.UtcNow;
                    newComment.ModeratorId = _userManager.GetUserId(User);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction("Details", "Posts", new { slug = newComment.Post.Slug }, "commentSection");
            }

            return View(comment);
        }

        #endregion

        #region Delete
        // GET: Comments/Delete/5
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.BlogUser)
                .Include(c => c.Moderator)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string slug)
        {
            var comment = await _context.Comments.FindAsync(id);
            comment.Deleted = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Comments");
        }
        #endregion

        #region Comment Exists
        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
        #endregion

        #region All Deleted
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AllDeleted()
        {
            var allComments = await _context.Comments
                .Where(c => c.Deleted != null)
                .ToListAsync();
            return View("DeletedIndex", allComments);
        }
        #endregion

        #region Permanent Delete
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> PermanentDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.BlogUser)
                .Include(c => c.Moderator)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }


        // POST: Comments/PermanentDelete/
        [HttpPost, ActionName("PermanentDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDeleteConfirmed(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("AllDeleted", "Comments");
        } 
        #endregion

    }
}
