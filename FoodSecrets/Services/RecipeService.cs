using RecipeCorner.Interfaces;
using RecipeCorner.Models;
using System.Text.Json;

namespace FoodSecrets.Services
{
    public class RecipeService : IRecipeMvc
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        public RecipeService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _http = httpClientFactory.CreateClient();
            _http.BaseAddress = new Uri(config["ApiSettings:BaseUrl"]); // central from appsettings.json
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<IEnumerable<RecipeDto>> GetAllAsync()
        {
            var response = await _http.GetAsync("api/Recipe");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"GetAll recipes failed: {response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<RecipeDto>>(json, _jsonOptions)!;
        }

        public async Task<RecipeDto?> GetByIdAsync(int id)
        {
            var response = await _http.GetAsync($"api/Recipe/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Get recipe failed: {response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RecipeDto>(json, _jsonOptions);
        }

        public async Task<bool> CreateAsync(RecipeDto recipe)
        {
            var response = await _http.PostAsJsonAsync("api/Recipe", recipe);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int id, RecipeDto recipe)
        {
            var response = await _http.PutAsJsonAsync($"api/Recipe/{id}", recipe);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/Recipe/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<RecipeDto>> GetByCuisineAsync(string cuisine)
        {
            var response = await _http.GetAsync($"api/Recipe/cuisine/{cuisine}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Get recipes by cuisine failed: {response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<RecipeDto>>(json, _jsonOptions) ?? Enumerable.Empty<RecipeDto>();
        }
        public async Task<RecipeDetailsDto?> GetDetailsAsync(int id)
        {
            var response = await _http.GetAsync($"api/Recipe/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RecipeDetailsDto>(json, _jsonOptions);
        }

    }
}
