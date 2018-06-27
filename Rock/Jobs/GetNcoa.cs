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
    /// Job to get a National Change of Address (NCOA) report for all active people's addresses.
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
        /// Job to get a National Change of Address (NCOA) report for all active people's addresses.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            // Get the job setting(s)
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            try
            {

                //Rock.Utility.Ncoa ncoa = new Utility.Ncoa((new Guid()).ToString());
                //ncoa.RequestNcoa();
            }
            catch ( System.Exception ex )
            {
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( ex, context2 );
                throw;
            }

        }

        private void SendNotifications( int groupId, List<Notification> notifications )
        {
            using ( var rockContext = new RockContext() )
            {
                var group = new GroupService( rockContext ).Get( groupId );

                if ( group != null )
                {
                    if ( notifications.Count == 0 )
                    {
                        return;
                    }

                    var notificationService = new NotificationService( rockContext );
                    foreach ( var notification in notifications.ToList() )
                    {
                        if ( notificationService.Get( notification.Guid ) == null )
                        {
                            notificationService.Add( notification );
                        }
                        else
                        {
                            notifications.Remove( notification );
                        }
                    }
                    rockContext.SaveChanges();

                    var notificationRecipientService = new NotificationRecipientService( rockContext );
                    foreach ( var notification in notifications )
                    {
                        foreach ( var member in group.Members )
                        {
                            if ( member.Person.PrimaryAliasId.HasValue )
                            {
                                var recipientNotification = new NotificationRecipient();
                                recipientNotification.NotificationId = notification.Id;
                                recipientNotification.PersonAliasId = member.Person.PrimaryAliasId.Value;
                                notificationRecipientService.Add( recipientNotification );
                            }
                        }
                    }

                    rockContext.SaveChanges();
                }
            }
        }
    }
}
