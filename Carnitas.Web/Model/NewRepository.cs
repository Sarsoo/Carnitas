using System.ComponentModel.DataAnnotations;
using Carnitas.Model.Source.SourceControl;

namespace Carnitas.Web.Model;

public class NewRepository
{
    [Required]
    [StringLength(50, ErrorMessage = "Name must be at least 1 character long.", MinimumLength = 1)]
    public string Name { get; set; }

    [Required]
    public RepositoryType Type { get; set; }

    [Required]
    public string GitUrl { get; set; }

    public string? RepositoryUrl { get; set; }
}
