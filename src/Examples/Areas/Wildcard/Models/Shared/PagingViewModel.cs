namespace Examples.Areas.Wildcard.Models.Shared
{
    public class PagingViewModel
    {
        public PagingViewModel(int page, int total)
        {
            Total = total;
            Page = page * PageSize > Total ? 0 : page;
        }

        public int Page { get; set; }

        public int PageSize { get; set; } = 20;

        public int Total { get; set; }

        public int Pages
        {
            get
            {
                int result = Total / PageSize;
                if (Total % PageSize > 0)
                {
                    result++;
                }

                return result;
            }
        }
    }
}