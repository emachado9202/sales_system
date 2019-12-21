using MovilShopStock.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MovilShopStock.Services
{
    public class CategoryService : GenericService
    {
        public static async Task<Category> Get(Guid id)
        {
            return await applicationDbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);
        }

        public static async Task<List<Category>> All()
        {
            return await applicationDbContext.Categories.ToListAsync();
        }
    }
}