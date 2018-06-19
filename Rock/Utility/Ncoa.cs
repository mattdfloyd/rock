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

namespace Rock.Utility
{
    class Ncoa
    {
        private string TRUE_NCOA_SERVER = "https://app.truencoa.com/api/";
        private int batchsize = 100;
        private string username = "gerhard@sparkdevnetwork.org";
        private string password = "testTrueNCOA";

        public void RequestNcoa()
        {
            Dictionary<int, PersonAddressItem> addresses = GetAddresses();
            string fileName = "_" + DateTime.Now.Ticks;
            UploadAddresses( addresses, fileName );
        }

        private Dictionary<int, PersonAddressItem> GetAddresses()
        {
            using ( RockContext rockContext = new RockContext() )
            {
                var familyGroupType = CacheGroupType.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                var homeLoc = CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                var inactiveStatus = CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );

                if ( familyGroupType != null && homeLoc != null && inactiveStatus != null )
                {
                    var personAliasDirectory = new PersonAliasService( rockContext )
                        .Queryable().AsNoTracking()
                        .ToDictionary( pa => pa.PersonId, pa => pa.AliasPersonId );

                    var activePeople = new PersonService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p =>
                        p.RecordStatusValueId != inactiveStatus.Id )
                        .Select( p => new
                        {
                            p.Id,
                            p.FirstName,
                            p.LastName
                        } ).ToDictionary( ap => ap.Id, ap => ap );

                    return new GroupMemberService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            m.Group.GroupTypeId == familyGroupType.Id &&
                            activePeople.ContainsKey( m.PersonId ) )
                        .Select( m => new
                        {
                            m.PersonId,
                            m.GroupId,
                            person = activePeople[m.PersonId],
                            HomeLocations = m.Group.GroupLocations
                                .Where( gl =>
                                    gl.GroupLocationTypeValueId.HasValue &&
                                    gl.GroupLocationTypeValueId == homeLoc.Id )
                                .Select( gl => gl.Location )
                                .ToList()
                        } )
                        .GroupBy( m => m.PersonId )
                        .Select( g => new
                        {
                            PersonId = g.Key,
                            HomeLocations = new PersonAddressItem()
                            {
                                PersonId = g.Key,
                                FamilyId = g.First().GroupId,
                                PersonAliasId = personAliasDirectory.GetValueOrNull( g.Key ) ?? 0,
                                FirstName = g.First().person.FirstName,
                                LastName = g.First().person.LastName,
                                Locations = g.SelectMany( m => m.HomeLocations ).ToList()
                            }
                        } )
                        .ToDictionary( k => k.PersonId, v => v.HomeLocations );
                }
            }

            return null;
        }

        private bool UploadAddresses( Dictionary<int, PersonAddressItem> addresses, string fileName )
        {
            try
            {
                PersonAddressItem[] addressArray = addresses.Values.ToArray();
                StringBuilder data = new StringBuilder();
                for ( int i = 0; i < addressArray.Length; i++ )
                {
                    PersonAddressItem personAddressItem = addressArray[i];
                    if ( personAddressItem.Locations != null && personAddressItem.Locations.Count > 0 )
                    {
                        var location = personAddressItem.Locations.First();
                        data.AppendFormat( "{0}={1}&", "individual_id", personAddressItem.PersonId );
                        data.AppendFormat( "{0}={1}&", "individual_first_name", personAddressItem.FirstName );
                        data.AppendFormat( "{0}={1}&", "individual_last_name", personAddressItem.LastName );
                        data.AppendFormat( "{0}={1}&", "address_line_1", location.Street1 );
                        data.AppendFormat( "{0}={1}&", "address_line_2", location.Street2 );
                        data.AppendFormat( "{0}={1}&", "address_city_name", location.City );
                        data.AppendFormat( "{0}={1}&", "address_state_code", location.State );
                        data.AppendFormat( "{0}={1}&", "address_postal_code", location.PostalCode );
                        data.AppendFormat( "{0}={1}&", "PersonAliasId", personAddressItem.PersonAliasId );
                        data.AppendFormat( "{0}={1}&", "FamilyId", personAddressItem.FamilyId );

                        if ( i % batchsize == 0 || i == addressArray.Length - 1 )
                        {
                            using ( WebClient wc = new WebClient() )
                            {
                                wc.Headers["user_name"] = username;
                                wc.Headers["password"] = password;
                                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                wc.UploadString( TRUE_NCOA_SERVER + $"files/{fileName}/records", data.ToString() );
                                data = new StringBuilder();
                            }
                        }

                        i++;
                    }
                }

                // check to see if the file is ready to process
                using ( WebClient wc = new WebClient() )
                {
                    wc.Headers["user_name"] = username;
                    wc.Headers["password"] = password;
                    try
                    {
                        string json = wc.DownloadString( TRUE_NCOA_SERVER + $"files/{fileName}" );
                        File file = new JavaScriptSerializer().Deserialize<File>( json );
                        if ( file.Status != "Mapped" )
                        {
                            Console.WriteLine( $"The filename: {fileName} is not in the correct status" );
                            return false;
                        }
                    }
                    catch ( Exception ex )
                    {
                        Console.WriteLine( $"Invalid filename: {fileName}" ); //todo: update exception
                        return false;
                    }
                }

                return true;
            }
            catch ( Exception ex )
            {
                return false;
            }
        }

        private bool CreateReport( string fileName )
        {
            try
            {
                // submit for processing
                using ( WebClient wc = new WebClient() )
                {
                    wc.Headers["user_name"] = username;
                    wc.Headers["password"] = password;
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.UploadString( TRUE_NCOA_SERVER + $"files/{fileName}", "PATCH", "status=submit" );
                }

                return true;
            }
            catch ( Exception ex )
            {
                return false;
            }
        }

        private bool IsReportCreated( string fileName )
        {
            try
            {
                using ( WebClient wc = new WebClient() )
                {
                    wc.Headers["user_name"] = username;
                    wc.Headers["password"] = password;
                    string json = wc.DownloadString( TRUE_NCOA_SERVER + $"files/{fileName}" );
                    File file = new JavaScriptSerializer().Deserialize<File>( json );
                    bool processing = ( file.Status == "Import" || file.Status == "Importing" || file.Status == "Parse" || file.Status == "Parsing" || file.Status == "Report" || file.Status == "Reporting" || file.Status == "Process" || file.Status == "Processing" );
                    return !processing;
                }
            }
            catch ( Exception ex )
            {
                return false;
            }
        }

        private bool CreateReportExport( string fileName, out string exportfileid )
        {
            exportfileid = null;
            try
            {
                using ( WebClient wc = new WebClient() )
                {
                    wc.Headers["user_name"] = username;
                    wc.Headers["password"] = password;
                    string json = wc.DownloadString( TRUE_NCOA_SERVER + $"files/{fileName}/report" );
                }

                // submit for exporting
                using ( WebClient wc = new WebClient() )
                {
                    wc.Headers["user_name"] = username;
                    wc.Headers["password"] = password;
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    string json = wc.UploadString( TRUE_NCOA_SERVER + $"files/{fileName}", "PATCH", "status=export" );
                    File file = new JavaScriptSerializer().Deserialize<File>( json );
                    exportfileid = file.Id;
                }

                return true;
            }
            catch ( Exception ex )
            {
                return false;
            }

        }

        private bool IsReportExportCreated( string exportfileid )
        {
            try
            {
                using ( WebClient wc = new WebClient() )
                {
                    wc.Headers["user_name"] = username;
                    wc.Headers["password"] = password;
                    string json = wc.DownloadString( TRUE_NCOA_SERVER + $"files/{exportfileid}" );
                    File file = new JavaScriptSerializer().Deserialize<File>( json );
                    bool exporting = ( file.Status == "Export" || file.Status == "Exporting" );
                    return !exporting;
                }
            }
            catch ( Exception ex )
            {
                return false;
            }
        }

        private bool DownloadExport( string exportfileid, out List<Record> records )
        {
            records = null;
            try
            {
                using ( WebClient wc = new WebClient() )
                {
                    wc.Headers["user_name"] = username;
                    wc.Headers["password"] = password;
                    string json = wc.DownloadString( TRUE_NCOA_SERVER + $"files/{exportfileid}/records" );
                    File file = new JavaScriptSerializer().Deserialize<File>( json );
                    var obj = JObject.Parse( json );
                    file = new JavaScriptSerializer().Deserialize<File>( json );
                    records = file.Records;
                }
                return true;
            }
            catch ( Exception ex )
            {
                return false;
            }
        }

        private void SaveRecords( List<Record> records, string fileName )
        {
            DataTable dtRecords = null;
            string recordsjson = JsonConvert.SerializeObject( records );
            dtRecords = (DataTable)JsonConvert.DeserializeObject( recordsjson, ( typeof( DataTable ) ) );
            StringBuilder sb = new StringBuilder();
            IEnumerable<string> columnNames = dtRecords.Columns.Cast<DataColumn>().Select( column => column.ColumnName );
            sb.AppendLine( string.Join( ",", columnNames ) );
            foreach ( DataRow row in dtRecords.Rows )
            {
                IEnumerable<string> fields = row.ItemArray.Select( field => string.Concat( "\"", field.ToString().Replace( "\"", "\"\"" ), "\"" ) );
                sb.AppendLine( string.Join( ",", fields ) );
            }

            if ( System.IO.File.Exists( fileName ) )
            {
                System.IO.File.Delete( fileName );
            }

            System.IO.File.WriteAllText( fileName, sb.ToString() );
        }

        private class PersonAddressItem
        {
            public int PersonId { get; set; }
            public int PersonAliasId { get; set; }
            public int FamilyId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<Location> Locations { get; set; }
        }

        private class File
        {
            public string Name { get; set; }
            public string Status { get; set; }
            public string Id { get; set; }
            public int RecordCount { get; set; }
            public List<Record> Records { get; set; }
        }

        private class Record
        {
            public string input_individual_id { get; set; }
            public string input_individual_first_name { get; set; }
            public string input_individual_last_name { get; set; }
            public string input_address_line_1 { get; set; }
            public object input_address_line_2 { get; set; }
            public string input_address_city { get; set; }
            public string input_address_state_code { get; set; }
            public string input_address_postal_code { get; set; }
            public string input_address_country_code { get; set; }
            public int global_id { get; set; }
            public int record_id { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public object company_name { get; set; }
            public string street_number { get; set; }
            public string street_pre_direction { get; set; }
            public string street_name { get; set; }
            public string street_post_direction { get; set; }
            public string street_suffix { get; set; }
            public string unit_type { get; set; }
            public string unit_number { get; set; }
            public string box_number { get; set; }
            public string city_name { get; set; }
            public string state_code { get; set; }
            public string postal_code { get; set; }
            public string postal_code_extension { get; set; }
            public string carrier_route { get; set; }
            public string address_status { get; set; }
            public string error_number { get; set; }
            public string address_type { get; set; }
            public string delivery_point { get; set; }
            public string check_digit { get; set; }
            public string delivery_point_verification { get; set; }
            public string delivery_point_verification_notes { get; set; }
            public string vacant { get; set; }
            public string congressional_district_code { get; set; }
            public string area_code { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
            public string time_zone { get; set; }
            public string county_name { get; set; }
            public string county_fips { get; set; }
            public string state_fips { get; set; }
            public string barcode { get; set; }
            public object lacs { get; set; }
            public string line_of_travel { get; set; }
            public string ascending_descending { get; set; }
            public string move_applied { get; set; }
            public string move_type { get; set; }
            public string move_date { get; set; }
            public double? move_distance { get; set; }
            public string match_flag { get; set; }
            public string nxi { get; set; }
            public object ank { get; set; }
            public string residential_delivery_indicator { get; set; }
            public string record_type { get; set; }
            public string record_source { get; set; }
            public string country_code { get; set; }
            public string address_line_1 { get; set; }
            public string address_line_2 { get; set; }
            public int address_id { get; set; }
            public int household_id { get; set; }
            public int individual_id { get; set; }
        }
    }
}
