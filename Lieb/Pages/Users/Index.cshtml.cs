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
    public class IndexModel : PageModel
    {
        private readonly Lieb.Data.LiebContext _context;

        public IndexModel(Lieb.Data.LiebContext context)
        {
            _context = context;
        }

        public IList<LiebUser> User { get;set; }

        public async Task OnGetAsync()
        {
            User = await _context.Users.ToListAsync();
        }
    }
}
