using System.ComponentModel.DataAnnotations;

namespace Carnitas.Web.Model;

public class NewOrganisation
{
    [Required]
    [StringLength(50, ErrorMessage = "Name must be at least 1 character long.", MinimumLength = 1)]
    public string Name { get; set; }
}