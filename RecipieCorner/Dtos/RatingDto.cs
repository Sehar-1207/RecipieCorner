using System.ComponentModel.DataAnnotations;

public class RatingDto
{
    public int Id { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public int RecipeId { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string ProfileImageUrl { get; set; }
    public DateTime commentAt { get; set; }
}