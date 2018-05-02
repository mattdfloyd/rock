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
    class CreateCandidateRequest
    {
        /// <summary>
        /// Gets or sets the candidate's first name.
        /// </summary>
        /// <value>
        /// The candidate first name.
        /// </value>
        [JsonProperty( "first_name" )]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the candidate's middle name.
        /// </summary>
        /// <value>
        /// The candidate middle name.
        /// </value>
        [JsonProperty( "middle_name" )]
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets if the candidate have a middle name.
        /// </summary>
        /// <value>
        /// If the candidate have a middle name.
        /// </value>
        [JsonProperty( "no_middle_name" )]
        public bool HaveMiddleName
        {
            get
            {
                return MiddleName.IsNullOrWhiteSpace();
            }
        }

        /// <summary>
        /// Gets or sets the candidate's first name.
        /// </summary>
        /// <value>
        /// The candidate last name.
        /// </value>
        [JsonProperty( "last_name" )]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the candidate's e-mail address.
        /// </summary>
        /// <value>
        /// The candidate e-mail address.
        /// </value>
        [JsonProperty( "email" )]
        public string Email { get; set; }
    }
}