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
using System.Web;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to get a Noational Change of Address (NCOA) report for all active people's addresses.
    /// </summary>
    [DisallowConcurrentExecution]
    public class GetNcoa : RockBlock, IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public GetNcoa()
        {
        }

        /// <summary>
        /// Job to get a Noational Change of Address (NCOA) report for all active people's addresses.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            Rock.Utility.Ncoa ncoa = new Utility.Ncoa();
            ncoa.RequestNcoa();
        }
    }
}
