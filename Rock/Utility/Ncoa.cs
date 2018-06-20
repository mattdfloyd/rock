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

namespace Rock.Utility
{
    public class Ncoa
    {
        private string trueNcoaServer = "https://app.testing.truencoa.com/api/"; // "https://app.truencoa.com/api/";
        private string TRUE_NCOA_SERVER = "https://app.testing.truencoa.com";
        private int batchsize = 100;
        private string username = "gerhard@sparkdevnetwork.org";
        private string password = "testTrueNCOA";
        private RestClient client = null;
        private string _id;


        public Ncoa(string id)
        {
            CreateRestClient();
            _id = id;
        }

        public void RequestNcoa()
        {
            Dictionary<int, PersonAddressItem> addresses = GetAddresses();
            string fileName = "_" + DateTime.Now.Ticks;
            UploadAddresses( addresses, fileName );
        }

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

        /// <summary>
        /// Creates the rest client.
        /// </summary>
        private void CreateRestClient()
        {
            client = new RestClient( TRUE_NCOA_SERVER );
            client.AddDefaultHeader( "user_name", username );
            client.AddDefaultHeader( "password", password );
            client.AddDefaultHeader( "content-type", "application/x-www-form-urlencoded" );
        }


        public bool UploadAddresses( Dictionary<int, PersonAddressItem> addresses, string fileName )
        {
            try
            {
                PersonAddressItem[] addressArray = addresses.Values.ToArray();
                StringBuilder data = new StringBuilder();
                for ( int i = 1; i <= addressArray.Length; i++ )
                {
                    PersonAddressItem personAddressItem = addressArray[i-1];
                    data.AppendFormat( "{0}={1}&", "individual_id", personAddressItem.PersonId );
                    data.AppendFormat( "{0}={1}&", "individual_first_name", personAddressItem.FirstName );
                    data.AppendFormat( "{0}={1}&", "individual_last_name", personAddressItem.LastName );
                    data.AppendFormat( "{0}={1}&", "address_line_1", personAddressItem.Street1 );
                    data.AppendFormat( "{0}={1}&", "address_line_2", personAddressItem.Street2 );
                    data.AppendFormat( "{0}={1}&", "address_city_name", personAddressItem.City );
                    data.AppendFormat( "{0}={1}&", "address_state_code", personAddressItem.State );
                    data.AppendFormat( "{0}={1}&", "address_postal_code", personAddressItem.PostalCode );
                    data.AppendFormat( "{0}={1}&", "address_country_code", personAddressItem.Country );
                    //data.AppendFormat( "{0}={1}&", "personAliasId", personAddressItem.PersonAliasId );
                    //data.AppendFormat( "{0}={1}&", "familyId", personAddressItem.FamilyId );

                    if ( i % batchsize == 0 || i == addressArray.Length )
                    {
                        var request = new RestRequest( $"api/files/{fileName}/records", Method.POST );
                        //request.AddHeader( "content-type", "application/x-www-form-urlencoded" );
                        request.AddHeader( "id", _id );
                        request.AddParameter( "application/x-www-form-urlencoded", data.ToString().TrimEnd('&'), ParameterType.RequestBody );
                        IRestResponse response = client.Execute( request );
                        if (response.StatusCode != HttpStatusCode.OK )
                        {
//Todo: message
                            return false;
                        }

                        data = new StringBuilder();

                        /*
                        IRestResponse response = client.Execute( request );

                        using ( WebClient wc = new WebClient() )
                        {
                            wc.Headers["user_name"] = username;
                            wc.Headers["password"] = password;
                            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                            wc.UploadString( trueNcoaServer + $"files/{fileName}/records", data.ToString() );
                            data = new StringBuilder();
                        }
                        */
                    }
                }

                /*
                // check to see if the file is ready to process
                using ( WebClient wc = new WebClient() )
                {
                    wc.Headers["user_name"] = username;
                    wc.Headers["password"] = password;
                    try
                    {
                        string json = wc.DownloadString( trueNcoaServer + $"files/{fileName}" );
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
                */

            }
            catch ( Exception ex )
            {
                return false;
            }

            try
            {
                var request = new RestRequest( $"api/files/{fileName}", Method.GET );
                //request.AddHeader( "content-type", "application/x-www-form-urlencoded" );
                request.AddHeader( "id", _id );
                request.AddParameter( "application/x-www-form-urlencoded", "status=submit", ParameterType.RequestBody );
                IRestResponse response = client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    //Todo: message
                    return false;
                }

                try
                {
                    File file = new JavaScriptSerializer().Deserialize<File>( response.Content );
                    if ( file.Status != "Mapped" )
                    {
                        Console.WriteLine( $"The filename: {fileName} is not in the correct status" );
                        return false;
                    }
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( $"Invalid response: {response.Content}" ); //todo: update exception
                    return false;
                }

                /*
                using ( WebClient wc = new WebClient() )
                {
                    wc.Headers["user_name"] = username;
                    wc.Headers["password"] = password;
                    string json = wc.DownloadString( trueNcoaServer + $"files/{fileName}" );
                    File file = new JavaScriptSerializer().Deserialize<File>( json );
                    bool processing = ( file.Status == "Import" || file.Status == "Importing" || file.Status == "Parse" || file.Status == "Parsing" || file.Status == "Report" || file.Status == "Reporting" || file.Status == "Process" || file.Status == "Processing" );
                    return !processing;
                }
                */
            }
            catch ( Exception ex )
            {
                return false;
            }

