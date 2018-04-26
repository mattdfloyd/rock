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
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using Rock.Attribute;
using Rock.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Checkr
{
    /// <summary>
    /// Checkr Background Check 
    /// </summary>
    [Description( "Checkr Background Check" )]
    [Export( typeof( BackgroundCheckComponent ) )]
    [ExportMetadata( "ComponentName", "Checkr" )]

    [UrlLinkField( "Request URL", "The Checkr URL to send requests to.", true, "https://services.priorityresearch.com/webservice/default.cfm", "", 0 )]
    [UrlLinkField( "Return URL", "The Web Hook URL for Checkr to send results to (e.g. 'http://www.mysite.com/Webhooks/ProtectMyMinistry.ashx').", true, "", "", 1 )]
    public class Checkr : BackgroundCheckComponent
    {
        /// <summary>
        /// Sends a background request to Checkr
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflow">The Workflow initiating the request.</param>
        /// <param name="personAttribute">The person attribute.</param>
        /// <param name="ssnAttribute">The SSN attribute.</param>
        /// <param name="requestTypeAttribute">The request type attribute.</param>
        /// <param name="billingCodeAttribute">The billing code attribute.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>
        /// True/False value of whether the request was successfully sent or not
        /// </returns>
        public override bool SendRequest( RockContext rockContext, Model.Workflow workflow,
                    CacheAttribute personAttribute, CacheAttribute ssnAttribute, CacheAttribute requestTypeAttribute,
                    CacheAttribute billingCodeAttribute, out List<string> errorMessages)
        {       
            errorMessages = new List<string>();

            try
            {
                // Check to make sure workflow is not null
                if ( workflow == null )
                {
                    errorMessages.Add( "The 'Protect My Ministry' background check provider requires a valid workflow." );
                    return false;
                }

                // Get the person that the request is for
                Person person = null;
                if ( personAttribute != null )
                {
                    Guid? personAliasGuid = workflow.GetAttributeValue( personAttribute.Key ).AsGuidOrNull();
                    if ( personAliasGuid.HasValue )
                    {
                        person = new PersonAliasService( rockContext ).Queryable()
                            .Where( p => p.Guid.Equals( personAliasGuid.Value ) )
                            .Select( p => p.Person )
                            .FirstOrDefault();
                        person.LoadAttributes( rockContext );
                    }
                }

                if ( person == null )
                {
                    errorMessages.Add( "The 'Protect My Ministry' background check provider requires the workflow to have a 'Person' attribute that contains the person who the background check is for." );
                    return false;
                }

                XElement orderElement = new XElement( "Order" );
                XElement subjectElement = new XElement( "Subject",
                    new XElement( "FirstName", person.FirstName ),
                    new XElement( "MiddleName", person.MiddleName ),
                    new XElement( "LastName", person.LastName )
                );
                orderElement.Add( subjectElement );

                if ( person.SuffixValue != null )
                {
                    subjectElement.Add( new XElement( "Generation", person.SuffixValue.Value ) );
                }
                if ( person.BirthDate.HasValue )
                {
                    subjectElement.Add( new XElement( "DOB", person.BirthDate.Value.ToString( "MM/dd/yyyy" ) ) );
                }

                if ( ssnAttribute != null )
                {
                    string ssn = Field.Types.SSNFieldType.UnencryptAndClean( workflow.GetAttributeValue( ssnAttribute.Key ) );
                    if ( !string.IsNullOrWhiteSpace( ssn ) && ssn.Length == 9 )
                    {
                        subjectElement.Add( new XElement( "SSN", ssn.Insert( 5, "-" ).Insert( 3, "-" ) ) );
                    }
                }

                if ( person.Gender == Gender.Male )
                {
                    subjectElement.Add( new XElement( "Gender", "Male" ) );
                }
                if ( person.Gender == Gender.Female )
                {
                    subjectElement.Add( new XElement( "Gender", "Female" ) );
                }

                string dlNumber = person.GetAttributeValue( "com.sparkdevnetwork.DLNumber" );
                if ( !string.IsNullOrWhiteSpace( dlNumber ) )
                {
                    subjectElement.Add( new XElement( "DLNumber", dlNumber ) );
                }

                if ( !string.IsNullOrWhiteSpace( person.Email ) )
                {
                    subjectElement.Add( new XElement( "EmailAddress", person.Email ) );
                }

                var homelocation = person.GetHomeLocation();
                if ( homelocation != null )
                {
                    subjectElement.Add( new XElement( "CurrentAddress",
                        new XElement( "StreetAddress", homelocation.Street1 ),
                        new XElement( "City", homelocation.City ),
                        new XElement( "State", homelocation.State ),
                        new XElement( "Zipcode", homelocation.PostalCode )
                    ) );
                }

                XElement aliasesElement = new XElement( "Aliases" );
                if ( person.NickName != person.FirstName )
                {
                    aliasesElement.Add( new XElement( "Alias", new XElement( "FirstName", person.NickName ) ) );
                }

                foreach ( var previousName in person.GetPreviousNames() )
                {
                    aliasesElement.Add( new XElement( "Alias", new XElement( "LastName", previousName.LastName ) ) );
                }

                if ( aliasesElement.HasElements )
                {
                    subjectElement.Add( aliasesElement );
                }

                return true;
            }

            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
                errorMessages.Add( ex.Message );
                return false;
            }
        }
    }
}
