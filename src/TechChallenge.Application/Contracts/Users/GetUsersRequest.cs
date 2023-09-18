using System;
using TechChallenge.Application.Contracts.Common;

namespace TechChallenge.Application.Contracts.Users
{
    /// <summary>
    /// Represents the query for getting the paged list of the users.
    /// </summary>
    public sealed class GetUsersRequest
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
        /// Initializes a new instance of the <see cref="GetUsersRequest"/> class.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">The page size.</param>
        public GetUsersRequest(int page, int pageSize)
        {
            Page = page;
            PageSize = Math.Min(Math.Max(pageSize, 0), 100);
        }

        #endregion
    }
}
