using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Data
{
    public class LegacyMovieContext : DbContext
    {
        public LegacyMovieContext (DbContextOptions<LegacyMovieContext> options)
            : base(options)
        {
        }

        public DbSet<SquadEstoque.Web.Models.Movie> Movie { get; set; } = default!;
    }
}
