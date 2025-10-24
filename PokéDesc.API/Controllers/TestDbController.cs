using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace PokéDesc.API.Controllers; // Important: le namespace correspond à ton projet

[ApiController]
[Route("api/[controller]")] // L'URL sera /api/TestDb
public class TestDbController : ControllerBase
{
    private readonly IMongoDatabase _database;

    public TestDbController(IMongoDatabase database)
    {
        _database = database;
    }

    [HttpGet("connection")]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            // Tente de lister les collections pour forcer une connexion
            var collectionNames = await _database.ListCollectionNames().ToListAsync();

            return Ok(new { 
                Message = "🎉 Connexion à MongoDB réussie !", 
                Collections = collectionNames 
            });
        }
        catch (Exception ex)
        {
            // Renvoie une erreur si la connexion échoue
            return StatusCode(500, new { 
                Message = "🔥 Échec de la connexion à MongoDB.", 
                Error = ex.Message,
                Hint = "Vérifiez votre chaîne de connexion dans le Secret Manager et que votre IP est autorisée sur MongoDB Atlas."
            });
        }
    }
}