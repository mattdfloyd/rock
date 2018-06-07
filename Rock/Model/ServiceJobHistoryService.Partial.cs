// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Compilation;

using Quartz;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.ServiceJobHistory"/> entity objects.
    /// </summary>
    public partial class ServiceJobHistoryService
    {
        /// <summary>
        /// Returns a queryable collection of all <see cref="Rock.Model.ServiceJobHistory">jobs history</see>
        /// </summary>
        /// <returns>A queryable collection of all <see cref="Rock.Model.ServiceJobHistory"/>jobs history</returns>
        public IQueryable<ServiceJobHistory> GetAllJobs(int serviceJobId)
        {
            return Queryable().Where( t => t.ServiceJobId == serviceJobId );
        }
    }
}
