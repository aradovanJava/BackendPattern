using Back.Db;
using Back.Models;

namespace Back.Utils.ImageFilteringCoC
{
    public abstract class BaseImageFilteringHandler
    {
        protected readonly BaseImageFilteringHandler? _next;
        protected FilterRequest _requests;

        public BaseImageFilteringHandler() { }

        public BaseImageFilteringHandler(BaseImageFilteringHandler next) => _next = next;

        public abstract List<Image> Handle(List<Image> filtered);

        protected List<Image> HandleNext(List<Image> filtered) =>
            _next != null ? _next.Handle(filtered) : filtered;

        public void SetRequests(FilterRequest requests)
        {
            _requests = requests;
            if(_next != null)
                _next.SetRequests(requests);
        }
    }
}
