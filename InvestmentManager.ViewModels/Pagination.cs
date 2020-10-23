using System;

namespace InvestmentManager.ViewModels
{
    public class Pagination
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public void SetPagination(int count, int number, int size)
        {
            PageNumber = number;
            TotalPages = (int)Math.Ceiling(count / (double)size);
        }
        public bool HasPreviousePage { get => PageNumber > 1;}
        public bool HasNextPage { get => PageNumber < TotalPages; }
    }
}
