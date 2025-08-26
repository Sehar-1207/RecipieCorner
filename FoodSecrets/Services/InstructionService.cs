using System.Text.Json;

namespace FoodSecrets.Services
{
    public class InstructionService : IinstructionsService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        public InstructionService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _http = httpClientFactory.CreateClient();
            _http.BaseAddress = new Uri(config["ApiSettings:BaseUrl"]);
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // Get all instructions
        public async Task<IEnumerable<InstructionDto>> GetAllAsync()
        {
            var response = await _http.GetAsync("api/Instruction");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"GetAll instructions failed: {response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<InstructionDto>>(json, _jsonOptions) ?? Enumerable.Empty<InstructionDto>();
        }

        // Get instruction by ID
        public async Task<InstructionDto?> GetByIdAsync(int id)
        {
            var response = await _http.GetAsync($"api/Instruction/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<InstructionDto>(json, _jsonOptions);
        }

        // Create new instruction
        public async Task<bool> CreateAsync(InstructionDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/Instruction", dto);
            return response.IsSuccessStatusCode;
        }

        // Update existing instruction
        public async Task<bool> UpdateAsync(int id, InstructionDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/Instruction/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        // Delete instruction
        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/Instruction/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
