using System.Net;

namespace MoviesMinimalAPI.Models
{
    public class APIResponses
    {
        public APIResponses()
        {
            Errors = new List<string>();
        }

        public bool Success { get; set; }
        public Object Result { get; set; }
        public HttpStatusCode StateCode { get; set; }
        public List<string> Errors { get; set; }
    }
}
