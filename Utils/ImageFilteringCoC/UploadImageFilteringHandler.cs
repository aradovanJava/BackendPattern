using Back.Models;

namespace Back.Utils.ImageFilteringCoC
{
    public class UploadImageFilteringHandler : BaseImageFilteringHandler
    {
        public UploadImageFilteringHandler(BaseImageFilteringHandler next) : base(next) { }

        public UploadImageFilteringHandler() { }

        public override List<Image> Handle(List<Image> filtered) =>
            _requests.Date != null
                ? HandleNext(filtered.Where(x => InDateRange(x.Uploaded)).ToList())
                : base.HandleNext(filtered);

        private bool InDateRange(DateTime uploaded)
        {
            DateTime start;
            DateTime end;

            if (_requests.Date.Split("|")[0] != "" && _requests.Date.Split("|")[1] != "")
            {
                DateTime.TryParse(_requests.Date.Split("|")[0], out start);
                DateTime.TryParse(_requests.Date.Split("|")[1], out end);
                return start <= uploaded && uploaded <= end.AddDays(1);
            }
            else if (_requests.Date.Split("|")[0] == "")
            {
                DateTime.TryParse(_requests.Date.Split("|")[1], out end);
                return uploaded <= end.AddDays(1);
            }
            else
            {
                DateTime.TryParse(_requests.Date.Split("|")[0], out start);

                return start <= uploaded;
            }
        }

    }
}
