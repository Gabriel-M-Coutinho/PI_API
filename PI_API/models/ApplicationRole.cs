using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace LeadSearch.Models
{
    [CollectionName("Roles")]
    public class ApplicationRole : MongoIdentityRole { }
}