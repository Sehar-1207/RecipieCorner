using FoodSecrets.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodSecrets.Controllers
{
    public class StepsController : Controller
    {
        private readonly IinstructionsService _instructionService;

        public StepsController(IinstructionsService instructionService)
        {
            _instructionService = instructionService;
        }

        // Display all instructions
        public async Task<IActionResult> Index()
        {
            var instructions = await _instructionService.GetAllAsync();
            return View(instructions); // Pass to the view
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(InstructionDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var result = await _instructionService.CreateAsync(dto);
            if (result) return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Failed to create instruction");
            return View(dto);
        }

        // Edit instruction form
        public async Task<IActionResult> Edit(int id)
        {
            var instruction = await _instructionService.GetByIdAsync(id);
            if (instruction == null) return NotFound();

            return View(instruction);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, InstructionDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var result = await _instructionService.UpdateAsync(id, dto);
            if (result) return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Failed to update instruction");
            return View(dto);
        }

        // Delete instruction confirmation
        public async Task<IActionResult> Delete(int id)
        {
            var instruction = await _instructionService.GetByIdAsync(id);
            if (instruction == null) return NotFound();

            return View(instruction);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _instructionService.DeleteAsync(id);
            if (result) return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Failed to delete instruction");
            var instruction = await _instructionService.GetByIdAsync(id);
            return View(instruction);
        }
    }
}
