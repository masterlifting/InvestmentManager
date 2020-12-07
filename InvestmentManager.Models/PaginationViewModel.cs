using System;
using System.Collections.Generic;

namespace InvestmentManager.Models
{
    public class PaginationViewModel<T>  where T : class
    {
        public List<T> Items { get; set; } = new List<T>();
        public Pagination Pagination { get; set; } = new Pagination();
    }
    public class Pagination
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public void SetPagination(int count, int number, int size)
        {
            PageNumber = number;
            TotalPages = (int)Math.Ceiling(count / (double)size);
        }
        public bool HasPreviousePage { get => PageNumber > 1; }
        public bool HasNextPage { get => PageNumber < TotalPages; }
    }
}
