using Microsoft.AspNetCore.Mvc;
using RecipeCorner.Interfaces;
using RecipeCorner.Models;

namespace RecipeCorner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public InstructionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var instructions = await _unitOfWork.instructions.GetAllAsync();
            var dtos = instructions.Select(i => new InstructionDto
            {
                Id = i.Id,
                Order = i.Order,
                StepInstruction = i.StepInstruction,
                Tip = i.Tip,
                RecipeId = i.RecipeId
            });

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var instruction = await _unitOfWork.instructions.GetByIdAsync(id);
            if (instruction == null) return NotFound();

            var dto = new InstructionDto
            {
                Id = instruction.Id,
                Order = instruction.Order,
                StepInstruction = instruction.StepInstruction,
                Tip = instruction.Tip,
                RecipeId = instruction.RecipeId
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InstructionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var instruction = new Instruction
            {
                Order = dto.Order,
                StepInstruction = dto.StepInstruction,
                Tip = dto.Tip,
                RecipeId = dto.RecipeId
            };

            await _unitOfWork.instructions.AddAsync(instruction);
            await _unitOfWork.SaveAsync();

            dto.Id = instruction.Id;
            return CreatedAtAction(nameof(GetById), new { id = instruction.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] InstructionDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");

            var existing = await _unitOfWork.instructions.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Order = dto.Order;
            existing.StepInstruction = dto.StepInstruction;
            existing.Tip = dto.Tip;
            existing.RecipeId = dto.RecipeId;

            _unitOfWork.instructions.Update(existing);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var instruction = await _unitOfWork.instructions.GetByIdAsync(id);
            if (instruction == null) return NotFound();

            _unitOfWork.instructions.Delete(instruction);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }
    }
}
