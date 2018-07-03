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
using Rock.Model;
using Rock.Utility;
using Rock.Utility.Settings.SparkData;
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
            var exceptions = new List<Exception>();
            // Get the job setting(s)
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            SparkDataConfig sparkDataConfig = Ncoa.GetSettings();

            if ( !sparkDataConfig.NcoaSettings.IsEnabled )
            {
                return;
            }

            try
            {
                Guid? SparkDataApiKeyGuid = sparkDataConfig.SparkDataApiKey.AsGuidOrNull();
                if ( SparkDataApiKeyGuid == null )
                {
                    exceptions.Add( new Exception( $"SparkDataApiKey '{sparkDataConfig.SparkDataApiKey.ToStringSafe()}' is empty or invalid." ) );
                    return;
                }
                switch ( sparkDataConfig.NcoaSettings.CurrentReportStatus )
                {
                    case "Start":
                    case "":
                    case null:
                        StatusStart( sparkDataConfig );
                        break;
                    case "Failed":
                        StatusFailed( sparkDataConfig );
                        break;
                    case "Pending: Report":
                        StatusPendingReport( sparkDataConfig );
                        break;
                    case "Pending: Export":
                        StatusPendingExport( sparkDataConfig );
                        break;
                    case "Complete":
                        StatusComplete( sparkDataConfig );
                        break;
                }
            }
            catch ( System.Exception ex )
            {
                exceptions.Add( ex );
            }
            finally
            {
                if ( exceptions.Any() )
                {
                    context.Result = $"Job finished with error(s).";

                    sparkDataConfig.NcoaSettings.CurrentReportStatus = "Failed";
                    Ncoa.SaveSettings( sparkDataConfig );

                    if ( sparkDataConfig.SparkDataApiKey.IsNotNullOrWhitespace() && sparkDataConfig.NcoaSettings.CurrentReportKey.IsNotNullOrWhitespace() )
                    {
                        Ncoa.CompleteFailed( sparkDataConfig.SparkDataApiKey, sparkDataConfig.NcoaSettings.CurrentReportKey );
                    }

                    Exception ex = new AggregateException( "One or more NCOA requirement failed ", exceptions );
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( ex, context2 );
                    throw ex;
                }
                else
                {
                    context.Result = $"Job Complete. NCOA Status: {sparkDataConfig.NcoaSettings.CurrentReportStatus}";
                }
            }
        }

        /// <summary>
        /// Current State is Failed. If recurring is enabled, retry.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        private void StatusFailed( SparkDataConfig sparkDataConfig )
        {
            if ( sparkDataConfig.NcoaSettings.RecurringEnabled )
            {
                sparkDataConfig.NcoaSettings.CurrentReportStatus = "Start";
                sparkDataConfig.NcoaSettings.PersonAliasId = null;
                Ncoa.SaveSettings( sparkDataConfig );
                StatusStart( sparkDataConfig );
            }
        }

        /// <summary>
        /// Current state is start. Start NCOA
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        private void StatusStart( SparkDataConfig sparkDataConfig )
        {
            var ncoa = new Ncoa();
            ncoa.Start( sparkDataConfig );
        }

        /// <summary>
        /// Current state is complete. Check if recurring is enabled and recurring interval have been reached.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        private void StatusComplete( SparkDataConfig sparkDataConfig )
        {
            if ( !sparkDataConfig.NcoaSettings.LastRunDate.HasValue ||
                ( sparkDataConfig.NcoaSettings.RecurringEnabled &&
                sparkDataConfig.NcoaSettings.LastRunDate.Value.AddDays( sparkDataConfig.NcoaSettings.RecurrenceInterval ) < RockDateTime.Now ) )
            {
                sparkDataConfig.NcoaSettings.CurrentReportStatus = "Start";
                sparkDataConfig.NcoaSettings.PersonAliasId = null;
                Ncoa.SaveSettings( sparkDataConfig );
                StatusStart( sparkDataConfig );
            }
        }

        /// <summary>
        /// Current state is pending report. Try to resume a pending report.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        private void StatusPendingReport( SparkDataConfig sparkDataConfig )
        {
            var ncoa = new Ncoa();
            ncoa.PendingReport( sparkDataConfig );
        }

        /// <summary>
        /// Current state is pending export report. Try to resume a pending export report.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        private void StatusPendingExport( SparkDataConfig sparkDataConfig )
        {
            var ncoa = new Ncoa();
            ncoa.PendingExport( sparkDataConfig );
        }
    }
}