            return true;
        }

        public bool CreateReport( string fileName )
        {
            try
            {
                // submit for processing
                var request = new RestRequest( $"api/files/{fileName}", Method.PATCH );
                //request.AddHeader( "content-type", "application/x-www-form-urlencoded" );
                request.AddHeader( "id", _id );
                request.AddParameter( "application/x-www-form-urlencoded", "status=submit", ParameterType.RequestBody );
                IRestResponse response = client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    //Todo: message
                    return false;
                }

                /*
                using ( WebClient wc = new WebClient() )
                {
                    wc.Headers["user_name"] = username;
                    wc.Headers["password"] = password;
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.UploadString( trueNcoaServer + $"files/{fileName}", "PATCH", "status=submit" );
                }
                */

                return true;
            }
            catch ( Exception ex )
            {
                return false;
            }
        }

        public bool IsReportCreated( string fileName )
        {
            try
            {
                var request = new RestRequest( $"api/files/{fileName}", Method.GET );
                //request.AddHeader( "content-type", "application/x-www-form-urlencoded" );
                request.AddHeader( "id", _id );
                IRestResponse response = client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    //Todo: message
                    return false;
                }

                try
                {
                    File file = new JavaScriptSerializer().Deserialize<File>( response.Content );
                bool processing = ( file.Status == "Import" || file.Status == "Importing" || file.Status == "Parse" || file.Status == "Parsing" || file.Status == "Report" || file.Status == "Reporting" || file.Status == "Process" || file.Status == "Processing" );
                return !processing;
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( $"Invalid response: {response.Content}" ); //todo: update exception
                    return false;
                }

                /*
                using ( WebClient wc = new WebClient() )
                {
                    wc.Headers["user_name"] = username;
                    wc.Headers["password"] = password;
                    string json = wc.DownloadString( trueNcoaServer + $"files/{fileName}" );
                    File file = new JavaScriptSerializer().Deserialize<File>( json );
                    bool processing = ( file.Status == "Import" || file.Status == "Importing" || file.Status == "Parse" || file.Status == "Parsing" || file.Status == "Report" || file.Status == "Reporting" || file.Status == "Process" || file.Status == "Processing" );
                    return !processing;
                }
                */
            }
            catch ( Exception ex )
            {
                return false;
            }
        }

        public bool CreateReportExport( string fileName, out string exportfileid )
        {
            exportfileid = null;
            try
            {
                /*
                using ( WebClient wc = new WebClient() )
                {
                    wc.Headers["user_name"] = username;
                    wc.Headers["password"] = password;
                    string json = wc.DownloadString( trueNcoaServer + $"files/{fileName}/report" );
                }
                */
                // submit for exporting
                var request = new RestRequest( $"api/files/{fileName}", Method.PATCH );
                //request.AddHeader( "content-type", "application/x-www-form-urlencoded" );
                request.AddHeader( "id", _id );
                request.AddParameter( "application/x-www-form-urlencoded", "status=export", ParameterType.RequestBody );
                IRestResponse response = client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    //Todo: message
                    return false;
                }

                try
                {
                    File file = new JavaScriptSerializer().Deserialize<File>( response.Content );
                    exportfileid = file.Id;
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( $"Invalid response: {response.Content}" ); //todo: update exception
                    return false;
                }

                /*
                using ( WebClient wc = new WebClient() )
                {
                    wc.Headers["user_name"] = username;
                    wc.Headers["password"] = password;
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    string json = wc.UploadString( trueNcoaServer + $"files/{fileName}", "PATCH", "status=export" );
                    File file = new JavaScriptSerializer().Deserialize<File>( json );
                    exportfileid = file.Id;
                }
                */

                return true;
            }
            catch ( Exception ex )
            {
                return false;
            }

        }

        public bool IsReportExportCreated( string exportfileid )
        {
            try
            {
                var request = new RestRequest( $"api/files/{exportfileid}", Method.GET );
                //request.AddHeader( "content-type", "application/x-www-form-urlencoded" );
                request.AddHeader( "id", _id );
                IRestResponse response = client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    //Todo: message
                    return false;
                }

                try
                {
                    File file = new JavaScriptSerializer().Deserialize<File>( response.Content );
                    bool exporting = ( file.Status == "Export" || file.Status == "Exporting" );
                    return !exporting;
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( $"Invalid response: {response.Content}" ); //todo: update exception
                    return false;
                }

                /*
                using ( WebClient wc = new WebClient() )
                {
                    wc.Headers["user_name"] = username;
                    wc.Headers["password"] = password;
                    string json = wc.DownloadString( trueNcoaServer + $"files/{exportfileid}" );
                    File file = new JavaScriptSerializer().Deserialize<File>( json );
                    bool exporting = ( file.Status == "Export" || file.Status == "Exporting" );
                    return !exporting;
                }
                */
            }
            catch ( Exception ex )
            {
                return false;
            }
        }

        public bool DownloadExport( string exportfileid, out List<TrueNcoaReturnRecord> records )
        {
            records = null;
                /*
                using ( WebClient wc = new WebClient() )
                {
                    wc.Headers["user_name"] = username;
                    wc.Headers["password"] = password;
                    string json = wc.DownloadString( trueNcoaServer + $"files/{exportfileid}/records" );
                    File file = new JavaScriptSerializer().Deserialize<File>( json );
                    var obj = JObject.Parse( json );
                    var recordsjson = (string)obj["Records"].ToString();
                    records = new JavaScriptSerializer().Deserialize<List<TrueNcoaReturnRecord>>( recordsjson );
                }
                */

                try
                {
                    var request = new RestRequest( $"api/files/{exportfileid}/records", Method.GET );
                    //request.AddHeader( "content-type", "application/x-www-form-urlencoded" );
                    request.AddHeader( "id", _id );
                    request.AddParameter( "application/x-www-form-urlencoded", "status=submit", ParameterType.RequestBody );
                    IRestResponse response = client.Execute( request );
                    if ( response.StatusCode != HttpStatusCode.OK )
                    {
                        //Todo: message
                        return false;
                    }

                    try
                    {
                        File file = new JavaScriptSerializer().Deserialize<File>( response.Content );
                        var obj = JObject.Parse( response.Content );
                        var recordsjson = (string)obj["Records"].ToString();
                        records = new JavaScriptSerializer().Deserialize<List<TrueNcoaReturnRecord>>( recordsjson );
                    }
                    catch ( Exception ex )
                    {
                        Console.WriteLine( $"Invalid response: {response.Content}" ); //todo: update exception
                        return false;
                    }


                    return true;
            }
            catch ( Exception ex )
            {
                return false;
            }
        }

        public void SaveRecords( List<TrueNcoaReturnRecord> records, string fileName )
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

        public void ParseData()
        {
        }

        public class PersonAddressItem
        {
            public int PersonId { get; set; }
            public int PersonAliasId { get; set; }
            public int FamilyId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Street1 { get; set; }
            public string Street2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string PostalCode { get; set; }
            public string Country { get; set; }
        }

        public class File
        {
            public string Name { get; set; }
            public string Status { get; set; }
            public string Id { get; set; }
            public int RecordCount { get; set; }
        }

        public class TrueNcoaReturnRecord
        {
            [JsonProperty( "input_individual_id" )]
            public string InputIndividualId { get; set; }

            [JsonProperty( "input_individual_first_name" )]
            public string InputIndividualFirstName { get; set; }

            [JsonProperty( "input_individual_last_name" )]
            public string InputIndividualLastName { get; set; }

            [JsonProperty( "input_address_line_1" )]
            public string InputAddressLine1 { get; set; }

            [JsonProperty( "input_address_line_2" )]
            public object InputAddressLine2 { get; set; }

            [JsonProperty( "input_address_city" )]
            public string InputAddressCity { get; set; }

            [JsonProperty( "input_address_state_code" )]
            public string InputAddressStateCode { get; set; }

            [JsonProperty( "input_address_postal_code" )]
            public string InputAddressPostalCode { get; set; }

            [JsonProperty( "input_address_country_code" )]
            public string InputAddressCountryCode { get; set; }

            [JsonProperty( "global_id" )]
            public int GlobalId { get; set; }

            [JsonProperty( "record_id" )]
            public int RecordId { get; set; }

            [JsonProperty( "first_name" )]
            public string FirstName { get; set; }

            [JsonProperty( "last_name" )]
            public string LastName { get; set; }

            [JsonProperty( "company_name" )]
            public object CompanyName { get; set; }

            [JsonProperty( "street_number" )]
            public string StreetNumber { get; set; }

            [JsonProperty( "street_pre_direction" )]
            public string StreetPreDirection { get; set; }

            [JsonProperty( "street_name" )]
            public string StreetName { get; set; }

            [JsonProperty( "street_post_direction" )]
            public string StreetPostDirection { get; set; }

            [JsonProperty( "street_suffix" )]
            public string StreetSuffix { get; set; }

            [JsonProperty( "unit_type" )]
            public string UnitType { get; set; }

            [JsonProperty( "unit_number" )]
            public string UnitNumber { get; set; }

            [JsonProperty( "box_number" )]
            public string BoxNumber { get; set; }

            [JsonProperty( "city_name" )]
            public string CityName { get; set; }

            [JsonProperty( "state_code" )]
            public string StateCode { get; set; }

            [JsonProperty( "postal_code" )]
            public string PostalCode { get; set; }

            [JsonProperty( "postal_code_extension" )]
            public string PostalCodeExtension { get; set; }

            [JsonProperty( "carrier_route" )]
            public string CarrierRoute { get; set; }

            [JsonProperty( "address_status" )]
            public string AddressStatus { get; set; }

            [JsonProperty( "error_number" )]
            public string ErrorNumber { get; set; }

            [JsonProperty( "address_type" )]
            public string AddressType { get; set; }

            [JsonProperty( "delivery_point" )]
            public string DeliveryPoint { get; set; }

            [JsonProperty( "check_digit" )]
            public string CheckDigit { get; set; }

            [JsonProperty( "delivery_point_verification" )]
            public string DeliveryPointVerification { get; set; }

            [JsonProperty( "delivery_point_verification_notes" )]
            public string DeliveryPointVerificationNotes { get; set; }

            [JsonProperty( "vacant" )]
            public string Vacant { get; set; }

            [JsonProperty( "congressional_district_code" )]
            public string CongressionalDistrictCode { get; set; }

            [JsonProperty( "area_code" )]
            public string AreaCode { get; set; }

            [JsonProperty( "latitude" )]
            public string Latitude { get; set; }

            [JsonProperty( "longitude" )]
            public string Longitude { get; set; }

            [JsonProperty( "time_zone" )]
            public string TimeZone { get; set; }

            [JsonProperty( "county_name" )]
            public string CountyName { get; set; }

            [JsonProperty( "county_fips" )]
            public string CountyFIPS { get; set; }

            [JsonProperty( "state_fips" )]
            public string StateFIPS { get; set; }

            /// <summary>
            /// Gets or sets the barcode.
            /// </summary>
            /// <value>
            /// The barcode.
            /// </value>
            [JsonProperty( "barcode" )]
            public string Barcode { get; set; }

            /// <summary>
            /// Gets or sets the Locatable Address Conversion System (LACS).
            /// </summary>
            /// <value>
            /// The Locatable Address Conversion System (LACS).
            /// </value>
            [JsonProperty( "lacs" )]
            public object LACS { get; set; }

            [JsonProperty( "line_of_travel" )]
            public string LineOfTravel { get; set; }

            [JsonProperty( "ascending_descending" )]
            public string AscendingDescending { get; set; }

            [JsonProperty( "move_applied" )]
            public string MoveApplied { get; set; }

            [JsonProperty( "move_type" )]
            public string MoveType { get; set; }

            [JsonProperty( "move_date" )]
            public string MoveDate { get; set; }

            [JsonProperty( "move_distance" )]
            public double? MoveDistance { get; set; }

            [JsonProperty( "match_flag" )]
            public string MatchFlag { get; set; }

            /// <summary>
            /// Gets or sets the NXI. Status code returned during NCOA processing; This code identifies if a new address was provided and gives description of why or why not.
            /// <list type="table">
            /// <listheader>
            /// <term>Return code</term>
            /// <term>Caption</term>
            /// <term>Description</term>
            /// </listheader>
            /// <item>
            /// <term>A</term>
            /// <term>Full Address Match</term>
            /// <term>Matched record and a new address has been provided (NCOAlink 18Month Service code only).</term>
            /// </item>
            /// <item>
            /// <term>00</term>
            /// <term>No Matching Address - No new address provided</term>
            /// <term>No Match (NCOAlink 18-month Service code only)</term>
            /// </item>
            /// <item>
            /// <term>01</term>
            /// <term>New address outside US - no new address provided</term>
            /// <term>Found COA: Foreign Move - The input record matched to a business, individual or family type master file record but the new address was outside USPS delivery area.</term>
            /// </item>
            /// <item>
            /// <term>02</term>
            /// <term>No forwarding address</term>
            /// <term>Found COA: Moved Left No Address - The input record matched to a business, individual or family type master file record and the new address was not provided to USPS.</term>
            /// </item>
            /// <item>
            /// <term>03</term>
            /// <term>Closed PO box - No new address provided</term>
            /// <term>Found COA: Box Closed No - The Input record matched to a business, individual or family type master file record which contains an old address of PO BOX that has been closed without a forwarding address provided.</term>
            /// </item>
            /// <item>
            /// <term>04</term>
            /// <term>Address 2 Missing for family move - No new address provided</term>
            /// <term>Cannot match COA: Street Address with Secondary - The input record matched to a family record type on master file with an old address that contained secondary information. The input record does not contain secondary information. This address match situation requires individual name matching logic to obtain a match and individual names do not match.</term>
            /// </item>
            /// <item>
            /// <term>05</term>
            /// <term>Too many matches - No new address provided</term>
            /// <term>Found COA: New 11-digit DPBC (Delivery Point Barcode) is Ambiguous - The input record matched to a business, individual or family type master file record. The new address on the master file record could not be converted to a deliverable address because the DPBC represents more than one delivery point.</term>
            /// </item>
            /// <item>
            /// <term>06</term>
            /// <term>Partial Match - No new address provided</term>
            /// <term>Cannot Match COA: Conflicting Directions: Middle Name Related -There is more than one COA (individual or family type) record for the match algorithm and the middle names or initials on the COAs are different. Therefore, a single match result could not be determined.</term>
            /// </item>
            /// <item>
            /// <term>07</term>
            /// <term>Partial Match - No new address provided</term>
            /// <term>Cannot Match COA: Conflicting Directions: Gender Related -There is more than one COA (individual or family type) record for the match algorithm and the genders of the names on the COAs are different. Therefore, a single match result could not be determined.</term>
            /// </item>
            /// <item>
            /// <term>08</term>
            /// <term>Too many possible matches - No new address provided</term>
            /// <term>Cannot Match COA: Other Conflicting Instructions - The input record matched to two master file (business, individual or family type) records. The two records in the master file were compared and due to differences in the new addresses, a match could not be made.</term>
            /// </item>
            /// <item>
            /// <term>09</term>
            /// <term>Family move address does not match individual name - No new address provided</term>
            /// <term>Cannot Match COA: High-rise Default - The input record matched to a family record on the master file from a High- rise address ZIP+4 coded to the building default. This address match situation requires individual name matching logic to obtain a match and individual names do not match.</term>
            /// </item>
            /// <item>
            /// <term>10</term>
            /// <term>Family move address does not match individual name</term>
            /// <term>Cannot Match COA: Rural Default - The input record matched to a family record on the master file from a Rural Route or Highway Contract Route address ZIP+4 coded to the route default. This address situation requires individual name matching logic to obtain a match and individual names do not match.</term>
            /// </item>
            /// <item>
            /// <term>11</term>
            /// <term>Only last name was matched - No new address provided</term>
            /// <term>Cannot Match COA: Individual Match: Insufficient COA Name for Match - There is a master file (individual or family type) record with the same surname and address but there is insufficient name information on the master file record to produce a match using individual matching logic.</term>
            /// </item>
            /// <item>
            /// <term>12</term>
            /// <term>Middle name does not match - No new address provided</term>
            /// <term>Cannot Match COA: Middle Name Test Failed - The input record matched to an individual or family record on the master file with the same address and surname. However, a match cannot be made because the input name contains a conflict with the middle name or initials on the master file record.</term>
            /// </item>
            /// <item>
            /// <term>13</term>
            /// <term>Gender does not match - No new address provided</term>
            /// <term>Cannot Match COA: Gender Test Failed - The input record matched to a master file (individual or family type) record.A match cannot be made because the gender of the name on the input record conflicts with the gender of the name on the master file record.</term>
            /// </item>
            /// <item>
            /// <term>14</term>
            /// <term>Undeliverable Address - No new address provided</term>
            /// <term>Found COA: New Address Would Not Convert at Run Time - The input record matched to a master file (business, individual or family type) record. The new address could not be converted to a deliverable address.</term>
            /// </item>
            /// <item>
            /// <term>15</term>
            /// <term>First name is missing - No new address provided</term>
            /// <term>Cannot Match COA: Individual Name Insufficient - There is a master file record with the same address and surname. A match cannot be made because the input record does not contain a first name or contains initials only.</term>
            /// </item>
            /// <item>
            /// <term>16</term>
            /// <term>No matching apt number - No new address provided</term>
            /// <term>Cannot Match COA: Secondary Number Discrepancy - The input record matched to a street level individual or family type record. However, a match is prohibited based on I of the following reasons: 1) There is conflicting secondary information on the input and master file record; 2) the input record contained secondary information and matched to a family record that does not contain secondary information. In item 2, this address match situation requires individual name matching logic to obtain a COA match and individual names do not match.</term>
            /// </item>
            /// <item>
            /// <term>17</term>
            /// <term>First name doesn't match - No new address provided</term>
            /// <term>Cannot Match COA: Other Insufficient Name - The input record matched to an individual or family master file record. The input name is different or not sufficient enough to produce a match.</term>
            /// </item>
            /// <item>
            /// <term>18</term>
            /// <term>Family move with General address - No new address provided</term>
            /// <term>Cannot Match COA: General Delivery - The input record matched to a family record on the master file from a General Delivery address. This address situation requires individual name matching logic to obtain a match and individual names do not match.</term>
            /// </item>
            /// <item>
            /// <term>19</term>
            /// <term>No Zip Code found - No new address provided</term>
            /// <term>Found COA: New Address not ZIP+4 coded - There is a change of address on file but the new address cannot be ZIP+4 coded and therefore there is no 11 -digit DPBC to store or return.</term>
            /// </item>
            /// <item>
            /// <term>20</term>
            /// <term>Cannot determine single address - No new address provided</term>
            /// <term>Cannot Match COA: Conflicting Directions after re-chaining - Multiple master file records were potential matches for the input record. The master file records contained different new addresses and a single match result could not be determined.</term>
            /// </item>
            /// <item>
            /// <term>66</term>
            /// <term>Deleted address with no forwarding address.</term>
            /// <term>Daily Delete - The input record matched to a business, individual or family type master file record with an old address that is present in the daily delete file. The presence of an address in the daily delete file means that a COA with this address is pending deletion from the master file and that no mail may be forwarded from this address.</term>
            /// </item>
            /// <item>
            /// <term>91</term>
            /// <term>Matched; secondary address may be missing</term>
            /// <term>Found COA: Secondary Number dropped from COA. The input record matched to a master file record. The master file record had a secondary number and the input address did not. A new address was provided.</term>
            /// </item>
            /// <item>
            /// <term>92</term>
            /// <term>Matched; secondary address may be incorrect</term>
            /// <term>Found COA: Secondary Number Dropped from input address. The input record matched to a master file record, but the input address had a secondary number and the master file record did not. The record is a ZIP + 4® street level match. A new address was provided.</term>
            /// </item>
            /// </list>
            /// </summary>
            /// <value>
            /// The NXI
            /// </value>
            [JsonProperty( "nxi" )]
            public string NXI { get; set; }

            /// <summary>
            /// Gets or sets the Address Not Known (ANK).
            /// <list type="table">
            /// <listheader>
            /// <term>Return code</term>
            /// <term>Caption</term>
            /// <term>Description</term>
            /// </listheader>
            /// <item>
            /// <term>77</term>
            /// <term>ANK - Address Not Known</term>
            /// <term>The record was not found. You should suppress these records from your database or flag these records for deletion and not mail to them.</term>
            /// </item>
            /// <item>
            /// <term>48</term>
            /// <term>48 Month NCOA</term>
            /// <term>The record was found in the 48 month NCOA. This record moved between 19-48 months ago.</term>
            /// </item>
            /// </list>
            /// </summary>
            /// <value>
            /// The ANK.
            /// </value>
            [JsonProperty( "ank" )]
            public object ANK { get; set; }

            [JsonProperty( "residential_delivery_indicator" )]
            public string residential_delivery_indicator { get; set; }

            [JsonProperty( "record_type" )]
            public string record_type { get; set; }

            [JsonProperty( "record_source" )]
            public string record_source { get; set; }

            [JsonProperty( "country_code" )]
            public string country_code { get; set; }

            [JsonProperty( "address_line_1" )]
            public string address_line_1 { get; set; }

            [JsonProperty( "address_line_2" )]
            public string address_line_2 { get; set; }

            [JsonProperty( "address_id" )]
            public int address_id { get; set; }

            [JsonProperty( "household_id" )]
            public int household_id { get; set; }

            [JsonProperty( "individual_id" )]
            public int individual_id { get; set; }

            [JsonProperty( "personAliasId" )]
            public string personAliasId { get; set; }

            [JsonProperty( "familyId" )]
            public string familyId { get; set; }
        }
    }
}
