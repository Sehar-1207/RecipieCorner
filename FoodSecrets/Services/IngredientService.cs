using System.Text.Json;

namespace FoodSecrets.Services
{
    public class IngredientService : IIngredientMvc
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        public IngredientService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _http = httpClientFactory.CreateClient();
            _http.BaseAddress = new Uri(config["ApiSettings:BaseUrl"]);
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // Get all ingredients
        public async Task<IEnumerable<IngredientDto>> GetAllAsync()
        {
            var response = await _http.GetAsync("api/Ingredient");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"GetAll ingredients failed: {response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<IngredientDto>>(json, _jsonOptions) ?? Enumerable.Empty<IngredientDto>();
        }

        // Get ingredient by ID
        public async Task<IngredientDto?> GetByIdAsync(int id)
        {
            var response = await _http.GetAsync($"api/Ingredient/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IngredientDto>(json, _jsonOptions);
        }

        // Create new ingredient
        public async Task<bool> CreateAsync(IngredientDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/Ingredient", dto);
            return response.IsSuccessStatusCode;
        }

        // Update existing ingredient
        public async Task<bool> UpdateAsync(int id, IngredientDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/Ingredient/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        // Delete ingredient
        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/Ingredient/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
