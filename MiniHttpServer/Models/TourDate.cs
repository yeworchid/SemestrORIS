namespace MiniHttpServer.Models
{
    public class TourDate
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int AvailableSeats { get; set; }
        public string Status { get; set; }
        
        // форматированная дата для отображения
        public string FormattedDate => DepartureDate.ToString("dd.MM.yyyy");
    }
}
