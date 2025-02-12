using DishDashRESTAPIDatabase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DishDashRESTAPIDatabase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly DishDashDb _db;

        public RecipeController(DishDashDb db)
        {
            _db = db;
        }

        // GET: api/recipe
        [HttpGet]
        public async Task<IActionResult> GetRecipes()
        {
            var recipes = await _db.Recipes.Select(r => new
            {
                r.Id,
                r.Category,
                r.Name,
                r.Ingredient,
                r.Instruction,
                r.ImageUrl
            }).ToListAsync();

            return Ok(recipes);
        }

        // GET: api/recipe/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipeById(int id)
        {
            var recipe = await _db.Recipes
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    r.Id,
                    r.Category,
                    r.Name,
                    r.Ingredient,
                    r.Instruction,
                    r.ImageUrl
                })
                .FirstOrDefaultAsync();

            if (recipe == null)
            {
                return NotFound();
            }
            return Ok(recipe);
        }

        // GET: api/recipe/favorites
        // Returns the favorite recipes for the currently logged-in user.
        [HttpGet("favorites")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetFavoriteRecipes()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User id not found in token.");
            }
            int userId = int.Parse(userIdClaim.Value);
            var user = await _db.Users.Include(u => u.FavoriteRecipes)
                                      .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            var favorites = user.FavoriteRecipes.Select(r => new
            {
                r.Id,
                r.Category,
                r.Name,
                r.Ingredient,
                r.Instruction,
                r.ImageUrl
            }).ToList();
            return Ok(favorites);
        }

        // POST: api/recipe
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRecipe([FromBody] Recipe recipe)
        {
            if (string.IsNullOrEmpty(recipe.Category) ||
                string.IsNullOrEmpty(recipe.Name) ||
                string.IsNullOrEmpty(recipe.Ingredient) ||
                string.IsNullOrEmpty(recipe.Instruction) ||
                string.IsNullOrEmpty(recipe.ImageUrl))
            {
                return BadRequest("All fields (Category, Name, Ingredient, Instruction, ImageUrl) are required.");
            }

            _db.Recipes.Add(recipe);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRecipeById), new { id = recipe.Id }, recipe);
        }

        // PUT: api/recipe/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRecipe(int id, [FromBody] Recipe updatedRecipe)
        {
            if (id != updatedRecipe.Id)
            {
                return BadRequest("ID mismatch.");
            }

            var recipe = await _db.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            recipe.Category = updatedRecipe.Category;
            recipe.Name = updatedRecipe.Name;
            recipe.Ingredient = updatedRecipe.Ingredient;
            recipe.Instruction = updatedRecipe.Instruction;
            recipe.ImageUrl = updatedRecipe.ImageUrl;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/recipe/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _db.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            _db.Recipes.Remove(recipe);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/recipe/{id}/favorite
        // Allows a normal user to add a recipe to their favorites.
        [HttpPost("{id}/favorite")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddFavorite(int id)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User id not found in token.");
            }
            int userId = int.Parse(userIdClaim.Value);

            var recipe = await _db.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound("Recipe not found.");
            }

            var user = await _db.Users.Include(u => u.FavoriteRecipes)
                                      .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            if (user.FavoriteRecipes.Any(r => r.Id == id))
            {
                return BadRequest("Recipe already added to favorites.");
            }

            user.FavoriteRecipes.Add(recipe);
            await _db.SaveChangesAsync();

            return Ok("Recipe added to favorites.");
        }

        // DELETE: api/recipe/{id}/favorite
        // Allows a normal user to remove a recipe from their favorites.
        [HttpDelete("{id}/favorite")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> RemoveFavorite(int id)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User id not found in token.");
            }
            int userId = int.Parse(userIdClaim.Value);

            var recipe = await _db.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound("Recipe not found.");
            }

            var user = await _db.Users.Include(u => u.FavoriteRecipes)
                                      .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            if (!user.FavoriteRecipes.Any(r => r.Id == id))
            {
                return BadRequest("Recipe is not in favorites.");
            }

            user.FavoriteRecipes.Remove(recipe);
            await _db.SaveChangesAsync();

            return Ok("Recipe removed from favorites.");
        }
    }
}
