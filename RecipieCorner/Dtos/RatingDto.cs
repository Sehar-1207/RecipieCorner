using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

public class RatingDto
{
    public int Id { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public int RecipeId { get; set; }
    [ValidateNever]
    public string UserId { get; set; }
    [ValidateNever]

    public string UserName { get; set; }
    [ValidateNever]

    public string ProfileImageUrl { get; set; }
    public DateTime commentAt { get; set; }
    [ValidateNever]
    public string RecipeName { get; set; }
}