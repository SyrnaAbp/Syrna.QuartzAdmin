using System;

namespace Syrna.QuartzAdmin
{
    [Serializable]
    public class PageMetadata
    {
        /// <summary>
        /// Page number. Start at 0
        /// </summary>
        public int Page { get; init; } = 0;
        /// <summary>
        /// Total number of records
        /// </summary>
        public int TotalCount { get; init; }
        /// <summary>
        /// Max number of records per page
        /// </summary>
        public int PageSize { get; init; } = 500;

        public static PageMetadata New(int Page, int PageSize)
        {
            return new PageMetadata
            {
                Page = Page,
                PageSize = PageSize
            };
        }
    }
}

