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
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Rock.Checkr.CheckrApi
{
    class Enums
    {
        /// <summary>
         /// Webhook Invitation Events
         /// </summary>
        [JsonConverter( typeof( StringEnumConverter ) )]
        public enum WebhookTypes
        {
            /// <summary>
            /// Invitation Created
            /// </summary>
            [EnumMember( Value = "invitation.created" )]
            InvitationCreated,

            /// <summary>
            /// Invitation Completed
            /// </summary>
            [EnumMember( Value = "invitation.completed" )]
            InvitationCompleted,

            /// <summary>
            /// Invitation Expired
            /// </summary>
            [EnumMember( Value = "invitation.expired" )]
            InvitationExpired,

            /// <summary>
            /// Report Created
            /// </summary>
            [EnumMember( Value = "report.created" )]
            ReportCreated
        }
    }
}
