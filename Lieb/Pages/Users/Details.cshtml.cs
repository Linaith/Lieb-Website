#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Lieb.Data;
using Lieb.Models;

namespace Lieb.Pages.Users
{
    public class DetailsModel : PageModel
    {
        private readonly Lieb.Data.LiebContext _context;

        public DetailsModel(Lieb.Data.LiebContext context)
        {
            _context = context;
        }

        public LiebUser User { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User = await _context.Users.FirstOrDefaultAsync(m => m.LiebUserId == id);

            if (User == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
