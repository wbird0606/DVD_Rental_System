namespace DVD_Rental.Models
{
    public class FilmModel
    {
        public int FilmId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ReleaseYear { get; set; }
        public string Language { get; set; }
        public int Length { get; set; }
        public string Rating { get; set; }
    }
}
