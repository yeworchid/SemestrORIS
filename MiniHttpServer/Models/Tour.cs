namespace MiniHttpServer.Models
{
    public class Tour
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string AdditionalDescription { get; set; }
        public string IncludedInPrice { get; set; }
        public string NotIncludedInPrice { get; set; }
        public decimal BasePrice { get; set; }
        public int Duration { get; set; }
        public string TourType { get; set; }
        public int CountryId { get; set; }
        public string CityName { get; set; }
        public string CoverImageUrl { get; set; }
        public List<TourDate> Dates { get; set; } = new List<TourDate>();
        public List<TourImage> Images { get; set; } = new List<TourImage>();
        
        public string Date1 => Dates.Count > 0 ? Dates[0].FormattedDate : "";
        public string Date2 => Dates.Count > 1 ? Dates[1].FormattedDate : "";
        public string Date3 => Dates.Count > 2 ? Dates[2].FormattedDate : "";
        public bool HasMoreDates => Dates.Count > 3;
        
        public string DatesOptionsHtml
        {
            get
            {
                var html = "";
                foreach (var date in Dates)
                {
                    html += $"<option value=\"{date.Id}\">{date.FormattedDate}</option>";
                }
                return html;
            }
        }
    }
    
    public class TourImage
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsCover { get; set; }
    }
}
