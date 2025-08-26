namespace FoodSecrets.Services
{
    public interface IinstructionsService
    {
        Task<IEnumerable<InstructionDto>> GetAllAsync();
        Task<InstructionDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(InstructionDto dto);
        Task<bool> UpdateAsync(int id, InstructionDto dto);
        Task<bool> DeleteAsync(int id);

    }
}