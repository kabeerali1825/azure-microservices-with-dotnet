using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wpm.Management.Api.DataAccess;

namespace Wpm.Management.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PetsController(ManagementDbContext dbContext, ILogger<PetsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                logger.LogInformation("Getting all pets with their breeds");
                var allPets = await dbContext.Pets.Include(p => p.Breed).ToListAsync();
                return Ok(allPets);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while getting all pets");
                return StatusCode(500, "An error occurred while retrieving pets.");
            }
        }

        [HttpGet("Id")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                logger.LogInformation("Getting pet with id {Id}", id);
                var petWithId = await dbContext.Pets
                    .Include(p => p.Breed)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (petWithId == null)
                    return NotFound();

                return Ok(petWithId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while getting pet by id {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the pet.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(NewPet newPet)
        {
            try
            {
                logger.LogInformation("Creating a new pet: {@NewPet}", newPet);

                // Validation
                if (newPet is null)
                    return BadRequest("Pet data is required.");

                if (string.IsNullOrWhiteSpace(newPet.Name))
                    return BadRequest("Name is required.");

                if (newPet.Age < 0)
                    return BadRequest("Age must be a non-negative integer.");

                // Optionally, check if the breed exists
                var breedExists = await dbContext.Set<Breed>().AnyAsync(b => b.Id == newPet.BreedId);
                if (!breedExists)
                    return BadRequest($"Breed with Id {newPet.BreedId} does not exist.");

                var pet = newPet.ToPet();
                dbContext.Pets.Add(pet);
                await dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = pet.Id }, pet);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while creating a new pet");
                return StatusCode(500, "An error occurred while creating the pet.");
            }
        }
    }
}

public record NewPet(string Name, int Age, int BreedId)
{
    public Pet ToPet()
    {
        return new Pet
        {
            Name = Name,
            Age = Age,
            BreedId = BreedId
        };
    }
}