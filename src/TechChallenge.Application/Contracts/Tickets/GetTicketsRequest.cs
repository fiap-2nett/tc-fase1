using System;

namespace TechChallenge.Application.Contracts.Tickets
{
    public sealed class GetTicketsRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        public int Page { get; }

        /// <summary>
        /// Gets or sets the page size. The max page size is 100.
        /// </summary>
        public int PageSize { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GetTicketsRequest"/> class.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">The page size.</param>
        public GetTicketsRequest(int page, int pageSize)
        {
            Page = page;
            PageSize = Math.Min(Math.Max(pageSize, 0), 100);
        }

        #endregion
    }
}
