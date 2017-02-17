/*
This source file is subject to version 3 of the GPL license,
that is bundled with this package in the file LICENSE, and is
available online at http://www.gnu.org/licenses/gpl.txt;
you may not use this file except in compliance with the License.

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
All Rights Reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Voat.Utilities
{
    public class PaginatedList<T> : List<T>, IPaginatedList
    {
        public int PageIndex { get; private set; }

        public int PageSize { get; private set; }

        public int TotalCount { get; private set; }

        public int TotalPages { get; private set; }

        public string RouteName { get; set; }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 0);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex + 1 < TotalPages);
            }
        }

        public PaginatedList(IQueryable<T> source, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = source.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            AddRange(source.Skip(PageIndex * PageSize).Take(PageSize));
        }

        //IAmAGate: Perf mods for caching
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalCount">If set to -1, the code will attempt to control paging without knowing the full count.</param>
        public PaginatedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount = -1)
        {

            if (totalCount < 0)
            {
                int currentCount = source.Count();
                int fakeTotal = currentCount;
                if (currentCount < pageSize)
                {
                    //no future pages
                    fakeTotal = Math.Max((pageIndex), 0) * pageSize + currentCount;
                }
                else
                {
                    fakeTotal = (pageIndex + 1) * pageSize + 1;
                }
                totalCount = fakeTotal;
            }

            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            AddRange(source);
        }
        
      
    }
}
