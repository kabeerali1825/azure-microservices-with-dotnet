using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wpm.Management.Api.DataAccess;

namespace Wpm.Management.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BreedsController(
        ManagementDbContext dbContext,
        ILogger<BreedsController> logger
    ) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                logger.LogInformation("Getting all breeds");
                var breeds = await dbContext.Set<Breed>().ToListAsync();
                return Ok(breeds);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while getting all breeds");
                return StatusCode(500, "An error occurred while retrieving breeds.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                logger.LogInformation("Getting breed with id {Id}", id);
                var breed = await dbContext.Set<Breed>().FirstOrDefaultAsync(b => b.Id == id);
                if (breed == null)
                {
                    logger.LogWarning("Breed with id {Id} not found", id);
                    return NotFound();
                }
                return Ok(breed);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while getting breed by id {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the breed.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Breed breed)
        {
            try
            {
                logger.LogInformation("Creating a new breed: {@Breed}", breed);

                // Validation
                if (breed is null)
                    return BadRequest("Breed data is required.");

                if (string.IsNullOrWhiteSpace(breed.Name))
                    return BadRequest("Name is required.");

                // Optionally, check for duplicate name
                var exists = await dbContext.Set<Breed>().AnyAsync(b => b.Name == breed.Name);
                if (exists)
                    return BadRequest($"A breed with the name '{breed.Name}' already exists.");

                dbContext.Add(breed);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Created new breed with id {Id}", breed.Id);
                return CreatedAtAction(nameof(GetById), new { id = breed.Id }, breed);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while creating a new breed");
                return StatusCode(500, "An error occurred while creating the breed.");
            }
        }
    }
}