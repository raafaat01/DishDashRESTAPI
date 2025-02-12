namespace DishDashRESTAPIDatabase.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Ingredient { get; set; }
        public string Instruction { get; set; }
        public string ImageUrl { get; set; }
    }
}
