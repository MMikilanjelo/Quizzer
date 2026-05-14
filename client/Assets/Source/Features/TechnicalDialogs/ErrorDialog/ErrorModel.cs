using Source.Shared;

namespace Source.Features.TechnicalDialogs.ErrorDialog
{
    public record ErrorModel
    {
        public string Message { get; private set; }
        public string Title { get; private set; }
        public static ErrorModel From(Error error)
        {
            return new ErrorModel
            {
                Title = "Oops! Something went wrong",
                Message = "Please try again later."
            };
        }
        
        private ErrorModel()
        {
        }
    }
}