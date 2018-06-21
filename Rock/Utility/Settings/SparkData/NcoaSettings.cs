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

namespace Rock.Utility.Settings.SparkData
{
    /// <summary>
    /// Settings for NCOA
    /// </summary>
    public class NcoaSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the last NCOA run date.
        /// </summary>
        /// <value>
        /// The last NCOA run date.
        /// </value>
        public DateTime LastRunDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [recurring enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [recurring enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool RecurringEnabled { get; set; }

        /// <summary>
        /// Gets or sets the recurrence interval.
        /// </summary>
        /// <value>
        /// The recurrence interval.
        /// </value>
        public int RecurrenceInterval { get; set; }

        /// <summary>
        /// Gets or sets the current NCOA report key.
        /// </summary>
        /// <value>
        /// The current NCOA report key.
        /// </value>
        public string CurrentReportKey { get; set; }

        /// <summary>
        /// Gets or sets the current NCOA report export key.
        /// </summary>
        /// <value>
        /// The current NCOA report export key.
        /// </value>
        public string CurrentReportExportKey { get; set; }

        /// <summary>
        /// Gets or sets the current report status.
        /// </summary>
        /// <value>
        /// The current report status.
        /// </value>
        public string CurrentReportStatus { get; set; }

        /// <summary>
        /// Gets or sets the person data view unique identifier.
        /// </summary>
        /// <value>
        /// The person data view unique identifier.
        /// </value>
        public string PersonDataViewGuid { get; set; }

        /// <summary>
        /// Gets or sets the current upload count to NCOA.
        /// </summary>
        /// <value>
        /// The current upload count to NCOA.
        /// </value>
        public int CurrentUploadCount { get; set; }

    }
}
