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
using Newtonsoft.Json;

namespace Rock.Checkr.CheckrApi
{
    /// <summary>
    /// JSON return structure for the Create Candidate API Call's request
    /// </summary>
    internal class CreateInvitationRequest
    {
        /// <summary>
        /// Gets or sets the package.
        /// </summary>
        /// <value>
        /// The package.
        /// </value>
        [JsonProperty( "package" )]
        public string Package { get; set; }

        /// <summary>
        /// Gets or sets the candidate's ID.
        /// </summary>
        /// <value>
        /// The candidate ID.
        /// </value>
        [JsonProperty( "candidate_id" )]
        public string CandidateId { get; set; }
    }
}