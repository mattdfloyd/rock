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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Model;
using Rock.Data;
using Rock.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Data.Entity;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using RestSharp;
using Rock.Utility.NcoaApi;

namespace Rock.Utility
{
    public class Ncoa
    {
        public Dictionary<int, PersonAddressItem> GetAddresses()
        {
            using ( RockContext rockContext = new RockContext() )
            {
                var familyGroupType = CacheGroupType.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                var homeLoc = CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                var inactiveStatus = CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );

                if ( familyGroupType != null && homeLoc != null && inactiveStatus != null )
                {
                    return new GroupMemberService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            m.Group.GroupTypeId == familyGroupType.Id && // Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY
                            m.Person.RecordStatusValueId != inactiveStatus.Id && // Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE
                            m.Group.GroupLocations.Any( gl => gl.GroupLocationTypeValueId.HasValue &&
                                     gl.GroupLocationTypeValueId == homeLoc.Id ) ) // CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME
                        .Select( m => new
                        {
                            m.PersonId,
                            m.GroupId,
                            m.Person.FirstName,
                            m.Person.LastName,
                            m.Person.Aliases,
                            HomeLocations = m.Group.GroupLocations
                                .Where( gl =>
                                    gl.GroupLocationTypeValueId.HasValue &&
                                    gl.GroupLocationTypeValueId == homeLoc.Id ) // CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME
                                .Select( gl => new
                                {
                                    gl.Location.Street1,
                                    gl.Location.Street2,
                                    gl.Location.City,
                                    gl.Location.State,
                                    gl.Location.PostalCode,
                                    gl.Location.Country
                                } ).FirstOrDefault()
                        } )
                        .GroupBy( m => m.PersonId )
                        .Select( g => new
                        {
                            PersonId = g.Key,
                            HomeLocations = new PersonAddressItem()
                            {
                                PersonId = g.Key,
                                FamilyId = g.FirstOrDefault().GroupId,
                                PersonAliasId = g.FirstOrDefault().Aliases.Count == 0 ? 0 : g.FirstOrDefault().Aliases.FirstOrDefault().Id,
                                FirstName = g.FirstOrDefault().FirstName,
                                LastName = g.FirstOrDefault().LastName,
                                Street1 = g.FirstOrDefault().HomeLocations.Street1,
                                Street2 = g.FirstOrDefault().HomeLocations.Street2,
                                City = g.FirstOrDefault().HomeLocations.City,
                                State = g.FirstOrDefault().HomeLocations.State,
                                PostalCode = g.FirstOrDefault().HomeLocations.PostalCode,
                                Country = g.FirstOrDefault().HomeLocations.Country
                            }
                        } )
                        .ToDictionary( k => k.PersonId, v => v.HomeLocations );
                }
            }

            return null;
        }

    }
}
