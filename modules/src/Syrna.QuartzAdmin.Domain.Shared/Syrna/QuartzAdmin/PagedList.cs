using System;
using System.Collections.Generic;
namespace Syrna.QuartzAdmin
{
    public class PagedList<T> : List<T>
    {
        public PageMetadata PageMetadata { get; set; }

        public PagedList(IEnumerable<T> collection) : this(collection, null)
        { }

        public PagedList(IEnumerable<T> collection, PageMetadata metadata) : base(collection)
        {
            PageMetadata = metadata;
        }
    }
}

