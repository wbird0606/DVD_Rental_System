using System.Collections.Generic;

public class FilmDetailModel
{
    public int FilmId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int ReleaseYear { get; set; }
    public string Language { get; set; }
    public int Length { get; set; }
    public string Rating { get; set; }

    public decimal RentalRate { get; set; }
    public int RentalDuration { get; set; }
    public decimal ReplacementCost { get; set; }

    public List<string> Actors { get; set; } = new List<string>();
    public List<string> Categories { get; set; } = new List<string>();
}
