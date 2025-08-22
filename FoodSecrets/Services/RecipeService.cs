namespace FoodSecrets.Services
{
    public class RecipeService : IRecipe
    {

        private readonly HttpClient _httpClient;

        public RecipeService(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("RecipeApi");
        }

        public async Task<IEnumerable<RecipeDto>> GetAllAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<RecipeDto>>("recipe");
            return result ?? new List<RecipeDto>();
        }

        public async Task<RecipeDto?> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<RecipeDto>($"recipe/{id}");
        }

        public async Task<bool> CreateAsync(RecipeDto recipe)
        {
            var response = await _httpClient.PostAsJsonAsync("recipe", recipe);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int id, RecipeDto recipe)
        {
            var response = await _httpClient.PutAsJsonAsync($"recipe/{id}", recipe);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"recipe/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}

